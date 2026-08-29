# GameSession Module

The GameSession module owns Event Horizon's mutable, persisted game state. It defines the save schema, generates binary-serializable models and version converters, exposes domain-oriented session facades, tracks changes, and translates legacy saves into the current model. Platform-specific local and cloud storage remain outside this module and operate through the shared storage contracts.

The module lives in `Assets/Modules/GameSession/` and compiles as the `GameSession` assembly. Runtime types use the `Session`, `Session.Model`, and `Session.Content` namespaces. `Assets/Scripts/Installers/GameInstaller.cs` is the main composition root: it binds `SessionData`, `ContentFactory`, session signals, and resource-change signals. Under `EDITOR_MODE` it binds `SessionDataStub` instead.

## Data Flow

```text
.Schema/versions.xml + .Schema/v1..v3/**
                  |
                  | generate_code.cmd
                  v
Scripts/GeneratedCode/Model + v1 + v2 + SessionLoader
                  |
                  | hand-written partial models and migration hooks
                  v
       Model.SaveGameData <-> SessionSerializer binary payload
                  |                         |
                  | ContentFactory          | legacy/current conversion
                  v                         v
 ISessionData / Session.Content facades   SessionLoader
                  |
                  v
       IDataStorage / ICloudStorage in the application layer
```

The XML schemas are the source of truth for persisted fields and generated collection behavior. Generated code is checked in but must not be edited by hand. Facades and partial model files are the supported locations for domain behavior and integration.

## Directory Map

| Path | Purpose | Edit policy |
| --- | --- | --- |
| `.Schema/versions.xml` | Ordered list of supported schema versions; currently 1.0, 2.0, and 3.0. | Edit when introducing a persisted format version. |
| `.Schema/v*/Objects/` | Full top-level object definitions for each released schema version. | Treat released versions as compatibility snapshots. |
| `.Schema/v*/Types/` | Full persisted struct definitions for each version. | Treat released versions as compatibility snapshots. |
| `.CodeGenerator/` | Git submodule containing the .NET Framework generator and templates. | Change in its own repository and update the submodule pointer intentionally. |
| `Scripts/GeneratedCode/Model/` | Current v3 save model and serializer methods. | Never hand-edit; regenerate. |
| `Scripts/GeneratedCode/v1/`, `v2/` | Older models still needed to read and convert saved games. | Never hand-edit; regenerate. |
| `Scripts/GeneratedCode/SessionLoader.cs` | Generated version dispatch and automatic field copies. | Never hand-edit; implement partial hooks instead. |
| `Scripts/Model/` | Hand-written partial structs, constructors, and database/ShipConstructor conversions for current models. | Edit for domain behavior that is not expressible in the schema. |
| `Scripts/Facade/` | `ISessionData`, `SessionData`, content wrappers, factory, serializer, and legacy adapter. | Edit for the public session API and runtime lifecycle. |
| `Scripts/Facade/Content/` | Interfaces and wrappers for game, fleet, inventory, quests, resources, map, purchases, and other domains. | Keep consumers on these interfaces where practical. |
| `Scripts/Migration/` | Hand-written generated-loader partial methods for semantic conversions. | Edit when automatic member copying is insufficient. |
| `Scripts/Obsolete/` | Reader and old facade model for the pre-header save format. | Compatibility-only; do not use for new features. |
| `Scripts/Extensions/` | Conversion between UTC ticks and elapsed game time. | Hand-written runtime code. |
| `Tests/` | NUnit tests for streams, integer coding, observable bitsets, and save round trips. | Add serialization and migration regression tests here. |

## Schema and Generated Model

Each version directory contains a complete `SaveGameData.xml` root plus the referenced objects and structs. The current v3 root contains game metadata, achievements, bosses, common values, events, fleet, IAP, PvP, inventory, quests, regions, research, resources, shop, social state, star map, statistics, upgrades, wormholes, and ship presets.

Schema members declare primitive or composite types and may declare collection value types, defaults, or options. For example, `options="encrypted"` generates the module's obscured numeric wrappers, while generated collections receive a parent `IDataChangedCallback` so mutations propagate to the root. The schema controls the binary field order; reordering or removing members in a released version changes how existing bytes are interpreted.

The generator reads all entries in `versions.xml` in numeric order and applies their schemas cumulatively. It emits the latest definitions in `Session.Model`, older definitions required for conversion in version namespaces, and a `SessionLoader` that copies unchanged members between versions. When it cannot transfer a member safely, the generated partial method comment lists the manual work required.

## Runtime Facade and Change Tracking

`ISessionData` combines `ISessionContent` with the storage-facing `IGameData` metadata. `ISessionContent` exposes narrow domain interfaces such as `IGameData`, `IFleetData`, `IInventoryData`, `IResourcesData`, `IQuestData`, and `IShipPresetsData`. Gameplay code should prefer these interfaces unless it specifically owns session creation or binary persistence.

`SessionData` owns the generated `Model.SaveGameData`. `ContentFactory` wraps each generated object in its `Session.Content` facade and supplies the database and the appropriate signal triggers. This layer is where mutations enforce domain rules, adapt database objects, and publish changes without placing those responsibilities in generated classes.

Generated setters and observable collections call `IDataChangedCallback.OnDataChanged`. `SessionData` increments its `DataVersion` only when entering a dirty state; serialization clears that dirty state so a later change can increment it again. `DataVersion` is save metadata used by storage synchronization. It is not the schema version encoded in the binary payload.

