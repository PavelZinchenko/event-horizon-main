# Services, ServicesFacade, and SecurityStub Modules

These three modules form the platform-service layer. `ServicesFacade` declares the contracts: interfaces, enums, and service signals. `Services` holds the default implementations that every build can use: settings, messenger, object pool, audio, local saves, and stubs for online features. `SecurityStub` replaces the private `Security` assembly in open-source builds. Platform implementations (Google Play, Game Center, Steam, ads, IAP, cloud saves) are in `Assets/ModulesPrivate/`; see [ModulesPrivate](modules_private.md).

| | ServicesFacade | Services | SecurityStub |
| --- | --- | --- | --- |
| Assembly | [ServicesFacade.asmdef](../Assets/Modules/ServicesFacade/ServicesFacade.asmdef) | [Services.asmdef](../Assets/Modules/Services/Services.asmdef) | [SecurityStub.asmdef](../Assets/Modules/SecurityStub/SecurityStub.asmdef) |
| References | CommonComponents, Zenject | AppConfiguration, CommonComponents, ServicesFacade, UniRx, Zenject | UniRx |
| Compiles when | Always | Always | `LICENSE_OPENSOURCE` is defined (`defineConstraints`) |
| Namespaces | `Services.*`, `GameServices.SceneManager`, global `EventType` | `Services.*`, `GameServices.Settings` | `Security`, `Services.InAppPurchasing`, global `StringExtensions` |
| Source | [22 files, 660 lines](../Assets/Modules/ServicesFacade/Scripts/) | [17 files, 1,700 lines](../Assets/Modules/Services/Scripts/) | [8 files, 170 lines](../Assets/Modules/SecurityStub/Scripts/) |
| Known issues | [Services issues](issues/services.md) | | |

The application layer binds all services in [`ServicesInstaller`](../Assets/Scripts/Installers/ServicesInstaller.cs) in `LoaderScene`. The bindings live for the whole session. The GameServices module (application lifecycle, scene manager, input) is described in [GameServices module](game_services.md).

## Contents

