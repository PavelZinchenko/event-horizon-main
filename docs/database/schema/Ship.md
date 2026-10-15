# Ship

A ship is a hull: the image, the cell layout, the weapon mounts (barrels), the engine nozzles, and the hull bonuses. A hull has no components. A [ShipBuild](ShipBuild.md) adds components, perks, and satellites to a hull. The ship constructor, combat, the shipyard, the workshop, loot, and every ship query read ship records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `6` |
| Kind | collection |
| Folder | [`Resources/Database/Ship/`](../../../Assets/Modules/Database/Resources/Database/Ship/) |
| Records in the built-in catalog | 151: 107 `Common`, 21 `Flagship`, 11 `Special`, 10 `Drone`, 2 `Starbase` |
| Schema | [Ship.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Ship.xml) |
| Generated class | `GameDatabase.DataModel.Ship` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Ship.cs), [hand-written part](../../../Assets/Modules/Database/Scripts/DataModel/Ship.cs)) |
| Runtime wrapper | `ShipModel` ([ShipModel.cs](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs)): the hull plus layout expansions and perks |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `ShipType` | enum [ShipType](enums.md#shiptype) | `Common` | The role of the hull. It selects the ship queries that can pick the hull (see [Ship type and rarity](#ship-type-and-rarity)). |
| `ShipRarity` | enum [ShipRarity](enums.md#shiprarity) | `Normal` | Rarity. It filters queries, raises prices, and blocks selling (`Unique`). |
| `SizeClass` | enum [SizeClass](enums.md#sizeclass) | `Frigate` | Hull class. It selects the hangar slot, the satellite fit, component restrictions, the crafting price, the spawn distance, and the radar marker. `Undefined` gives a circle collider. |
| `Name` | string | | Display name, usually a localization key. It is the default name of every ship with this hull. The player can rename a ship. |
| `Description` | string | | Text in the workshop item panel ([ItemDescriptionPanel.cs:69](../../../Assets/Scripts/Legacy/GUI/ViewModel/Craft/ItemDescriptionPanel.cs#L69)). An empty value hides the text. |
| `Faction` | ref [Faction](Faction.md) | neutral | Hull faction. It is the research tree of the hull's ship technology, the faction of builds without `BuildFaction`, the shipyard that can upgrade the ship, and the faction filter of exploration encounters. `0` means neutral. |
| `IconImage` | image (`ShipIcon`) | | Small icon in fleet lists, the star map, and the loading screen. A missing sprite falls back to `ModelImage`. |
| `IconScale` | float | 0.1…100 | Scale of `IconImage` in fleet and hangar lists. The ship editor multiplies it by 1.4. |
| `ModelImage` | image (`Ship`) | | Hull sprite. The name also selects an optional combat prefab `Resources/Combat/Ships/<name>` (see [Combat object](#combat-object)). |
| `ModelScale` | float | 0.1…500 | Hull size in combat world units. It also sets the explosion type, the wreck default, the satellite fit, and the star-map icon size. |
| `EngineColor` | color | white | Color of the engine lights and trails. Combat also uses it for the shield bubble ([ShipFactory.cs:108](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L108)). |
| `Engines` | struct list [Engine](structures.md#engine) | | Engine nozzles. Each one gets a light and a trail. An empty list gives no engine effects. |
| `Layout` | [layout](../../database.md#layout) | | The cell grid. The cell count drives armor, weight, engine stats, price, experience, and threat. |
| `Barrels` | struct list [Barrel](structures.md#barrel) | | Weapon mounts. The n-th barrel belongs to the n-th group of connected weapon cells (see [Barrels](#barrels)). |
| `Features` | struct [ShipFeatures](#shipfeatures) | | Hull bonuses: resistances, multipliers, regeneration, built-in devices. |
| `VisualEffects` | struct [ShipVisualEffects](#shipvisualeffects) | | Overrides of the destruction effect, sound, and wreck. |
| `CellsExpansions` | enum [ToggleState](enums.md#togglestate) | `Default` | Whether the player can buy extra cells around the layout. `Default` uses `ShipSettings.DisableCellsExpansions`. See [Cell expansions](#cell-expansions). |
| `ColliderTolerance` | float | 0.02, 0…1 | Simplification tolerance of the polygon collider that combat builds from the sprite. A larger value gives fewer vertices. No built-in record sets it. |
| `EnginePosition`, `EngineSize` | vector, float | obsolete | One engine nozzle. Migration 1.0 → 1.1 adds it to `Engines` ([DatabaseUpgrader_0_1.cs:29](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_0_1.cs#L29)). |
| `ShipCategory` | int | obsolete | Old role. Migration 1.0 → 1.1 maps 1 → `Rare`, 2 → `Flagship`, 3 → `Special`, 4 → `Starbase` (type and size class), 5 → `Hidden`, 6 → `Drone` ([line 42](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_0_1.cs#L42)). |
| `EnergyResistance`, `KineticResistance`, `HeatResistance`, `Regeneration`, `BuiltinDevices`, `BaseWeightModifier` | | obsolete | Old hull bonuses. Migration 1.0 → 1.1 moves them into `Features`. `BaseWeightModifier` becomes `ShipWeightBonus` ([line 65](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_0_1.cs#L65)). The migration replaces an existing `Features` value. |

## Structures

### ShipFeatures

Hull bonuses. Schema: [ShipFeatures.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ShipFeatures.xml). `ShipModel` turns each `...Bonus` field into a multiplier `1 + bonus` ([ShipModel.cs:101](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs#L101)). The formulas are on [ShipSettings: Stat formulas](ShipSettings.md#stat-formulas).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `EnergyResistance`, `KineticResistance`, `HeatResistance` | float | −100…100 | Resistance bonus. It adds `bonus × (armor + equipment resistance)` to the resistance value. `1` gives 50 % resistance on a hull without resistance equipment, `5` gives 83 %. Negative values lower resistance. |
| `ShipWeightBonus` | float | −1…100 | Multiplies the hull weight and the minimum weight. `-0.5` halves them. |
| `EquipmentWeightBonus` | float | −1…100 | Multiplies the weight of installed components. |
| `VelocityBonus` | float | −1…100 | Multiplies engine power (speed and acceleration). |
| `TurnRateBonus` | float | −1…100 | Multiplies turn power. |
| `ArmorBonus` | float | −1…100 | Multiplies armor points, armor repair, and equipment resistance. |
| `ShieldBonus` | float | −1…100 | Multiplies shield points and shield recharge. |
| `EnergyBonus` | float | −1…100 | Multiplies energy points and the recharge from components. The base recharge from `ShipSettings` is not multiplied. |
| `DroneBuildSpeedBonus` | float | −1…100 | Multiplies the drone reconstruction speed. |
| `DroneAttackBonus`, `DroneDefenseBonus` | float | −1…100 | Added to the drone damage and defense multipliers of the drone bays. |
| `Regeneration` | bool | | The hull is bionic (organic). It repairs 1 % of its armor per second. Components with `NotForOrganicShips` do not fit, `NotForMechanicShips` components fit only here, and repair bots do not fit. Hives on infected planets use only bionic builds. |
| `BuiltinDevices` | ref list [Device](Device.md) | | Devices that every ship of this hull has without a component. A `Manual` device gets key slot 5, other devices are automatic ([ShipBuilder.cs:85](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L85)). A component with the same `DeviceClass` does not fit the ship. |

The `ItemDescriptionPanel` lists the hull bonuses in the workshop ([ItemDescriptionPanel.cs:177](../../../Assets/Scripts/Legacy/GUI/ViewModel/Craft/ItemDescriptionPanel.cs#L177)). The drone bonuses do not appear there.

### ShipVisualEffects

Destruction overrides. Schema: [ShipVisualEffects.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ShipVisualEffects.xml).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `LeaveWreck` | enum [ToggleState](enums.md#togglestate) | `Default` | Whether the destroyed ship leaves a wreck sprite. `Default` leaves a wreck when `ModelScale` ≥ 0.9. |
| `CustomExplosionEffect` | ref [VisualEffect](VisualEffect.md) | | Explosion effect. `0` uses the `ShipSettings` default. |
| `CustomExplosionSound` | audio | | Explosion sound. Empty uses the `ShipSettings` default. |

8 built-in hulls set `LeaveWreck` to `Disabled`. No built-in hull sets a custom explosion.

## Notes

### Ship type and rarity

[`ShipBuildQuery`](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs) selects builds by the hull's `ShipType` and `ShipRarity`. The build's `AvailableForEnemy` and `AvailableForPlayer` flags apply first (see [ShipBuild](ShipBuild.md#queries)).

| Consumer | Rule | Code |
| --- | --- | --- |
| Enemy and player queries | `Common` and `Flagship` only. | [ShipBuildQuery.cs:100](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L100) |
| `Starbases()`, `Drones()` | `Starbase`, `Drone` only. | [line 114](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L114) |
| `Common()`, `CommonAndRare()` | `Common` type with `Normal` (or `Rare`) rarity. Flagships are excluded. | [line 130](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L130) |
| `Flagships()` | `Flagship` type, any rarity. Boss fleets and exploration bosses use it. | [line 128](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L128) |
| `HiddenShips()` | Always empty. See [DB15.1](../../issues/database.md#db15-smaller-defects). | [line 142](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L142) |
| Random satellites on enemies | `Common` type only. | [ShipExtensions.cs:110](../../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L110) |
| Selling | Not `Unique`, and `Common` or `Flagship` type. | [ShipExtensions.cs:82](../../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L82) |
| Market price | A rarity other than `Normal` doubles the price. | [MarketShipItem.cs:26](../../../Assets/Scripts/Domain/Economy/ItemType/MarketShipItem.cs#L26) |
| Scrapping | `Common` type, not `Unique`. | [ScrapPanelViewModel.cs:129](../../../Assets/Scripts/Legacy/GUI/ViewModel/CargoHold/ScrapPanelViewModel.cs#L129) |
| Exploration wrecks and encounters | `Common` type with `Normal` or `Rare` rarity. Small = Frigate or Destroyer, big = Cruiser or Battleship. | [ExplorationData.cs:229](../../../Assets/Scripts/Domain/Exploration/ExplorationData.cs#L229), [line 282](../../../Assets/Scripts/Domain/Exploration/ExplorationData.cs#L282) |
| Teleport effects | `Starbase` and `Special` hulls cannot be teleported. | [TeleportAction.cs:64](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/TeleportAction.cs#L64) |
| Boss loot | A `Flagship` drops extra components of the ship's faction. Class 2 or more also drops a star. | [LootGenerator.cs:51](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L51) |
| Star-map fleets | The `Flagship` leads the fleet icon. `Starbase` ships are not drawn. | [OrbitalObjects.cs:190](../../../Assets/Scripts/Legacy/Map/System/OrbitalObjects.cs#L190), [FleetObjects.cs:24](../../../Assets/Scripts/Legacy/Map/System/FleetObjects.cs#L24) |
| Starbase | Cannot carry satellites. The ship editor hides some stats. Combat uses a slow starbase engine. | [ShipExtensions.cs:18](../../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L18), [ShipFactory.cs:232](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L232) |

`SizeClass` has these consumers:

- Hangar slots: the player fleet has a slot count per class ([PlayerSkills.cs:146](../../../Assets/Scripts/Domain/Player/PlayerSkills.cs#L146)). A ship fits a slot of its class or a bigger one ([ShipSquad.cs:81](../../../Assets/Scripts/Domain/Player/ShipSquad.cs#L81)). The exploration ship must be a `Frigate`.
- Spawn distance: `FilterByStarDistance` skips a build when `GalaxySettings.ShipMinSpawnDistance(SizeClass)` is greater than the star level ([ShipBuildQuery.cs:89](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L89)).
- Component restrictions: `ComponentRestrictions.ShipSizes` ([CompatibilityChecker.cs:13](../../../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs#L13)).
- Combat: a `Starbase`-class ship cannot be devoured ([DevourAction.cs:34](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/DevourAction.cs#L34)). The radar marker offset grows for `Titan` and `Starbase` ([Radar.cs:102](../../../Assets/Scripts/Gui/Combat/Radar.cs#L102)).

### Stats

The hull adds base values through [ShipSettings](ShipSettings.md#stat-formulas), multiplied by the `Features` bonuses. The cell count of the layout, including bought cells, is the main factor for armor, weight, and engine stats. Weight and engine power then give the speed limits ([EngineStats.cs:28](../../../Assets/Modules/ShipConstructor/Scripts/Model/EngineStats.cs#L28)).

Other formulas that use the cell count `n`:

| Value | Formula | Code |
| --- | --- | --- |
| Crafting price | `5 × n²` credits. `Titan` × 3, else `Rare` × 1.5. | [Price.cs:40](../../../Assets/Modules/Database/Scripts/Extensions/Price.cs#L40) |
| Crafting stars | `Titan`: `n / 10`. `Rare`: `1 + (n − 30) / 10`. Others: `n / 70`. | [Price.cs:52](../../../Assets/Modules/Database/Scripts/Extensions/Price.cs#L52) |
| Sale price, scrap | `n²` credits, `n / 5` scrap. | [ShipExtensions.cs:22](../../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L22) |
| Threat | `power(level) × (n + satellite cells) × (1 + 0.5 × class)` | [Threat.cs:10](../../../Assets/Modules/ShipConstructor/Scripts/Maths/Threat.cs#L10) |
| Experience reward | `(1 + (level + 1)^1.2) × n^1.2 × (1 + class)` | [Experience.cs:53](../../../Assets/Modules/ShipConstructor/Scripts/Maths/Experience.cs#L53) |

*Class* is the [DifficultyClass](ShipBuild.md#difficulty-class) of an enemy build.

### Barrels

`OnDataDeserialized` rebuilds `Barrels` through [`BarrelConverter`](../../../Assets/Modules/Database/Scripts/Utils/BarrelConverter.cs). The converter finds the groups of connected weapon cells (`4`) and numbers them in reading order of their first cell. It pairs the n-th barrel with the n-th group.

- When the counts differ, the converter logs `barrels do not fit layout <id>` ([BarrelConverter.cs:19](../../../Assets/Modules/Database/Scripts/Utils/BarrelConverter.cs#L19)).
- Extra barrels are dropped. Extra weapon groups have no barrel, so a weapon installed there never fires.
- The ship builder gives barrel indices 0…n−1 to the hull, then continues with the satellites' barrels ([ShipBuilder.cs:183](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L183)).

Perks can change barrels: `AutoTargeting` raises every arc to at least 20°, and `WeaponClass` clears every slot restriction. Both work on a copy ([ShipModel.cs:119](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs#L119)).

### Cell expansions

The player can buy extra cells in a shipyard. A *customizable cell* is an empty cell next to a hull cell (left, right, up, or down). This includes a one-cell ring outside the grid on each side that the hull touches ([LayoutModifications.cs:102](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L102), [line 248](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L248)).

1. `CellsExpansions` `Enabled` or `Disabled` decides. `Default` uses `ShipSettings.DisableCellsExpansions` ([LayoutModifications.cs:17](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L17)). The built-in setting is `false`, so every hull allows expansions.
2. A bought cell can be `Outer`, or the type of one of its neighbors in the original layout ([line 148](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L148)). A bought weapon cell becomes a custom weapon cell (`X`). It joins the barrel of the neighboring weapon group and takes its slot restriction ([ShipLayoutObsolete.cs:257](../../../Assets/Modules/ShipConstructor/Scripts/ShipLayoutObsolete.cs#L257)).
3. The shipyard price grows with the number of bought cells. The shipyard must have level `5 × cells + 5` and belong to the ship's faction, unless the ship is neutral ([UpgradePanel.cs:206](../../../Assets/Scripts/Gui/ShipService/UpgradePanel.cs#L206)).
4. The session saves the bought cells. They count as normal cells for all stats.

`ShipBuild.ExtendedLayout` converts every customizable cell at once, but those cells do not count and are not saved. See [ShipBuild](ShipBuild.md#extendedlayout).

### Shipyard upgrades

A shipyard can add a modification slot to a player ship. The required level is the workshop level of the hull's [technology](Technology.md#workshop), or 50 when the hull has no technology, plus a quality bonus. The shipyard must belong to the ship's faction, unless the ship is neutral ([UnlockSlotPanel.cs:44](../../../Assets/Scripts/Gui/Craft/UnlockSlotPanel.cs#L44)). The modifications themselves are on [ShipModSettings](ShipModSettings.md).

### Combat object

`ShipFactory.CreateShipObject` builds the combat object ([ShipFactory.cs:243](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L243)):

1. A prefab `Resources/Combat/Ships/<ModelImage>` wins when it exists. The prefab brings its own sprite and collider.
2. Otherwise the `Default` prefab gets the `ModelImage` sprite. A hull with `SizeClass` `Undefined` gets a circle collider. Other hulls get a polygon collider from the sprite, simplified with `ColliderTolerance`.
3. The body scale is `ModelScale`. The body mass is the ship weight ([line 324](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L324)).
4. Each engine gets a light of size `5 × Engine.Size / ModelScale` and a trail, both in `EngineColor` ([line 277](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L277)).

Destruction ([ShipFactory.cs:284](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L284)):

| Value | Rule |
| --- | --- |
| Small ship | `ModelScale` < 0.9. The threshold is in code. |
| Explosion effect | `CustomExplosionEffect`, else `ShipSettings.DroneExplosionEffect` for small ships or `ShipExplosionEffect` for others. With no effect at all, an old hard-coded explosion plays. |
| Explosion sound | `CustomExplosionSound`, else the matching `ShipSettings` sound. |
| Wreck | `LeaveWreck` `Enabled` or `Disabled` decides. `Default` gives a wreck for ships that are not small. |

### Satellite fit

A ship can carry a satellite when the satellite has a size class and the ship's class is equal or bigger, or when the satellite has no size class and `ModelScale` ≥ 2 × the satellite's `ModelScale` ([SatelliteExtensions.cs:18](../../../Assets/Modules/ShipConstructor/Scripts/Satellites/SatelliteExtensions.cs#L18)). The `SatelliteSize` perk raises both limits. Starbases carry no satellites. See [Satellite](Satellite.md#fit).

### Other consumers

- Crafted ships: the workshop builds a ship from the hull and copies the components of the first player build of the same hull ([Technology.cs:132](../../../Assets/Scripts/Domain/Technologies/Technology.cs#L132)). A hull without a player build is crafted empty.
- Saved ships store the hull ID, not the build ID ([ShipInfo.cs:50](../../../Assets/Modules/GameSession/Scripts/Model/ShipInfo.cs#L50)). A changed or removed hull ID breaks saves.
- `ExplorationSettings.OutpostShip` and `TurretShip` reference hulls directly. `LootContent` with type `EmptyShip` gives a hull without components.
- Star map: the fleet icon scales with `√ModelScale` ([ShipIcon.cs:28](../../../Assets/Scripts/Legacy/Map/ShipIcon.cs#L28)).

### Data rules

1. Give the layout one barrel per group of connected weapon cells, in reading order. The loader only logs a mismatch.
2. Give every `Common` and `Flagship` hull at least one player build when the hull has a technology. Otherwise the crafted ship is empty.
3. Keep `ModelScale` at 0.9 or more for ships that should leave a wreck and use the big explosion, or set `VisualEffects` explicitly.
4. Use `SizeClass` `Undefined` only for round hulls. It gives a circle collider and counts as smaller than `Frigate`.
5. Do not use the obsolete fields. The migration from 1.0 overwrites `Features`.
