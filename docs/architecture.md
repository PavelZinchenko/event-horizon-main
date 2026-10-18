# Architecture

Event Horizon is split into assembly-defined modules and wired together with Zenject. Game flow runs through a state machine, and modules communicate through signals. Static game content comes from a schema-driven database that mods can replace.

For the folder-by-folder module map, see [Project structure](project_structure.md). Each subsystem below links to the doc that owns its detail.

## Cross-Cutting Patterns

### Dependency Injection (Zenject)

- `Assets/Scripts/Installers/GameInstaller.cs` sets up the project-wide container: random generation, purchasing, configuration, and core services.
- `ServicesInstaller.cs` registers shared services, including `IDatabase`.
- Each scene has its own installer, such as `MainMenuSceneInstaller`, `CombatSceneInstaller`, `ExplorationSceneInstaller`, and `EhopediaSceneInstaller`.
- Services are registered and consumed through interfaces (`BindInterfacesTo<T>()`).

### Data-Driven Design

- Static game content is JSON in `Assets/Modules/Database/Resources/Database/`.
- XML schemas generate the C# types for game, editor, and migration code. See [Database code generator](database/code_generator.md).
- A mod can override or replace the built-in database. See [Database module](database.md).

### Game State Machine

- `Assets/Scripts/GameStateMachine/StateMachine.cs` manages game flow. `StateType.cs` lists the state kinds.
- The states live in `States/`. They include `InitializationState`, `MainMenuState`, `StarMapState`, `TravelState`, `ExplorationState`, `CombatState`, `QuestState`, `DialogState`, `ShipEditorState`, and `SkillTreeState`.
- `GameStateFactory` creates the states.

### Signals

- Modules communicate through signals instead of direct references. The base types are in `Assets/Modules/CommonComponents/Scripts/Signals/` (`ISignal`, `Signal`, `SmartWeakSignal`).
- The quest system follows progress through `QuestEventSignal`. See [Quests module](quests.md).

## Engine-free modules

The domain core compiles without `UnityEngine`, so its tests run with plain `dotnet test`. An engine-free assembly sets `"noEngineReferences": true` in its asmdef, and the Unity compiler then rejects any `UnityEngine` use in it. Code that needs the engine lives in a companion assembly, usually named `<Module>.Unity`.

| Engine-free assembly | Engine-side companion | Owning doc |
| --- | --- | --- |
| `Diagnostics` | `Diagnostics.Unity` (`UnityLogger`) | [diagnostics.md](diagnostics.md) |
| `Utilites` | — | [utilites.md](utilites.md) |
| `CommonComponents` | `CommonComponents.Unity` (geometry, meshes, Zenject signal bindings, converters) | [common_components.md](common_components.md) |
| `CodeWriter.ExpressionParser` | — | [expression_parser.md](expression_parser.md) |
| `Security.Crypto` (commercial), `SecurityStub.Crypto` (open source) | `Security`, `SecurityStub` | [modules_private.md](modules_private.md), [services.md](services.md) |
| `Database` | `Database.Unity` (`Resources` storage, Unity JSON, sprite and audio decoding, `UnityDatabaseFactory`) | [database.md](database.md) |
| `NVorbis` (third-party) | — (`Database.Unity` uses it) | [plugins.md](plugins.md) |
| `Localization.Abstractions` | `Localization` | [localization.md](localization.md) |
| `ShipConstructor` | — | [ship_constructor.md](ship_constructor.md) |
| `GameSession` | `Assets/Scripts/Services/Unity/SessionPlayTimeTicker.cs` | [game_session.md](game_session.md) |
| `CombatSimulation.Physics` | — | [combat_simulation.md](combat_simulation.md) |

### Rules

