# Application Layer (`Assets/Scripts/`)

`Assets/Scripts/` is the application layer. It connects the modules into a playable game: it wires the dependency-injection container, runs the game state machine, holds the campaign domain logic (galaxy, player, economy, research), and contains most of the game UI.

The folder has no `.asmdef` file. All code compiles into Unity's default `Assembly-CSharp` assembly. This assembly can reference every module, but no module can reference it. Only `Assets/Editor/` scripts depend on it.

Known architectural problems are listed in [Application layer issues](issues/application_layer.md). For the wider module map, see [Project structure](project_structure.md) and [Architecture](architecture.md).

## Size and Status

The folder contains 474 C# files and about 37,400 lines. About 2,000 lines are commented-out code.

| Folder | Files | Lines | Purpose | Status |
| --- | --- | --- | --- | --- |
| `Installers/` | 12 | 970 | Zenject installers for the root container and each scene. | Active |
| `GameStateMachine/` | 27 | 2,460 | Stack-based state machine that loads scenes for each game state. | Active; 2 states are never created |
| `Domain/` | 76 | 7,130 | Campaign logic: player data, galaxy content, economy, research, exploration, quest adapters. | Active |
| `GameServices/` | 17 | 1,550 | Save/load coordination, notifications, random numbers, holidays, arena client. | Active |
| `Combat/` | 18 | 2,050 | Combat model builder and scene managers that sit on top of the `BattleSimulator` module. | Active |
| `Gui/` | 102 | 7,350 | Newer UI: main menu, settings, combat HUD, shipyard, hangar, crafting, dialogs. | Active |
| `Services/` | 7 | 620 | Internet time, game time, rate-game, signal-to-messenger bridge. | Active |
| `GameContent/` | 3 | 130 | ScriptableObject lookup tables for currency, planet, and skill icons. | Active |
| `Utils/` | 2 | 80 | Scene helpers such as `DefaultCamera`. | Active |
| `Legacy/` | 210 | 15,100 | Older code: star map rendering, older UI panels, fleets, regions, inventories, debug console. | Mixed: about 150 files are live, about 60 are dead |