`SessionData` also implements Zenject `ITickable` and accumulates `TimePlayed` while a game's start time is set. Creating a new game assigns a new UTC-tick game ID, resets played time and data version, initializes every facade, and, by default, carries purchased entitlements and stars forward from the previous in-memory session.

## Binary Format and Loading

`SessionSerializer` writes this module-level envelope:

| Field | Representation |
| --- | --- |
| Header | `0x5AFE5AFE` as an unsigned 32-bit value. |
| Serializer format | Signed 32-bit value; the current and minimum supported value are both `1`. |
| Schema version | `(major << 16) + minor`; the current generated model writes 3.0. |
| Payload | Generated `SaveGameData.Serialize` output using `SessionDataWriter`. |
| End marker | `0xDEADC0DE` as an unsigned 32-bit value. |

The generated reader uses plain values or Elias gamma coding according to the generated member type and options. The GameSession serializer itself does not compress this envelope; a storage implementation may wrap or transform the resulting bytes separately.

On load, a valid header selects the current serializer. `SessionLoader` reads v1, v2, or v3 and upgrades older generated models in memory. The EOF marker detects truncated or misaligned payloads. If the header is absent, `LegacyDataLoader` reads the older `Scripts/Obsolete` representation, transfers it into a v1 generated model, and then runs the normal version conversions.

`SessionData.TryDeserialize` initializes facades only after the payload has loaded, restores the storage metadata supplied by the backend, clears the dirty state, then fires `SessionDataLoadedSignal` followed by `SessionCreatedSignal`. Handler failures are logged after the data is installed and do not turn a successful deserialization into a failure.

## Application Save Lifecycle

`InitializationState` loads the selected database/mod first, initializes localization, then asks the active `IDataStorage` to load `SessionData` for that database ID. If no compatible save exists, editable databases may import an original save; otherwise a new game is created.

`GameDataManager` coordinates storage outside the module. It fires `SessionAboutToSaveSignal`, saves on application pause, focus loss, disable, selected scene loads, IAP completion, and explicit cloud operations, and routes mod changes through the matching per-mod save. Local and cloud backends receive `SessionData` as `ISerializableGameData`.

`SessionDataStub` exists for the separate `EDITOR_MODE` database/ship-editor workflow. Its persistence and new-game methods throw `NotImplementedException`, and its content fields are not initialized by normal game loading. Do not use it as a lightweight saved-game implementation.

## Versioning and Migration Workflow

For a persisted model change:

1. Decide whether the change can remain compatible or requires a new schema version. For a released layout change, add a new full version directory and entry in `.Schema/versions.xml`; do not rewrite an older compatibility snapshot.
2. Update the new version's object/type XML, including the root `SaveGameData.xml` when a top-level section changes.
3. From `Assets/Modules/GameSession`, run `generate_code.cmd`. The script invokes `.CodeGenerator\CodeGenerator\bin\Release\CodeGenerator.exe .Schema\versions.xml Scripts` and rebuilds `Scripts/GeneratedCode/`.
4. Review every generated migration comment. Implement required semantic conversions in a matching partial `SessionLoader` file under `Scripts/Migration/`.
5. Add or update partial types in `Scripts/Model/` for constructors and conversions that cannot be generated.
6. If the public domain surface changed, update the `Session.Content` interface/wrapper, `ISessionContent`, `ContentFactory`, and any signals as one change.
7. Add round-trip and old-version migration fixtures before depending on the new field in gameplay.

The generator submodule targets .NET Framework 4.8. If its Release executable is missing, build `Assets/Modules/GameSession/.CodeGenerator/CodeGenerator.sln` before running the batch file. Because `generate_code.cmd` uses module-relative paths, run it with `Assets/Modules/GameSession` as the working directory.

## Common Change Locations

- Add or change persisted data: a new/current `.Schema` snapshot, followed by generation.
- Add domain operations over generated data: `Scripts/Facade/Content/`.
- Convert a new version or changed type: `Scripts/Migration/` using the generated partial hook.
- Convert session structs to Database or ShipConstructor models: `Scripts/Model/` partials.
- Change envelope validation or legacy routing: `Scripts/Facade/SessionSerializer.cs` and `LegacyDataLoader.cs`.
- Change local/cloud save timing or platform storage: application code under `Assets/Scripts/GameServices/` or the owning Services module, not GameSession.
- Add a session change notification: `Scripts/Signals.cs`, its `ContentFactory` injection, and the application installer bindings.

## Validation Checklist

1. Run `GameSession.Tests` in the Unity Test Runner. The current suite covers stream primitives, Elias gamma coding, bitsets, and a generated-model round trip.
2. For every schema change, round-trip the current model and load at least one fixture from each affected older major version.
3. Exercise the headerless legacy path when touching `Scripts/Obsolete`, `LegacyDataLoader`, or v1 conversion.
4. Verify the generated diff contains only expected field, collection, version, and loader changes; never repair generated output by hand.
5. Mutate scalar and collection data through the facade and confirm dirty propagation, `DataVersion`, and signals fire once as intended.
6. Save, restart, and load with both the default database and a mod-specific database ID.
7. Test failed, truncated, wrong-format, and wrong-version input through the storage boundary and confirm the application falls back without partially initialized content.
