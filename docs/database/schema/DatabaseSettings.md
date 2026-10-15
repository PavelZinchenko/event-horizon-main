# DatabaseSettings

Database settings store the data version of a catalog and decide whether a mod loads on top of the built-in catalog or replaces it. The storages read the version to select the migration path, and the database reads `UnloadOriginalDatabase` when it loads a mod. The mod name and ID fields have no effect.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `102` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/Database.json`](../../../Assets/Modules/Database/Resources/Database/Settings/Database.json) |
| Schema | [DatabaseSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/DatabaseSettings.xml) |
| Generated class | `GameDatabase.DataModel.DatabaseSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/DatabaseSettings.cs)), read as `IDatabase.DatabaseSettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog; empty means the schema default.

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `DatabaseVersion` | int | 1… | 1 | Major data version. The storage reads it before the load and compares it with the compiled version ([Versioning and migration](../../database.md#versioning-and-migration)). |
| `DatabaseVersionMinor` | int | 0… | 7 | Minor data version. A lower value runs the migration steps from that version up to the current one. |
| `ModName` | string | | | No effect. It only feeds `IDatabase.Name` ([Database.cs:26](../../../Assets/Modules/Database/Scripts/Database.cs#L26)), and no code reads that property. |
| `ModId` | string | | | No effect. No code reads it. |
| `ModVersion` | int | | | No effect. No code reads it ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)). |
| `UnloadOriginalDatabase` | bool | | | `true`: the game does not load the built-in catalog under the mod ([Database.cs:149](../../../Assets/Modules/Database/Scripts/Database.cs#L149)). The mod must then contain every record that the game needs. |

## Notes

### Version

| Storage | Where the version comes from | Code |
| --- | --- | --- |
| Built-in catalog | Unity Editor: the first `DatabaseSettings` record. Player build: the compiled version. | [ResourceDatabaseStorage.cs:91](../../../Assets/Modules/Database/Scripts/Storage/ResourceDatabaseStorage.cs#L91) |
| Folder mod | The first JSON file with item type `102`. Without one: version 1.0. | [FolderDatabaseStorage.cs:35](../../../Assets/Modules/Database/Scripts/Storage/FolderDatabaseStorage.cs#L35) |
| Packed mod | The file header. The database editor writes this record's version into the header. | [MainWindow.cs:283](../../../Assets/Modules/Database/.Editor/GameDatabase/MainWindow.cs#L283) |

A storage with the current version loads directly. Any other version goes through the upgrader, which runs every migration step from the stored version on. The steps assume a complete database, so a partial mod without this record can lose data ([DB2](../../issues/database.md#db2-migrations-assume-a-complete-database)).

### Mod name and ID

The mod list and the save key do not use `ModName` or `ModId`. A folder mod takes its name and ID from a two-line `id` file in the folder, or from the folder name ([FolderDatabaseStorage.cs:16](../../../Assets/Modules/Database/Scripts/Storage/FolderDatabaseStorage.cs#L16)). A packed mod takes them from its header. The database editor writes that header from the same `id` file ([MainWindow.cs:267](../../../Assets/Modules/Database/.Editor/GameDatabase/MainWindow.cs#L267)). The settings menu shows the storage name ([SettingsDatabase.cs:82](../../../Assets/Scripts/Gui/MainMenu/SettingsDatabase.cs#L82)). The game keeps one save per storage ID ([GameDataManager.cs:90](../../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L90)).

### Replacing the built-in catalog

With `UnloadOriginalDatabase` `true`, code that uses [hard-coded IDs](../../database.md#hard-coded-ids) can fail ([DB15.10](../../issues/database.md#db15-smaller-defects)). Settings that the mod does not contain get schema defaults. See [Mods and merge rules](../../database.md#mods-and-merge-rules).

### Data rules

1. Give every mod a `DatabaseSettings` record with the version of the game that the mod targets. Without it, a folder mod is read as version 1.0 and goes through every migration step.
2. Do not set a version above the current one. The upgrader throws `invalid database version`, and the mod does not load.
3. Set the mod name and ID in the `id` file, not in `ModName` and `ModId`.
4. Set `UnloadOriginalDatabase` only for a complete catalog.
