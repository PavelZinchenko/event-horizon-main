# Database Module

The Database module owns Event Horizon's static game catalog: ships and builds, components, weapons and ammunition, factions, quests, AI behavior trees, global balance settings, and the identifiers that connect those records. It turns schema-defined JSON into a typed, read-only runtime model and can layer a user mod over the built-in catalog.

The module lives in `Assets/Modules/Database/` and compiles as the `Database` assembly with the `GameDatabase` root namespace. Most consumers depend on `IDatabase`; the main Zenject registration is in `Assets/Scripts/Installers/ServicesInstaller.cs`.

## Data Flow

```text
.Schema/versions.xml + .Schema/v1/**
                 |
                 | generate_code.cmd / generate_migration_code.cmd
                 v
Scripts/GeneratedGameCode/ + Scripts/GeneratedMigrationCode/
                 |
Resources/Database/**/*.json or an external mod
                 |
                 v
IDataStorage -> DatabaseContent (serializable records) -> Database.Loader
                 |                                      |
                 | optional version migration           v
                 +-----------------------------> runtime DataModel + IDatabase
```

The schema is the source of truth for fields, enums, expressions, object references, generated serializers, and the generated portion of `IDatabase`. JSON is the source of truth for the built-in game catalog. Hand-written partial classes add domain behavior that the schema cannot express.

## Directory Map

| Path | Purpose | Edit policy |
| --- | --- | --- |
| `.Schema/versions.xml` | Declares available schema versions and the current major/minor version. | Edit when versioning the schema. |
| `.Schema/v1/` | XML definitions for enums, expressions, objects, settings, quests, AI, and weapons. | Edit for data-shape changes. |
| `.CodeGenerator/` | Git submodule containing the .NET Framework 4.8 generator and T4 templates. | Change in its own repository; do not treat it as generated output. |
| `.Editor/` | Git submodule containing the Windows database editor and analyzer. | Change in its own repository. |
| `Resources/Database/` | Built-in JSON catalog loaded through Unity `Resources`. | Edit for content and balance changes. |
| `Resources/DatabaseMigration/` | Data used by specific migrations; currently includes the v1.3 component-mod replacement set. | Edit only with the owning migration. |
| `Scripts/GeneratedGameCode/` | Generated runtime DTOs, enums, data models, loader, storage interfaces, and `IDatabase` members. | Never hand-edit. Regenerate. |
| `Scripts/GeneratedMigrationCode/` | Generated version-specific DTOs, content containers, and migration dispatcher. | Never hand-edit. Regenerate. |
| `Scripts/Migration/` | Hand-written partial migration steps between minor versions. | Edit when a schema change needs data transformation. |
| `Scripts/DataModel/` | Hand-written partials and adapters that extend generated runtime objects. | Edit for runtime domain behavior. |
| `Scripts/Storage/` | Unity Resources, folder, packed-file, and JSON serialization implementations. | Edit for loading or persistence behavior. |
| `Scripts/Types/`, `Extensions/`, `Expression/`, `Query/`, `Utils/` | Custom value types, convenience APIs, expression integration, queries, and conversion helpers. | Hand-written runtime code. |
| `Tests/` | Editor-only NUnit tests for default data and ship/faction invariants. | Add module-level runtime/data tests here. |

`Plugins/NVorbis/` is also a Git submodule and supplies Ogg decoding. The assembly additionally references Security/SecurityStub, CommonComponents, the expression parser, Diagnostics, and NVorbis.

## Authored Data

`Resources/Database/` is grouped by domain rather than by C# namespace. Its main areas are `Ai`, `Ammunition`, `Component`, `Device`, `Dronebay`, `Faction`, `Prefabs`, `Quests`, `Satellite`, `Settings`, `Ship`, `Technology`, and `Weapon`.

Every JSON document identifies its generated record type with `ItemType`. Collection records also have an integer `Id`; settings records are singletons. `Resources/Database/Settings/Database.json` records the data version, which must agree with `.Schema/versions.xml` and the version constants regenerated into `GeneratedGameCode/Database.cs`. The current schema and built-in data are version 1.7.

