# Project Structure

Event Horizon is a Unity 6 project organized around assembly-defined C# modules. This page is a high-level, living map of the repository; add links to deeper documentation as individual areas are documented.

## Repository Root

- `Assets/` contains gameplay code, scenes, resources, art, editor tools, and vendored Unity plugins.
- `Packages/` declares Unity Package Manager dependencies. `manifest.json` is the primary package configuration.
- `ProjectSettings/` stores project-wide Unity settings, including the editor version and build configuration.
- `docs/` contains contributor-facing technical documentation.
- `UIElementsSchema/` contains schemas used by Unity UI authoring tools.
- `README.md` provides the public project and community overview.

## Assets at a Glance

### Code and architecture

- `Assets/Scripts/` is the application layer: game-state flow, installers, domain coordination, services, GUI integration, and legacy code.
- `Assets/Editor/` and `Assets/Tools/` contain Unity Editor extensions and development utilities.

Assembly boundaries are defined by `.asmdef` files. Check references before moving code between modules.

## Modules

`Assets/Modules/` contains core game features and public service implementations.

- `AppConfiguration` - Generated application configuration plus shared font and UI theme assets.
- `BattleSimulator` - Combat simulation, ship behavior, weapons, physics, and combat AI. See [BattleSimulator module](battle_simulator.md).
- `CommonComponents` - Shared domain types for identifiers, economy, galaxy layout, and other common concepts. See [CommonComponents module](common_components.md).
- `Database` - Schema-driven game data, runtime access and serialization, migrations, and code-generation tools. See [Database module](database.md).
- `GalaxyMap` - **TBD.**
- `GameServices` - Cross-platform application, input configuration, timing, and scene-management services.
- `GameSession` - Schema-driven session state, save/load support, and serialization code generation. See [GameSession module](game_session.md).
- `Quests` - Quest construction and runtime processing, including requirements, actions, fleets, and rewards. See [Quests module](quests.md).
- `ResourceLocator` - Central runtime resource-location support.
- `SecurityStub` - Fallback security, validation, request, and purchase-processing implementations.
- `Services` - Default or local implementations for accounts, assets, audio, messaging, object pooling, ads, and purchasing.
- `ServicesFacade` - Interfaces and shared contracts consumed by service clients and implementations.
- `ShipConstructor` - Ship assembly, component compatibility, layout, statistics, and related calculations.
- `ShipConstructor2` - **TBD.**
- `ShipModelGenerator` - **TBD.**

## ModulesShared

`Assets/ModulesShared/` contains reusable systems used across multiple game features.

- `Diagnostics` - Project logging, assertion, and Unity logger adapters.
- `Gui` - Shared UI framework, window management, themes, and reusable controls.
- `Localization` - Runtime localization and language-specific XML translation resources. See [Localization module](localization.md).
- `Resources` - **TBD.**
- `ShipEditor` - Ship-layout editing context, scene integration, selection, and mesh-building tools. See [ShipEditor module](ship_editor.md).
- `Utilites` - General collections, random-number generation, serialization, and security-oriented value types. The directory name intentionally retains its existing spelling.

## ModulesPrivate

`Assets/ModulesPrivate/` contains non-public or platform-specific implementations.

- `Plugins` - Private third-party integrations, including iOS Keychain and StansAssets wrappers.
- `Security` - Production encryption, certificate and bundle validation, request creation, and platform security integration.
- `Services` - Platform implementations for accounts, ads, assets, in-app purchases, secure storage, and related services.

## Scenes, Data, and Media

- `Assets/Scenes/` contains entry and feature scenes such as `LoaderScene`, `MainMenuScene`, `StarMapScene`, and `CombatScene`.
- `Assets/Resources/` and module-specific `Resources/` folders contain assets loaded at runtime.
- `Assets/ScriptableObjects/` contains serialized configuration assets.
- `Assets/Animation/`, `Audio/`, `Fonts/`, `Sprites/`, and `SplashImage/` contain presentation assets.
- `Assets/BundledAssets/` holds bundle-oriented content; `Assets/StreamingAssets/` is for raw platform files and is ignored by Git.
- `Assets/Plugins/` plus vendor-named folders contain third-party and platform-specific integrations.

## Tests and Generated Files

Tests are generally co-located with their assemblies under `Tests/`, for example `Assets/Modules/Database/Tests/` and `Assets/ModulesShared/Utilites/Tests/`. Expression parser tests live in `Assets/ExpressionParser/Tests/`.

Database and session modules include schemas and code generators in `.Schema/` and `.CodeGenerator/`. Use their checked-in scripts rather than hand-editing generated output.

Do not edit or commit Unity caches and local outputs such as `Library/`, `Temp/`, `obj/`, `Logs/`, `UserSettings/`, `Builds/`, generated IDE project files, or ignored platform plugin artifacts. Keep Unity `.meta` files with their corresponding assets.

## Where New Work Goes

- Add self-contained gameplay features to the closest assembly under `Assets/Modules/`.
- Put game-wide orchestration and scene integration in `Assets/Scripts/`.
- Put reusable infrastructure in `Assets/ModulesShared/` and platform/private implementations in `Assets/ModulesPrivate/`.
- Add tests beside the owning assembly in a `Tests/` folder with a test assembly definition.