1. Do not reference an engine assembly, Zenject, or a non-engine-free module from an engine-free assembly.
2. Use `GameDatabase.Model.Vector2Data` and `ColorData` instead of `Vector2` and `Color`. Engine code converts them with the `ToUnity()` and `ToData()` extensions in `CommonComponents.Unity`. C# has no implicit conversion between them, so every call site converts explicitly.
3. Use `Utilites.MathEx` instead of `Mathf`. It matches `Mathf` exactly: `Clamp` never throws, `RoundToInt` rounds half to even. See [Utilites: MathEx](utilites.md#mathex).
4. Log with `GameDiagnostics.Trace` (all builds) or `GameDiagnostics.Debug` (editor only). Outside Unity, `Trace` writes to the console. See [Diagnostics](diagnostics.md#redirecting-trace).
5. Use Utilites `RandomState` or `PcgRandom` for randomness, or take an `IRandomSource` (ExpressionParser). Never use `UnityEngine.Random`. A change of generator in seeded code changes saves: ask the owner first.
6. Pass engine services in from outside: frame time as a method argument (`SessionData.AddPlayTime`), storage and JSON through constructor parameters (`Database`), and `Resources` access through a static hook only when generated code cannot take a parameter (`MigrationResources.StorageProvider`).
7. When a file moves into a `*.Unity` assembly, move it with `git mv` together with its `.meta` file, so the asset GUID stays the same.

### Running the tests without Unity

```bash
dotnet test Tests~/EngineFree.Tests.csproj
```

[`Tests~/EngineFree.Tests.csproj`](../Tests~/EngineFree.Tests.csproj) compiles the sources of the engine-free assemblies and the test folders of Utilites, CommonComponents, ExpressionParser, Database, GameSession, and CombatSimulation.Physics into one .NET 8 test assembly. Unity ignores folders whose names end in `~`.

1. The project uses C# 9 (the Unity language version) and NUnit 3.14. NUnit 4 removed the classic `Assert.AreEqual` that the tests use.
2. BouncyCastle and IonicZlib come from the DLLs in `Assets/`. Newtonsoft comes from NuGet, at the version of Unity's `com.unity.nuget.newtonsoft-json` package.
3. When `ModulesPrivate/Security/Crypto/ModEncryption.cs` exists, the project compiles the commercial crypto and defines `LICENSE_COMMERCIAL`, which enables `PackedModEncryptionTests`. Otherwise it compiles `SecurityStub.Crypto` and defines `LICENSE_OPENSOURCE`.
4. `UNITY_EDITOR` and `IAP_DISABLED` are not defined, so the code compiles as in a release player build.
5. All sources share one assembly, so `internal` members are visible everywhere. A test that compiles here can still fail to compile in Unity; Unity is the authority on assembly boundaries.
6. To add a test folder or an engine-free assembly, add its folder to the `Compile` items of the project. Exclude its `Unity/` and `Tests/` subfolders where they belong to other assemblies.
7. Tests that need `Resources` or other engine APIs (`Database.Unity.Tests`) run only in the Unity Test Runner.

## Subsystems

| Subsystem | Summary | Owning doc |
| --- | --- | --- |
| Database | Schema-driven static catalog; JSON with generated types; mod overrides; version migration; storage from Resources, a folder, or a packed file. | [database.md](database.md), [database/editor.md](database/editor.md), [database/code_generator.md](database/code_generator.md) |
| Expressions | Text formulas in database fields compiled to delegates on first use. | [expression_parser.md](expression_parser.md) |
| Combat | Real-time simulation built from modular ship components; Unity 2D physics through `RigidBodyAdapter` and `PhysicsManager`; behavior-tree AI; `CombatManager` handles pausing. | [battle_simulator.md](battle_simulator.md) |
| Ship construction | Ship model (hull, layout, components, satellites, perks); compatibility rules; modifications; stats calculation; experience, threat, and distance formulas. | [ship_constructor.md](ship_constructor.md) |
| Ship editor | Ship-layout editing context and mesh-building tools. | [ship_editor.md](ship_editor.md) |
| Quests | Database quest graphs started by game events; requirements, dialogs, battles, loot; side effects through `IQuestActionProcessor`. | [quests.md](quests.md) |
| Save system | The `GameSession` module persists session state in a schema-generated binary format with versioned migration. The serializer does not compress; a storage backend may transform the bytes. | [game_session.md](game_session.md) |
| Star map | Prefabs and legacy scripts that draw stars, regions, and star systems; view modes, zoom, and map input. | [galaxy_map.md](galaxy_map.md) |
| Application layer | `Assets/Scripts/`: installers, game states, campaign domain, game UI, legacy code; known architectural problems. | [application_layer.md](application_layer.md), [issues/application_layer.md](issues/application_layer.md) |
| Game services | Application lifecycle and pause, additive scene loading for the state machine, combat input with key rebinding. | [game_services.md](game_services.md) |
| Platform services | Contracts in `ServicesFacade`; default implementations in `Services`; `SecurityStub` for open-source builds; per-platform selection in `ServicesInstaller`. | [services.md](services.md) |
| Private services | Commercial accounts, local and cloud saves, in-app purchases, ads, music download, arena requests, save and mod decryption. | [modules_private.md](modules_private.md) |
| Server | PHP and MySQL: arena (asynchronous PvP), purchase backup, music asset bundles, web pages. | [server.md](server.md) |
| GUI | Window manager, animated windows, themes, debug console, component list, vendored list and UI plugins. | [gui.md](gui.md) |
| Localization | XML catalogs in `Assets/ModulesShared/Localization/Resources/`; per-language plural rules; runtime language switching; `FontTable` for platform fonts. | [localization.md](localization.md) |
| Resources | Sprite and sound lookup by name with mod fallback; media folders and `Resources` content. | [resource_locator.md](resource_locator.md), [resources/index.md](resources/index.md) |
| Common types | Shared identifiers, economy and galaxy types, signals, general helpers; known defects. | [common_components.md](common_components.md), [issues/common_components.md](issues/common_components.md) |
| Utilities | Inventory collections, PCG random numbers, obscured values, binary serialization helpers. | [utilites.md](utilites.md) |
| Diagnostics | `Debug` (editor only) and `Trace` (all builds) loggers. | [diagnostics.md](diagnostics.md) |
| Configuration | Generated `AppConfig` constants, scripting define symbols, UI theme and font table assets. | [app_configuration.md](app_configuration.md) |
| Third-party code | Integration of every plugin and SDK: version, license, users, platform notes. | [plugins.md](plugins.md) |
| Build | Manual Unity builds; platform settings, build hooks, signing, packages, editor tools. | [build.md](build.md) |

Known defects are in [issues/](issues/), in a file named after the owning doc.