The runtime serializer is `UnityJsonSerializer`, backed by Unity's `JsonUtility`. Recursive behavior-tree records are a deliberate exception and use Newtonsoft.Json. The editor-side serializer uses Newtonsoft.Json and serializes fields rather than properties.

Do not place arbitrary `TextAsset` files below `Resources/Database/`: `ResourceDatabaseStorage` attempts to parse every loaded text asset as a database JSON record. Keep Unity `.meta` files paired with their assets.

## Generated and Hand-Written Models

Generation produces several cooperating layers:

- `GameDatabase.Serializable` classes mirror JSON and retain source filenames for diagnostics and editor updates.
- `GameDatabase.DataModel` classes are the hydrated runtime objects. References such as IDs are resolved while `Database.Loader` constructs these objects.
- The generated part of `IDatabase` exposes settings, typed collections, typed `Get...` methods, and media/localization lookups.
- The generated `Database` partial stores records in ID-indexed dictionaries. A missing ID returns that model's `DefaultValue` instead of throwing.
- Hand-written partials in `Scripts/DataModel/` add derived behavior, adapters, cloning, compatibility conversions, and deserialization hooks.

Records marked `Disabled` remain significant while content is merged, but `Database.Loader` omits them from the runtime collections. This lets a mod suppress a built-in record with the same ID.

`ShipBuildQuery` is the main query facade currently owned by the module. It composes lazy filters over `IDatabase.ShipBuildList` for player ships, enemies, drones, starbases, faction, difficulty, size, and distance.

## Runtime Loading

`GameDatabase.Database` is registered as a singleton and normally initialized by `InitializationState`:

1. `LookForMods()` scans supported mod locations.
2. The selected mod ID is passed to `TryLoad`; otherwise `LoadDefault()` is used.
3. A storage backend streams records and media into generated `DatabaseContent`.
4. Older content is upgraded to the current schema in memory.
5. Unless the mod's `DatabaseSettings.UnloadOriginalDatabase` is true, built-in content is loaded as its parent.
6. `Database.Loader` resolves references and publishes the runtime model.
7. `DatabaseLoaded` notifies localization, UI, and other caches that consume database data.

Mod content loads before built-in content. For JSON records, the first record for an ID or singleton wins, so a mod overrides the default and the default catalog fills only missing entries. Duplicate IDs inside the initially loaded storage are errors. Image, audio, and localization keys use case-insensitive runtime dictionaries.

The default catalog uses `ResourceDatabaseStorage("Database")`. External mods use one of two formats:

- `FolderDatabaseStorage` recursively reads JSON, PNG/JPEG images, WAV/Ogg audio, and language-named XML localization. It is editable and uses a two-line `id` file, when present, for display name and stable ID; otherwise the folder name is used for both.
- `FileDatabaseStorage` reads the editor's compressed/encrypted packed format. It supports JSON, images, localization, and WAV/Ogg entries and is read-only at runtime.

### Packed Mod Encryption

New packed mods use authenticated hybrid encryption. The editor serializes the existing payload, zlib-compresses it, encrypts it with a fresh AES-256-GCM key and 12-byte nonce, and wraps that key with the game's RSA-3072 public key using OAEP with SHA-256 for both OAEP and MGF1. The commercial Security module owns the obfuscated private key and does not cache its decoded form.

Format version 1 is fixed-width and little-endian:

| Field | Size |
| --- | ---: |
| Magic `0xDADABA5E` | 4 bytes |
| Format version `1` | 4 bytes |
| RSA-wrapped AES key | 384 bytes |
| GCM nonce | 12 bytes |
| GCM authentication tag | 16 bytes |
| Encrypted zlib payload | Remaining bytes |

The magic, format version, wrapped key, and nonce are supplied to GCM as associated data. Any unsupported version, truncated envelope, key-unwrapping failure, or authentication failure rejects the mod before decompression. Files beginning with the legacy `0xDA7ABA5E` magic retain the old XOR loader, and the older headerless fallback remains supported. Open-source builds do not contain the private key and therefore cannot load the authenticated format.

