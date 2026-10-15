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
