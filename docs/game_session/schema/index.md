# GameSession Save Schema

The save schema is a set of XML files in [`Assets/Modules/GameSession/.Schema/`](../../../Assets/Modules/GameSession/.Schema/). It defines every persisted field of a game session, its type, and its position in the binary save. The [code generator](../code_generator.md) turns it into the `Session.Model` classes. This page describes the XML format, the versioning rules, and the history of the three versions. For the module overview, see [GameSession module](../../game_session.md).

## Contents

- [Files](#files)
- [XML format](#xml-format)
- [Member types](#member-types)
- [Attributes](#attributes)
- [Objects and structs](#objects-and-structs)
- [Root members (v3)](#root-members-v3)
- [Conventions](#conventions)
- [Versioning rules](#versioning-rules)
- [Version history](#version-history)

## Files

| Path | Content |
| --- | --- |
| [`versions.xml`](../../../Assets/Modules/GameSession/.Schema/versions.xml) | Ordered list of versions: `v1` = 1.0, `v2` = 2.0, `v3` = 3.0. |
| `v<N>/SaveGameData.xml` | The root object of version N. |
| `v<N>/Objects/*.xml` | Objects of version N. |
| `v<N>/Types/*.xml` | Structs of version N. |

Each version folder is a **complete snapshot**: it defines every type, changed or not. The generator compares snapshots to find what changed. It reads all `*.xml` files under a version folder recursively, so `Objects/` and `Types/` are a convention only.

```xml
<schema>
    <version name="v1" major="1" minor="0" />
    <version name="v2" major="2" minor="0" />
    <version name="v3" major="3" minor="0" />
</schema>
```

`name` is the folder. `major` and `minor` are the version written into the save envelope. Only the major is usable; see [GS5.9](../../issues/game_session.md#gs5-smaller-defects).

## XML format

One file holds one type:

```xml
<data name="ResourcesData" type="object">
    <member name="Money" type="long" options="encrypted" />
    <member name="Resources" type="inventory" value="int" options="encrypted" />
</data>
```

| Element / attribute | Meaning |
| --- | --- |
| `data/@name` | Type name. Unique across objects and structs of one version. Becomes the C# type name. |
| `data/@type` | `object` or `struct`. See [Objects and structs](#objects-and-structs). |
| `member` | One field. **The order of `member` elements is the order of the bytes in the save.** |
| `member/@name` | Field name. Becomes the property name (first letter upper-cased) and the private field `_name`. |
| `member/@type` | Member type. See [Member types](#member-types). |
| `member/@key` | Key type of a `map`. Required for `map`. |
| `member/@value` | Element type of `list`, `set`, `inventory`; value type of `map`. |
| `member/@options` | Free text. The generator searches it for the words `encrypted` and `notimportant`. |
| `member/@encoding` | `elias` (default) or `plain`. Not used in v1–v3. |
| `member/@default` | Initial value for a new game. |

## Member types

| `type` | C# type | Notes |
| --- | --- | --- |
| `int`, `uint`, `short`, `ushort`, `sbyte`, `byte`, `long`, `ulong` | same | Variable-length coded; see [Value coding](../serialization.md#value-coding). |
| `single` | `float` | 4 bytes. |
| `bool` | `bool` | 1 bit. |
| `string` | `string` | UTF-8 with an `int` length. `null` reads back as `""`. |
| `list` | `ObservableList<value>` | Ordered. |
| `set` | `ObservableSet<value>` | Unordered, unique. |
| `map` | `ObservableMap<key, value>` | Unique keys. |
| `inventory` | `ObservableInventory<value>` | Item → quantity (`int`). `Remove` deletes an item when its quantity reaches 0. |
| `bitset` | `ObservableBitset` | Set of `uint` stored as runs. For dense ID sets such as explored stars. No `value`. |
| name of an object | generated class | A child object. |
| name of a struct | generated struct | Allowed in structs and as a collection element. Not as a direct member of an object ([GS5.13](../../issues/game_session.md#gs5-smaller-defects)). |

Collection element, key, and value types can be any scalar or struct type. Current schemas use no collection of objects.

## Attributes

| Attribute | Value | Effect |
| --- | --- | --- |
| `options` | `encrypted` | Scalar `int` / `long` only: stored in memory as `ObscuredInt` / `ObscuredLong` against memory scanners. The save holds the plain value. On collections it has no effect ([GS5.6](../../issues/game_session.md#gs5-smaller-defects)). On other scalar types the generator fails. |
| `options` | `notimportant` | The setter does not mark the session dirty. Used for UI state and counters that change every frame: `GameData.TotalPlayTime`, `StarMapData.MapModeZoom`, `StarModeZoom`. The value is still saved with the next real change. |
| `encoding` | `elias`, `plain` | Per-member wire encoding. Leave it empty. Changing it on a released version breaks old saves. |
| `default` | empty | `default(T)`, or `string.Empty` for strings. |
| `default` | `timestamp` | `DateTime.UtcNow.Ticks` cast to the member type. `GameData.Seed` uses it, so a new game gets a time-based seed. |
| `default` | other text | Inserted into C# literally. Example: `FleetData.ExplorationShipId` has `-1`. |

## Objects and structs

| | `object` | `struct` |
| --- | --- | --- |
| C# kind | `class` | `readonly partial struct` |
| Mutable | Yes: scalar setters, child setters, collections. | Only through its collections. Scalars are fixed at construction. |
| Change tracking | Has a parent. Every change calls `OnDataChanged` up to `SaveGameData` and `SessionData`. | Passes the owner's callback to its collections. A struct replaced in a collection notifies through the collection. |
| Identity | One instance per slot. | Copied by value; copies share collection instances. |
| Hand-written code | Rare; logic goes into the `Session.Content` facades. | Common: constructors and conversions in [`Scripts/Model/`](../../../Assets/Modules/GameSession/Scripts/Model/). |
| Typical use | A section of the save (`FleetData`) or a container. | A record in a collection (`ShipInfo`, `QuestProgress`). |

## Root members (v3)

[`SaveGameData.xml`](../../../Assets/Modules/GameSession/.Schema/v3/SaveGameData.xml) lists 20 objects. They are written in this order:

| # | Member | Type | Content | Facade |
| --- | --- | --- | --- | --- |
| 1 | `Game` | `GameData` | Seed, start time, play time, supply ship timer, random counter. | [`IGameData`](game.md#gamedata) |
| 2 | `Achievements` | `Achievements` | Unlocked achievement IDs. | [`IAchievementData`](game.md#achievements) |
| 3 | `Bosses` | `BossData` | Defeat count and last defeat time per boss. | [`IBossData`](world.md#bossdata-and-bossinfo) |
| 4 | `Common` | `CommonObjectData` | Generic `int` and `long` values per object ID. | [`ICommonObjectData`](game.md#commonobjectdata) |
| 5 | `Events` | `EventData` | Completion time per event star. | [`IEventData`](world.md#eventdata) |
| 6 | `Fleet` | `FleetData` | Player ships, hangar slots, exploration ship. | [`IFleetData`](fleet.md#fleetdata) |
| 7 | `Iap` | `IapData` | Purchased entitlements and stars. | [`IIapData`](economy.md#iapdata) |
| 8 | `Pvp` | `PvpData` | Arena fight counter and timers. | [`IPvpData`](social.md#pvpdata) |
| 9 | `Inventory` | `Inventory` | Stored components and satellites. | [`IInventoryData`](economy.md#inventory) |
| 10 | `Quests` | `QuestData` | Quest progress and statistics, faction and character relations. | [`IQuestData`](quests.md#questdata) |
| 11 | `Regions` | `RegionData` | Region factions, starbase defense power, captured bases. | [`IRegionData`](world.md#regiondata) |
| 12 | `Research` | `ResearchData` | Researched technologies, research points per faction. | [`IResearchData`](economy.md#researchdata) |
| 13 | `Resources` | `ResourcesData` | Money, stars, fuel, tokens, special resources. | [`IResourcesData`](economy.md#resourcesdata) |
| 14 | `Shop` | `ShopData` | Purchases per star and item. | [`IShopData`](economy.md#shopdata) |
| 15 | `Social` | `SocialData` | Daily reward dates. | [`ISocialData`](social.md#socialdata) |
| 16 | `StarMap` | `StarMapData` | Player position, explored and secured stars, planets, bookmarks. | [`IStarMapData`](world.md#starmapdata) |
| 17 | `Statistics` | `Statistics` | Unlocked ship hulls, defeated enemy count. | [`IStatisticsData`](game.md#statistics) |
| 18 | `Upgrades` | `UpgradesData` | Player experience, skills, skill reset counter. | [`IUpgradesData`](economy.md#upgradesdata) |
| 19 | `Wormholes` | `WormholeData` | Wormhole routes. | [`IWormholeData`](world.md#wormholedata) |
| 20 | `ShipPresets` | `ShipPresetsData` | Saved ship editor presets (new in v3). | [`IShipPresetsData`](fleet.md#shippresetsdata) |

The facades are in [`Scripts/Facade/Content/`](../../../Assets/Modules/GameSession/Scripts/Facade/Content/). The Facade column links to the per-field reference. The reference is grouped into six domain pages: [Game](game.md), [Fleet](fleet.md), [Economy](economy.md), [Quests](quests.md), [World](world.md), and [Social](social.md). The module note lists the [facade map](../../game_session.md#content-facades).

## Conventions

- **IDs.** Database references (ships, components, technologies, factions, quests, characters) are stored as `int` or `uint` record IDs, not as `ItemId<T>`. The facades and the `Scripts/Model/` partials convert them.
- **Stars and regions.** Star IDs and region IDs are `int` map keys, or `uint` bitset members. They are generated from the game seed and are not database IDs.
- **Time.** Two clocks are used. Absolute times are `long` UTC ticks (`GameData.StartTime`, `SupplyShipStartTime`). Since v2, several times are `int` whole hours since `GameData.StartTime` (`BossInfo.LastDefeatTime`, `PurchaseInfo.Time`, `EventData.CompletedTime`), converted by [`GameTimeExtensions`](../../../Assets/Modules/GameSession/Scripts/Extensions/GameTimeExtensions.cs). Check the unit of each time field before using it.
- **Packed values.** Some `long` values pack several fields. `StarMapData.PlanetData` keys are `(starId << 32) + planetId` ([StarMapData.cs:82](../../../Assets/Modules/GameSession/Scripts/Facade/Content/StarMapData.cs#L82)). `ShipInfo.Modifications` elements are serialized ship perks. The owning domain code defines each packing.

## Versioning rules

1. A released version folder is a compatibility snapshot. Do not change the meaning, order, or set of its members. The only safe in-place edit is widening an integer type with the same signedness ([Compatibility rules](../serialization.md#compatibility-rules)).
2. A layout change needs a new folder `v<N+1>` with a full copy of the previous snapshot, a new entry in `versions.xml`, and a new **major** number.
3. Every type that changed, and every type that contains it, gets a new generated class. Old classes move to `Session.v<N>` and lose `Serialize`.
4. Read the hook summary in the regenerated `SessionLoader.cs`. Implement every listed item in a `Scripts/Migration/SessionLoader_v<N>.cs` partial.
5. Keep the old snapshot until no supported save can contain it. Removing a version folder drops support for saves of that version.

See [Versioning and migration workflow](../../game_session.md#versioning-and-migration-workflow) for the full procedure.

## Version history

| Version | Root members | Changes from the previous version | Conversion |
| --- | --- | --- | --- |
| 1.0 | 19 | First header-based format. Headerless legacy saves are converted into it. | [Headerless legacy format](../serialization.md#headerless-legacy-format) |
| 2.0 | 19 | `EventData.CompletedTime`: `map<int,long>` (ticks) → `map<int,int>` (hours). `BossInfo.LastDefeatTime`: `long` → `int` (hours). `PurchaseInfo.Time`: `long` → `int` (hours). `Inventory.Components`: `inventory<long>` (packed) → `inventory<InventoryComponentInfo>` (new struct). `RegionData.DefeatedFleetCount` → `MilitaryPower` (`map<int,uint>`) + `CapturedBases` (`bitset`). `StarMapData.StarData` (`map<int,int>`) → `DiscoveredStars`, `SecuredStars`, `EnemiesOnStars` (`bitset`). | [v1.0 → v2.0](../serialization.md#v10--v20) |
| 3.0 | 20 | New root member `ShipPresets` (`ShipPresetsData`, struct `ShipPresetInfo`). | [v2.0 → v3.0](../serialization.md#v20--v30) |

In-place edits of released snapshots: `ShipComponentInfo.Modification` (`sbyte` → `int`) in v1–v3 and `InventoryComponentInfo.Modification` (`byte` → `uint`) in v2–v3. Both are compatible widenings.
