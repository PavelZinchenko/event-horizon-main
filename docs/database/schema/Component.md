# Component

A component is an installable ship module. It has a cell shape, passive stats, and optionally one active part: a weapon, a device, or a drone bay. The player installs components in the ship editor. Enemy builds list them in [ShipBuild](ShipBuild.md). The ship editor, the ship builder, combat, loot, shops, the workshop, and saves read component records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `1` |
| Kind | collection |
| Folder | [`Resources/Database/Component/`](../../../Assets/Modules/Database/Resources/Database/Component/) |
| Records in the built-in catalog | 264: 115 weapons, 48 drone bays, 25 devices, 76 passive modules. One file has no `Id` (see [Data rules](#data-rules)). |
| Schema | [Component.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Component.xml), [ComponentRestrictions.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ComponentRestrictions.xml) |
| Generated class | `GameDatabase.DataModel.Component` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Component.cs), [hand-written part](../../../Assets/Modules/Database/Scripts/DataModel/Component.cs)) |
| Runtime wrapper | `ComponentInfo` ([ComponentInfo.cs](../../../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs)): the component plus a [modification](ComponentMod.md) and a quality. `CommonComponent` ([CommonComponent.cs](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs)): the installed component on a ship of a given size. |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Name` | string | | Display name, usually a localization key. |
| `Description` | string | | Text in the ship editor and workshop item panels. An empty value hides the text. |
| `DisplayCategory` | enum [ComponentCategory](enums.md#componentcategory) | `Undefined` | UI group. It selects the ship editor list group of non-weapons, the weapon/module filter of the cargo hold and the market, and the type label of a component technology. See [Display category](#display-category). |
| `Availability` | enum [Availability](enums.md#availability) | `None` | Which random pools may pick the component, and whether the ship editor can unlock it without research. See [Availability](#availability). 30 built-in records omit the field and are enemy-only. |
| `ComponentStatsId` | ref [ComponentStats](ComponentStats.md) | required | Passive stats. Model name `Stats`. The loader throws when it is missing. Several components can share one record. |
| `Faction` | ref [Faction](Faction.md) | neutral | Faction filter of random pools: faction shops, outposts, and quest loot pick components of a faction or neutral ones. `0` and `-1` mean neutral. The research tree comes from the component's [Technology](Technology.md), not from this field. |
| `Level` | int | 0… | Power level. It limits random pools (`Level` ≤ 1.5 × the star level), sets the random quality, and sets the price. See [Random selection](#random-selection) and [Price](#price). |
| `Icon` | image (`Component`) | | Icon in lists, in the workshop, and on the ship in the ship editor. |
| `Color` | color | white | Tint of `Icon`. |
| `Layout` | [layout](../../database.md#layout) | | Shape of the component. Every character other than `0` is an occupied cell. The cell count multiplies `PerOneCell` stats. |
| `CellType` | string | obsolete, still read | The hull cell type that every occupied cell needs: the first character, a [CellType](enums.md#celltype) value. Empty means any cell. The schema marks it obsolete, but placement depends on it ([DB9](../../issues/database.md#db9-live-fields-are-marked-obsolete)). See [Placement](#placement). |
| `DeviceId` | ref [Device](Device.md) | | The device. Model name `Device`. |
| `WeaponId` | ref [Weapon](Weapon.md) | | The firing behavior. Model name `Weapon`. It fires only together with `AmmunitionId`. A weapon doubles the price. |
| `AmmunitionId` | ref [Ammunition](Ammunition.md) or [AmmunitionObsolete](AmmunitionObsolete.md) | | The projectile. The loader looks up the same ID in both tables. Model names `Ammunition` and `AmmunitionObsolete`. See [Shared ID field](../../database.md#shared-id-field-componentammunitionid) and [DB10](../../issues/database.md#db10-two-schema-members-share-one-json-field). |
| `WeaponSlotType` | char | | Weapon slot letter. The component fits a weapon barrel whose `Barrel.WeaponClass` contains the letter, or a barrel without restriction. An empty value fits every weapon barrel. It also selects the weapon group in the ship editor list ([WeaponSlots](WeaponSlots.md)). Only components with `CellType` `4` use it. |
| `DroneBayId` | ref [DroneBay](DroneBay.md) | | The drone bay. Model name `DroneBay`. It works only together with `DroneId`. |
| `DroneId` | ref [ShipBuild](ShipBuild.md) | | The build that the bay launches. Model name `Drone`. See [ShipBuild: How a build becomes a ship](ShipBuild.md#how-a-build-becomes-a-ship). |
| `Restrictions` | struct [ComponentRestrictions](#componentrestrictions) | | Ship classes, hull type, and installation limits. |
| `PossibleModifications` | ref list [ComponentMod](ComponentMod.md) | | Modifications that random generation and crafting can roll. An installed or stored modification outside this list is replaced on load (see [Saves](#saves)). A missing ID becomes the empty modification. |

## Structures

### ComponentRestrictions

Schema: [ComponentRestrictions.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ComponentRestrictions.xml). A missing `Restrictions` object means no restriction.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `ShipSizes` | flags [SizeClass](enums.md#sizeclass) | | Hull classes that can install the component. JSON stores a list of values, for example `[0, 1, 2, 3]`. An empty list allows every class. For a satellite component, the carrying ship's class counts. |
| `NotForOrganicShips` | bool | | The component does not fit a bionic hull (`Ship.Features.Regeneration`). |
| `NotForMechanicShips` | bool | | The component fits only a bionic hull. |
| `MaxComponentAmount` | int | 0… | Maximum number of copies of this component on one ship, satellites included. `0` means no limit. No built-in record sets it. |
| `ComponentGroupTag` | ref [ComponentGroupTag](ComponentGroupTag.md) | | A shared limit for a group of components. |
| `UniqueComponentTag` | string | obsolete | Old group key. Migration 1.6 → 1.7 creates one `ComponentGroupTag` per key with the highest `MaxComponentAmount` of the group, and clears the field ([DatabaseUpgrader_6_7.cs:21](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_6_7.cs#L21)). |

Built-in data: 21 components have a group tag, 3 allow only `Frigate`…`Battleship`, and 5 are bionic-only (`NotForMechanicShips`).

## Notes

### Component kinds

The game derives the kind of a component from its references:

| Kind | Fields | Runtime part | Code |
| --- | --- | --- | --- |
| Weapon | `WeaponId` + `AmmunitionId` | A weapon on the barrel under the component. `Ammunition` gives the new bullet pipeline, `AmmunitionObsolete` the old one. | [CommonComponent.cs:64](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L64), [line 80](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L80) |
| Device | `DeviceId` | A ship system. | [line 97](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L97) |
| Drone bay | `DroneBayId` + `DroneId` | A drone launcher. | [line 114](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L114) |
| Passive | none of them | Stats only. | |

The code allows several parts on one component, but no built-in record combines them. The activation type of the component comes from the weapon, else the device, else the drone bay ([Component.cs:8](../../../Assets/Modules/Database/Scripts/Extensions/Component.cs#L8)). A weapon without ammunition, or a drone bay without a drone, installs and keeps its stats and energy cost, but has no active part.

### Placement

The ship editor ([ShipLayoutModel.cs:153](../../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs#L153)) and the old layout ([ShipLayoutObsolete.cs:184](../../../Assets/Modules/ShipConstructor/Scripts/ShipLayoutObsolete.cs#L184)) apply the same rules to every occupied cell of `Layout`:

1. The hull cell must be free and inside the grid.
2. When both the hull cell and `CellType` are weapon cells (`4`), the slot letter decides: no `WeaponSlotType`, no barrel restriction, or the letter in `Barrel.WeaponClass`.
3. Otherwise `CellType` must be compatible with the hull cell ([CellType.cs:8](../../../Assets/Modules/ShipConstructor/Scripts/CellType.cs#L8)): an empty `CellType` fits any cell, an equal type fits, and `InnerOuter` matches `Inner` and `Outer` in both directions. Empty cells and customizable cells (`*`) accept nothing.

Satellite components use the satellite layout and barrels. The ship builder links a weapon to a barrel through `InstalledComponent.BarrelId`.

### Compatibility and limits

`CompatibilityChecker` ([CompatibilityChecker.cs:13](../../../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs#L13)) rejects a component when one rule fails:

1. `ShipSizes` is not empty and does not contain the ship's `SizeClass`.
2. The bionic restriction does not match the hull.
3. The device is a `RepairBot` and the hull is bionic ([line 32](../../../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs#L32)).
4. The device class is already a built-in device of the hull ([line 36](../../../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs#L36)).

`MaxComponentAmount` and the group tag count the ship and both satellites together.

| Consumer | Rule | Code |
| --- | --- | --- |
| Ship editor | A component that fails a rule or a limit cannot be placed. | [ComponentTracker.cs:41](../../../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs#L41) |
| Fleet validation | Removes components that do not fit, and returns them to the inventory. It runs on a new session, after shipyard upgrades, and after modification changes. | [ShipValidator.cs:37](../../../Assets/Scripts/Domain/Player/ShipValidator.cs#L37), [PlayerFleet.cs:96](../../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L96) |
| Ship builder (every ship in combat) | Skips a component silently, in list order. It counts the limits before the compatibility check, so a skipped incompatible component still uses up a limit ([DB15.18](../../issues/database.md#db15-smaller-defects)). | [ShipBuilder.cs:93](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L93) |

### Stats and energy

`CommonComponent.GetStats` builds the stats of one installed component ([CommonComponent.cs:21](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L21)):

1. [ComponentStats](ComponentStats.md), multiplied by the cell count of `Layout` for `PerOneCell` stats.
2. Plus the device's `PassiveEnergyConsumption`, multiplied by `0.05 × n` when `ScaleEnergyWithShipSize` is set. `n` is the hull cell count, bought cells included, satellites excluded ([ShipBuilder.cs:111](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L111)).
3. Plus the drone bay's `PassiveEnergyConsumption × (0.5 + 0.005 × n)` ([line 29](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L29)).
4. Then the [modification](ComponentMod.md#effects).

The ship builder installs components that need no energy first. A component with a positive passive consumption installs only when the ship's current energy recharge covers its consumption. Otherwise only its consumption and weight count ([ShipBuilder.cs:137](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L137)). The ship totals are on [ShipSettings: Stat formulas](ShipSettings.md#stat-formulas).

### Display category

| Consumer | Rule | Code |
| --- | --- | --- |
| Ship editor component list | Weapons (`WeaponId` set) go to the weapon groups by `WeaponSlotType`. Other components go to Armor (`Defense`), Energy, Engine, Drones, or Special (all other values). | [ComponentTreeNode.cs:51](../../../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs#L51) |
| Cargo hold and market filter | `Weapon` is a weapon, every other value is a module. | [CargoHold.cs:184](../../../Assets/Scripts/Gui/StarMap/CargoHold.cs#L184) |
| Technology label | Weapon, Armor, Drone bay, Reactor, Engine, or Device. | [Technology.cs:156](../../../Assets/Scripts/Domain/Technologies/Technology.cs#L156) |
| Random outposts | `Drones` components are drone upgrades. | [RandomOutpostBuilder.cs:93](../../../Assets/Scripts/Domain/Exploration/RandomOutpostBuilder.cs#L93) |

### Availability

| Value | Random pools | Ship editor unlock | Other |
| --- | --- | --- | --- |
| `None` | Never | Needs a technology | A ship with such a component cannot enter the arena ([ShipValidator.cs:11](../../../Assets/Modules/ShipConstructor/Scripts/Ships/ShipValidator.cs#L11)). |
| `Common` | All pools | Always allowed ([ShipEditorModel.cs:199](../../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipEditorModel.cs#L199)) | The only value that faction shops sell. |
| `Rare` | Pools that allow rare items | Needs a technology | |
| `Special`, `Hidden`, `LootOnly` | Never | Needs a technology | Explicit sources only: [Loot](Loot.md) items, builds, technologies. |

The code meant `Special` components to stay locked in place, and `None` components to stay off player ships. The check exists, but nothing calls it ([DB15.17](../../issues/database.md#db15-smaller-defects)). The unlock rule with technologies is on [Technology: Unlocking components](Technology.md#unlocking-components).

### Random selection

`ComponentInfo.TryCreateRandomComponent` picks from `Common` (and `Rare`, when allowed) components of the given faction or neutral, with `Level` ≤ 1.5 × the requested level ([ComponentInfo.cs:48](../../../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs#L48)).

| Source | Pool | Maximum quality | Code |
| --- | --- | --- | --- |
| Faction shop | `Common`, region faction or neutral, star level | `P1` | [FactionInventory.cs:74](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/FactionInventory.cs#L74) |
| Black market | `Common` and `Rare`, any faction, star level + 75 | `P3` | [BlackMarketInventory.cs:63](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/BlackMarketInventory.cs#L63) |
| Outposts, hives, captured starbases, space worms | `Common` and `Rare`, owner faction or neutral | `P3` | [LootGenerator.cs:413](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L413) |
| Quest loot (`RandomComponents`) | `Common` and `Rare`, `Level` ≤ 1.5 × quest level, the loot faction filter | `P3` | [Loot.cs:136](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs#L136) |
| Arena shop | `Common` (and `Rare` with the trader skill), faction or neutral, `Level` ≤ star level | always `P3` | [ArenaInventory.cs:46](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/ArenaInventory.cs#L46) |
| Christmas rewards and shop | `Common` and `Rare`, `Level` ≤ level + 50 | always `P2` or `P3` | [LootGenerator.cs:316](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L316), [SantaInventory.cs:61](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/SantaInventory.cs#L61) |
| Exploration outposts and turrets | `Common` and `Rare`, filtered by stats | none | [RandomOutpostBuilder.cs:63](../../../Assets/Scripts/Domain/Exploration/RandomOutpostBuilder.cs#L63), [RandomTurretBuilder.cs:60](../../../Assets/Scripts/Domain/Exploration/RandomTurretBuilder.cs#L60) |

The random quality depends on the requested level `l` and the component level `L`, both raised to at least 10 ([ComponentQuality.cs:49](../../../Assets/Modules/ShipConstructor/Scripts/ComponentQuality.cs#L49)):

| Quality | Condition |
| --- | --- |
| `P3` | `l ≥ 2L + 150` |
| `P2` | `l ≥ 5L/3 + 100` |
| `P1` | `l ≥ 4L/3 + 40` |
| none | between the positive and negative bounds |
| `N1`, `N2`, `N3` | `l ≤ 0.9L`, `0.8L`, `0.7L` |

The game then moves the quality by up to one step at random, caps it at the maximum quality, and picks a random entry of `PossibleModifications` ([ComponentQuality.cs:19](../../../Assets/Modules/ShipConstructor/Scripts/ComponentQuality.cs#L19)).

### Price

`q` is the price factor of the [quality](enums.md#modificationquality): 50 % for `N3` up to 300 % for `P3`. A component without a modification uses 100 %.

| Value | Formula | Code |
| --- | --- | --- |
| Price in credits | `1 + (50 + 20 × Level) × q`, then × 2 for a weapon | [Price.cs:62](../../../Assets/Modules/Database/Scripts/Extensions/Price.cs#L62) |
| Premium price in stars | `1 + Level × q / 10`, × 1.5 for a weapon | [Price.cs:84](../../../Assets/Modules/Database/Scripts/Extensions/Price.cs#L84) |
| Crafting | the credit price without quality, and `Level / 75` stars | [Price.cs:30](../../../Assets/Modules/Database/Scripts/Extensions/Price.cs#L30) |

The workshop crafts `Common` quality without a modification, `Improved` with `P1`, `Excellent` with `P2`, and `Superior` with `P2` or `P3` ([Technology.cs:204](../../../Assets/Scripts/Domain/Technologies/Technology.cs#L204)). It offers the quality choice only when `PossibleModifications` is not empty ([CraftPanel.cs:108](../../../Assets/Scripts/Gui/Craft/CraftPanel.cs#L108)).

### Saves

The session stores a component as its ID, modification ID, quality, and upgrade level ([InventoryComponentInfo.cs:23](../../../Assets/Modules/GameSession/Scripts/Model/InventoryComponentInfo.cs#L23)).

- An unknown component ID is dropped from the inventory with a log message.
- A stored modification outside `PossibleModifications` is replaced on load. The inventory rolls a random modification of the same quality ([PlayerInventory.cs:50](../../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L50)). An installed component gets the first entry of the list ([ShipValidator.cs:101](../../../Assets/Scripts/Domain/Player/ShipValidator.cs#L101)).

### Data rules

1. Always set `Id`. [`Component/Afterburner_buggy.json`](../../../Assets/Modules/Database/Resources/Database/Component/Afterburner_buggy.json) has no `Id` and loads as component `0`, so every empty component reference resolves to it ([DB4](../../issues/database.md#db4-null-references-have-two-sentinels-and-id-0-is-not-protected)).
2. Set `Availability`. The default `None` makes the component enemy-only and keeps it out of every random pool.
3. Give a weapon both `WeaponId` and `AmmunitionId`, and a drone bay both `DroneBayId` and `DroneId`.
4. Set `CellType`. An empty value lets the component sit on any cell, weapon cells included. 4 built-in components have no `CellType`.
5. Give player components at least one entry in `PossibleModifications`. With an empty list, random items and crafted items have no modification.
6. Do not remove a modification from `PossibleModifications` lightly. Saved items with that modification change on the next load.
7. A changed or removed component ID breaks saves, technologies, builds, and loot. Components 85, 96, 213, 215, 219, and 220 are also [hard-coded](../../database.md#hard-coded-ids).
