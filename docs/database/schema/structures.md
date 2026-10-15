# Shared structures

These structures appear inside several tables. Structures that only one table uses are documented on that table's page.

Back to the [schema index](index.md).

## Barrel

A weapon mount of a [Ship](Ship.md) or [Satellite](Satellite.md). Schema: [Barrel.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Barrel.xml).

The n-th barrel belongs to the n-th group of connected weapon cells (`4`) in the layout. Groups are numbered in reading order of their first cell. See [Layout](../../database.md#layout).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Position` | vector | | Mount position relative to the hull center. The hull image spans −1…1 on both axes, and +x is the nose. Combat halves the value to get body units ([ShipFactory.cs:379](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L379)). |
| `Rotation` | float | −360…360 | Firing direction in degrees relative to the hull's forward direction. |
| `Offset` | float | 0…1 | Distance from `Position` to the point where bullets spawn, along `Rotation`. Combat uses `(Offset + Size) / 2`. |
| `AutoAimingArc` | float | 0…360 | Turret arc in degrees. `0` is a fixed mount. A component's `ComponentStats.AutoAimingArc` can widen it, never narrow it. |
| `RotationSpeed` | float | 0…1000 | Turret turn speed in degrees per second. `0` means the default of 270. `ComponentStats.TurretTurnSpeed` is added. The arc becomes 0 when the resulting speed is not positive. |
| `WeaponClass` | string | | Allowed weapon-slot letters (for example `"CL"`). Empty means any weapon fits. A component fits when its `WeaponSlotType` is empty or listed here. [WeaponSlots](WeaponSlots.md) only names the letters for the component list. |
| `Image` | image (Satellite) | | Turret sprite drawn on the platform. Empty means no visible turret. |
| `Size` | float | 0…100 | Turret sprite size, in the same units as `Position`. It also moves the bullet spawn point outward. |
| `PlatformType` | int | obsolete | Old platform kind. Migration 1.0→1.1 converts it to `AutoAimingArc`. |

Consumers: [`ShipBuilder.WeaponPlatform`](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs), [`ShipFactory.CreatePlatform`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs), [`ShipLayoutModel.IsCellCompatible`](../../../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs).

## Engine

An engine nozzle of a [Ship](Ship.md). Schema: [Engine.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Engine.xml).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Position` | vector | | Nozzle position relative to the hull center. The hull image spans −1…1, and +x is the nose, so nozzles usually have a negative x. |
| `Size` | float | 0…1 | Size of the engine light and trail. The light size is `5 × Size / Ship.ModelScale`. |

Combat draws one light and one trail per engine in `Ship.EngineColor`. Accelerator devices add a flash at each engine.

## InstalledComponent

A component placed in a [ShipBuild](ShipBuild.md) or [SatelliteBuild](SatelliteBuild.md). Schema: [InstalledComponent.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/InstalledComponent.xml).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `ComponentId` | ref [Component](Component.md) | required | The installed component. The loader throws when it is missing. Model name `Component`. |
| `Modification` | ref [ComponentMod](ComponentMod.md) | | Optional modification. `0` means none. |
| `Quality` | enum [ModificationQuality](enums.md#modificationquality) | | Quality of the modification. Ignored without a modification. |
| `X`, `Y` | int | −32768…32767 | Cell of the component's top-left layout corner in the hull layout. Can be negative for bought extra cells outside the original grid. |
| `BarrelId` | int | 0…255 | Index of the barrel that a weapon uses. The ship builder computes it from the position; data uses `-1` for non-weapons. For satellite components, the barrel index continues after the hull's barrels. |
| `Behaviour` | int | 0…10 | Drone behavior of a drone bay: `0` aggressive, `1` defensive. Selects `DroneBay.OffensiveDroneAI` or `DefensiveDroneAI`. |
| `KeyBinding` | int | −10…10 | Control key slot. `0` or more: the player key slot. Negative: automatic activation that keeps an energy reserve of `0.1 × (−KeyBinding − 1)` (−1 = no reserve, −2 = 10 %). Passive components ignore it. |

The ship builder skips a component when its `MaxComponentAmount` or group-tag limit is already reached, when it does not fit the ship, or when the ship's energy recharge cannot pay its consumption. Energy-independent components install first.

## StatModification

One stat change of a [ComponentMod](ComponentMod.md). Schema: [StatModification.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/StatModification.xml).

| Field | Type | Meaning |
| --- | --- | --- |
| `Type` | enum [StatModificationType](enums.md#statmodificationtype) | The stat to change. |
| `Gray3`, `Gray2`, `Gray1` | float | Values for the negative qualities `N3`, `N2`, `N1`. |
| `Green`, `Purple`, `Gold` | float | Values for the positive qualities `P1`, `P2`, `P3`. |

Each value is a multiplier (`1.25` = +25 %), except for `ExtraHitPoints`, where it is an absolute number of armor points. The modification text receives the values as format arguments `{0}`, `{1}`, … in row order, shown as signed percents (or a signed number for `ExtraHitPoints`). Consumer: [`ComponentModification`](../../../Assets/Modules/ShipConstructor/Scripts/Modification/ComponentModification.cs).

## FactionFilter

A faction filter. Generated class `RequiredFactions`. Used by [Fleet](Fleet.md), [Quest](Quest.md) origins, and [LootContent](#lootcontent). Schema: [FactionFilter.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/FactionFilter.xml).

| Field | Type | Meaning |
| --- | --- | --- |
| `Type` | enum `FactionFilterType` | See the table below. |
| `List` | ref list [Faction](Faction.md) | The listed factions. `0` in the list means the neutral faction. |

| `Type` | Value | A faction passes when |
| --- | ---: | --- |
| `AllButList` | 0 | It is not in `List`. With an empty list, every faction passes. This is the default. |
| `ListOnly` | 1 | It is in `List`. |
| `StarOwnersAndList` | 2 | It owns the quest star's region, or it is in `List`. |
| `AllAvailable` | 3 | It suits the level (`List` is ignored). For fleets: it has wandering ships at this level. For loot: it is not hidden from merchants and its `HomeStarDistance` ≤ level. For research: its research tree is visible and `HomeStarDistance` ≤ level. For bases: it has territories at this level. |

Implementation: [`Domain.Quests.FactionFilter`](../../../Assets/Modules/Quests/Scripts/Utils/FactionFilter.cs).

## LootContent

*Switch on `Type`.* A loot description. Used by [Loot](Loot.md), the `HaveItem` [requirement](#requirement), and [DebugSettings](DebugSettings.md) codes. Schema: [LootContent.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/LootContent.xml). Runtime: [`Domain.Quests.Loot`](../../../Assets/Modules/Quests/Scripts/Loot/Loot.cs).

All random choices use a generator seeded with the quest seed plus the loot ID, so a quest always produces the same loot. Item details per type: [Loot: Item creation](Loot.md#item-creation).

| `Type` | Value | Fields | Result |
| --- | ---: | --- | --- |
| `None` | 0 | – | Nothing. |
| `SomeMoney` | 1 | `ValueRatio` | Random credits for level `quest level × ValueRatio` (at least 1). |
| `Fuel` | 2 | `MinAmount`, `MaxAmount` | Fuel. |
| `Money` | 3 | `MinAmount`, `MaxAmount` | Credits. |
| `Stars` | 4 | `MinAmount`, `MaxAmount` | Stars (premium currency). |
| `StarMap` | 5 | – | Reveals the star map around the quest star. |
| `RandomComponents` | 10 | `MinAmount`, `MaxAmount`, `ValueRatio`, `Factions` | Random `Common`/`Rare` components with level ≤ 1.5 × (quest level × `ValueRatio`) from factions that pass the filter. |
| `RandomItems` | 20 | `MinAmount`, `MaxAmount`, `Items` | Picks min(*amount*, entries with a positive weight) different entries; a higher weight is more likely. With all weights 0, picks *amount* different entries uniformly. See [Loot: RandomItems](Loot.md#randomitems). |
| `AllItems` | 21 | `Items` | Every entry. |
| `ItemsWithChance` | 22 | `Items` | Each entry independently with probability `Weight` (0…1). |
| `QuestItem` | 25 | `ItemId` (QuestItem), amounts | A [QuestItem](QuestItem.md). |
| `Ship` | 30 | `ItemId` ([ShipBuild](ShipBuild.md)) | A ship with the build's components. |
| `EmptyShip` | 31 | `ItemId` ([Ship](Ship.md)) | An empty hull. |
| `Component` | 35 | `ItemId` ([Component](Component.md)), amounts | Components. |
| `Blueprint` | 40 | `ItemId` ([Technology](Technology.md)) | A researched technology. |
| `ResearchPoints` | 41 | `MinAmount`, `MaxAmount`, `Factions` | Research points of one random faction that passes the filter. |
| `Satellite` | 45 | `ItemId` ([Satellite](Satellite.md)), amounts | Satellites. |

| Field | Type | Range | Meaning |
| --- | --- | --- | --- |
| `Type` | enum `LootItemType` | | Selects the variant. |
| `ItemId` | ref (by type) | required | The item. Model names: `QuestItem`, `Ship`, `ShipBuild`, `Component`, `Blueprint`, `Satellite`. The loader throws when it is missing. |
| `MinAmount`, `MaxAmount` | int | 0…999,999,999 | Amount range. The amount is drawn uniformly. For `RandomItems`, the number of picks. |
| `ValueRatio` | float | 0.001…1000 | Multiplier on the quest level. |
| `Factions` | struct [FactionFilter](#factionfilter) | | Faction filter. |
| `Items` | struct list [LootItem](#lootitem) | | Nested entries. |

## LootItem

An entry of a `LootContent` list.

| Field | Type | Meaning |
| --- | --- | --- |
| `Weight` | float | Relative weight for `RandomItems`. Probability (0…1) for `ItemsWithChance`. Ignored by `AllItems`. |
| `Loot` | struct [LootContent](#lootcontent) | The nested loot. Nesting has no depth limit. |

## Requirement

*Switch on `Type`.* A condition for a [Quest](Quest.md) start, a dialog action, or a transition. Schema: [Requirement.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Quests/Requirement.xml). Runtime: [`RequirementsBuilder`](../../../Assets/Modules/Quests/Scripts/Factory/RequirementsBuilder.cs) and [`Requirements/`](../../../Assets/Modules/Quests/Scripts/Requirements/).

"Quest star" is the star where the quest was given. "Current star" is the player's star. In `Quest.Requirement` some types check another star; see [Quest: Start requirements](Quest.md#start-requirements). `Any`, `All`, and `None` with an empty list: only `None` is met.

| `Type` | Value | Fields | Met when |
| --- | ---: | --- | --- |
| `Empty` | 0 | – | Always. |
| `Any` | 1 | `Requirements` | At least one child is met. |
| `All` | 2 | `Requirements` | All children are met. |
| `None` | 3 | `Requirements` | No child is met. |
| `PlayerPosition` | 6 | `MinValue`, `MaxValue`, `AllowUnsafeStars` | The current star's level is in [`MinValue`, `MaxValue`] (max below min means no upper bound), and the star is secured unless unsafe stars are allowed. |
| `RandomStarSystem` | 7 | `MinValue`, `MaxValue`, `AllowUnsafeStars` | The player is at one random star chosen at a distance of `MinValue`…`MaxValue` (1–1000) from the quest star. The choice uses the quest seed. |
| `AggressiveOccupants` | 8 | – | The current star has occupants that attack. |
| `QuestCompleted` | 9 | `ItemId` (Quest) | The quest has been completed at least once. |
| `QuestActive` | 10 | `ItemId` (Quest) | The quest is active. |
| `CharacterRelations` | 15 | `Character`, `MinValue`, `MaxValue` | The character's relation (−100…100) is in range. When min > max, a max of 0 means no upper bound and a min of 0 no lower bound. `Character` is required. |
| `FactionRelations` | 16 | `MinValue`, `MaxValue` | The reputation with the quest star's region is in range. A captured region has 100. |
| `StarbaseCaptured` | 17 | – | The quest star's region is captured. |
| `FactionStarbasePower` | 18 | `MinValue`, `MaxValue` | The quest star's starbase power in percent is in range (0…100,000). |
| `IsHostileFaction` | 19 | – | The faction of the current star's region has `NoMissions`. |
| `Faction` | 20 | `Faction` | The current star's region belongs to this faction. |
| `HaveQuestItem` | 25 | `ItemId` (QuestItem), `MinValue` (alias `Amount`, ≥ 1) | The player owns at least this many. |
| `HaveItem` | 26 | `Loot` (inline LootContent) | The player can give away all items of this loot. |
| `HaveItemById` | 27 | `ItemId` (Loot) | The same for a [Loot](Loot.md) record. |
| `ComeToOrigin` | 30 | `AllowUnsafeStars` | The player is at the quest star. |
| `TimeSinceQuestStart` | 40 | `MinValue` (Minutes), `MaxValue` (Hours) | At least `Hours × 60 + Minutes` of game time passed since this quest started. |
| `TimeSinceLastCompletion` | 41 | `MinValue` (Minutes), `MaxValue` (Hours) | The same time passed since the last completion of this quest. |

Field ranges by case: `PlayerPosition` and `RandomStarSystem` 0…10,000; relations −100…100; starbase power 0…100,000; minutes and hours 0…999,999. `BoolValue` has the alias `AllowUnsafeStars`.

## SoundTrack

A music track. Used by [MusicPlaylist](MusicPlaylist.md) and [CombatRules](CombatRules.md).

| Field | Type | Meaning |
| --- | --- | --- |
| `Audio` | audio | Name of an audio file of the loaded mod, without the extension. Music looks up only mod audio, not built-in clips, and ignores the `*` loop marker. See [MusicPlaylist: Clip lookup](MusicPlaylist.md#clip-lookup). |

## ShipToValue

A pair of a [Ship](Ship.md) reference (`Ship`) and an integer (`Value`, ≥ 0). The schema defines it, but no table uses it.