"Legacy" does not mean "unused". The star map, skill tree, research tree, market, and quest dialogs all run on `Legacy/` code. See [Legacy folder](#legacy-folder).

## Composition Root

The game has no `ProjectContext`. The root container is the `SceneContext` in `LoaderScene`, which declares the Zenject contract name `services`. Every other scene has a child `SceneContext` whose parent contract is `services`. `Resources/ZenjectDefaultSceneContractConfig.asset` maps the contract to `BaseTestScene`. This is an editor feature: when Play Mode starts in a scene without a loaded parent, Zenject first loads `BaseTestScene`, which binds stub services. See [Plugins: Zenject](plugins.md#zenject-extenject).

`LoaderScene` stays loaded for the whole session because `InitializationState` stays at the bottom of the state stack. Root bindings therefore live as long as the application.

### Root installers (in `LoaderScene`)

- `ServicesInstaller` binds platform services: settings, `IDatabase`, localization, UI theme, `GuiManager`, audio, `IApplication`, scene manager, internet time, account, local storage, cloud storage, ads, rate-game, secure storage, and the asset loader. It selects platform and license variants with `#if` chains (`LICENSE_OPENSOURCE`, `UNITY_ANDROID`, `ADS_UNITY`, and others). It also binds the service-level signals.
- `GameInstaller` binds game services: random generator, IAP, `GameDataManager`, star map and star content, research, player data, quest manager and its context adapters, factories, every game state with its factory, and about 60 game signals.

### Scene installers

| Installer | Scene | Binds |
| --- | --- | --- |
| `CommonGuiSceneInstaller` | `CommonGuiScene` | Scene `Messenger`. |
| `MainMenuSceneInstaller` | `MainMenuScene` | Scene `Messenger`. |
| `StarMapSceneInstaller` | `StarMapScene` | Scene `Messenger`, object pool. |
| `DefaultSceneInstaller` | `SettingsScene`, `SkillTreeScene` | Scene `Messenger` for a configured `GameScene`. |
| `CombatSceneInstaller` | `CombatScene` | Combat simulation (`Scene`, collisions, AI, factories), combat HUD panels, `CombatManager`, `CombatTimer`, input. |
| `CoreCombatSceneInstaller` | `CoreCombatScene` | `CoreCombatAdapter` over the new combat core, combat HUD panels and presenters, input. No legacy simulation bindings. See [Combat simulation: Unity integration](combat_simulation.md#unity-integration). |
| `ExplorationSceneInstaller` | `ExplorationScene` | Same simulation stack plus `ExplorationData` and `ExplorationSceneManager`. |
| `EhopediaSceneInstaller` | `EhopediaScene` | Simulation stack for ship previews and `EhopediaSceneManager`. |
| `ConfigSceneInstaller` | `ConfigureControlsScene` | Simulation stack for the controls test range. |
| `ObjectFactoryInstaller` | `ShipEditorScene` | `IGameObjectFactory`. |
| `TestSceneInstaller` | `BaseTestScene` | Stub services for UI testing without the loader. |

The combat, exploration, Ehopedia, and controls installers repeat about 20 identical simulation bindings.

### Per-state bindings

A state can add bindings to the next scene container through `IGameState.InstallBindings`. Before the state machine loads scenes, it sets `SceneContext.ExtraBindingsInstallMethod` to that method. For example, `CombatState` binds `ICombatModel`, `ExplorationState` binds `Planet`, and `ShipEditorState` binds `IShipEditorContext`.

## Game State Machine

`GameStateMachine/StateMachine.cs` keeps a stack of `IGameState` objects. Each state lists the scenes it needs in `RequiredScenes`. After every change, the machine asks `IGameSceneManager` to load the union of the scenes of all states on the stack. Scenes load additively. When the scene manager reports `Ready`, the top state becomes `Active` and `GameStateChangedSignal` fires.

Operations:

- `LoadState` replaces the top state.
- `LoadStateAdditive` suspends the top state and pushes a new one (dialogs, travel, overlays).
- `UnloadActiveState` pops the top state. If the stack becomes empty, the application quits.
- `ReloadState` unloads and reloads the scenes of the top state (used after a language change).

`BaseState` turns the `Condition` property (`NotLoaded`, `Active`, `Suspended`, `Reloading`) into the hooks `OnLoad`, `OnUnload`, `OnSuspend`, `OnResume`, `OnActivate`, `OnDeactivate`, `OnBeginReload`, and `OnReloaded`. Its helpers `LoadState`, `LoadStateAdditive`, and `Unload` do nothing when the state is not active. Many signal handlers depend on this guard (see [Signals and messenger](#signals-and-messenger)).

To add a state, change four places: the state class with a nested `Factory`, a `Bind` and a `BindFactory` call in `GameInstaller.BindStateMachine`, and a `Create...` method in `GameStateFactory`.

### States

| State | Scenes | Entered from | Role |
| --- | --- | --- | --- |
| `InitializationState` | Loader, CommonGui | `StateMachine.Initialize` | Loads mods and the database, starts localization, validates ship builds in editable databases, signs in, loads or creates the save, then pushes `MainMenuState`. |
| `MainMenuState` | MainMenu | Initialization, exit from game | Starts or continues the game, quick battle, ship editor (database mode), Ehopedia, settings, controls test. Shows the daily reward on non-standalone platforms. |
| `StartingNewGameState` | none | Main menu | Sets the start time, fuel, starting inventory, and starting fleet, then loads the star map. |
| `StarMapState` | StarMap | Most gameplay states | Main hub. Starts travel, battles, exploration, skill tree, ship editor, shop, workshop, shipyard. It also implements `IQuestActionProcessor`, so quest side effects run here. |
| `TravelState`, `RetreatState` | StarMap | Star map | Animate the flight over time, then set the new player position. `TravelState` consumes fuel. `RetreatState` flies to the nearest safe visited star. |
| `DialogState` | StarMap | Star map | Opens one window and runs a callback when it closes. |
| `QuestState` | StarMap | Star map (quest action) | Shows a quest dialog for an `IUserInteraction`. |
| `CombatState` | Combat | Star map, arena, quests, star content | Runs combat for an `ICombatModel`, applies the reward, invokes the completion callback, then loads the reward window or the star map. |
| `CombatRewardState` | Combat | Combat | Shows the reward window, then loads the star map. |
| `QuickCombatState` | Combat or CoreCombat | Main menu | Builds random or test fleets from the database. `@` in the test ship ID loads `CoreCombatScene`. |
| `ExplorationState` | Exploration | Star map | Planet exploration. |
| `SkillTreeState` | SkillTree | Star map | Player skill tree. |
| `ShipEditorState` | ShipEditor | Main menu, star map | Ship editor with either a player-inventory context or a database context. |
| `EhopediaState` | Ehopedia | Main menu, star map | In-game encyclopedia. |
| `TestingState` | ConfigureControls | Main menu | Control configuration test range. |
| `DailyRewardState` | MainMenu | Main menu | Collects and shows the daily reward. |
| `ModalDialogState` | caller-defined | Main menu (settings) | Pauses the application and shows a modal window. |
| `AnnouncementState` | MainMenu | never | Dead. No code creates it, and its window prefab does not exist. |
| `EditorInitializationState` | CommonGui | never | Dead. Older standalone ship-editor entry point. |

## Session Lifecycle

`SessionData` (in the [GameSession module](game_session.md)) fires two signals after a save loads or a new game starts:

1. `SessionDataLoadedSignal`: services read their state from the session. Other services may not be ready yet.
2. `SessionCreatedSignal`: services may call each other.

Most stateful services derive from `GameServices/GameServiceBase`, which subscribes to both signals in its constructor. A service receives the signals only if the container created it before the session loaded. `GameServiceBase` implements `IInitializable`, so `BindInterfacesAndSelfTo` makes Zenject create these services at startup. Services bound with plain `Bind` are created on first use and work only when another eager service depends on them.

Some services keep their own copy of the data and write it back only when `SessionAboutToSaveSignal` fires. `PlayerFleet` is the main example: it holds `IShip` objects and serializes them into the session on that signal.

`GameServices/DataManager/GameDataManager` (a `MonoBehaviour` created by the container) decides when to save:

- When `StarMapScene` loads.
- When the application pauses or loses focus.
- When an in-app purchase completes.
- When the component is disabled (application exit).

After every scene load it also calls `Resources.UnloadUnusedAssets()` and a forced `GC.Collect()`. It also handles mod switching, cloud saves, and save export/import.

## Signals and Messenger

The application layer uses two event systems at the same time.

- **Signals** are typed classes from `CommonComponents.Signals` (`Signal<T>`, `SmartWeakSignal<T>`). Each signal and its nested `Trigger` are bound as singletons in an installer with `BindSignal` and `BindTrigger`. Subscribers add handlers in their constructors and usually never remove them. `SmartWeakSignal` holds subscribers through weak references, so an object stops receiving events only after the garbage collector frees it.
- **Messenger** (`Services.Messenger`) is an enum-keyed event bus. `EventType` is a global enum with about 56 values. Each scene binds its own `Messenger`, which removes that scene's listeners when the scene unloads. The shared store is `MessengerContext`.

`Services/Messenger/SignalsTranslator.cs` subscribes to about 25 signals and re-broadcasts them as messenger events. Older UI listens to the messenger; newer code listens to signals. `AotProblemsWorkaround.cs` is never called; it exists only so that IL2CPP generates code for generic messenger listeners.

`ExitSignal` is shared by many states. Its meaning depends on the active state: leave combat, leave exploration, close Ehopedia, close the skill tree, return from the controls test, or quit from the main menu. `CombatManager` fires it to end a battle.

## Domain

`Domain/` contains campaign rules. Namespaces do not follow the folder names (see the table).

| Folder | Namespaces | Contents |
| --- | --- | --- |
| `Player/` | `GameServices.Player`, `Domain.Player`, `Domain.Shipyard`, `Constructor.Ships` | `PlayerFleet` (ships, hangar groups, exploration ship), `PlayerResources` (money, stars, tokens, fuel, quest resources), `PlayerSkills` (skill levels and derived bonuses), `PlayerInventory` (components, satellites), `MotherShip` (position, view mode, fuel and range rules), `DailyReward`, `StarMapManager` (arrival events, scanner), `ShipValidator`, `ShipSquad`. |
| `Galaxy/` | `Galaxy`, `Galaxy.StarContent` | `StarData` (facade for a star's state and content), `StarMap` (distances, visible stars, map filters), `Star` (value-type facade), `StarObjectType`. `StarContent/` has one service per content type: occupants, boss, ruins, challenge, local event, survival, wormhole, starbase, Christmas tree, hive. |
| `Economy/` | `Economy.ItemType`, `Economy.Products`, `GameServices.Economy` | `IItemType` implementations (currency, component, ship, blueprint, map, IAP, and others), `ItemTypeFactory`, `IProduct`/`CommonProduct` with price and quantity providers, `ProductFactory`, `LootGenerator`. |
| `Exploration/` | `Game.Exploration` | `Planet` (procedural planet), `ExplorationData` (objectives, loot, enemies), random outpost, turret, and enemy builders. |
| `Quests/` | `Domain.Quests`, `GameServices.Quests` | Adapters that implement the [Quests module](quests.md) context interfaces over the session and star map. `QuestCombatModelFacctory` builds quest battles. |
| `Research/` | `GameServices.Research` | `Research`: research points per faction and researched technologies. |
| `Technologies/` | `GameServices.Database`, `DataModel.Technology` | `Technologies` and `ITechnology` wrappers over database technologies. |
| `Skills/` | `GameServices.Database` | `Skills`: loads the skill tree from `Resources/skills.json`. |

Core domain types also live in `Legacy/`: `IFleet` and its implementations, the `Fleet` factory, `RegionMap` and `Region`, `IInventory` and `InventoryFactory`, `IReward`, `Experience`, and `SkillType`.

### Galaxy model

Star positions, levels, and region layout are deterministic functions of the star ID and the game seed (`StarLayout` and `RegionLayout` in the [CommonComponents module](common_components.md)). The session stores only what changed: visited stars, occupant state, bookmarks, defeated bosses, captured bases.

`StarData.GetObjects` decides what a star contains. It rolls a number from 0 to 999 with `IRandom.RandomInt(starId, 1000)` and maps value ranges to content types. Eighteen stars with IDs below 24 get a fixed, shuffled set of content. These rules are in code, not in the database.

### Combat entry

Star content services, quests, and the arena build an `ICombatModel` with `CombatModelBuilder` (`Combat/Domain/`). Then they fire `StartBattleSignal` with the model and an `Action<ICombatModel>` callback. `StarMapState` handles the signal and loads `CombatState`. When combat ends, `CombatState` computes and applies the reward, then invokes the callback. The callback exists only in memory.

## Combat Glue (`Combat/`)

The combat simulation is in the [BattleSimulator module](battle_simulator.md). This folder contains game-specific glue:

- `Domain/`: `CombatModel`, `CombatModelBuilder`, `FleetModel`, `ShipInfo`, `CombatReward` (experience and loot after combat).
- `Manager/CombatManager`: spawns asteroids and the planet, brings in the next ship when one dies, handles surrender, ship change, and "call next enemy", and fires `ExitSignal` when one side has no ships left.
- `Manager/CombatTimer`: time-limit rules.
- `Manager/ExplorationSceneManager`, `EhopediaSceneManager`: scene logic for exploration and Ehopedia previews.
- `Background/`, `Camera/`, `Services/ResourcePreloader`, `Unit/Object/EnemySpawner`.
- `Integration/`: the contracts that the combat HUD reads and commands (`IRosterView`, `IBattleView`, `IShipViews`, `IShipControlSource`, `ICombatCommands`, `IScenePresentation`) and `CombatExperienceRecorder`. `Legacy/` implements them over `BattleSimulator`; `Core/` over the new combat core ([Combat simulation: Unity integration](combat_simulation.md#unity-integration)).

## Game Services (`GameServices/`)

- `DataManager/`: `GameDataManager` (see [Session lifecycle](#session-lifecycle)) and a stub for test scenes.
- `Gui/`: `GuiHelper` (shortcuts to open common windows) and `NotificationManager` (shows messages for IAP and cloud events).
- `OfflineMultiplayer/`: the arena client. `OfflineMultiplayer` talks to the game server through `Security.RequestFactory` and UniRx observables. `PlayerInfo` and `EnemyPlayerInfo` hold ratings and fleets. `FleetWebSerializer` encodes fleets as zlib-compressed, URL-safe Base64 (format versions 1–4). `ArenaTimeManager` limits battles to 10 per recovery window.
- `Random/RandomGenerator`: seed-based random numbers for deterministic galaxy content.
- `HolidayManager`: detects Christmas, Halloween, and Easter from internet time and starts holiday quests.
- `Audio/DatabaseMusicPlaylist`: chooses music from the database playlists.

## Services (`Services/`)

- `InternetTime/InternetTimeService`: requests UTC time from `worldtimeapi.org` every 30 minutes. Daily rewards and holidays use it.
- `InternetTime/GameTime`: counts play time from `DateTime.Now` while a gameplay state is active.
- `Messenger/`: `SignalsTranslator` and `AotProblemsWorkaround`.
- `RateGame/`: store links for rating the game.
- `Unity/ShaderTimeProvider`: sets the global shader property `_UnscaledTime`.
- `Diagnostics/FreezeDetector`: dead.

## UI

The game UI uses uGUI with `UnityEngine.UI.Text`. TextMeshPro and UI Toolkit are not used in this folder. Windows are `IWindow` objects placed in scenes or prefabs. `GuiManager` (in `ModulesShared/Gui`) finds them by string ID. Code opens a window with `IGuiManager.OpenWindow(id, new WindowArgs(...))`. `WindowArgs` is an untyped `object[]`; the window reads the arguments by position with `Get<T>(index)`. The `WindowNames` classes in each `Gui/` subfolder hold the IDs.

UI components are `MonoBehaviour` classes that receive services through `[Inject]` fields or methods. They read and change domain services directly. Older components listen to the messenger; newer ones listen to signals.

| Folder | Contents |
| --- | --- |
| `Gui/MainMenu/` | Main menu and settings tabs (general, combat, account, database/mods, load/save, progress, purchases, key bindings). |
| `Gui/StarMap/` | Star-map game menu, cargo hold, IAP store, arena window, military base, hangar and ship management. |
| `Gui/Combat/` | Combat HUD: ship controls, stats panels, radar, ship list and selection, timer, combat menu, reward window. |
| `Gui/ShipService/` | Shipyard: layout, painting, upgrades. |
| `Gui/Craft/` | Workshop crafting and ship modifications. |
| `Gui/Exploration/` | Planet panel, scanning, loot, fleet selection. |
| `Gui/Quests/` | Quest event dialog parts. |
| `Gui/Dialogs/` | Confirmation, purchase confirmation, message, daily reward, loading, saving, sign-in. |
| `Gui/Ehopedia/` | Encyclopedia windows. |
| `Gui/Controls/`, `Utils/`, `Actors/`, `Behaviours/` | Shared widgets and helpers. |
| `Legacy/GUI/ViewModel/` | Older panels, mostly still live: star-map panels (status, faction, boss, ruins, survival, wormhole, black market, research tree), skill tree, market dialog, quest dialog parts, control setup, dev console. |
| `Legacy/Map/` | Star map rendering: `GalaxyMap`, `Star`, `StarSystem`, `OrbitalObjects`, map scaling and navigation, player ship icon. |

## Legacy Folder

`Legacy/` holds about 40% of the folder's code. It is a mix of live and dead code, not a frozen archive.

**Live code:**

- `Map/`: all star map rendering in `StarMapScene` and the prefabs in `Modules/GalaxyMap/Prefabs/`.
- `GUI/ViewModel/`, `GUI/Controls/`, `GUI/Helpers/`, `GUI/Layout/`: many star-map, skill-tree, combat, and dialog panels.
- `GUI/LoadingScreen.cs`, `GUI/Background.cs`, `Resources/SharedResources.cs`.
- `Game/Galaxy/` (`RegionMap`, `Region`), `Game/Quests/Inventory/` (all market and store inventories), `Game/Skills/` (`SkillType`, `Experience`), `Game/Quests/IReward.cs`, `Game/CombatResult.cs`.
- `Model/` (`IFleet`, fleet types, `Fleet` factory, regulations).
- `Debug/Cheats.cs`, `Debug/DatabaseCodesProcessor.cs`: dev-console commands.
- `Controller/Manager.cs`: controls test range. `Combat/Settings.cs`: combat sound settings.

**Dead code** (no reference from code, scenes, or prefabs; verified by type name and script GUID):

- `GameObjects/`: all files except `Square.cs` are fully commented out.
- `Common/`: everything except `RectTransformHelpers.cs`. This includes `TouchInput.cs`, which uses the old `UnityEngine.Input` API; the project enables only the new Input System, so that code would throw if it ran.
- `Game/Quests/Builder/LocalQuestBuilder.cs` (commented out), `Game/Skills/EventListener.cs`, `Game/Skills/ISkill.cs`, `Game/Upgrades/UpgradeType.cs`.
- `GUI/Console.cs`, `GUI/Helpers/CanvasGroupBlink.cs`, `GUI/Helpers/CloseOnEscape.cs`, `GUI/Input/` (both files).
- `GUI/ViewModel/CargoHold/` (all files), `Common/PanelControllerAutoClose.cs`, `Craft/RequiredResources.cs`, `Craft/RequirementsPanel.cs`, `Craft/ResourceItem.cs`, `Notifications/NotificationsPanel.cs`, `Quests/ShipsPanel.cs`.
- `GUI/ViewModel/StarMap/`: `ActiveShipViewModel`, `CompanionItemViewModel`, `CompartmentViewModel`, `ControlPanelViewModel`, `GameButtonsViewModel`, `MilitaryPanelViewModel`, `ModuleButtonViewModel`, `PowerUpsPanelViewModel`, `ShipPanelViewModel`, `StatisticsViewModel`.
- `Model/Regulations/Companions.cs`, `Unity/CameraRotation.cs`.
- `Game/Config.cs` is attached in `LoaderScene` and bound in `GameInstaller`, but nothing injects it.

Dead code outside `Legacy/`: `AnnouncementState` and `Gui/Dialogs/AnnouncementWindow.cs`, `EditorInitializationState`, `Gui/Dialogs/DebugLogWindow.cs` (the window object exists, but this script is not attached), `Gui/Common/ProductItem.cs`, `Combat/Domain/ShipBuilderExtensions.cs`, `Services/Diagnostics/FreezeDetector.cs`, and the empty `Services/Discord/` folder. The `EDITOR_MODE` symbol is never defined, so its branches never compile. `CombatManager.IsGamePaused` is always false.

In total, about 70 files and 3,900 lines can be deleted without changing behavior.

## Conditional Compilation

| Symbol | Effect |
| --- | --- |
| `LICENSE_OPENSOURCE` / `LICENSE_COMMERCIAL` | Open-source builds bind stub account, storage, ads, and IAP services. Commercial builds bind the implementations from `Assets/ModulesPrivate/`. |
| `IAP_DISABLED`, `IAP_UNITY` | Turns IAP off or on. `IAP_DISABLED` also changes gameplay and UI in about 18 places (for example, military stars and the star-currency panel). |
| `ADS_DISABLED`, `ADS_UNITY`, `ADS_GOOGLE`, `ADS_APPODEAL` | Selects the ads manager. |
| `NO_GPGS`, `NO_INTERNET` | Turns off Google Play Games and online services. |
| `UNITY_EDITOR` | Also changes gameplay: local events use a random seed and are never marked complete; quick combat accepts test ship IDs; some cheats are available. |
| `EDITOR_MODE` | Never defined. Dead. |

The defines for each platform are in `ProjectSettings/ProjectSettings.asset` (`scriptingDefineSymbols`).

## Where New Work Goes

- New game states: `GameStateMachine/States/`, plus the bindings in `GameInstaller.BindStateMachine` and a method in `GameStateFactory`.
- New campaign rules: `Domain/`. Prefer signals over the messenger. Read tunable values from the database, not from constants.
- New windows: `Gui/<Area>/`, with the ID in that area's `WindowNames`.
- Do not add code to `Legacy/`. When you change a live `Legacy/` file, consider moving it to its owning folder.
