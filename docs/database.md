# Database Module

The Database module holds the static game catalog. The catalog contains ship hulls and builds, components and their stats, weapons and ammunition, devices, drone bays, satellites, factions, technologies, quests, loot, combat rules, AI behavior trees, visual effects, and global balance settings. Almost every gameplay system reads it. The module also loads player mods and merges them with the built-in catalog.

The module lives in [`Assets/Modules/Database/`](../Assets/Modules/Database/). It compiles as the `Database` assembly with the root namespace `GameDatabase`. Consumers use the [`IDatabase`](../Assets/Modules/Database/Scripts/IDatabase.cs) interface. [`ServicesInstaller`](../Assets/Scripts/Installers/ServicesInstaller.cs#L41) binds one `GameDatabase.Database` instance as a non-lazy singleton.

Related documents:

- [Schema reference](database/schema/index.md): every table, field, structure, and enum.
- [Database Editor](database/editor.md): the external Windows editor, the analyzer, mod packing, and encryption.
- [Database Code Generator](database/code_generator.md): the XML schema format and the generated code.
- Known problems: [Database Module Issues](issues/database.md).

## Contents

- [Terms](#terms)
- [Directory map](#directory-map)
- [The catalog at a glance](#the-catalog-at-a-glance)
- [Records, IDs, and references](#records-ids-and-references)
- [JSON format](#json-format)
- [Value types](#value-types)
- [Expressions](#expressions)
- [Generated code](#generated-code)
- [Hand-written runtime code](#hand-written-runtime-code)
- [Loading pipeline](#loading-pipeline)
- [Mods and merge rules](#mods-and-merge-rules)
- [Storage backends](#storage-backends)
- [Packed mod format and encryption](#packed-mod-format-and-encryption)
- [Versioning and migration](#versioning-and-migration)
- [Consumers](#consumers)
- [Hard-coded IDs](#hard-coded-ids)
- [Validation and tests](#validation-and-tests)
- [Commands and tooling](#commands-and-tooling)
- [Authoring rules](#authoring-rules)
- [Common changes](#common-changes)

## Terms

| Term | Meaning |
| --- | --- |
| Record | One JSON document. Every record has an `ItemType` field. |
| Item type | The value of `ItemType`. It selects the table that the record belongs to. See [ItemType](database/schema/enums.md#itemtype). |
| Collection record | A record with an integer `Id`. A table can have many collection records. Example: `Component`. Item types below 100. |
| Settings record | A singleton record without an `Id`. A database has at most one record of each settings type. Example: `ShipSettings`. Item types 100 and above. |
| Structure (struct) | A nested JSON object inside a record. It has no `Id` and no `ItemType`. Example: `Barrel`. |
| Reference | An integer field that holds the `Id` of another record. The schema type is `object`. |
| Switch object | A record or structure whose fields depend on the value of one enum field. Example: `LootContent.Type`. |
| Legacy object | A record type whose generated class stores all fields in one mutable `Stats` struct. Used by `Device`, `Weapon`, `DroneBay`, and `AmmunitionObsolete`. |
| Expression | A string field that holds a formula. The game compiles it to a function. See [Expressions](#expressions). |
| Storage | A source of records and media: Unity `Resources`, a mod folder, or a packed mod file. See [Storage backends](#storage-backends). |
| Content | The parsed but unresolved records of one or more storages (`DatabaseContent`). |
| Data model | The resolved runtime objects that consumers read (`GameDatabase.DataModel`). |
| Mod | An external database that the player selects. It loads before the built-in catalog and overrides it. |

## Directory map

| Path | Purpose | Edit policy |
| --- | --- | --- |
| [`.Schema/versions.xml`](../Assets/Modules/Database/.Schema/versions.xml) | Current schema version (`1.7`). | Edit when the data version changes. |
| [`.Schema/v1/Enums/`](../Assets/Modules/Database/.Schema/v1/Enums/) | 47 enum definitions. | Edit for new enum values. Never renumber values. |
| [`.Schema/v1/Expressions/`](../Assets/Modules/Database/.Schema/v1/Expressions/) | 4 expression signatures (`IntToInt`, `IntToFloat`, `FloatToFloat`, `SizeClassToInt`). | Rarely changed. |
| [`.Schema/v1/Objects/`](../Assets/Modules/Database/.Schema/v1/Objects/) | 68 record, settings, and structure definitions. | Edit for data-shape changes. |
| `.CodeGenerator/` | Git submodule. .NET Framework 4.8 generator with T4 templates. | Change in its own repository. Unity ignores folders that start with `.`. |
| `.Editor/` | Git submodule. Windows Database Editor and Analyzer (.NET 10). | Change in its own repository. |
| [`Resources/Database/`](../Assets/Modules/Database/Resources/Database/) | Built-in catalog: 1,857 JSON records. Unity loads it through `Resources`. | Edit for content and balance. Put only database JSON here. |
| [`Resources/DatabaseMigration/v1.3/`](../Assets/Modules/Database/Resources/DatabaseMigration/v1.3/) | The 21 component modifications that migration 1.3→1.4 installs. | Edit only with that migration. |
| [`Scripts/GeneratedGameCode/`](../Assets/Modules/Database/Scripts/GeneratedGameCode/) | Generated enums, DTOs, data model, loader, `DatabaseContent`, and the generated part of `IDatabase`. | Never edit by hand. Regenerate. |
| [`Scripts/GeneratedMigrationCode/`](../Assets/Modules/Database/Scripts/GeneratedMigrationCode/) | Generated version dispatcher and v1 DTOs for migration. | Never edit by hand. Regenerate. |
| [`Scripts/Migration/v1/`](../Assets/Modules/Database/Scripts/Migration/v1/) | Hand-written minor-version migration steps. | Edit when old data needs transformation. |
| [`Scripts/DataModel/`](../Assets/Modules/Database/Scripts/DataModel/) | Hand-written partial classes for generated models. | Edit for derived runtime behavior. |
| [`Scripts/Storage/`](../Assets/Modules/Database/Scripts/Storage/) | Storage backends and JSON serializers. | Edit for loading behavior. |
| [`Scripts/Types/`](../Assets/Modules/Database/Scripts/Types/) | Value types: `ItemId`, `Layout`, `ColorData`, `ImageData`, `ImmutableCollection`, and others. | Hand-written. |
| [`Scripts/Expression/`](../Assets/Modules/Database/Scripts/Expression/) | Bridge to the `CodeWriter.ExpressionParser` library. | Hand-written. |
| [`Scripts/Extensions/`](../Assets/Modules/Database/Scripts/Extensions/) | Helpers over data model types, including hard-coded price formulas. | Hand-written. |
| [`Scripts/Query/ShipBuildQuery.cs`](../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs) | Lazy filter chain over ship builds. | Hand-written. |
| [`Scripts/Legacy/`](../Assets/Modules/Database/Scripts/Legacy/) | Name-to-ID tables for saves from before numeric IDs. | Read-only legacy. |
| [`Scripts/Utils/`](../Assets/Modules/Database/Scripts/Utils/) | `BarrelConverter` and old binary helpers. | Hand-written. |
| [`Tests/`](../Assets/Modules/Database/Tests/) | Editor-only NUnit tests that need no Unity API, and packed-mod fixtures. | Add module tests here. |
| [`Unity/`](../Assets/Modules/Database/Unity/) | `Database.Unity` assembly: `ResourceDatabaseStorage`, `UnityJsonSerializer`, `UnityDatabaseFactory`, sprite and audio converters, `OggAudioClip`, the WAV parser, `ColorMode.Apply`. `Unity/Tests/` holds the tests that load `Resources`. | Engine-side code. |
| `Plugins/NVorbis/` | Git submodule. Ogg Vorbis decoder for mod audio. | Third-party. |

The [assembly definition](../Assets/Modules/Database/Database.asmdef) sets `noEngineReferences`, so the database compiles without UnityEngine. It references `Security.Crypto` (commercial) or `SecurityStub.Crypto` (open source), `CommonComponents`, `CodeWriter.ExpressionParser`, `Diagnostics`, and `Utilites`. Engine-side code is in `Database.Unity` ([`Unity/`](../Assets/Modules/Database/Unity/)). The game creates the database with [`UnityDatabaseFactory.Create()`](../Assets/Modules/Database/Unity/UnityDatabaseFactory.cs), which passes the `Resources` storage, the JSON serializer, and the mods folder to the `Database` constructor. Some value types that the database uses live in `CommonComponents`: [`SpriteId`](../Assets/Modules/CommonComponents/Scripts/DataTypes/SpriteId.cs), [`AudioClipId`](../Assets/Modules/CommonComponents/Scripts/DataTypes/AudioClipId.cs), and [`StatMultiplier`](../Assets/Modules/CommonComponents/Scripts/DataTypes/StatMultiplier.cs).

## The catalog at a glance

The built-in catalog has these record counts (2026-10-11):

| Domain | Tables (record count) | Folder under `Resources/Database/` |
| --- | --- | --- |
| Ships | [Ship](database/schema/Ship.md) (151), [ShipBuild](database/schema/ShipBuild.md) (388), [Satellite](database/schema/Satellite.md) (25), [SatelliteBuild](database/schema/SatelliteBuild.md) (39) | `Ship/`, `Ship/Builds/`, `Satellite/`, `Satellite/Builds/` |
| Components | [Component](database/schema/Component.md) (264), [ComponentStats](database/schema/ComponentStats.md) (79), [ComponentMod](database/schema/ComponentMod.md) (21), [ComponentGroupTag](database/schema/ComponentGroupTag.md) (14) | `Component/`, `Component/Stats/`, `Component/Modifications/`, `Component/Tags/` |
| Ship systems | [Device](database/schema/Device.md) (36), [DroneBay](database/schema/DroneBay.md) (6), [Weapon](database/schema/Weapon.md) (110) | `Device/`, `Dronebay/`, `Weapon/`, `Weapon/Obsolete/` |
| Ammunition and effects | [Ammunition](database/schema/Ammunition.md) (56), [AmmunitionObsolete](database/schema/AmmunitionObsolete.md) (81), [BulletPrefab](database/schema/BulletPrefab.md) (21), [VisualEffect](database/schema/VisualEffect.md) (24), [GameObjectPrefab](database/schema/GameObjectPrefab.md) (3) | `Ammunition/`, `Ammunition/Bullets/`, `Ammunition/Effects/`, `Ammunition/Obsolete/`, `Prefabs/` |
| Progression | [Technology](database/schema/Technology.md) (361), [Faction](database/schema/Faction.md) (20) | `Technology/`, `Faction/` |
| Quests | [Quest](database/schema/Quest.md) (20), [Loot](database/schema/Loot.md) (19), [Fleet](database/schema/Fleet.md) (7), [Character](database/schema/Character.md) (7), [QuestItem](database/schema/QuestItem.md) (10), [CombatRules](database/schema/CombatRules.md) (8) | `Quests/` and subfolders |
| AI | [BehaviorTree](database/schema/BehaviorTree.md) (74) | `Ai/` |
| Settings | 13 singleton records (plus the starting-inventory `Loot`) | `Settings/` |
| Unused | [ComponentStatUpgrade](database/schema/ComponentStatUpgrade.md) (0), [StatUpgradeTemplate](database/schema/StatUpgradeTemplate.md) (0) | – |

Folder names only help authors. The loader reads every JSON file under the root and uses `ItemType`, not the folder, to choose the table.

The main reference graph (arrows point from the record that holds the reference to the referenced record):

```text
ShipBuild ──> Ship ──> Faction
    │          └─> Device (built-in devices), VisualEffect (explosion)
    ├─> SatelliteBuild ──> Satellite
    ├─> BehaviorTree (custom AI)
    └─> InstalledComponent[] ──> Component, ComponentMod
                                    │
Component ──> ComponentStats        ├─> Device ──> VisualEffect, GameObjectPrefab, Ammunition
          ──> ComponentGroupTag     ├─> Weapon ──> VisualEffect
          ──> Faction               ├─> Ammunition ──> BulletPrefab, VisualEffect, Ammunition (spawned)
          ──> ComponentMod[]        ├─> AmmunitionObsolete (same Id field as Ammunition)
                                    └─> DroneBay ──> BehaviorTree;  Drone ──> ShipBuild

Technology ──> Component | Ship | Satellite, Faction, Technology[] (dependencies)

Quest ──> Node[] ──> Fleet ──> ShipBuild[], CombatRules
              ├──> Loot ──> QuestItem | Ship | ShipBuild | Component | Technology | Satellite
              ├──> Character ──> Faction, Loot, Fleet
              └──> Quest, Faction;   Requirement ──> Quest, QuestItem, Loot, Character, Faction

Settings ──> ShipBuild, Ship, Faction, Loot, Quest, CombatRules, BehaviorTree, VisualEffect
```

## Records, IDs, and references

### Record identity

- A collection record has an integer `Id`. The `Id` is unique per item type, not across the whole database. A `Component` and a `Ship` can both have `Id` 5.
- File names and folders do not matter at run time. Only the `ItemType` and `Id` identify a record.
- A settings record has no `Id`. A database has at most one record of each settings type. See [Merge rules](#merge-rules) for duplicates.
- Saved games store record IDs. Never reuse or renumber an ID of released content.
- The built-in data uses no `Id` 0. Treat `0` as reserved (see below).

### Null references

A reference field is an integer. The JSON default for a missing integer is `0`, so `0` means "no reference" in practice. The built-in data also uses `-1` for "no faction" in 38 component records. The runtime does not define a sentinel:

1. The loader looks up the ID in the target table. Any ID that is not found becomes the target type's `DefaultValue`.
2. For most types, `DefaultValue` is `null`. For `Faction`, `DefaultValue` is [`Faction.Empty`](../Assets/Modules/Database/Scripts/DataModel/Faction.cs) (ID 0, gray, name `$NeutralFaction`). For `ComponentMod`, it is an empty modification.
3. Consequence: if a record with `Id` 0 existed, every empty reference of that type would point to it. Do not create records with `Id` 0.

[`ItemId<T>.IsNull`](../Assets/Modules/Database/Scripts/Types/ItemId.cs#L18) tests `Id < 0`, and `ItemId<T>.Empty` is `-1`. This does not match the JSON convention. See [issue DB4](issues/database.md#db4-null-references-have-two-sentinels-and-id-0-is-not-protected).

### Reference resolution

The generated [`Database.Loader`](../Assets/Modules/Database/Scripts/GeneratedGameCode/Database.cs#L257) resolves every reference when it builds the data model:

| Schema form | Missing target | Disabled target |
| --- | --- | --- |
| `object` | `DefaultValue` (usually `null`). No error. | Same as missing. |
| `object` with `options="notnull"` | Throws `DatabaseException`. The whole load fails. | Throws. |
| `object_list` | Throws `DatabaseException` for the missing element, unless the type has a non-null `DefaultValue` (`Faction`, `ComponentMod`). | Same as missing. |

A record registers itself in the ID map before it resolves its own references. Reference cycles (for example, technology A depends on B and B depends on A) therefore load without infinite recursion. Code that later walks such a cycle can still recurse forever. See [issue DB12](issues/database.md#db12-the-loader-does-not-detect-reference-cycles).

### Object identity at run time

Data model classes do not override `Equals`. Code compares them by reference (`item.Faction == faction`). This is correct only while all objects come from the same load. A database reload creates new objects. Persist IDs, not objects.

### Shared ID field: `Component.AmmunitionId`

`Component` declares the JSON field `AmmunitionId` twice: once as a reference to [Ammunition](database/schema/Ammunition.md) and once as a reference to [AmmunitionObsolete](database/schema/AmmunitionObsolete.md). The loader resolves the same number in both tables. The built-in data keeps the two ID ranges disjoint, so only one lookup succeeds. If both succeed, the component gets two weapons. See [issue DB10](issues/database.md#db10-two-schema-members-share-one-json-field).

### Disabled records

Every record has a `Disabled` flag. The loader keeps disabled records in `DatabaseContent` but does not create data model objects for them. Because content loading uses "first record wins", a mod can hide a built-in record: it adds a record with the same `ItemType` and `Id` and sets `Disabled: true`. References to a disabled record resolve like references to a missing record.

## JSON format

The built-in catalog uses Unity's [`JsonUtility`](../Assets/Modules/Database/Unity/Storage/UnityJsonSerializer.cs) for reading. Rules that follow from it:

| Data | JSON form | Notes |
| --- | --- | --- |
| Item type | `"ItemType": 6` | Integer value of the [ItemType](database/schema/enums.md#itemtype) enum. |
| Enum | Integer | Enum names are not accepted. |
| Enum set (`enum_flags`) | Array of integers | `"ShipSizes": [0, 1, 2]`. A list of values, not a bit mask. |
| Boolean, int, float | JSON literals | Out-of-range numbers are clamped to the schema range when the data model is built. |
| Missing field | Omitted | Becomes the schema `default`, or the C# default (`0`, `false`, `null`) when the schema has none. |
| Reference | Integer `Id` | `0` means none. See [Null references](#null-references). |
| Reference list | Array of integers | `[3, 7]` |
| Color | `"#RRGGBB"` or `"#AARRGGBB"` | Hex. Six digits get alpha `FF`. Invalid text becomes white and logs an error. See [`ColorData`](../Assets/Modules/CommonComponents/Scripts/DataTypes/ColorData.cs). |
| Vector | `{"x": 0.5, "y": -0.25}` | Lowercase keys. |
| Char | One-character string | `"WeaponSlotType": "C"` |
| Layout | String of cell characters | Read row by row. See [Layout](#layout). |
| Image | Sprite name string | Resolved by [`ResourceLocator`](../Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs). See [Media](#media-images-and-audio). |
| Audio clip | Clip name string | A `*` anywhere in the name marks the clip as looping and is removed from the name. |
| Prefab | Prefab name string | Resolved under `Resources/Combat/Bullets/`, `Objects/`, or `Effects/` by the prefab kind. |
| Expression | String formula | See [Expressions](#expressions). |

`BehaviorTree` records are an exception: [`UnityJsonSerializer`](../Assets/Modules/Database/Unity/Storage/UnityJsonSerializer.cs#L12) reads them with Newtonsoft.Json, because `JsonUtility` cannot read recursive classes. The external editor writes all records with Newtonsoft.Json and omits default values.

Unity's `Resources` system turns every file under `Resources/Database/` into an asset. [`ResourceDatabaseStorage`](../Assets/Modules/Database/Unity/Storage/ResourceDatabaseStorage.cs) parses every `TextAsset` there as a record. A non-database text file there breaks loading. Keep each `.meta` file next to its JSON file.

## Value types

### Layout

[`Layout`](../Assets/Modules/Database/Scripts/Types/Layout.cs) is a square grid stored as a string. The grid size is `ceil(sqrt(length))`. Missing characters are padded with `0`. Cell `(x, y)` is character `y * size + x`. `CellCount` counts every character that is not `0` and not `*`.

| Char | [CellType](database/schema/enums.md#celltype) | Meaning in a ship or satellite layout | Meaning in a component layout |
| --- | --- | --- | --- |
| `0` | `Empty` | No cell. | Not occupied. |
| `1` | `Outer` | Outer hull cell (armor, engines, outer modules). | Occupies a cell. Placement uses `Component.CellType`, not these characters. |
| `2` | `Inner` | Inner cell (reactors, internal modules). | Same. |
| `3` | `InnerOuter` | Accepts inner and outer components. | Same. |
| `4` | `Weapon` | Weapon cell. Adjacent weapon cells form one barrel. | Same. |
| `5` | `Engine` | Engine cell. | Same. |
| `*` | (customizable) | Empty cell that the player can buy as an extra cell. See [Ship: Cell expansions](database/schema/Ship.md#cell-expansions). | – |
| `X` | (custom weapon) | Runtime only: a bought extra cell next to a weapon cell. | – |

[`BarrelConverter`](../Assets/Modules/Database/Scripts/Utils/BarrelConverter.cs) groups connected weapon cells. It numbers the groups in reading order. The n-th group gets the n-th entry of `Barrels`. When the counts differ, it logs `barrels do not fit layout <id>` and drops the extra entries.

### Media: images and audio

- An image field stores a sprite name. [`SpriteId`](../Assets/Modules/CommonComponents/Scripts/DataTypes/SpriteId.cs) adds a category from the schema `typeid` (for example `Ship`, `Component`, `Effect`).
- [`ResourceLocator.GetSprite`](../Assets/Modules/ResourceLocator/Scripts/ResourceLocator.cs) looks up built-in sprites first, by category. Only when no built-in sprite has that name does it ask `IDatabase.GetImage(name)` for a mod image.
- A mod therefore cannot replace a built-in sprite by name. Mod image keys include the file extension (`ship.png`), so mod sprite names normally do not collide with built-in names.
- Audio works the same way: built-in clips from the `ResourceLocator` prefab win, and mod clips are a fallback. Mod audio keys have no extension. Music is the exception: playlists look up only mod clips. See [MusicPlaylist](database/schema/MusicPlaylist.md#clip-lookup).
- Ship and satellite `ModelImage` names also select a combat prefab `Resources/Combat/Ships/<name>` or `Combat/Satellites/<name>`. A name without a prefab uses the `Default` prefab with the sprite. See [BattleSimulator: Ships](battle_simulator.md#ships).

### Other types

| Type | File | Purpose |
| --- | --- | --- |
| `ItemId<T>` | [ItemId.cs](../Assets/Modules/Database/Scripts/Types/ItemId.cs) | Typed record ID. Hash and equality use the integer. |
| `ImmutableCollection<T>` | [ImmutableCollection.cs](../Assets/Modules/Database/Scripts/Types/ImmutableCollection.cs) | Read-only list for list fields. The default value is an empty collection. |
| `ColorData` | [ColorData.cs](../Assets/Modules/CommonComponents/Scripts/DataTypes/ColorData.cs) | Parsed color, engine-free. Engine code converts it with `.ToUnity()`. |
| `PrefabId` | [PrefabId.cs](../Assets/Modules/Database/Scripts/Types/PrefabId.cs) | Prefab name plus kind (`Bullet`, `Object`, `Effect`). |
| `NodeId` | [NodeId.cs](../Assets/Modules/Database/Scripts/Types/NodeId.cs) | Quest node ID. Clamped to 0–1000. See [issue DB8](issues/database.md#db8-quest-node-ids-above-1000-collide). |
| `IImageData`, `IAudioClipData` | [ImageData.cs](../Assets/Modules/Database/Scripts/Types/ImageData.cs), [AudioClipData.cs](../Assets/Modules/Database/Scripts/Types/AudioClipData.cs) | Encoded mod media: png/jpg bytes, or wav/ogg bytes plus the format. `ToSprite()` and `ToAudioClip()` in [DatabaseAssetConverters.cs](../Assets/Modules/Database/Unity/DatabaseAssetConverters.cs) decode them once and cache the result. Ogg clips over 100 KB stream from memory. |

## Expressions

An expression field stores a formula as a string. The schema gives it a signature: one parameter and a result type.

| Signature | Parameter | Result |
| --- | --- | --- |
| `IntToInt` | `int` | `int` |
| `IntToFloat` | `int` | `float` |
| `FloatToFloat` | `float` | `float` |
| `SizeClassToInt` | [SizeClass](database/schema/enums.md#sizeclass) | `int` |

Each field names its parameter with `arguments`, for example `level` or `distance`. A formula can use:

1. The parameter by its name: `MIN(1000, 300 + 5*distance)`.
2. Other non-expression fields of the same record by their field name: `BaseFuelCapacity + 50*level`.
3. Other expression fields of the same record as functions: `FuelTankCapacity(level)`.
4. Enum value names of the parameter type: `IF(size == Destroyer, 5, 0)`.
5. The functions of the [CodeWriter.ExpressionParser](../Assets/ExpressionParser/) library, for example `MIN`, `MAX`, `IF`, `RANDOM`. See [Expression Parser: Language reference](expression_parser.md#language-reference) for the operators, the functions, and the number rules.

Runtime behavior:

1. The generated code wraps each field in a function object (for example [`IntToInt`](../Assets/Modules/Database/Scripts/GeneratedGameCode/Expressions/IntToInt.cs)). Consumers call it like a delegate: `GalaxySettings.EnemyLevel(distance)`.
2. The formula compiles on the first call, not at load time ([IntToInt.cs:26](../Assets/Modules/Database/Scripts/GeneratedGameCode/Expressions/IntToInt.cs#L26)).
3. An empty string or a formula that fails to compile becomes a function that returns 0. The failure is only logged ([ExpressionBuilder.cs:40](../Assets/Modules/Database/Scripts/Expression/ExpressionBuilder.cs#L40)). See [issue DB6](issues/database.md#db6-expression-errors-appear-late-and-become-zero).
4. The result is clamped to the schema `minvalue`/`maxvalue`.
5. A missing field gets the schema `default` formula.

## Generated code

[`generate_code.cmd`](../Assets/Modules/Database/generate_code.cmd) runs the code generator on `.Schema/versions.xml`. It writes [`Scripts/GeneratedGameCode/`](../Assets/Modules/Database/Scripts/GeneratedGameCode/). See [Database Code Generator](database/code_generator.md) for the schema language.

| Layer | Namespace | Example | Role |
| --- | --- | --- | --- |
| Enums | `GameDatabase.Enums` | `Enums/SizeClass.cs` | One C# enum per schema enum. |
| Serializable | `GameDatabase.Serializable` | `Serializable/Component.cs` (`ComponentSerializable`) | JSON DTOs with public fields. They keep obsolete fields so that migrations can read them. `FileName` stores the source path for diagnostics. |
| Content | `GameDatabase.Storage` | [`DatabaseContent`](../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs) | Implements `IContentLoader`. Parses JSON into DTOs, keeps the first record per ID, stores media. |
| Data model | `GameDatabase.DataModel` | `DataModel/Component.cs` | Read-only runtime objects with resolved references, clamped numbers, and compiled expressions. |
| Database | `GameDatabase` | [`Database.cs`](../Assets/Modules/Database/Scripts/GeneratedGameCode/Database.cs) | The generated part of `IDatabase`: one property per settings type, `<Type>List` and `Get<Type>(ItemId)` per collection type, `GetImage`, `GetAudioClip`, `GetLocalization`, and the `Loader`. |

How schema options change the generated data model:

| Schema feature | Generated result |
| --- | --- |
| `alias` on `<data>` | Different class name. `Quest` → `QuestModel`, `Loot` → `LootModel`, `BehaviorTree` → `BehaviorTreeModel`. |
| `alias` on `<member>` | Different property name. `ComponentStatsId` → `Stats`. |
| `switch` | Abstract base class plus one subclass per enum value (`LootContent_Money`), and a visitor interface `I<Name>Factory<T>`. Consumers implement the visitor. Adding an enum value breaks every visitor until it handles the new case. |
| `options="legacy"` | The class has one public `Stats` struct with all fields, obsolete ones included. Consumers copy and modify the struct. |
| `options="obsolete"` | The field stays in the DTO but not in the data model. The external editor does not write it back. |
| `options="notnull"` | The loader throws when the reference is missing. |
| `minvalue`, `maxvalue` | The data model clamps the value. The editor limits input. |
| `default` | Initial value of the DTO field. |
| `expression` | A private function object plus a public delegate property. |

`Get<Type>(id)` never throws. A missing ID returns the type's `DefaultValue`.

## Hand-written runtime code

| File | Adds |
| --- | --- |
| [`Database.cs`](../Assets/Modules/Database/Scripts/Database.cs) | Mod discovery, `LoadDefault`, `TryLoad`, the content/parent merge, `DatabaseLoading`/`DatabaseLoaded` events, and the temporary `SaveShipBuild`/`SaveSatelliteBuild` used by the in-game build editor. |
| [`IDatabase.cs`](../Assets/Modules/Database/Scripts/IDatabase.cs) | The hand-written part of the interface and `ModInfo`. |
| [`DataModel/Faction.cs`](../Assets/Modules/Database/Scripts/DataModel/Faction.cs) | `Faction.Empty` and the `HomeStarRange`/`WanderingShipsRange` ranges. A `0` bound means "no bound". |
| [`DataModel/Component.cs`](../Assets/Modules/Database/Scripts/DataModel/Component.cs) | `Component.Empty` and `CellType`, read from the obsolete schema field `CellType`. |
| [`DataModel/Ship.cs`](../Assets/Modules/Database/Scripts/DataModel/Ship.cs), [`Satellite.cs`](../Assets/Modules/Database/Scripts/DataModel/Satellite.cs) | Rebuild `Barrels` through `BarrelConverter` so that each barrel knows its layout position. |
| [`DataModel/Barrel.cs`](../Assets/Modules/Database/Scripts/DataModel/Barrel.cs) | Mutable clones for ship modifications that change barrels. |
| [`DataModel/ShipBuild.cs`](../Assets/Modules/Database/Scripts/DataModel/ShipBuild.cs) | `Faction` = `BuildFaction`, or the hull faction when `BuildFaction` is empty. `SetComponents` for the build editor. |
| [`DataModel/CombatRules.cs`](../Assets/Modules/Database/Scripts/DataModel/CombatRules.cs) | `CombatRulesAdapter`: evaluates the level formulas once and turns `Default` ship selection into `NoRetreats` unless the player has a rescue unit or skill bonuses are disabled. |
| [`DataModel/BulletController.cs`](../Assets/Modules/Database/Scripts/DataModel/BulletController.cs), [`BulletTrigger.cs`](../Assets/Modules/Database/Scripts/DataModel/BulletTrigger.cs) | Derived controller flags and bullet-spawn offsets from expressions. |
| [`DataModel/InstalledComponent.cs`](../Assets/Modules/Database/Scripts/DataModel/InstalledComponent.cs), [`LootModel.cs`](../Assets/Modules/Database/Scripts/DataModel/LootModel.cs) | Constructors for runtime-created instances. |
| [`Extensions/Price.cs`](../Assets/Modules/Database/Scripts/Extensions/Price.cs) | Hard-coded price and crafting formulas for components, ships, and satellites. |
| [`Extensions/ComponentList.cs`](../Assets/Modules/Database/Scripts/Extensions/ComponentList.cs), [`Factions.cs`](../Assets/Modules/Database/Scripts/Extensions/Factions.cs) | Availability and faction filters. |
| [`Extensions/Colors.cs`](../Assets/Modules/Database/Unity/Extensions/Colors.cs) | `ColorMode.Apply`. |
| [`Query/ShipBuildQuery.cs`](../Assets/Modules/Database/Scripts/Query/ShipBuildQuery.cs) | Ship build selection for enemies, players, starbases, and drones. Maps star level to difficulty class (`level / 25`). |
| [`Legacy/`](../Assets/Modules/Database/Scripts/Legacy/) | Old string names of ships, builds, satellites, and technologies. Used only by obsolete save loaders in `GameSession`. |

## Loading pipeline

[`InitializationState`](../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L69) loads the database at startup. [`GameDataManager.LoadMod`](../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L76) loads it again when the player changes the mod.

1. `LookForMods()` scans the mod folder. It creates a `FileDatabaseStorage` for each file and a `FolderDatabaseStorage` for each subfolder to read the name and ID. Invalid mods are logged and skipped.
2. `TryAddModFromFile(path)` registers packed mods that the player chose outside the mod folder (`GameSettings.ExternalMods`).
3. If `GameSettings.ActiveMod` is set, `TryLoad(id)` loads that mod. Otherwise, or on failure, `LoadDefault()` loads the built-in catalog.
4. [`Database.Load`](../Assets/Modules/Database/Scripts/Database.cs#L143) clears all tables and raises `DatabaseLoading`.
5. `LoadContent(storage)` reads the selected storage into a new `DatabaseContent`. If the storage version is not the current version, the [upgrader](#versioning-and-migration) reads it and exports the upgraded records instead.
6. Unless the mod's `DatabaseSettings.UnloadOriginalDatabase` is true, `LoadExtraContent` reads the built-in catalog into the same content. Records that are already present are skipped.
7. [`Database.Loader.Load`](../Assets/Modules/Database/Scripts/GeneratedGameCode/Database.cs#L271) creates data model objects for every non-disabled record, copies media, and creates every settings object. A missing settings record becomes an object with schema defaults.
8. `DatabaseLoaded` fires. Subscribers: [`Technologies`](../Assets/Scripts/Domain/Technologies/Technologies.cs), [`PrefabCache`](../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs), [`UiThemeLoader`](../Assets/ModulesShared/Gui/Scripts/Theme/UiThemeLoader.cs), [`DebugConsoleLogger`](../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsoleLogger.cs), and [`SignalsTranslator`](../Assets/Scripts/Services/Messenger/SignalsTranslator.cs). `InitializationState` initializes localization directly after the load.
9. When the database is editable (Unity Editor or folder mod), `InitializationState` checks every ship build for viability, every satellite build for layout fit, and every technology for missing dependencies. It only logs errors.
10. The session loads with the database ID as its key. Each mod has its own save. See [GameSession storage](game_session/storage.md#mod-saves).

`LoadDefault()` catches and logs every exception. When it fails, the database stays empty and every settings property is `null`.

## Mods and merge rules

### Locations

| Platform | Mod folder |
| --- | --- |
| Android (player) | `Application.persistentDataPath/Mods/` |
| Standalone and Unity Editor | `<project or game folder>/Mods/` (`Application.dataPath + "/../Mods/"`) |
| Other platforms (iOS, WebGL) | No folder scan. A packed mod can still be added by path when the platform UI offers it. |

A file in the mod folder is a packed mod. A subfolder is a folder mod.

### Merge rules

| Data | Rule |
| --- | --- |
| Collection record | The mod record wins. The built-in record with the same `ItemType` and `Id` is skipped. Built-in records with other IDs stay. |
| Settings record | The mod record replaces the whole built-in record. There is no field-level merge. A mod that changes one setting must copy all other fields of that settings record, or they fall back to schema defaults. |
| Disabled mod record | Hides the built-in record with the same ID. |
| `UnloadOriginalDatabase = true` | The built-in catalog is not loaded. The mod must be complete. Code that uses [hard-coded IDs](#hard-coded-ids) can then fail. |
| Images | Built-in sprites win by name. Mod images are a fallback (see [Media](#media-images-and-audio)). |
| Audio | Built-in clips win by name. Mod clips are a fallback. |
| Localization | A mod XML file named after a language (for example `English.xml`) adds strings. See [Localization](localization.md). |
| Duplicate ID or second settings record inside one storage | Depends on how the storage loads. A mod at the current version loads in the [`DatabaseContent`](../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L19) constructor, where `_allowDuplicates` is still false: the duplicate throws `DatabaseException` and the mod does not load. The built-in catalog (`LoadParent`) and an upgraded mod load after the constructor: the first record wins and later ones are skipped silently. Order depends on file enumeration. See [DB15.29](issues/database.md#db15-smaller-defects). |

## Storage backends

| | [`ResourceDatabaseStorage`](../Assets/Modules/Database/Unity/Storage/ResourceDatabaseStorage.cs) | [`FolderDatabaseStorage`](../Assets/Modules/Database/Scripts/Storage/FolderDatabaseStorage.cs) | [`FileDatabaseStorage`](../Assets/Modules/Database/Scripts/Storage/FileDatabaseStorage.cs) |
| --- | --- | --- | --- |
| Source | `Resources/Database/` | Mod subfolder | Packed mod file |
| Name and ID | Folder name, ID `""` | Folder name, or the two lines of an `id` file (name, then ID) | From the packed header |
| Version | Editor: from `DatabaseSettings` in the data. Player: the compiled current version. | From the first `DatabaseSettings` record. Without one: 1.0. | From the header. Headerless legacy files: 1.0. |
| JSON | All `TextAsset`s under the path | `*.json`, recursive | Entries of type 1 |
| Images | Editor only: `Texture2D` assets (none exist today) | `*.png`, `*.jpg`, `*.jpeg`; key = file name with extension | Entries of type 2 |
| Audio | None | `*.wav`, `*.ogg`; key = file name without extension | Entries of type 4 (WAV) and 5 (Ogg) |
| Localization | None | `*.xml`; key = file name without extension | Entries of type 3 |
| Bad JSON | Editor: logged and skipped. Player: the load fails. | The load fails. | Logged and skipped. |
| Media decoding | – | Lazy, on first use | Lazy, from stored bytes |
| Editable | Editor only | Yes | No (`UpdateItem` throws) |

[`IDataStorage.UpdateItem`](../Assets/Modules/Database/Scripts/Storage/IDatabaseStorage.cs) exists only for `Database.SaveShipBuild` and `SaveSatelliteBuild`. The in-game build editor ([`DatabaseEditorContext`](../Assets/ModulesShared/ShipEditor/Scripts/Context/DatabaseEditorContext.cs)) uses them to write a changed build back to its JSON file. The output uses `JsonUtility` formatting, which differs from the external editor's formatting.

## Packed mod format and encryption

New packed mods use authenticated hybrid encryption. The editor serializes the payload, compresses it with zlib, encrypts it with a fresh AES-256-GCM key and a 12-byte nonce, and wraps that key with the game's RSA-3072 public key (OAEP, SHA-256 for OAEP and MGF1). The commercial `Security` module owns the obfuscated private key and does not cache its decoded form.

Format version 1 is fixed-width and little-endian:

| Field | Size |
| --- | ---: |
| Magic `0xDADABA5E` | 4 bytes |
| Format version `1` | 4 bytes |
| RSA-wrapped AES key | 384 bytes |
| GCM nonce | 12 bytes |
| GCM authentication tag | 16 bytes |
| Encrypted zlib payload | Remaining bytes |

The magic, format version, wrapped key, and nonce are GCM associated data. An unsupported version, a truncated envelope, a key-unwrapping failure, or an authentication failure rejects the mod before decompression. Files that start with the legacy magic `0xDA7ABA5E` use the old XOR loader. Files without a known magic use the older headerless loader. Open-source builds do not contain the private key and cannot load the authenticated format.

The decompressed payload starts with a header (format ID, name, ID, major and minor version) and continues with typed entries until the end of the stream: `1` JSON, `2` image, `3` localization, `4` WAV, `5` Ogg. See [Database Editor: Payload Format](database/editor.md#payload-format) for the byte layout of each entry.

## Versioning and migration

### Version sources

| Source | Value today |
| --- | --- |
| [`.Schema/versions.xml`](../Assets/Modules/Database/.Schema/versions.xml) | `v1`, major 1, minor 7 |
| Generated constants `Database.VersionMajor`/`VersionMinor` | 1, 7 |
| [`Resources/Database/Settings/Database.json`](../Assets/Modules/Database/Resources/Database/Settings/Database.json) | `DatabaseVersion` 1, `DatabaseVersionMinor` 7 |
| Packed mod header | Version of the editor that packed it |

A storage whose version equals the compiled version loads directly. Any other version goes through [`DatabaseUpgrader`](../Assets/Modules/Database/Scripts/GeneratedMigrationCode/DatabaseUpgrader.cs#L21):

1. Major version 0 is treated as 1.0.
2. Versions outside 1.0–1.7 throw `invalid database version`. A mod from a newer game version does not load.
3. The upgrader reads the storage into the generated v1 migration DTOs.
4. It runs each minor step in order from the storage version to 1.7.
5. It serializes every DTO back to JSON and feeds it to the normal `DatabaseContent`.

### Migration steps

| Step | File | What it does |
| --- | --- | --- |
| 1.0 → 1.1 | [DatabaseUpgrader_0_1.cs](../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_0_1.cs) | Converts barrel and component `PlatformType`/`AlterWeaponPlatform` to `AutoAimingArc` (1→360°, 2→80°, 3→20°). Moves `Ship.EnginePosition`/`EngineSize` into `Engines`. Maps `Ship.ShipCategory` to `ShipType`, `ShipRarity`, and `SizeClass`. Moves ship resistances, weight, regeneration, and built-in devices into `Features`. Maps `Faction.Hidden` and `Hostile` to the new flags. Clamps negative `BuildFaction` to 0. |
| 1.1 → 1.2 | [DatabaseUpgrader_1_2.cs](../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_1_2.cs) | Creates a starting-inventory `Loot` (100 credits, 20 stars, five components, two satellites) with the next free loot ID, and sets `GalaxySettings.StartingInvenory`. |
| 1.2 → 1.3 | [DatabaseUpgrader_2_3.cs](../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_2_3.cs) | Moves `StartingInvenory` to the correctly spelled `StartingInventory`. Derives `ShipBuild.AvailableForEnemy` and `AvailableForPlayer` from `NotAvailableInGame`, the hull rarity, and the difficulty class. |
| 1.3 → 1.4 | [DatabaseUpgrader_3_4.cs](../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_3_4.cs) | Deletes all component modifications and loads the built-in set from `Resources/DatabaseMigration/v1.3/`. It reads them through [`MigrationResources.GetStorage`](../Assets/Modules/Database/Scripts/Migration/MigrationResources.cs). `UnityDatabaseFactory.Create()` sets `MigrationResources.StorageProvider`; without it, this step throws `InvalidOperationException`. Code that builds a `Database` outside Unity must set the provider before it loads a 1.3 mod. |
| 1.4 → 1.5 | [DatabaseUpgrader_4_5.cs](../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_4_5.cs) | Adds `CombatRules` 1 with defaults and sets `CombatSettings.DefaultCombatRules` to 1. |
| 1.5 → 1.6 | [DatabaseUpgrader_5_6.cs](../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_5_6.cs) | Converts the obsolete `BulletBody.Type` to `AiBulletBehavior`, `ParentVelocityEffect`, `AttachedToParent`, and a controller (`Homing`, `Beam`). Throws when new and obsolete fields are mixed. |
| 1.6 → 1.7 | [DatabaseUpgrader_6_7.cs](../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_6_7.cs) | Replaces `ComponentRestrictions.UniqueComponentTag` strings with `ComponentGroupTag` records. Adds implicit group tags for unique device classes (one teleporter, one stealth, one shield, and so on), numbered from 1. |

The migration steps assume a complete database. Partial mods can lose data or override built-in records. See [issue DB2](issues/database.md#db2-migrations-assume-a-complete-database).

### Schema change rules

1. Never change an existing enum value or `ItemType` number.
2. To rename a field in code only, add an `alias`. The JSON name stays.
3. To rename or remove a field in data, add the new field, mark the old one `obsolete`, copy values in a new migration step, and raise the minor version.
4. Do not delete an obsolete field while a migration step still reads it.
5. Run both generators after any schema change: `generate_code.cmd` and `generate_migration_code.cmd`. The migration code must match the schema; see [issue DB3](issues/database.md#db3-the-migration-code-is-older-than-the-schema).
6. Update `Settings/Database.json` when the minor version changes.
7. Rebuild the external editor (`rebuild_editor.cmd`) so that modders get the new fields.

## Consumers

The database feeds almost every subsystem. This table lists the main readers. Field-level consumers are on each [schema page](database/schema/index.md).

| Subsystem | Code | Tables and settings it reads |
| --- | --- | --- |
| Ship stats | [`ShipBuilder`](../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs), [`ShipStatsCalculator`](../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs), [`ShipEquipmentStats`](../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs), [`ShipModel`](../Assets/Modules/ShipConstructor/Scripts/Ships/ShipModel.cs), [`EngineStats`](../Assets/Modules/ShipConstructor/Scripts/Model/EngineStats.cs) | Ship, ShipBuild, Component, ComponentStats, ComponentMod, ComponentGroupTag, Device, DroneBay, Weapon, Satellite, ShipSettings, ShipModSettings |
| Ship editor | [`ShipLayoutModel`](../Assets/ModulesShared/ShipEditor/Scripts/Model/ShipLayoutModel.cs), [`ComponentTracker`](../Assets/ModulesShared/ShipEditor/Scripts/Model/ComponentTracker.cs), [`CompatibilityChecker`](../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs) | Layouts, `Component.CellType`, `WeaponSlotType`, `Barrel.WeaponClass`, restrictions, WeaponSlots |
| Combat | [`ShipFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs), [`WeaponFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/WeaponFactory.cs), [`DeviceFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs), [`BulletFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs), [`CompositeEffect`](../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs), [`PrefabCache`](../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs) | Ammunition, AmmunitionObsolete, BulletPrefab, VisualEffect, GameObjectPrefab, Device, Weapon, DroneBay, ShipSettings |
| Combat AI | [`ControllerFactory`](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs), [`NodeBuilder`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/NodeBuilder.cs), [`RequirementChecker`](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/BehaviorTree/RequirementChecker.cs) | BehaviorTree, CombatSettings, DroneBay AI fields, ShipBuild.CustomAI |
| Combat rules | [`CombatModelBuilder`](../Assets/Scripts/Combat/Domain/CombatModelBuilder.cs), [`CombatManager`](../Assets/Scripts/Combat/Manager/CombatManager.cs), [`CombatTimer`](../Assets/Scripts/Combat/Manager/CombatTimer.cs) | CombatRules, CombatSettings.DefaultCombatRules, GalaxySettings rules, GalaxySettings.EnemyLevel |
| Quests | [Quests module](quests.md): `NodeBuilder`, `RequirementsBuilder`, `Loot`, `FleetFactory`, `QuestGiver` | Quest, Loot, Fleet, Character, QuestItem, CombatRules, Faction |
| Galaxy | [`Region`](../Assets/Scripts/Legacy/Game/Galaxy/Region.cs), [`Fleet` factories](../Assets/Scripts/Legacy/Model/Factories/Fleet.cs), `Domain/Galaxy/StarContent/` | Faction, FactionsSettings, GalaxySettings, ShipBuild queries |
| Exploration | `Domain/Exploration/`, [`ExplorationSceneManager`](../Assets/Scripts/Combat/Manager/ExplorationSceneManager.cs) | ExplorationSettings, ShipBuild, Component |
| Economy and loot | [`LootGenerator`](../Assets/Scripts/Domain/Economy/LootGenerator.cs), `Domain/Economy/ItemType/`, merchant inventories | Component, Faction, Technology, QuestItem, Satellite, price formulas |
| Research | [`Technologies`](../Assets/Scripts/Domain/Technologies/Technologies.cs), [`Research`](../Assets/Scripts/Domain/Research/Research.cs) | Technology, Faction |
| Player progression | [`PlayerSkills`](../Assets/Scripts/Domain/Player/PlayerSkills.cs), [`Skills`](../Assets/Scripts/Domain/Skills/Skills.cs), [`StartingNewGameState`](../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs) | SkillSettings, GalaxySettings starting data |
| Events | [`HolidayManager`](../Assets/Scripts/GameServices/HolidayManager.cs) | SpecialEventSettings |
| UI | [`UiTheme`](../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs), [`MainMenu`](../Assets/Scripts/Gui/MainMenu/MainMenu.cs), [`ComponentTreeNode`](../Assets/ModulesShared/Gui/Scripts/ComponentList/ComponentTreeNode.cs) | UiSettings, WeaponSlots, LocalizationSettings |
| Audio | [`DatabaseMusicPlaylist`](../Assets/Scripts/GameServices/Audio/DatabaseMusicPlaylist.cs) | MusicPlaylist, CombatRules.CustomSoundtrack |
| Debug | [`DatabaseCodesProcessor`](../Assets/Scripts/Legacy/Debug/DatabaseCodesProcessor.cs), [`DebugConsoleLogger`](../Assets/ModulesShared/Gui/Scripts/DebugConsole/DebugConsoleLogger.cs) | DebugSettings |
| Localization | [Localization module](localization.md) | `GetLocalization(language)` for mod XML |
| Saves | [GameSession](game_session.md) | Record IDs; legacy name tables |

## Hard-coded IDs

Some code refers to records by number. A mod that removes or changes these records breaks the feature. The application-level problem is [issue 9](issues/application_layer.md#9-game-rules-are-in-code-instead-of-the-database).

| ID | Table | Used by | Purpose |
| --- | --- | --- | --- |
| 18 | Device | [`InfectedModification`](../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/InfectedModification.cs#L16) | Toxic waste device added by the `Infected` ship perk. |
| 5, 6, 7, 11, 12, 13, 14, 16 | QuestItem | [`CommodityType`](../Assets/Scripts/Legacy/Economy/Data/CommodityType.cs#L3), [`LootGenerator`](../Assets/Scripts/Domain/Economy/LootGenerator.cs#L425) | Commodities in exploration and wreck loot. |
| 25 | QuestItem | [`PlayerResources`](../Assets/Scripts/Domain/Player/PlayerResources.cs#L94), [`UiTheme.Import`](../Assets/ModulesShared/Gui/Scripts/Theme/UiTheme.cs#L346) | Snowflake currency and color. |
| 52, 65, 76 | Component | [`RandomOutpostBuilder`](../Assets/Scripts/Domain/Exploration/RandomOutpostBuilder.cs#L106), [`RandomTurretBuilder`](../Assets/Scripts/Domain/Exploration/RandomTurretBuilder.cs#L86) | Drone factory, fuel cells, reactor of exploration outposts and turrets. |
| 85 | Component | [`RandomTurretBuilder`](../Assets/Scripts/Domain/Exploration/RandomTurretBuilder.cs#L63) | Fallback turret weapon. |
| 96, 213, 215, 219, 220 | Component | [`LootGenerator`](../Assets/Scripts/Domain/Economy/LootGenerator.cs#L321) | Christmas rewards. |
| 232 | ShipBuild | [`Fleet.Xmas`](../Assets/Scripts/Legacy/Model/Factories/Fleet.cs#L125) | Christmas starbase. |
| 226, 218 | ShipBuild | [`Controller.Manager`](../Assets/Scripts/Legacy/Controller/Manager.cs#L47) | Controls test scene. |
| 264, 277–282 | Technology | [`SantaInventory`](../Assets/Scripts/Legacy/Game/Quests/Inventory/SantaInventory.cs#L40) | Christmas shop. |
| several | ShipBuild | [`Cheats`](../Assets/Scripts/Legacy/Debug/Cheats.cs#L64), `Challenge.DefaultShipBuild` | Debug fleets and fallback. |

## Validation and tests

| Check | Where | When |
| --- | --- | --- |
| JSON parse, unknown `ItemType` | `DatabaseContent.LoadJson` | Every load. Policy differs by storage (see [Storage backends](#storage-backends)). |
| `notnull` and list references | Generated `Loader` | Every load. Throws. |
| Barrel count vs. weapon cells | `BarrelConverter` | Every load. Logs. |
| Ship build viability, satellite fit, tech dependencies | `InitializationState` | Startup, editable databases only. Logs. |
| Default catalog sanity | [`DatabaseTests`](../Assets/Modules/Database/Unity/Tests/DatabaseTests.cs) | Unity Test Runner (Edit Mode). Spawn distance order, player ships exist, enemy ships exist at distances 0–1000, each faction has ships. |
| Packed mod envelope | [`PackedModEncryptionTests`](../Assets/Modules/Database/Tests/PackedModEncryptionTests.cs) | Unity Test Runner, or `dotnet test Tests~/EngineFree.Tests.csproj` without Unity ([Engine-free modules](architecture.md#engine-free-modules)). Commercial license only (`LICENSE_COMMERCIAL`). |
| Broken references, statistics | External Analyzer (`analyzer.cmd`) | Manual. |

A schema-driven scan of the built-in catalog on 2026-10-11 found:

- No unresolved references, except 38 component `Faction` fields that use `-1` for "neutral" (they resolve to `Faction.Empty`, as intended).
- No disabled records, no records with `Id` 0, no duplicate IDs.
- Barrel counts match the weapon-cell groups of every ship and satellite.
- No satellite has more than one barrel.
- Every quest transition targets an existing node. The highest node ID is 100.
- No technology dependency cycles.
- Every weapon-slot letter on components and barrels is declared in `WeaponSlots`.

## Commands and tooling

Run the batch files from `Assets/Modules/Database`, because they use relative paths:

```bat
cd Assets\Modules\Database

rem Regenerate Scripts\GeneratedGameCode
generate_code.cmd

rem Regenerate Scripts\GeneratedMigrationCode
generate_migration_code.cmd

rem Regenerate the editor model and build the editor and analyzer
rebuild_editor.cmd

rem Open the editor or the analyzer on Resources\Database
editor.cmd
analyzer.cmd
```

- The generator (`.CodeGenerator`) targets .NET Framework 4.8. Build it first if `DatabaseCodeGenerator.exe` is missing.
- The editor and analyzer (`.Editor`) target .NET 10 (`net10.0-windows7.0`). `editor.cmd` and `analyzer.cmd` start the executables from that output folder.
- `rebuild_editor.cmd` calls `MSBuild.exe` by an absolute path. Adjust the path to your Visual Studio installation.
- `.CodeGenerator/test.cmd` runs an editor-code generation pass into `.CodeGenerator/Output`. It is a generator smoke check, not a test suite.

## Authoring rules

1. Give every new collection record a new positive `Id`. Never reuse a released ID.
2. Use `0` for "no reference". Do not create records with `Id` 0.
3. Keep `Ammunition` and `AmmunitionObsolete` IDs disjoint.
4. Keep the number of `Barrels` equal to the number of connected weapon-cell groups in the layout. Give a satellite at most one barrel.
5. Declare every weapon-slot letter in [WeaponSlots](database/schema/WeaponSlots.md) before you use it in `Component.WeaponSlotType` or `Barrel.WeaponClass`.
6. Set `Component.CellType` for every component. It controls placement, although the schema marks it obsolete. See [issue DB9](issues/database.md#db9-live-fields-are-marked-obsolete).
7. Keep quest node IDs between 1 and 1000.
8. Avoid dependency cycles in technologies and `SubTree` cycles in behavior trees.
9. Write expressions carefully. Test them in play, because the loader does not compile them.
10. In a mod, include a `DatabaseSettings` record with the current version. Without it, the loader treats the mod as version 1.0 and runs every migration step.
11. In a mod, copy a whole settings record before you change one field.

## Common changes

| Change | Steps |
| --- | --- |
| Balance or add content of an existing type | Edit JSON under `Resources/Database/<domain>/` with the external editor or by hand. Run the game in the editor and check the Console. Run `DatabaseTests`. |
| Add a field to a record | Add the `<member>` to the XML. Run both generators. Use the new property in consumers. Rebuild the editor. A new field with a default needs no migration. |
| Add an enum value | Add the item with an explicit new `value`. Regenerate. Implement the new case in every visitor (`I<Name>Factory<T>`) and `switch` that uses the enum. |
| Add a quest node, requirement, or loot type | Add the enum value and the `case` members. Regenerate. Implement the visitor method in the [Quests module](quests.md). |
| Add a behavior tree node | Add the enum value and its parameters in `BehaviorTreeNode.xml`. Regenerate. Implement the node in `BattleSimulator` (see [Add a behaviour tree node](battle_simulator.md#add-a-behaviour-tree-node)). |
| Add a device class | Add the `DeviceClass` value. Regenerate. Add a case to `DeviceFactory` (see [Add a device](battle_simulator.md#add-a-device)). |
| Add a settings record | Add an XML file with `type="settings"` and a new `ItemType` value of 100 or more. Regenerate. Add the JSON under `Settings/`. |
| Add a derived property | Add a partial class under `Scripts/DataModel/` and use `OnDataDeserialized`. |
| Rename or remove a field in data | See [Schema change rules](#schema-change-rules). |
| Change mod discovery or merging | Edit [`Database.cs`](../Assets/Modules/Database/Scripts/Database.cs) and `Scripts/Storage/`. Test folder and packed mods. |
| Change the generator or editor | Work in the submodule repository. Update the submodule pointer in this repository on purpose. |