1. [Dependency direction](#dependency-direction)
2. [Platform selection](#platform-selection)
3. [Contract catalog](#contract-catalog)
4. [Settings](#settings)
5. [Messenger](#messenger)
6. [Object pool and factory](#object-pool-and-factory)
7. [Audio](#audio)
8. [Asset loader](#asset-loader)
9. [Account](#account)
10. [Save storage](#save-storage)
11. [In-app purchases and IAP storage](#in-app-purchases-and-iap-storage)
12. [Advertisements](#advertisements)
13. [Coroutine manager](#coroutine-manager)
14. [SecurityStub](#securitystub)
15. [Service signals](#service-signals)
16. [Adding a platform service](#adding-a-platform-service)

## Dependency direction

```
Assembly-CSharp (installers, game) ──► ServicesFacade ◄── Services
                │                            ▲               (default impl.)
                │                            └── ServicesPrivate, GameServices,
                │                                ResourceLocator, Gui, ShipEditor, ...
                └──► SecurityStub  (or Security, private)
```

Modules depend on `ServicesFacade` interfaces only. The installer is the single place that chooses implementations. `ServicesFacade` uses `GameDatabase.Model.SpriteId` and `AudioClipId`, which live in CommonComponents, so it does not depend on the Database assembly.

## Platform selection

`ServicesInstaller` picks one implementation per contract with `#if` chains. "Private" means the class is in `Assets/ModulesPrivate/Services/`.

| Contract | Open source (`LICENSE_OPENSOURCE`) | Commercial builds |
| --- | --- | --- |
| `IGameSettings` | `GameSettings` | same |
| `IResourceLocator` | [`ResourceLocator`](resource_locator.md) | same |
| `IApplication` | `WebGlApplication` / `StandaloneApplication` / `MobileApplication` by platform (GameServices) | same |
| `IGameSceneManager` | `GameSceneManager` (GameServices) | same |
| `IMessengerContext` | `MessengerContext` | same |
| `IGameObjectFactory` | `GameObjectFactory` | same |
| `IMusicPlayer`, `ISoundPlayer` | `MusicPlayer`, `SoundPlayer` (scene components) | same |
| `ICoroutineManager` | `CoroutineManager` (new GameObject) | same |
| `IAccount` | `EmptyAccount` | Editor: `EditorModeAccount`. Android (not `NO_GPGS`): `GoogleAccount`. iOS: `GameCenterAccount`. Standalone: `SteamAccount`. Else `EmptyAccount`. |
| `IDataStorage` | `PlayerPrefsStorage` | WebGL: `PlayerPrefsStorage`. Android: `AndroidLocalStorage`. iOS and editor: `ExportableLocalStorage`. macOS: `MacLocalStorage`. Windows: `WindowsLocalStorage`. Else `LocalStorage`. |
| `ISavegameExporter` | `SaveGameExporterStub` | Bound by the storage class when it supports export; otherwise the stub (`IfNotBound`). |
| `ICloudStorage` | `EmptyCloudStorage` | Android (not `NO_GPGS`): `GoogleCloudStorage` + `GooglePlayServices`. iOS: `AppleCloudStorage`. Else `EmptyCloudStorage`. |
| `IAdsManager` | `AdsManagerStub` | `ADS_GOOGLE`: `GoogleAdsManager`. `ADS_UNITY`: `UnityAdsManager`. `ADS_APPODEAL`: `AppodealAdsManager`. `ADS_DISABLED`: stub. |
| `IIapStorage` | `EmptyStorage` | iOS: `KeyChainStorage`. `NO_INTERNET` or `IAP_DISABLED`: `EmptyStorage`. Else `WebStorage`. |
| `IAssetLoader` | `LocalAssetLoader` | `AssetLoader` (downloads the music bundle) |
| `IInAppPurchasing` | `InAppPurchasingStub` (bound in `GameInstaller`) | Unity IAP implementation |
| Rate game | `NullRateGameService` unless iOS/Android | `IosRateGameService` / `AndroidRateGameService` (application layer) |

The editor uses `PlayerPrefsStorage` only in open-source mode. Commercial editor sessions write files through `ExportableLocalStorage`.

## Contract catalog

| Folder in ServicesFacade | Types |
| --- | --- |
| `Account/` | `IAccount`, `Status`, `AccountStatusChangedSignal` |
| `Advetisements/` | `IAdsManager`, `AdStatus`, `RewardedVideoCompletedSignal`, `AdStatusChangedSignal` |
| `Application/` | `IApplication`, `GamePauseCounter`, `GamePausedSignal`, `AppActivatedSignal` |
| `Assets/` | `IAssetLoader`, `Status`, `AssetLoaderStatusChangedSignal` |
| `Audio/` | `IMusicPlayer`, `AudioTrackType`, `ISoundPlayer`, `IMusicPlaylist`, `MusicPlaylist` (ScriptableObject), `MusicPlayerStub`, `SoundPlayerStub` |
| `IapStorage/` | `IIapStorage`, `IapData`, `IapDataSavedSignal` |
| `InAppPurchasing/` | `IInAppPurchasing`, `IIapItem`, `IapException`, purchase signals |
| `Legacy/` | `ICoroutineManager` |
| `Messenger/` | `EventType`, `IMessenger`, `IMessengerContext`, `Callback` delegates |
| `ObjectPool/` | `IObjectPool`, `IGameObjectFactory` |
| `ResourceLocator/` | `IResourceLocator` |
| `SceneManager/` | `GameScene`, `GameSceneExtension`, `IGameSceneManager`, `State`, scene signals |
| `Settings/` | `IGameSettings` |
| `Storage/` | `IDataStorage`, `ICloudStorage`, `ISavedGame`, `CloudStorageStatus`, `ISavegameExporter`, cloud signals |

`ISerializableGameData` (the save payload) is in CommonComponents: [ISerializableGameData.cs](../Assets/Modules/CommonComponents/Scripts/Services/ISerializableGameData.cs).

### Scene names

[`GameScene`](../Assets/Modules/ServicesFacade/Scripts/SceneManager/SceneName.cs) lists the scenes the state machine can load. `ToSceneName()` maps each value to a Unity scene name; `ToGameScene()` maps back. `GameScene.Battle` maps to `BattleScene`, which does not exist (see [S9](issues/services.md#s9-gamescenebattle-points-to-a-missing-scene)). For the scene list, see [Scenes](scenes.md).

## Settings

[`GameSettings`](../Assets/Modules/Services/Scripts/Settings/GameSettings.cs) implements `IGameSettings` on top of `PlayerPrefs`. It is bound `AsSingle().NonLazy()` with its concrete type too, because some code injects `GameSettings` directly (for example `SoundPlayer` and `Background`).

Each property wraps a small struct (`StringDataItem`, `IntDataItem`, `FloatDataItem`, `BoolDataItem`) that reads and writes one key. Writing the default value deletes the key, except for `bool` items. `ExternalMods` is a `FileList` (an `IList<string>` stored as one newline-separated string).

| Property | Key | Default | Meaning |
| --- | --- | --- | --- |
| `MusicVolume` | `music_volume` | 1.0 | Music volume. |
| `SoundVolume` | `sound_volume` | 0.5 | Sound effects volume. |
| `CameraZoom` | `camera_zoom` | 0 | Combat camera zoom. |
| `Language` | `language` | "" | Selected language. |
| `RateButtonClicked` | `settings` | false | The player used the rate-game button. |
| `SignedIn` | `signedin` | true | Sign in automatically at start. |
| `ControlsLayout` | `controlsLayout` | "" | Touch control layout. |
| `SlideToMove` | `slideToMove` | false | Touch movement mode. |
| `ThrustWidthJoystick` | `joystickThrust` | false | Thrust from the virtual joystick. |
| `StopWhenWeaponActive` | `stopOnFire` | false | Stop thrust while firing. |
| `QualityMode` | `graphicQuality` | 1 | Graphics quality level. |
| `RunInBackground` | `runInBackground` | false | Keep the game running without focus (standalone). |
| `AppStartCounter` | `startCounter` | 0 | Incremented in `Initialize()` at every start. |
| `CenterOnPlayer` | `centerOnPlayer` | false | Combat camera follows the player ship. |
| `ShowDamage` | `showDamage` | true | Show damage numbers. |
| `ShowEnemyMessages` | `showDamage` (bug, should be `enemyComms`) | true | Show enemy chat messages. See [S1](issues/services.md#s1-showenemymessages-is-stored-in-the-showdamage-key). |
| `AutoSave` | `autosave` | false | Periodic saves. |
| `EditorText` | none (`null`) | "" | Created with the parameterless struct constructor, so it has no key. Nothing reads or writes it. |
| `ActiveMod` | `mod` | "" | Selected mod. |
| `LastFacebookPostDate` | `fpost` | 0 | Leftover of a removed sharing feature. |
| `LastDailyRewardDate` | `rdata` | 0 | Day of the last daily reward. |
| `DontAskAgainId` | `dontask` | 0 | Suppressed confirmation dialog. |
| `KeyBindings` | `keyBindings` | "" | JSON of [`Input.Configuration`](game_services.md#input). |
| `UseMouse` | `mouse` | true | Mouse steering in combat. |
| `ExternalMods` | `externalMods` | empty | File paths of mods added from disk. |
| `UniqueId` (class only) | `guid` | new GUID | Per-install identifier. |

`PlayerPrefs.Save()` runs when the game pauses (`GamePausedSignal` with `true`) and when the container is disposed.

## Messenger

The messenger is the older, enum-keyed event bus. New code should use signals; see [Application layer: Signals and messenger](application_layer.md#signals-and-messenger).

- [`MessengerContext`](../Assets/Modules/Services/Scripts/Messenger/MessengerContext.cs) is a root singleton. For every `EventType` it keeps a `SortedList<GameScene, Delegate>`: one combined delegate per scene.
- [`Messenger`](../Assets/Modules/Services/Scripts/Messenger/Messenger.cs) is bound in each scene installer with that scene's `GameScene` value. `AddListener` adds the handler under its scene. `Broadcast` forwards to the context, which calls the delegates of all scenes in `GameScene` order.
- On `SceneBeforeUnloadSignal` for its scene, a `Messenger` removes all handlers of that scene and disables itself. A later `AddListener` throws `InvalidOperationException`.
- `RemoveListener` throws `NotImplementedException`. Handlers live until their scene unloads.
- Handler types must match exactly: a listener added as `Callback<int>` and a broadcast with `Broadcast<long>` throws `InvalidCastException`.

`EventType` has 56 values. 15 of them are never broadcast: `ConstructorShipChanged`, `GameSettingsChanged`, `WindowOpened`, `WindowClosed`, `AchievementUnlocked`, `CraftStarted`, `CraftFinished`, `CommonResourcesChanged`, `UpgradeLevelChanged`, `ActiveShipsChanged`, `ShowShipControls`, `GamePaused`, `LocalizationChanged`, `GuardianDefeated`, and `GraphicsQualityChanged`. `CloudDataLoaded` and `PlayerShipCountChanged` are broadcast but have no listener. Two of the unsent events have live listeners; see [S6](issues/services.md#s6-two-messenger-events-are-never-sent).

## Object pool and factory

- [`GameObjectFactory`](../Assets/Modules/Services/Scripts/ObjectPool/GameObjectFactory.cs) instantiates a prefab through the Zenject container (`InstantiatePrefab`), so `[Inject]` members are filled. It then detaches the object from its parent. `CreateEmpty` creates the pool's parent object under the container's default parent.
- [`GameObjectPool`](../Assets/Modules/Services/Scripts/ObjectPool/GameObjectPool.cs) implements `IObjectPool`. The star map, combat, exploration, Ehopedia, and controls-test installers bind one per scene. It keeps unused instances per prefab in a `HashSet` and the prefab of every issued object in a dictionary.
  - `GetObject(prefab)` reuses an unused instance or creates one (with or without injection), then moves it to the scene root.
  - `ReleaseObject` deactivates the object and parents it under the pool. Unknown objects are destroyed.
  - `PreloadObjects` creates inactive instances up to a count.
  - `Tick` removes destroyed objects from the prefab map every 10 seconds.
  - `Dispose` (scene unload) sends `OnDestroy` to pooled objects and destroys them.

## Audio

### Music

[`MusicPlayer`](../Assets/Modules/Services/Scripts/Audio/MusicPlayer.cs) is a `MonoBehaviour` in `LoaderScene` with one `AudioSource`.

1. At injection it reads `MusicVolume`. If the volume is above zero, it asks `IAssetLoader.LoadMusicBundle` for the default playlist.
2. `Play(track)` only records the requested `AudioTrackType` (`Menu`, `Game`, `Combat`, `Exploration`). `Update` starts a new clip whenever the source is idle: it takes a random clip of that type from the custom playlist, or else from the default playlist.
3. `Playlist = value` sets the custom playlist and restarts the current track. The application layer sets it from database playlists through [`DatabaseMusicPlaylist`](../Assets/Scripts/GameServices/Audio/DatabaseMusicPlaylist.cs).
4. `AppActivatedSignal(false)` mutes the source.

The default playlist is the `MusicPlaylist` asset [`Playlist.asset`](../Assets/BundledAssets/MusicBundle/Playlist.asset) in the `musicbundle` asset bundle.

### Sound effects

[`SoundPlayer`](../Assets/Modules/Services/Scripts/Audio/SoundPlayer.cs) is a `MonoBehaviour` in `LoaderScene` with several `AudioSource` components.

- `Play(clip, soundId, loop)` and `Play(AudioClipId, soundId)` add a request to a locked queue. A clip already queued three times is dropped. `Stop(soundId)` queues a stop request.
- `Update` drains the queue. A clip that is still loading is retried every 0.2 s, up to 10 times. A request with a non-zero `soundId` reuses the source that already plays that ID, so looping sounds (engines, beams) can be stopped later. Otherwise it takes an idle source, or steals the non-looping source closest to its end.
- Pause and resume follow `AppActivatedSignal`.

## Asset loader

`IAssetLoader.LoadMusicBundle` provides the default music playlist.

- [`LocalAssetLoader`](../Assets/Modules/Services/Scripts/Assets/LocalAssetLoader.cs) (open source) loads `StreamingAssets/musicbundle` with `AssetBundle.LoadFromFile` and the `Playlist` asset from it. On failure it logs an error and never calls back.
- The bundle is built with **Assets > Build AssetBundles** ([CreateAssetBundles.cs](../Assets/Editor/CreateAssetBundles.cs)) into `Assets/StreamingAssets/`, which Git ignores. A fresh clone has no music until someone builds the bundle (see [S4](issues/services.md#s4-open-source-builds-have-no-music-until-the-bundle-is-built)).
- The private `AssetLoader` downloads the bundle from the game server on WebGL, Android, and iOS (`.server/assetbundles/`, see [Server](server.md#asset-bundles)). On standalone it loads `StreamingAssets/musicbundle`, like `LocalAssetLoader`. See [ModulesPrivate: Music bundle download](modules_private.md#music-bundle-download).

## Account

`IAccount` is the platform sign-in: `SignIn`, `SignOut`, `Status`, `DisplayName`, `Id`, `LoadUserIcon`. Status changes fire `AccountStatusChangedSignal`.

- [`EmptyAccount`](../Assets/Modules/Services/Scripts/Account/EmptyAccount.cs): always `NotConnected`; cannot sign in.
- [`EditorModeAccount`](../Assets/Modules/Services/Scripts/Account/EditorModeAccount.cs): connects after one second with the name "Cheater" and avatar `M_07`. Used in the commercial editor.

The arena uses `IAccount.Id` as the player ID.

## Save storage

`IDataStorage` stores one `ISerializableGameData` per mod. `ICloudStorage` lists and transfers cloud copies. `ISavegameExporter` exports and imports a save file. The [GameSession module](game_session.md) and [`GameDataManager`](application_layer.md#session-lifecycle) drive them. Formats are described in [GameSession: Storage](game_session/storage.md).

[`PlayerPrefsStorage`](../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs) is the open-source and WebGL backend.

- Save: if `GameId` and `DataVersion` did not change since the last load or save, it does nothing. Otherwise it writes a Base64 string to the key `savegame`: format ID `3`, game ID, time played, data version, `AppConfig.version`, then the zlib-compressed session payload.
- Load: reads `savegame` for the base game or `savegame.<mod>` for a mod.
- `TryImportOriginalSave` loads the base-game save into a mod session.

Save always writes `savegame`, even in a mod. See [S2](issues/services.md#s2-playerprefsstorage-writes-mod-saves-into-the-base-game-slot).

`EmptyCloudStorage` reports `NotReady` and has no games. `SaveGameExporterStub` logs `NotImplementedException`.

## In-app purchases and IAP storage

- `IInAppPurchasing` lists `IIapItem` products, restores purchases, and confirms pending ones. Purchase results fire `InAppPurchaseCompletedSignal`, `InAppPurchaseFailedSignal`, `SupporterPackPurchasedSignal`, and `InAppItemListUpdatedSignal`. [`InAppPurchasingStub`](../Assets/Modules/Services/Scripts/InAppPurchasing/InAppPurchasingStub.cs) returns no products.
- `IIapStorage` keeps an `IapData` record outside the save file: purchased stars, supporter pack, remove-ads. Two records merge with `+` (maximum stars, bitwise OR of flags). [`EmptyStorage`](../Assets/Modules/Services/Scripts/IapStorage/EmptyStorage.cs) returns an empty record. The private `WebStorage` keeps it on the game server; see [Server](server.md#iap-backup). No code writes the record today ([MP1](issues/modules_private.md#mp1-the-purchase-backup-is-never-written)).

## Advertisements

`IAdsManager` shows rewarded and interstitial ads and reports `AdStatus`. A finished rewarded video fires `RewardedVideoCompletedSignal`. [`AdsManagerStub`](../Assets/Modules/Services/Scripts/Advetisements/AdsManagerStub.cs) has ads disabled.

## Coroutine manager

[`CoroutineManager`](../Assets/Modules/Services/Scripts/Legacy/CoroutineManager.cs) gives non-`MonoBehaviour` code a place to run coroutines (`StartCoroutine`) and to run a string callback on the next `Update` (`StartActionOnNextUpdate`). Users: the arena client, the shipyard window, cheats, and the private `WebStorage`.

## SecurityStub

The private `Security` assembly holds save obfuscation, mod decryption, server request building, and console code checks. It does not sign requests or validate purchases; see [ModulesPrivate: Security assembly](modules_private.md#security-assembly). `SecurityStub` has the same public types with no-op bodies, so open-source builds compile and run offline.

| Type | Stub behaviour | Used by |
| --- | --- | --- |
| [`BundleValidator.IsValidBundle`](../Assets/Modules/SecurityStub/Scripts/BundleValidator.cs) | Always `true`. | Only the private `GoogleAdsManager`. Unused in open-source builds. |
| [`DebugCommands`](../Assets/Modules/SecurityStub/Scripts/DebugCommands.cs) | `Decode` parses the command as an integer; `Encode` prints the integer. The device hash is ignored. | Developer console, [`CodeGenerator`](../Assets/Editor/CodeGenerator.cs) editor window. |
| [`EncryptedReadStream`](../Assets/Modules/SecurityStub/Crypto/EncryptedStream.cs) | Pass-through read-only stream. | [`FileDatabaseStorage`](../Assets/Modules/Database/Scripts/Storage/FileDatabaseStorage.cs) when it reads legacy packed mods. |
| [`Encryption`](../Assets/Modules/SecurityStub/Scripts/Encryption.cs) | `Encrypt` does nothing; `TryDecrypt` returns `true`. | Only the private `LocalStorage` (save files). Unused in open-source builds. |
| [`ModEncryption`](../Assets/Modules/SecurityStub/Crypto/ModEncryption.cs) | Throws `NotSupportedException` ("Authenticated packed mods require the commercial Security module"). Sizes: 384-byte wrapped key, 12-byte nonce, 16-byte tag. | [Database: encrypted mods](database.md). |
| [`RequestFactory`](../Assets/Modules/SecurityStub/Scripts/RequestFactory.cs) | Arena requests return `Observable.Empty`. Item and IAP requests return `new WWW("")`. | Arena client, cheats, `WebStorage`. |
| [`IapPurchaseProcessor`](../Assets/Modules/SecurityStub/Scripts/IapPurchaseProcessor.cs) | No purchases. `SupporterPack_Id` is empty. | Nothing in open-source builds; the private IAP code has its own class. |
| [`StringExtensions.Encrypt/Decrypt`](../Assets/Modules/SecurityStub/Scripts/StringEncryptor.cs) | Identity. | Only the editor window [`StringEncryptorWindow`](../Assets/Editor/StringEncryptorWindow.cs). |

With the stub, the arena never finds an opponent, cheats work by plain number, and encrypted packed mods fail to load.

## Service signals

`ServicesInstaller.BindSignals` binds these signals and their triggers in the root container:

- Cloud: `CloudStorageStatusChangedSignal`, `CloudLoadingCompletedSignal`, `CloudSavingCompletedSignal`, `CloudOperationFailedSignal`, `CloudSavedGamesReceivedSignal`.
- Account: `AccountStatusChangedSignal`.
- Scenes: `SceneBeforeUnloadSignal`, `SceneLoadedSignal`, `SceneManagerStateChangedSignal`.
- Application: `GamePausedSignal`, `AppActivatedSignal`.
- Messages: `ShowMessageSignal`, `ShowDebugMessageSignal`.
- Ads: `RewardedVideoCompletedSignal`, `AdStatusChangedSignal`.
- Other: `IapDataSavedSignal`, `AssetLoaderStatusChangedSignal`, `LocalizationChangedSignal`.
- GUI (in `InstallBindings`): `WindowOpenedSignal`, `WindowClosedSignal`, `WindowDestroyedSignal`, `EscapeKeyPressedSignal`.

The IAP signals are bound in `GameInstaller`.

## Adding a platform service

1. Add or extend the interface in `ServicesFacade`. Keep it free of platform SDK types.
2. Add a default or stub implementation in `Services`, so open-source builds still compile.
3. Add the platform implementation in `Assets/ModulesPrivate/Services/` (or in a new public module if it has no secrets).
4. Add an `#if` branch in `ServicesInstaller`. Keep the `LICENSE_OPENSOURCE` branch first.
5. If the service needs session data, derive it from `GameServiceBase` in the application layer instead (see [Application layer: Session lifecycle](application_layer.md#session-lifecycle)).
