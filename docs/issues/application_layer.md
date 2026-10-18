# Application Layer Issues

This list records architectural problems in `Assets/Scripts/` for future refactoring. It does not list local bugs. A bug appears only when it shows the effect of a structural problem. For a description of the current design, see [Application layer](../application_layer.md).

Each issue has four parts: the problem, the evidence, the effect, and a recommended direction. The order is approximate priority: issues near the top block or simplify the issues below them.

## 1. No assembly boundaries in the application layer

**Problem.** All 474 files compile into `Assembly-CSharp`. Folder names suggest layers (Domain, GameServices, Gui, GameStateMachine), but the compiler does not enforce them.

**Evidence.**
- Domain depends on the state machine: seven `Domain/Galaxy/StarContent/*` services use `GameStateMachine.States` to fire `StartBattleSignal`.
- Domain depends on UI: item types in `Domain/Economy/ItemType/` read the static `Gui.Theme.UiTheme.Current`. `XmasBoxItem` uses `GameServices.Gui`.
- 35 of 76 `Domain/` files import `UnityEngine`.
- Test assemblies cannot reference `Assembly-CSharp`. No campaign rule (fuel, loot, star content, research, daily reward, arena timer) has an automated test.

**Effect.** Every change recompiles the whole layer. Hidden dependencies grow without review. Domain rules cannot be tested outside Play Mode.

**Direction.** Add `.asmdef` files in dependency order: Domain (no Unity UI, no state machine), then game services, then the state machine, then UI. Start with `Domain/` and move the types it needs out of `Legacy/` (see issue 2). Replace `UiTheme.Current` in domain code with data (currency or quality IDs) that the UI maps to colors.

## 2. `Legacy/` holds live core code, and dead code is mixed in

**Problem.** `Legacy/` holds 40% of the code (210 files, 15,100 lines). The name says "old", but about 150 files are live.

