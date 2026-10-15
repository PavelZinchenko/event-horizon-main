# ShipBuild

A ship build is a [Ship](Ship.md) hull with installed components, up to three perks, two optional satellites, and an optional AI. The game creates every non-crafted ship from a build: enemies, starbases, drones, starting ships, and ships from loot and shops.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `8` |
| Kind | collection |
| Folder | [`Resources/Database/Ship/Builds/`](../../../Assets/Modules/Database/Resources/Database/Ship/Builds/) |
| Records in the built-in catalog | 388: 155 with `DifficultyClass` 0, 155 with 1, 46 with 2, 32 with 3 |
| Schema | [ShipBuild.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ShipBuild.xml) |
| Generated class | `GameDatabase.DataModel.ShipBuild` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/ShipBuild.cs), [hand-written part](../../../Assets/Modules/Database/Scripts/DataModel/ShipBuild.cs)) |
| Runtime wrapper | `CommonShip` and `EnemyShip` ([CommonShip.cs](../../../Assets/Modules/ShipConstructor/Scripts/Ships/CommonShip.cs), [EnemyShip.cs](../../../Assets/Modules/ShipConstructor/Scripts/Ships/EnemyShip.cs)) |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `ShipId` | ref [Ship](Ship.md) | required | The hull. Model name `Ship`. The loader throws when it is missing. |
| `AvailableForPlayer` | bool | `true` | The player queries can pick the build: loot, shops, challenges, and the component set of crafted ships (see [Queries](#queries)). |
| `AvailableForEnemy` | bool | `true` | The enemy, starbase, and drone queries can pick the build. |
| `DifficultyClass` | enum [DifficultyClass](enums.md#difficultyclass) | `Default` | Elite class of an enemy build. It sets the star levels where the build appears, random component upgrades, threat, experience, and the class badge (see [Difficulty class](#difficulty-class)). |
| `BuildFaction` | ref [Faction](Faction.md) | neutral | Overrides the hull faction. `ShipBuild.Faction` returns `BuildFaction`, or `Ship.Faction` when this is neutral ([ShipBuild.cs:13](../../../Assets/Modules/Database/Scripts/DataModel/ShipBuild.cs#L13)). The wandering-ships filter, `BelongToFaction`, the black market, and the shipyard use it. Quest fleets use the hull faction instead ([DB14](../../issues/database.md#db14-quest-fleets-filter-by-hull-faction)). |
| `CustomAI` | ref [BehaviorTree](BehaviorTree.md) | | AI tree for this ship when the computer controls it. `0` uses the `CombatSettings` tree for the role: `EnemyAI`, `StarbaseAI`, or `CloneAI` ([ControllerFactory.cs:42](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs#L42)). For a drone, the drone bay's AI comes first. |
| `Components` | struct list [InstalledComponent](structures.md#installedcomponent) | | Installed components at hull cell positions. |
| `Perks` | struct [ShipBuildPerks](#shipbuildperks) | | Up to three ship modifications. |
| `ExtendedLayout` | bool | | Converts every customizable cell around the layout into a hull cell (see [ExtendedLayout](#extendedlayout)). |
| `RandomColor` | bool | | Gives each ship a random hue and a saturation of up to 0.25 ([CommonShip.cs:38](../../../Assets/Modules/ShipConstructor/Scripts/Ships/CommonShip.cs#L38)). |
| `LeftSatelliteBuild`, `RightSatelliteBuild` | ref [SatelliteBuild](SatelliteBuild.md) | | Satellites in the left and right slot. `0` means none. The game does not check the satellite size here. |
| `NotAvailableInGame` | bool | obsolete | Old flag. Migration 1.2 → 1.3 derives `AvailableForEnemy` (not this flag and the hull is not `Unique`) and `AvailableForPlayer` (not this flag and `DifficultyClass` is `Default`) ([DatabaseUpgrader_2_3.cs:20](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_2_3.cs#L20)). 7 built-in records still carry it. |

## Structures

### ShipBuildPerks

Schema: [ShipBuildPerks.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ShipBuildPerks.xml).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Perk1`, `Perk2`, `Perk3` | enum [ShipPerkType](enums.md#shipperktype) | `Empty` | A ship modification. The effects and their values are on [ShipModSettings](ShipModSettings.md#modifications). |

`ShipModel` creates one modification per non-empty perk ([ShipModel.cs:30](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs#L30)). The perks behave like modifications that the player buys in a shipyard, and the session saves them with the ship. Perk `UnlimitedRespawn` throws ([DB1](../../issues/database.md#db1-the-unlimitedrespawn-perk-crashes-ship-creation)). Drones ignore perks. In the built-in catalog only the 32 builds of class 3 have perks.

## Notes

### How a build becomes a ship

| Use | Code | What the game takes from the build |
| --- | --- | --- |
| Enemy ship (fleets, quests, exploration, challenges) | [ShipExtensions.Create](../../../Assets/Modules/ShipConstructor/Scripts/Extensions/ShipExtensions.cs#L104), [EnemyShip](../../../Assets/Modules/ShipConstructor/Scripts/Ships/EnemyShip.cs) | Everything. The ship level is `GalaxySettings.EnemyLevel` of the star level ± up to 10. A `Common` hull gets a random satellite in each empty slot with a 2/3 chance (see [SatelliteBuild](SatelliteBuild.md#random-satellites)). |
| Player ship (starting fleet, loot, shops) | [CommonShip(ShipBuild)](../../../Assets/Modules/ShipConstructor/Scripts/Ships/CommonShip.cs#L25) | Hull, components, perks, satellites, `RandomColor`, `CustomAI`. The components are locked. The starting fleet unlocks them ([StartingNewGameState.cs:78](../../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L78)). |
| Drone | [DroneBay.cs:36](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs#L36) | Hull, components, `ExtendedLayout`, `CustomAI` (after the bay AI). No perks, no satellites, no color. Any build can be a drone; the `Drone` hull type only matters for queries. |
| Crafted ship | [Technology.cs:132](../../../Assets/Scripts/Domain/Technologies/Technology.cs#L132) | Only the components of the first `PlayerShips` build of the hull. |

The session saves a player ship by hull ID ([ShipInfo.cs:50](../../../Assets/Modules/GameSession/Scripts/Model/ShipInfo.cs#L50)). After a reload, the ship has no `BuildFaction`, `CustomAI`, or difficulty class, and `ExtendedLayout` cells are gone. Components, perks, satellites, and color stay.

### Queries

[`ShipBuildQuery`](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs) is a lazy filter chain. Each start method applies the build flag and the hull type:

| Start method | Rule | Main users |
| --- | --- | --- |
| `EnemyShips` | `AvailableForEnemy`, hull `Common` or `Flagship` | Star fleets ([Fleet.cs](../../../Assets/Scripts/Legacy/Model/Factories/Fleet.cs)), quest fleets ([FleetFactory.cs:63](../../../Assets/Modules/Quests/Scripts/Factory/FleetFactory.cs#L63)), challenges, exploration |
| `PlayerShips` | `AvailableForPlayer`, hull `Common` or `Flagship` | Loot ([LootGenerator.cs:393](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L393)), black market, faction shops, challenge player ship, crafted ships, loading screen |
| `Starbases` | `AvailableForEnemy`, hull `Starbase` | Capital fleets ([Fleet.cs:97](../../../Assets/Scripts/Legacy/Model/Factories/Fleet.cs#L97)) |
| `Drones` | `AvailableForEnemy`, hull `Drone` | Exploration encounters |

`FilterByStarDistance(level, mode)` then applies up to three filters ([ShipBuildQuery.cs:77](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L77)):

| Filter | Rule |
| --- | --- |
| Faction | `ShipBuild.Faction` has wandering ships at this level. See [Faction: Distance ranges](Faction.md#distance-ranges). |
| Difficulty | Class between `Class1` (from level 50, else `Default`) and `level / 25`. |
| Size | `GalaxySettings.ShipMinSpawnDistance(SizeClass)` ≤ level. |

`BelongToFaction(faction)` compares `ShipBuild.Faction`. `Random` returns `null` on an empty result. The boss factory then throws when no flagship fits ([DB15.16](../../issues/database.md#db15-smaller-defects)).

### Difficulty class

An enemy ship keeps the class as `ExtraThreatLevel` ([EnemyShip.cs:12](../../../Assets/Modules/ShipConstructor/Scripts/Ships/EnemyShip.cs#L12)). The class does not multiply stats directly.

| Consumer | Effect | Code |
| --- | --- | --- |
| Enemy selection | See the difficulty filter above. Bosses use class 1, or up to 2 above level 50. Capital starbases use `Default` below level 40, else class 1. | [ShipBuildQuery.cs:196](../../../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs#L196), [Fleet.cs:36](../../../Assets/Scripts/Legacy/Model/Factories/Fleet.cs#L36) |
| Component upgrades | Class 1 or more gives each component a random quality between `P1` and `P3`. The maximum is `ship level / 75` (at most 3). From ship level 150, every component gets at least `P1`. An existing better modification stays. | [IComponentConverter.cs:46](../../../Assets/Modules/ShipConstructor/Scripts/Builder/IComponentConverter.cs#L46) |
| Threat | × `(1 + 0.5 × class)` | [Threat.cs:18](../../../Assets/Modules/ShipConstructor/Scripts/Maths/Threat.cs#L18) |
| Experience reward | × `(1 + class)` | [Experience.cs:53](../../../Assets/Modules/ShipConstructor/Scripts/Maths/Experience.cs#L53) |
| Boss loot | A flagship of class 2 or more drops one star and more components. | [LootGenerator.cs:57](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L57) |
| Arena | A ship with a class cannot join the arena. | [ShipValidator.cs:13](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipValidator.cs#L13) |
| UI | Class badge in fleet and combat lists. | [FleetPanel.cs:47](../../../Assets/Scripts/Gui/Quests/FleetPanel.cs#L47) |
| Drones | `ShipBuilder` multiplies damage, armor, and shield by `1 + 0.5 × class`, but the drone bay overwrites all three. No effect. | [ShipBuilder.cs:22](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L22), [DB15.14](../../issues/database.md#db15-smaller-defects) |

### ExtendedLayout

`ExtendedLayout` calls `FullyUpgrade` on the layout ([ShipModel.cs:22](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs#L22)). Each [customizable cell](Ship.md#cell-expansions) becomes a weapon cell when it touches a weapon cell, else inner, else engine, else outer ([LayoutModifications.cs:122](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/LayoutModifications.cs#L122)). Components can use these cells.

The conversion does not raise the added-cell count. The cells therefore add no armor or weight per cell, and the session does not save them. See [DB15.15](../../issues/database.md#db15-smaller-defects). All 32 built-in builds with `ExtendedLayout` are enemy-only builds of class 3.

### Startup check

In an editable database (the built-in catalog in the Unity Editor, or a folder mod), `InitializationState` builds every build of a `Common` or `Drone` hull that is available to the player or to enemies. It logs `invalid build: <id>` when the ship is not viable ([InitializationState.cs:102](../../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L102), [ShipValidator.cs:30](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipValidator.cs#L30)):

1. The energy recharge is 0 or less.
2. One shot of a weapon costs more energy than the ship has.
3. The ship has no weapon and no drone bay.

The check does not test placement. The ship builder skips components that do not fit or exceed their limits (see [InstalledComponent](structures.md#installedcomponent)).

### Data rules

1. Keep at least one enemy build per faction with wandering ships at every level, and one flagship of class 1 for each faction that can host a boss fleet.
2. Set `AvailableForPlayer` to `false` on elite builds (class 1 or more), as the 1.3 migration did. The player queries do not filter by class.
3. Use `ExtendedLayout` only on enemy builds. Player ships lose the cells on reload.
4. Do not use perk `UnlimitedRespawn` ([DB1](../../issues/database.md#db1-the-unlimitedrespawn-perk-crashes-ship-creation)).
5. A changed build ID does not break saves, but quests, settings (`StartingShipBuilds`, `DefaultStarbaseBuild`, `HiveShipBuild`), drone bays, and [hard-coded IDs](../../database.md#hard-coded-ids) reference builds by ID.
