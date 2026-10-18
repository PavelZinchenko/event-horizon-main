# GameSession Storage

This page describes how the game stores the session outside the GameSession module: when it saves, which backend each platform uses, the file and cloud formats around the session bytes, per-mod saves, and export and import. For the session bytes themselves, see [Binary format](serialization.md). For the module overview, see [GameSession module](../game_session.md).

The storage code is outside the module. The contracts are in [`ServicesFacade/Scripts/Storage/`](../../Assets/Modules/ServicesFacade/Scripts/Storage/). The open-source backends are in [`Services/Scripts/Storage/`](../../Assets/Modules/Services/Scripts/Storage/). The commercial backends are in [`ModulesPrivate/Services/Scripts/Storage/`](../../Assets/ModulesPrivate/Services/Scripts/Storage/), a private submodule. The save timing is in [`GameDataManager`](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs).

## Contents

- [Terms](#terms)
- [Overview](#overview)
- [Contracts](#contracts)
- [Backend selection](#backend-selection)
- [Startup load](#startup-load)
- [Saving](#saving)
- [Mod saves](#mod-saves)
- [Local files](#local-files)
- [PlayerPrefs storage](#playerprefs-storage)
- [Cloud saves](#cloud-saves)
- [Export and import](#export-and-import)
- [Restoring purchases](#restoring-purchases)
- [Failure behavior](#failure-behavior)
- [Checklist for a storage change](#checklist-for-a-storage-change)

## Terms

| Term | Meaning |
| --- | --- |
| Session bytes | The output of `SessionData.Serialize`: the envelope and payload described in [Binary format](serialization.md). |
| Storage header | The fields that a backend writes in front of the session bytes: game ID, play time, `DataVersion`, and more. Each backend has its own header and its own format number. |
| Local storage | The `IDataStorage` backend. It holds the one save that the game loads at startup. |
| Cloud storage | The `ICloudStorage` backend. It holds named save slots that the player saves and loads by hand. |
| Main file | The current local save of one database: `savegame` for the built-in database, the mod ID for a mod. |
| Write-back | Domain services copy their live objects into the session when `SessionAboutToSaveSignal` fires. |

## Overview

```text
                         InitializationState ──TryLoad / TryImportOriginalSave / CreateNewGame──┐
                                                                                                ▼
 GameDataManager ──SessionAboutToSaveSignal──► PlayerFleet, PlayerInventory, GameTime ──► SessionData
        │                                                                                     ▲ │
        ├── SaveSession ─────────────► IDataStorage.Save ── header + Serialize() ──► file /   │ │
        │                                (LocalStorage, PlayerPrefsStorage)         PlayerPrefs│ │
        ├── LoadMod ─────────────────► IDataStorage.TryLoad ─────── TryDeserialize ────────────┘ │
        ├── SaveGameToCloud ─────────► ICloudStorage / ISavedGame ── CloudDataAdapter ──► Google, Apple
        ├── LoadGameFromCloud ───────► ISavedGame.Load ──────────── CloudDataAdapter.TryLoad ────┘
        └── Export / ImportProgress ─► ISavegameExporter (ExportableLocalStorage, native file picker)
```

The module provides two calls: `ISerializableGameData.Serialize()` and `TryDeserialize(gameId, timePlayed, dataVersion, modId, data, startIndex)` ([ISerializableGameData.cs](../../Assets/Modules/CommonComponents/Scripts/Services/ISerializableGameData.cs)). The backends store the four metadata values from `Services.Storage.IGameData` in their header and pass them back on load. `SessionData` implements both interfaces; see [SessionData](../game_session.md#sessiondata).

## Contracts

| Contract | Members | Implementations |
| --- | --- | --- |
| [`IDataStorage`](../../Assets/Modules/ServicesFacade/Scripts/Storage/IDataStorage.cs#L5) | `Save(data)`, `TryLoad(data, mod)`, `TryImportOriginalSave(data, mod)` | `LocalStorage` and its platform subclasses, `PlayerPrefsStorage`, `CloudDataAdapter` (load only) |
| [`ICloudStorage`](../../Assets/Modules/ServicesFacade/Scripts/Storage/ICloudStorage.cs#L26) | `Status`, `Games`, `Synchronize()`, `Save(filename, data)`, `TryLoadFromCopy(data, mod)`, `LastErrorMessage` | `GoogleCloudStorage`, `AppleCloudStorage`, `EmptyCloudStorage` |
| [`ISavedGame`](../../Assets/Modules/ServicesFacade/Scripts/Storage/ICloudStorage.cs#L16) | `Name`, `ModificationTime`, `Save(data)`, `Load(data, mod)`, `Delete()` | `GoogleSavedGame`, `AppleSavedGame` |
| [`ISavegameExporter`](../../Assets/Modules/ServicesFacade/Scripts/Storage/ISavegameExporter.cs#L3) | `Export(callback)`, `Import(data, mod, callback)` | `ExportableLocalStorage` and `AndroidLocalStorage`, `SaveGameExporterStub` |

`TryImportOriginalSave` loads the built-in game's save into a mod session. It returns `false` for the built-in database.

Cloud operations are asynchronous. Their results arrive as signals declared next to `ICloudStorage` ([ICloudStorage.cs:39](../../Assets/Modules/ServicesFacade/Scripts/Storage/ICloudStorage.cs#L39)):

| Signal | Payload | Listeners |
| --- | --- | --- |
| `CloudStorageStatusChangedSignal` | `CloudStorageStatus` | `SignalsTranslator` → `SettingsLoadSave` (spinner, list, error text). |
| `CloudSavedGamesReceivedSignal` | — | `SignalsTranslator` → `SettingsLoadSave` (rebuilds the slot list). |
| `CloudLoadingCompletedSignal` | — | [`NotificationManager`](../../Assets/Scripts/GameServices/Gui/NotificationManager.cs#L57) (message), `SignalsTranslator` ([line 145](../../Assets/Scripts/Services/Messenger/SignalsTranslator.cs#L145)). |
| `CloudSavingCompletedSignal` | — | `NotificationManager` (message). |
| `CloudOperationFailedSignal` | `string` error | `NotificationManager` (message). |

`CloudStorageStatus` is `NotReady`, `Idle`, `Synchronizing`, `Loading`, or `Saving`. `Save`, `Load`, and `Delete` return at once unless the status is `Idle`. `Synchronize` also runs from `NotReady`.

## Backend selection

[`ServicesInstaller`](../../Assets/Scripts/Installers/ServicesInstaller.cs#L102) selects the backends with compile-time symbols. The first matching line wins.

| Condition | `IDataStorage` | `ISavegameExporter` | `ICloudStorage` |
| --- | --- | --- | --- |
| `LICENSE_OPENSOURCE` | `PlayerPrefsStorage` | stub | `EmptyCloudStorage` |
| WebGL player | `PlayerPrefsStorage` | stub | `EmptyCloudStorage` |
| Android player | `AndroidLocalStorage` | `AndroidLocalStorage` | `GoogleCloudStorage` (without `NO_GPGS`), else empty |
| iOS player | `ExportableLocalStorage` | `ExportableLocalStorage` | `AppleCloudStorage` |
| macOS player | `MacLocalStorage` | stub | `EmptyCloudStorage` |
| Windows player | `WindowsLocalStorage` | stub | `EmptyCloudStorage` |
| Unity Editor | `ExportableLocalStorage` | `ExportableLocalStorage` | `EmptyCloudStorage` |
| Other | `LocalStorage` | stub | `EmptyCloudStorage` |

`BindInterfacesTo` binds every interface of the class, so an exportable local storage is also the `ISavegameExporter`. The stub binding uses `IfNotBound` ([line 119](../../Assets/Scripts/Installers/ServicesInstaller.cs#L119)). The stub logs `NotImplementedException` and never calls the callback.

The project settings define `LICENSE_COMMERCIAL` for every platform, so release builds use the private backends. A build without the `ModulesPrivate` submodule must define `LICENSE_OPENSOURCE`. The `Security` assembly is then replaced by [`SecurityStub`](../../Assets/Modules/SecurityStub/Scripts/Encryption.cs), whose encryption does nothing.

## Startup load

[`InitializationState.OnLoad`](../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L53):

1. Find mods and add the external mod files from the settings.
2. Load the database of `GameSettings.ActiveMod`. If that fails, load the built-in database ([line 84](../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L84)). The setting keeps the failed mod ID.
3. Call `IDataStorage.TryLoad(session, database.Id)` ([line 139](../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L139)).
4. If that fails and the database is editable, call `TryImportOriginalSave`. A mod then starts from a copy of the built-in game's progress.
5. If both fail, call `SessionData.CreateNewGame(database.Id)`. The session is not started; [`StartingNewGameState`](../../Assets/Scripts/GameStateMachine/States/StartingNewGameState.cs#L47) starts it later.

Each load fires `SessionDataLoadedSignal` and `SessionCreatedSignal`, so domain services build their live state here. See [Loading a save](../game_session.md#loading-a-save).

## Saving

### Triggers

[`GameDataManager`](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs) is a `MonoBehaviour`. Its private `SaveSession` ([line 151](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L151)) does three things:

1. Return if the session has not started (`Game.GameStartTime == 0`).
2. Fire `SessionAboutToSaveSignal`.
3. Call `IDataStorage.Save(session)`.

| Event | Code |
| --- | --- |
| The application is paused | `OnApplicationPause(true)` ([line 180](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L180)) |
| The window loses focus | `OnApplicationFocus(false)` |
| The object is disabled (application quit, scene unload) | `OnDisable` |
| The star map scene is loaded | `OnLevelLoaded(GameScene.StarMap)` ([line 198](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L198)) |
| An in-app purchase completes | `OnIapCompleted` |
| Before a mod switch or a database reload | `LoadMod` ([line 85](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L85)) |

There is no timed autosave. The old cloud autosave is commented out.

### Write-back

Three services keep live state outside the session and copy it in only when `SessionAboutToSaveSignal` fires:

| Service | Writes | Code |
| --- | --- | --- |
| `PlayerFleet` | All ships through `Fleet.UpdateShips`, if any ship changed. | [PlayerFleet.cs:183](../../Assets/Scripts/Domain/Player/PlayerFleet.cs#L183) |
| `PlayerInventory` | Components and satellites, if either collection is dirty. | [PlayerInventory.cs:87](../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L87) |
| `GameTime` | `Game.TotalPlayTime` from the live counter. | [GameTime.cs:40](../../Assets/Scripts/Services/InternetTime/GameTime.cs#L40) |

So the session is complete only right after the signal. Any code that serializes the session without the signal stores the fleet, the inventory, and the play time of the previous save. One cloud path does this: see [GS7](../issues/game_session.md#gs7-a-cloud-save-to-a-new-slot-skips-the-write-back).

### Skipping unchanged saves

`LocalStorage` and `PlayerPrefsStorage` remember the `GameId` and `DataVersion` of the last file they read or wrote. `Save` returns without writing when both match ([LocalStorage.cs:55](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L55), [PlayerPrefsStorage.cs:22](../../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs#L22)). `DataVersion` grows once per save with changes; see [Change tracking](../game_session.md#change-tracking-and-dataversion). A failed write does not update the remembered values, so the next trigger tries again.

Cloud backends never skip.

## Mod saves

Each database has its own save. The session carries the database ID as `ModId`, and the backends use it to choose the file and to check a loaded file.

[`GameDataManager.LoadMod(id, allowReload)`](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L76):

1. Return if `id` is the current database and `allowReload` is false.
2. Save the current session.
3. Load the database `id`. On failure, go to step 6.
4. Load that database's save, or import the built-in save into an editable mod, or create a new game. This is the same order as the startup load.
5. Store the ID in `GameSettings.ActiveMod` and show a message.
6. On any exception: show the error, load the built-in database, and load its save or create a new game.

`SettingsDatabase` calls `LoadMod(item.Id)`. `MainMenu.ReloadDatabase` calls `LoadMod(currentId, true)` ([MainMenu.cs:101](../../Assets/Scripts/Gui/MainMenu/MainMenu.cs#L101)).

| Backend | Built-in database | Mod `X` | Mod check on load |
| --- | --- | --- | --- |
| `LocalStorage` | `savegame` | file `X` | The header mod must equal the requested mod, ignoring case ([LocalStorage.cs:217](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L217)). |
| `PlayerPrefsStorage` | key `savegame` | reads key `savegame.X`, but writes key `savegame` | None. See [GS8](../issues/game_session.md#gs8-playerprefs-storage-writes-every-mod-into-the-built-in-save). |
| Cloud | one shared slot list | the same slot list | The stored mod must equal the requested mod, but saves always store an empty mod. See [GS9](../issues/game_session.md#gs9-cloud-saves-do-not-store-the-mod-id). |

**Import of the original save.** `LocalStorage.TryImportOriginalSave` reads the built-in game's main file (then its backups) and passes the mod ID as an override, so the session gets the mod's `ModId` ([LocalStorage.cs:32](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L32)). The imported session keeps the built-in game's `GameId` and `DataVersion`, so the first mod file is written only after the first change. Only editable mods import; other mods start a new game.

## Local files

[`LocalStorage`](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs) is the base class of the file backends. The platform subclasses change only the folders.

### File layout

`LocalStorage.Save` writes local format 3 ([LocalStorage.cs:66](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L66)). Integers and strings use [`GameModel.Serialization.Helpers`](../../Assets/Modules/CommonComponents/Scripts/Serialization/Helpers.cs): little-endian fixed-size integers, and strings as an `int` byte length plus UTF-8 bytes. An empty or `null` string is length 0.

| Field | Type | Since format | Content |
| --- | --- | --- | --- |
| Format | `int` | — | `3`. |
| Game ID | `long` | 0 | `IGameData.GameId`. |
| Time played | `long` | 0 | `IGameData.TimePlayed`, seconds. |
| Data version | `long` | 1 | `IGameData.DataVersion`. 0 for older files. |
| Game version | `string` | 2 | `AppConfig.version` of the build that wrote the file. `"old"` for older files. |
| Mod ID | `string` | 3 | `IGameData.ModId`. `null` for older files, which equals the built-in database. |
| Session bytes | rest | — | `Serialize()` output. |

`Security.Encryption.Encrypt` then transforms the whole buffer in place and appends one checksum byte ([LocalStorage.cs:77](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L77)). The commercial implementation XORs each byte with a pseudo-random stream seeded by the buffer size ([Encryption.cs:7](../../Assets/ModulesPrivate/Security/Scripts/Encryption.cs#L7)). It hides the bytes from casual editing and detects corruption; it is not cryptographic protection. `TryDecrypt` reverses it in place and rejects a wrong checksum.

The files are not compressed.

### Files

| File | Written when | Read when |
| --- | --- | --- |
| `<main>` (`savegame` or the mod ID) | Every save that is not skipped. | First on load. |
| `<main>.bak` | The first save after the main file loaded: the main file is copied first ([LocalStorage.cs:63](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L63)). So it holds the state at the last successful startup or mod load. | When the main file is missing or does not load. |
| `<main>.err` | The main file exists but does not load. | Never. For diagnosis only. See [GS5.19](../issues/game_session.md#gs5-smaller-defects). |
| `<main>.<game version>` | Android only: a file from another game version was loaded ([AdnroidLocalStorage.cs:110](../../Assets/ModulesPrivate/Services/Scripts/Storage/AdnroidLocalStorage.cs#L110)). | Never. |

### Load order

`LocalStorage.TryLoad(session, mod)` ([LocalStorage.cs:12](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L12)):

1. Set the current mod, which selects the file names.
2. Read the main file. If it loads, remember that the main file is loaded and return `true`.
3. If it exists but does not load, write it to the error file.
4. Try each backup file in the platform's order. Return `true` on the first one that loads.

`TryDeserializeData` ([line 200](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L200)) decrypts, reads the header by format, checks the mod, and calls `session.TryDeserialize`. Every failed attempt goes through `SessionData.TryDeserialize`, so it can replace the live model: see [GS1](../issues/game_session.md#gs1-a-failed-load-replaces-the-live-model). The game version and the format number are not checked against the build; the session's own schema version decides whether the bytes load.

### Platform folders

| Class | Main folder | Backup search order |
| --- | --- | --- |
| `LocalStorage` | `Application.persistentDataPath` | `<main>.bak` |
| [`WindowsLocalStorage`](../../Assets/ModulesPrivate/Services/Scripts/Storage/WindowsLocalStorage.cs) | `Application.dataPath/../SavesDir/`, next to the game's data folder | `SavesDir/<main>.bak`, then the old `persistentDataPath` main file and its `.bak` |
| [`MacLocalStorage`](../../Assets/ModulesPrivate/Services/Scripts/Storage/MacLocalStorage.cs) | `Application.dataPath/../../SavesDir/`, next to the application bundle | Same as Windows |
| [`AndroidLocalStorage`](../../Assets/ModulesPrivate/Services/Scripts/Storage/AdnroidLocalStorage.cs) | Both the internal files folder and the external files folder | The other folder's main file, both `.bak` files (newer first), then `persistentDataPath` main and `.bak` |
| [`ExportableLocalStorage`](../../Assets/ModulesPrivate/Services/Scripts/Storage/ExportableLocalStorage.cs) (iOS, Editor) | `Application.persistentDataPath` | `<main>.bak` |

Windows and macOS read the old `persistentDataPath` files only as backups. The first save then writes to `SavesDir`, which moves old installations to the new folder.

**Android.** `TryLoadMainFile` loads the newer of the two main files and remembers which folder it came from ([AdnroidLocalStorage.cs:18](../../Assets/ModulesPrivate/Services/Scripts/Storage/AdnroidLocalStorage.cs#L18)). `TrySaveMainFile` writes both folders. Only if both writes fail does it write `persistentDataPath` ([line 46](../../Assets/ModulesPrivate/Services/Scripts/Storage/AdnroidLocalStorage.cs#L46)). The backup copy and the export use the remembered folder. The error file always goes to the external folder.

## PlayerPrefs storage

[`PlayerPrefsStorage`](../../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs) is used in open-source builds and on WebGL. It stores one Base64 string per key:

| Field | Type | Content |
| --- | --- | --- |
| Format | `int` | `3`. Not checked on load. |
| Game ID | `long` | |
| Time played | `long` | Seconds. |
| Data version | `long` | |
| Game version | `string` | Read and ignored. |
| Session bytes | rest | Compressed with zlib (`Ionic.Zlib.ZlibStream`). |

There is no mod field, no encryption, and no backup. `Save` always writes the key `savegame` ([PlayerPrefsStorage.cs:40](../../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs#L40)). `TryLoad` reads `savegame` for the built-in database and `savegame.<mod>` for a mod ([line 64](../../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs#L64)). This mismatch is [GS8](../issues/game_session.md#gs8-playerprefs-storage-writes-every-mod-into-the-built-in-save). `TryImportOriginalSave` reads `savegame` with the mod ID as the session's `ModId`.

## Cloud saves

### Entry points

The cloud slots are shown in [`SettingsLoadSave`](../../Assets/Scripts/Gui/MainMenu/SettingsLoadSave.cs) in the main menu settings. Opening the panel calls `Synchronize` when the storage is `NotReady`.

| Action | `GameDataManager` call | Steps |
| --- | --- | --- |
| Save to a new slot | `SaveGameToCloud(name)` ([line 114](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L114)) | `IDataStorage.Save`, then `ICloudStorage.Save(name, session)`. No `SessionAboutToSaveSignal` and no started check: [GS7](../issues/game_session.md#gs7-a-cloud-save-to-a-new-slot-skips-the-write-back). |
| Overwrite a slot | `SaveGameToCloud(game)` ([line 121](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L121)) | `SessionAboutToSaveSignal`, `IDataStorage.Save`, then `ISavedGame.Save`. |
| Load a slot | `LoadGameFromCloud(game)` ([line 129](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L129)) | `ISavedGame.Load(session, database.Id)`. The current session is not saved first. The local file gets the cloud state at the next save trigger. |
| Delete a slot | `ISavedGame.Delete()` from the UI | Deletes, then synchronizes. |
| Load the last downloaded copy | `LoadGameFromLocalCopy()` ([line 135](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L135)) | `ICloudStorage.TryLoadFromCopy`. Only the cheat menu calls it ([Cheats.cs:188](../../Assets/Scripts/Legacy/Debug/Cheats.cs#L188)). |

Loading, overwriting, and deleting a slot ask for confirmation first. The result arrives as `CloudLoadingCompletedSignal`, `CloudSavingCompletedSignal`, or `CloudOperationFailedSignal`.

After a cloud load, the next local save writes the cloud state to the main file. If the main file was loaded at startup and has not been saved since, the first save backs it up to `.bak` first. Otherwise the previous local progress is gone.

### Cloud format

[`CloudDataAdapter`](../../Assets/ModulesPrivate/Services/Scripts/Storage/CloudDataAdapter.cs) builds and reads the cloud bytes for both cloud backends. It has its own format numbers, independent of the local format.

| Field | Type | Since cloud format | Content |
| --- | --- | --- | --- |
| Format | `int` | — | `4` on write ([line 117](../../Assets/ModulesPrivate/Services/Scripts/Storage/CloudDataAdapter.cs#L117)). |
| Game ID | `long` | 2 | |
| Time played | `long` | 2 | Seconds. |
| Data version | `long` | 3 | 0 for older saves. |
| Mod ID | `string` | 4 | The `mod` argument of `Serialize`, which no caller passes, so it is always empty. |
| Session bytes | rest | 2 | `Serialize()` output. No encryption, no compression. |

There is no game version field. `TryLoad` ([line 20](../../Assets/ModulesPrivate/Services/Scripts/Storage/CloudDataAdapter.cs#L20)) rejects a stored mod that differs from the requested mod, then calls `session.TryDeserialize` with the stored mod ID.

Formats 0 and 1 are older cloud saves of the built-in game. `LoadLegacyData` repacks their twelve sections into the headerless legacy container and gives them a new game ID ([line 65](../../Assets/ModulesPrivate/Services/Scripts/Storage/CloudDataAdapter.cs#L65)). The repacked container declares 13 sections, so the load always fails: [GS5.23](../issues/game_session.md#gs5-smaller-defects).

### Google Play Games

[`GoogleCloudStorage`](../../Assets/ModulesPrivate/Services/Scripts/Storage/GoogleCloudStorage.cs) compiles only for Android without `NO_GPGS`.

| Operation | Behavior |
| --- | --- |
| `Synchronize` | Needs a connected Google account; otherwise it starts the sign-in and stays `NotReady`. Fetches the slot list. An account connection while `NotReady` synchronizes again. |
| `Save(description, data)` | Creates a new slot `savegame.<UtcNow.Ticks>` with the description as the visible name ([line 117](../../Assets/ModulesPrivate/Services/Scripts/Storage/GoogleCloudStorage.cs#L117)). `ICloudStorage.Save(filename, …)` maps to this overload, so the "file name" is the description. |
| `GoogleSavedGame.Save` | Overwrites the slot by its file name. The metadata gets the played time and the description. Conflicts resolve with `UseLongestPlaytime`. |
| `Load` | Reads the slot, writes the bytes to `persistentDataPath/savegame.cloud` ([line 212](../../Assets/ModulesPrivate/Services/Scripts/Storage/GoogleCloudStorage.cs#L212)), then loads through `CloudDataAdapter`. |
| `TryLoadFromCopy` | Loads `savegame.cloud`. |

The played time in the metadata is wrong by a factor of 10⁷: [GS5.24](../issues/game_session.md#gs5-smaller-defects).

### Apple Game Center

[`AppleCloudStorage`](../../Assets/ModulesPrivate/Services/Scripts/Storage/AppleCloudStorage.cs) uses the Stan's Assets iOS Native saved-games API (`ISN_GameSaves`).

| Operation | Behavior |
| --- | --- |
| `Synchronize` | Fetches the slot list. No account check. |
| `Save(name, data)` | Removes characters that are not valid in file names and saves under that name. Saving under an existing name overwrites the slot. `AppleSavedGame.Save` saves under its own name. |
| `Load` | Asks the slot for its data and waits for `ActionDataLoaded`, then loads through `CloudDataAdapter`. |
| `TryLoadFromCopy` | Not supported; returns `false`. |

Defects in the error handling: [GS5.20, GS5.21, GS5.22](../issues/game_session.md#gs5-smaller-defects).

## Export and import

The settings screen ([`SettingsProgress`](../../Assets/Scripts/Gui/MainMenu/SettingsProgress.cs)) offers export and import on iOS, Android, and in the Editor. The other platforms bind the stub.

| Operation | Behavior |
| --- | --- |
| Export | `ExportableLocalStorage.Export` passes the main file to `NativeFilePicker.ExportFile` ([ExportableLocalStorage.cs:7](../../Assets/ModulesPrivate/Services/Scripts/Storage/ExportableLocalStorage.cs#L7)). The file is the encrypted local file, so only the same build family can read it. Nothing is saved first: [GS5.25](../issues/game_session.md#gs5-smaller-defects). |
| Import | `NativeFilePicker.PickFile`, then the normal local `TryDeserializeData` with the current mod check. The result is `Cancelled`, `Error` (the file cannot be read), `InvalidFormat` (it does not load), or `Success`. The imported state reaches the main file at the next save trigger. |

`SettingsProgress.DeleteProgress` calls `GameDataManager.CreateNewGame`, which keeps the purchases. The old main file stays until the new game is started and saved.

## Restoring purchases

`GameDataManager.RestorePurchases` ([line 58](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L58)) reads the purchase record from `IIapStorage`, which is separate from the save. It reapplies the supporter pack and adds the difference between the stored and the session's purchased stars to both `Iap.Stars` and `Resources.Stars`. See [IapData](schema/economy.md#iapdata).

## Failure behavior

| Situation | Result |
| --- | --- |
| Main file damaged | Written to `.err`; the newest working backup loads. The next save writes a new main file without making a backup. |
| All local files damaged at startup | New game. Purchases are kept only if a rejected model is still in `SessionData`; see [GS1](../issues/game_session.md#gs1-a-failed-load-replaces-the-live-model). |
| Save from a newer build | The session rejects the schema version; handled as damaged. See [GS2](../issues/game_session.md#gs2-unknown-schema-versions-are-not-rejected). |
| Local file of another mod | Rejected by the mod check. |
| File write fails | Logged. `DataVersion` is not recorded, so the next trigger retries. |
| Cloud load fails | `CloudOperationFailedSignal`. The live session may already be replaced ([GS1](../issues/game_session.md#gs1-a-failed-load-replaces-the-live-model)), except when the mod check rejects the save first. |
| Cloud operation while another is running | Ignored without a message. |

## Checklist for a storage change

1. Never change the meaning of an existing header field. Add a field behind a new format number and keep reading every older format.
2. Write the session's `ModId` into every new format, and check it on load.
3. Fire `SessionAboutToSaveSignal` before every `Serialize` call outside `SaveSession`.
4. Keep the session bytes opaque. Schema changes belong in the module; see [Versioning and migration workflow](../game_session.md#versioning-and-migration-workflow).
5. Test with the built-in database and a mod: save, restart, load; switch mods both ways; load a damaged main file and check that the backup loads.
6. For a cloud change, test a save made by the current release build.