**Evidence.**
- 111 `Legacy/` scripts are attached to scenes or prefabs. Star map rendering (`Legacy/Map/`), the skill tree, the research tree, the market dialog, and the quest dialogs run on them.
- Core domain types live there: `IFleet` and all fleet types, the `Fleet` factory, `RegionMap`, `Region`, `IInventory`, `InventoryFactory`, `IReward`, `Experience`, `SkillType`. `Domain/`, `Combat/`, and `GameStateMachine/` depend on them.
- About 60 files in this folder have no reference from code, scenes, or prefabs. With the dead files outside it, the total is about 70 files and 3,900 lines. 21 files are fully commented out. The [Legacy folder](../application_layer.md#legacy-folder) section has the full list.
- `Legacy/Common/TouchInput.cs` and `Legacy/GUI/Input/` use the old `UnityEngine.Input` API, but the project enables only the new Input System. That code would throw if anything called it.

**Effect.** Nobody can tell from the folder name whether a file is safe to change or delete. Refactoring plans based on the folder name will be wrong.

**Direction.**
1. Delete the dead files listed in the doc. Remove `GameModel.Config` from `LoaderScene` and `GameInstaller`.
2. Move live domain types (`Model/`, `Game/Galaxy/`, `Game/Quests/Inventory/`, `Game/Skills/`) into `Domain/`.
3. Move live UI into `Gui/`, and star map rendering into the `GalaxyMap` module, which today contains only prefabs.
4. Delete the `Legacy/` folder when it is empty.

## 3. Two event systems and a bridge between them

**Problem.** Code communicates through two event systems with different rules, plus plain C# events and UniRx streams.

**Evidence.**
- Signal classes (`Signal<T>`, `SmartWeakSignal<T>`) are bound one by one with `BindSignal` and `BindTrigger` in `GameInstaller`, `ServicesInstaller`, and `TestSceneInstaller`: 86 registrations in about 175 lines.
- The messenger has a global `EventType` enum with about 56 values. Scripts contain about 113 `AddListener` and 52 `Broadcast` calls for it.
- `Services/Messenger/SignalsTranslator.cs` subscribes to about 25 signals only to re-broadcast them as messenger events.
- `AotProblemsWorkaround.cs` must list every generic messenger payload type so that IL2CPP builds work.
- Messenger payloads are not checked at compile time. A listener with the wrong `T` for an `EventType` fails only at run time.

**Effect.** One fact (for example, money changed) travels through two systems. A developer must know which system each listener uses. Event flow cannot be traced with "find references".

**Direction.** Pick one typed system: Zenject's built-in `SignalBus`, or the existing `Signal<T>` types with a single registration helper. Migrate messenger listeners one `EventType` at a time, and remove each translation from `SignalsTranslator` as you go. Delete the messenger, `EventType`, and the AOT workaround at the end.

## 4. Subscription lifetime depends on the garbage collector

**Problem.** Subscribers add handlers in constructors and never remove them. `SmartWeakSignal` keeps subscribers only through weak references. An object leaves the event list when the garbage collector frees it, not when its owner stops using it.

**Evidence.**
- Game states are transient objects. `MainMenuState`, `StarMapState`, `CombatState`, `ExplorationState`, `SkillTreeState`, `EhopediaState`, `TestingState`, `DailyRewardState`, and `QuickCombatState` all subscribe to the same `ExitSignal`. After a state leaves the stack, it still receives signals until the next collection.
- Correct behavior depends on the `Condition != Active` guard inside `BaseState.LoadState` and `Unload`, and on manual guards in many handlers.
- `GameDataManager.Cleanup` runs `GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced)` and `Resources.UnloadUnusedAssets()` after every scene load. This causes a frame hitch, and it also makes subscriber cleanup timing depend on scene changes.

**Effect.** Stale handlers run in an order that is hard to predict. A new handler without a guard can act on an old state. Profiling and memory behavior depend on forced collections.

**Direction.** Give states and services an explicit lifetime. Implement `IDisposable` and unsubscribe in `Dispose`, or use `SignalBus` subscriptions tied to a container. Call `Dispose` on a state when the machine sets it to `NotLoaded`. After stale subscribers are gone, remove the forced `GC.Collect`.

## 5. Game flow uses an ambiguous global `ExitSignal` and in-memory callbacks

**Problem.** The meaning of state transitions is implicit. One signal means different things in different states. Combat results travel through closures.

**Evidence.**
- `CombatManager.Exit` fires `ExitSignal` to end a battle. `CombatState` treats it as "combat completed". `MainMenuState` treats it as "quit the application". `SkillTreeState` treats it as "return to the star map".
- `StartBattleSignal` carries an `Action<ICombatModel>` callback from the code that started the battle (star content, quests, arena) into `CombatState`. The callback does not exist in the save file.
- `CombatState.OnCombatCompleted` calls `LoadState` twice (reward window, then star map). The second call works only because the state is no longer active, so the guard ignores it.
- Quest side effects (`IQuestActionProcessor`) are implemented by `StarMapState`, a game state with 26 constructor parameters.

**Effect.** A new state that subscribes to `ExitSignal` can react to exits that belong to other states. If the process ends during a battle (common on mobile), the code that started the battle never learns the result. Transitions cannot be traced from one place.

**Direction.**
1. Replace `ExitSignal` with specific signals or with direct return values from the state (for example, `CombatFinished(result)`).
2. Represent a battle request as data: a type and an ID such as "starbase 123" or "quest 45, node 7". A handler service then applies the result. This data can be saved, so an unfinished battle can be settled after a restart.
3. Move `IQuestActionProcessor` out of `StarMapState` into its own service.

## 6. Service initialization order is implicit

**Problem.** Stateful services start through the `SessionDataLoadedSignal` and `SessionCreatedSignal` signals. The design works only if the container creates every service before the first session loads.

**Evidence.**
- `GameServiceBase` implements `IInitializable` only to force eager creation through `BindInterfacesAndSelfTo`. Its `Initialize` method body is commented out.
- Services bound with plain `Bind` (for example, `HolidayManager`) work only because an eager service happens to depend on them.
- Handlers in each phase run in subscription order, which is construction order.
- Circular dependencies are resolved with `[Inject]` fields: `PlayerFleet` ↔ `PlayerSkills`, and `StarData` ↔ `StarMap` ↔ star content services. Scripts contain 601 `[Inject] private` fields.

**Effect.** If you add a service or change one binding, a service can miss the session-loaded event or run before the data it needs. These failures show only at run time and only in some start paths.

**Direction.** Add one session-lifecycle coordinator that calls an `ISessionListener` interface on registered services in a defined order. Use constructor injection for required dependencies. Break the cycles by extracting the shared data (for example, skill levels) into a separate read-only service.

## 7. Two sources of truth for persistent data

**Problem.** Some services copy session data into their own objects. They write the copy back only when `SessionAboutToSaveSignal` fires. Every save path must remember to fire that signal first.

**Evidence.**
- `PlayerFleet` keeps `IShip` objects and calls `_session.Fleet.UpdateShips` only in `OnSessionAboutToSave`. `GameTime` writes play time the same way.
- `GameDataManager.SaveGameToCloud(string)` calls `_localStorage.Save` and `_cloudStorage.Save` without firing the signal. The `ISavedGame` overload fires it. The first path can save stale fleet data.
- Saving happens in several places: `StarMapScene` load, pause, focus loss, IAP completion, `OnDisable`, mod change, cloud save.

**Effect.** Data loss depends on which save path runs. Reviews cannot verify persistence locally.

**Direction.** Make the session model the only state, and have services work directly on `ISessionData` facades. If a cache is needed, flush it inside the storage layer before every serialization, not through a signal that each caller must fire. Put save points into one save-policy service.

## 8. UI talks to domain services directly and passes untyped arguments

**Problem.** UI `MonoBehaviour` classes contain view logic and domain calls. There are two UI generations with different styles.

**Evidence.**
- Window arguments are `WindowArgs(params object[])`. Windows read them by position with `Get<T>(index)`. A wrong order or type fails at run time with an `InvalidCastException`.
- Window IDs are strings in seven `WindowNames` classes. `GuiManager` finds windows by name in loaded scenes.
- Panels inject domain services and change them directly (for example, `MainMenu`, `GameMenu`, `MarketDialog`). The `ViewModel` namespace in `Legacy/GUI/ViewModel/` contains views, not view models.
- Older panels listen to the messenger. Newer panels listen to signals. UI state (map filters) is stored in the domain service `Galaxy.StarMap`.
- All text uses `UnityEngine.UI.Text` (about 200 fields). No game UI uses TextMeshPro.

**Effect.** UI behavior cannot be tested without scenes. Changing a window's arguments needs a manual search for all callers. Text quality and localization fonts are limited by the old text component.

**Direction.** Introduce typed window requests (one argument class per window) and a typed window registry. Move domain calls from panels into small presenters that can be tested. When a panel moves, replace `Text` with TextMeshPro, or build new screens with UI Toolkit, which is already in the package list.

## 9. Game rules are in code instead of the database

**Problem.** The project says it is data-driven and supports mods, but many campaign rules are constants or code.

**Evidence.**
- `StarData.GetObjects` hard-codes the value ranges for wormholes, events, survival, arena, bosses, ruins, military bases, challenges, hives, black markets, and Christmas trees. It also hard-codes the fixed content of the first stars.
- `PlayerSkills.AvailableHangarSlots` hard-codes hangar slot counts (3 + skill, 1 + skill, and so on).
- `ArenaTimeManager` hard-codes 10 battles and a 10-minute recovery time.
- `LocalEvent.IsActive` hard-codes the cooldown formula.
- `QuickCombatState.GetUnlockedShips` uses legacy build names `f0s1`–`f7s1`. `Controller.Manager` uses build IDs 226 and 218. `Cheats` uses many build IDs.
- The skill tree comes from `Resources/skills.json` and the layout of `SkillTreeScene`, so mods cannot change it.

**Effect.** Mods cannot change these rules. Balance changes need a new build. Mods that remove the referenced builds break quick combat and the controls test.

**Direction.** Move these values into database settings (`GalaxySettings`, `SkillSettings`, and similar) through the schema and code generator. Replace hard-coded build IDs with database references.

## 10. Platform and license variants are selected with `#if` across the code

**Problem.** Platform, store, license, and feature choices are compile-time symbols spread through installers, gameplay, and UI.

**Evidence.**
- `ServicesInstaller` has eight `#if` chains for the application type, account, storage, cloud, ads, rate-game, secure storage, and the asset loader.
- `IAP_DISABLED` appears about 18 times in gameplay and UI code. For example, it removes military stars from `StarData.GetObjects` and hides the star-currency panel.
- `UNITY_EDITOR` changes gameplay rules. In the editor, `LocalEvent.Start` uses a random seed and never marks the event as complete.
- `EDITOR_MODE` is used but never defined.
- There are four shipped platforms (Android, iOS, Steam, WebGL) and two license variants.

**Effect.** Most combinations compile only on their own build machine. The editor does not behave like any shipped build. Galaxy content differs between builds with and without IAP.

**Direction.** Move platform choices into one installer per platform or a platform-profile asset. Replace gameplay `#if` checks with runtime feature flags from settings or the database. Remove `EDITOR_MODE`, and remove the gameplay differences between the editor and builds.

## 11. Outdated libraries and Unity APIs

**Problem.** The layer depends on libraries and APIs that are obsolete, unmaintained, or replaced.

**Evidence.**
- **Zenject 9.2 (Extenject):** about 20 state factories derive from `Zenject.Factory<T>`, which this version marks `[Obsolete]` in favor of `PlaceholderFactory<T>`. Zenject itself receives few updates.
- **UniRx 6.2.2:** the library is no longer developed. The arena client uses `ObservableWWW`, which wraps the deprecated `UnityEngine.WWW` (`ModulesPrivate/Security/RequestFactory.cs`). `InternetTimeService` polls `UnityWebRequest` with `Observable.EveryUpdate`.
- **Async patterns:** code mixes coroutines (10 files), UniRx streams, and callbacks. It does not use `async`/`await` or Unity's `Awaitable`.
- **Legacy UI text:** `UnityEngine.UI.Text` everywhere (see issue 8).
- **Ionic.Zlib:** `FleetWebSerializer` uses it, but .NET Standard 2.1 has `System.IO.Compression`.
- **Old Input API:** only in dead code (see issue 2).

**Effect.** Each Unity upgrade risks removed APIs. New developers must learn three async styles.

**Direction.** Replace `Factory<T>` with `PlaceholderFactory<T>` (a mechanical change). Move HTTP calls to `UnityWebRequest` with `Awaitable` or UniTask. Remove UniRx after that. Decide whether to stay on Extenject or move to a maintained container before larger DI work (issues 4 and 6).

## 12. Time comes from several unrelated sources

**Problem.** Time-based rules use different clocks.

**Evidence.**
- `InternetTimeService` gets UTC from the free public service `worldtimeapi.org`. Daily rewards and holiday events use it, and `HolidayManager` falls back to local time when it is not available.
- `LocalEvent`, `StartingNewGameState`, `DailyReward`, and `ArenaTimeManager` use `DateTime.UtcNow`.
- `GameTime` counts play time from `DateTime.Now` (local time). `SessionData` counts it from `Time.deltaTime`.

**Effect.** Changing the device clock affects some rules but not others. If the free service fails or changes, daily rewards and events stop, and nothing reports it.

**Direction.** Add one `IClock` service with "trusted UTC" and "local" values. Get trusted time from the game's own server, which the arena already uses, or from the store platform. Use the clock in all time-based rules.

## 13. Startup and mod loading are duplicated procedural code

**Problem.** Database and save loading are written twice, with different fallbacks.

**Evidence.**
- `InitializationState.OnLoad` loads external mods, the active mod or the default database, localization, validates ship builds and technologies, signs in, loads or imports or creates the save, shows an ad, and sets the playlist.
- `GameDataManager.LoadMod` repeats the database-load and save-load sequence with another fallback path.

**Effect.** The two paths can disagree (for example, about validation or localization reload). Startup cannot be tested without the full scene setup.

**Direction.** Extract a `GameBootstrapper` service with explicit steps (load database, validate, load localization, load session). Call it from both places. Run database validation in the database tooling, not at game start.

## 14. Boilerplate in installers and states

**Problem.** Adding a feature needs many manual registrations.

**Evidence.**
- Each game state needs four edits (see [Game state machine](../application_layer.md#game-state-machine)).
- Each signal needs two binding lines. The installers contain 86 of these pairs.
- The combat, exploration, Ehopedia, and controls installers repeat about 20 identical simulation bindings.
- `DialogState` and `QuestState` hard-code `GameScene.StarMap`, so dialogs cannot be reused from other states.

**Effect.** Missed registrations fail at run time. Installers are hard to review.

**Direction.** Move the shared simulation bindings into one reusable installer. Register signals and states by convention (reflection at install time, or a source generator). Let the caller pass the scenes for a dialog state.

## 15. Inconsistent naming and namespaces

**Problem.** Namespaces do not match folders, and type names repeat.

**Evidence.**
- 82 files are in the global namespace, for example `GalaxyMap`, `LoadingScreen`, `Cheats`, and `TouchInput`.
- `Domain/` uses 16 namespaces, including `GameServices.Player`, `Game.Exploration`, `Economy.ItemType`, and `Constructor.Ships`.
- `Legacy/` uses `ViewModel`, `GameModel`, `Model`, and nested `namespace X { namespace Y {` blocks.
- Duplicate class names in different namespaces: `PlayerFleet`, `PlayerInventory`, `Planet`, `StarBase`, `FleetPanel` (3), `ItemsPanel`, `InformationPanel`, `LootPanel`, `CombatMenu`, `CargoHold`, `Background`.
- Misspellings in public API: `QuestCombatModelFacctory`, `DailyRewardAwailableSignal`, `CreateDaylyRewardState`, `CreateEchopediaState`.

**Effect.** Developers need aliases such as `using PlayerFleet = GameServices.Player.PlayerFleet` to avoid collisions. Search results are noisy.

**Direction.** Make namespaces match folders when you move code for issues 1 and 2. Rename the duplicates and the misspelled names at the same time. Unity serializes components by script GUID, so these renames do not break scene references, but `[FormerlySerializedAs]` is needed when a serialized field name changes.