On Android outside the editor, folder discovery uses `Application.persistentDataPath/Mods/`. Standalone builds and the Unity Editor use the `Mods/` directory beside the game/project directory (`Application.dataPath + "/../Mods/"`). Other platforms currently return without directory discovery, although an explicitly selected external packed file can still be registered through `TryAddModFromFile` where the surrounding platform UI supports it.

Folder media is loaded lazily on first access. Packed-file media is decoded lazily from stored bytes. `SaveShipBuild` and `SaveSatelliteBuild` are temporary APIs: writes work only for editable storage, while packed mods reject updates.

## Versioning and Migration

`GeneratedMigrationCode/DatabaseUpgrader.cs` validates the source version, deserializes into version-specific generated classes, runs each required minor migration in order, and exports the result to the current `IContentLoader`. The supported range is currently 1.0 through 1.7; a legacy major version of 0 is normalized to 1.0.

The generated dispatcher declares partial migration hooks. Their implementations live in `Scripts/Migration/v1/DatabaseUpgrader_<from>_<to>.cs`. These files perform semantic transformations such as creating new records, mapping obsolete fields, or loading migration-only resource data. Obsolete schema members must remain available long enough for those migrations to read old content.

For a schema change, coordinate all of the following:

1. Update the XML schema and `.Schema/versions.xml` as appropriate.
2. Add or update the hand-written migration step when existing databases or mods need transformation.
3. Update `Resources/Database/Settings/Database.json` and the built-in records.
4. Regenerate game and migration code.
5. Rebuild the external editor if its generated model changed.
6. Validate both the current built-in database and at least one database from the oldest newly supported version.

Do not remove obsolete fields merely because current JSON no longer writes them; first check the generated migration model and every migration step.

## Commands and Tooling

The checked-in batch files use paths relative to `Assets/Modules/Database`, so run them with that directory as the working directory:

```bat
cd Assets\Modules\Database

rem Generate Scripts\GeneratedGameCode
generate_code.cmd

rem Generate Scripts\GeneratedMigrationCode
generate_migration_code.cmd

rem Regenerate the editor model and build the Windows editor/analyzer solution
rebuild_editor.cmd

rem Launch the database editor for Resources\Database
editor.cmd

rem Launch the analyzer for Resources\Database
analyzer.cmd
```

`rebuild_editor.cmd` currently invokes the Visual Studio 2022 Community `MSBuild.exe` from its default installation path. The generator project targets .NET Framework 4.8. Restore/build the `.CodeGenerator` submodule first if `DatabaseCodeGenerator.exe` is absent. The editor and analyzer target Windows.

The `.CodeGenerator/test.cmd` name is historical in the current checkout: it invokes an editor-code generation pass into `.CodeGenerator/Output`; it is useful as a generator smoke check but is not the Unity/NUnit Database test suite.

## Validation Checklist

For a data-only change:

1. Open or analyze `Resources/Database` with the external tools.
2. Let Unity reimport the changed JSON and check the Console for parse, duplicate-ID, and unresolved-reference errors.
3. Run `Database.Tests` in the Unity Test Runner. The current tests load the default catalog and check spawn-distance ordering, player/enemy ship availability, and faction fleet coverage.
4. Exercise the affected gameplay query or system in the editor.

For a schema or migration change, also regenerate both code trees, review the generated diff, rebuild the external editor, and load old-version content through the migration path. Generated warnings at the top of a file are authoritative: never repair generated output by hand.

## Common Change Locations

- Balance or add an existing record type: `Resources/Database/<domain>/`.
- Add or change a persisted field or enum: `.Schema/v1/`, followed by generation and migration review.
- Add runtime-only behavior to a generated model: a matching partial class in `Scripts/DataModel/` or `Scripts/Extensions/`.
- Add a reusable catalog query: `Scripts/Query/` against `IDatabase`.
- Change mod discovery or merge behavior: `Scripts/Database.cs` and `Scripts/Storage/`.
- Change an old-version transformation: `Scripts/Migration/`, plus generated migration code when the schema changed.
- Change the editor or generator itself: work in the corresponding Git submodule and update the parent repository's submodule pointer intentionally.
