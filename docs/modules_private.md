# ModulesPrivate

`Assets/ModulesPrivate/` is a private Git submodule. It holds the commercial implementations of the platform services and the security code: platform sign-in, file and cloud saves, in-app purchases, ads, the music download, arena server requests, save and mod decryption, and two vendored iOS plugins. The public repository declares the contracts in [ServicesFacade](services.md) and ships stubs in `Services` and `SecurityStub`, so the game also builds without this submodule.

This page describes the architecture and the integration points only. It does not list keys, server addresses, store product IDs, or ad unit IDs.

| | Security | ServicesPrivate | iOS plugins |
| --- | --- | --- | --- |
| Assembly | [Security.asmdef](../Assets/ModulesPrivate/Security/Security.asmdef) | [ServicesPrivate.asmdef](../Assets/ModulesPrivate/Services/ServicesPrivate.asmdef) | [iOSKeychainPlugin.asmdef](../Assets/ModulesPrivate/Plugins/FSG/iOSKeychainPlugin.asmdef), [StansAssets.asmdef](../Assets/ModulesPrivate/Plugins/StansAssets/StansAssets.asmdef); editor scripts in `StansAssetsEditor/` compile into `Assembly-CSharp-Editor` |
| References | AppConfiguration, UniRx, BouncyCastle (precompiled) | AppConfiguration, CommonComponents, GameSession, Localization, ResourceLocator, ServicesFacade, Security, SecurityStub, UniRx, Zenject, Unity IAP, Unity Services Core, Unity Ads, Google Play Games, Steamworks.NET, NativeFilePicker, StansAssets, iOSKeychainPlugin, and two missing GUIDs ([MP11](issues/modules_private.md#mp11-two-assembly-references-do-not-resolve)) | none |
| Compiles when | `LICENSE_COMMERCIAL` (`defineConstraints`) | `LICENSE_COMMERCIAL` | Always, on every platform |
| Namespaces | `Security`; global `StringExtensions`, `EncryptedStrings`, `CertificateChecker`, Java wrappers | `Services.Account`, `Services.Advertisements`, `Services.Assets`, `Services.GooglePlay`, `Services.IapStorage`, `Services.InAppPurchasing`, `Services.Storage`; global `SteamManager`, `UnityEngineThread` | `FSG.iOSKeychain`, `SA.*`, global `ISN_*`, `GK_*`, `GameCenterManager` |
| Source | [10 files, 780 lines](../Assets/ModulesPrivate/Security/Scripts/) | [22 files, 3,080 lines](../Assets/ModulesPrivate/Services/Scripts/) | FSG: 13 C# files, 3,760 lines. Stan's Assets: 227 C# files, 35,070 lines |
| Known issues | [ModulesPrivate issues](issues/modules_private.md) | | |

All three assemblies are `autoReferenced`, so `Assembly-CSharp` sees them without explicit references. [`ServicesInstaller`](../Assets/Scripts/Installers/ServicesInstaller.cs) and [`GameInstaller`](../Assets/Scripts/Installers/GameInstaller.cs) are the only places that name the private service classes.

## Contents

1. [Terms](#terms)
2. [Build variants](#build-variants)
3. [Dependency direction](#dependency-direction)
4. [Source map](#source-map)
5. [Security assembly](#security-assembly)
6. [Accounts](#accounts)
7. [Local and cloud saves](#local-and-cloud-saves)
8. [In-app purchases](#in-app-purchases)
9. [Purchase backup storage](#purchase-backup-storage)
10. [Advertisements](#advertisements)
11. [Music bundle download](#music-bundle-download)
12. [Server requests](#server-requests)
13. [iOS plugins](#ios-plugins)
14. [Legacy and unused code](#legacy-and-unused-code)
15. [Working on the private services](#working-on-the-private-services)

## Terms

| Term | Meaning |
| --- | --- |
| Commercial build | A build with `LICENSE_COMMERCIAL`. It needs this submodule. All checked-in platform settings use it. |
| Open-source build | A build with `LICENSE_OPENSOURCE`. It uses the public stubs and does not need this submodule. |
| Stub | A public class with the same contract and no platform behaviour, for example `EmptyAccount` or `SecurityStub.RequestFactory`. |
| Purchase backup | An `IapData` record (purchased stars, supporter pack, remove-ads flag) stored outside the save file, so purchases survive a new install. |
| Obfuscation | A reversible transformation with a key that ships in the client. It hides data from casual inspection. It is not cryptographic protection. |

## Build variants

The license symbol selects the assemblies:

- `LICENSE_COMMERCIAL` compiles `Security` and `ServicesPrivate`. `SecurityStub` does not compile.
- `LICENSE_OPENSOURCE` compiles `SecurityStub`. `Security` and `ServicesPrivate` do not compile, and `ServicesInstaller` binds the stubs.

The iOS plugin assemblies have no define constraint. They compile in both variants and on every platform.

The checked-in [ProjectSettings.asset](../ProjectSettings/ProjectSettings.asset) defines these symbols:

| Platform | License | Store | Ads | IAP | Other |
| --- | --- | --- | --- | --- | --- |
| Android | `LICENSE_COMMERCIAL` | `STORE_GOOGLE` | `ADS_UNITY` | `IAP_UNITY` | `STEAMWORKS_NET` |
| iOS | `LICENSE_COMMERCIAL` | `STORE_APPLE` | `ADS_UNITY` | `IAP_UNITY` | `NO_GPGS` |
| Standalone | `LICENSE_COMMERCIAL` | `STORE_STEAM` | `ADS_DISABLED` | `IAP_DISABLED` | `NO_GPGS`, `STEAMWORKS_NET` |
| WebGL | `LICENSE_COMMERCIAL` | `STORE_WEB` | `ADS_DISABLED` | `IAP_DISABLED` | `NO_GPGS`, `STEAMWORKS_NET` |

A clone without the submodule must replace `LICENSE_COMMERCIAL` with `LICENSE_OPENSOURCE` first; see [AppConfiguration](app_configuration.md) and [AC3](issues/app_configuration.md#ac3-checked-in-defines-need-the-private-submodule). For the full contract-to-class table of both variants, see [Services: Platform selection](services.md#platform-selection).

With these settings the live private services are:

| Platform | Account | Local save | Cloud save | IAP | Purchase backup | Ads |
| --- | --- | --- | --- | --- | --- | --- |
| Android | `GoogleAccount` | `AndroidLocalStorage` | `GoogleCloudStorage` | `InAppPurchasing` | `WebStorage` | `UnityAdsManager` |
| iOS | `GameCenterAccount` | `ExportableLocalStorage` | `AppleCloudStorage` | `InAppPurchasing` | `KeyChainStorage` | `UnityAdsManager` |
| Windows, macOS | `SteamAccount` | `WindowsLocalStorage`, `MacLocalStorage` | none | stub | none | stub |
| WebGL | none | `PlayerPrefsStorage` (public) | none | stub | none | stub |
| Editor | `EditorModeAccount` (public) | `ExportableLocalStorage` | none | by build target | by build target | by build target |

Every commercial platform uses the private [`AssetLoader`](#music-bundle-download) and the private `Security` assembly.

## Dependency direction

```
Assembly-CSharp ── installers ──► ServicesPrivate ──► ServicesFacade (contracts)
       │                               │   ├──► Security ──► BouncyCastle
       │                               │   ├──► GameSession (ISessionData for purchases)
       │                               │   └──► SDKs: Unity IAP, Unity Ads, Google Play Games,
       │                               │              Steamworks.NET, NativeFilePicker,
       │                               │              StansAssets, iOSKeychainPlugin
       ├──► Security.RequestFactory (arena client, cheats)
       └──► Security.DebugCommands (developer console)

Database ──► Security (mod decryption) — or SecurityStub in open-source builds
```

No public module references `ServicesPrivate` types. Game code depends on the `ServicesFacade` interfaces, with two exceptions:

- `IapPurchaseProcessor` is a concrete class. `GameInstaller` binds it, and `GameDataManager` and `SupporterPackItem` inject it. Both builds have a class with this name: the private one in `ServicesPrivate`, the stub in `SecurityStub`.
- `Security.RequestFactory`, `Security.DebugCommands`, and `Security.ModEncryption` are static classes. Callers compile against whichever of `Security` and `SecurityStub` is present.

## Source map

| Folder | Files | Role |
| --- | --- | --- |
| [Security/Scripts/](../Assets/ModulesPrivate/Security/Scripts/) | `ModEncryption`, `ModPrivateKey` | Decrypt authenticated packed mods. |
| | `EncryptedStream` | Decrypt legacy packed mods. |
| | `Encryption` | Obfuscate local save files. |
| | `RequestFactory` | Build arena, cheat, and purchase-backup HTTP requests. |
| | `DebugCommands` | Encode and check developer console codes. |
| | `StringEncryptor` | `StringExtensions.Encrypt` and `Decrypt` for obfuscated string constants. |
| | `BundleValidator` | Check the Android application ID. |
| | `CertificateChecker`, `JavaWrappers` | Android signing certificate check. Unused. |
| [Services/Scripts/Account/](../Assets/ModulesPrivate/Services/Scripts/Account/) | `GoogleAccount`, `GameCenterAccount`, `SteamAccount` | `IAccount` per platform. |
| [Services/Scripts/Steamworks.NET/](../Assets/ModulesPrivate/Services/Scripts/Steamworks.NET/) | `SteamManager` | Steam API start-up and callbacks. |
| [Services/Scripts/GooglePlayGames/](../Assets/ModulesPrivate/Services/Scripts/GooglePlayGames/) | `GooglePlayGamesService` (class `GooglePlayServices`) | Activates Play Games. |
| [Services/Scripts/Storage/](../Assets/ModulesPrivate/Services/Scripts/Storage/) | `LocalStorage` and subclasses, `GoogleCloudStorage`, `AppleCloudStorage`, `CloudDataAdapter`, `GameDataStub` | Save files, cloud slots, export and import. |
| [Services/Scripts/InAppPurchasing/](../Assets/ModulesPrivate/Services/Scripts/InAppPurchasing/) | `InAppPurchasing`, `IapPurchaseProcessor` | Unity IAP 5 store and the reward for each product. |
| [Services/Scripts/IapStorage/](../Assets/ModulesPrivate/Services/Scripts/IapStorage/) | `WebStorage`, `KeyChainStorage` | Purchase backup. |
| [Services/Scripts/Advetisements/](../Assets/ModulesPrivate/Services/Scripts/Advetisements/) | `UnityAdsManager`, `GoogleAdsManager` | `IAdsManager`. |
| [Services/Scripts/Assets/](../Assets/ModulesPrivate/Services/Scripts/Assets/) | `AssetLoader` | Music bundle. |
| [Services/Scripts/Legacy/](../Assets/ModulesPrivate/Services/Scripts/Legacy/) | `UnityEngineThread` | Main-thread action queue. |
| [Plugins/FSG/](../Assets/ModulesPrivate/Plugins/FSG/) | `iOSKeychainPlugin` | iOS Keychain access. |
| [Plugins/StansAssets/](../Assets/ModulesPrivate/Plugins/StansAssets/), [Plugins/StansAssetsEditor/](../Assets/ModulesPrivate/Plugins/StansAssetsEditor/) | iOS Native 9.11, iOS Deploy | Game Center, saved games, Xcode post-processing. |

## Security assembly

`Security` has the same public surface as [`SecurityStub`](services.md#securitystub). The stub table in the Services doc lists every type and its open-source behaviour. The commercial behaviour:

| Type | Commercial behaviour | Callers |
| --- | --- | --- |
| [`ModEncryption.Decrypt`](../Assets/ModulesPrivate/Security/Scripts/ModEncryption.cs) | Unwraps the AES-256 key with RSA-OAEP (SHA-256) and decrypts the payload with AES-GCM through BouncyCastle. Any failure throws `InvalidDataException`. It clears the key material in `finally`. | [Database: packed mod format](database.md#packed-mod-format-and-encryption) |
| [`ModPrivateKey`](../Assets/ModulesPrivate/Security/Scripts/ModPrivateKey.cs) | Holds the RSA private key in obfuscated form and decodes it on each call. `Fingerprint` matches the public key fingerprint in the editor ([Database Editor: Encryption](database/editor.md#encryption-format-version-1)). | `ModEncryption` only (`internal`) |
| [`EncryptedReadStream`](../Assets/ModulesPrivate/Security/Scripts/EncryptedStream.cs) | De-obfuscates a legacy packed mod while it is read. The last byte is a checksum; the stream does not check it ([MP10](issues/modules_private.md#mp10-the-legacy-mod-stream-ignores-its-checksum)). | `FileDatabaseStorage` |
| [`Encryption`](../Assets/ModulesPrivate/Security/Scripts/Encryption.cs) | Obfuscates a save buffer in place and appends a checksum byte. `TryDecrypt` reverses it and checks the byte. | `LocalStorage`; see [GameSession: Storage](game_session/storage.md) |
| [`RequestFactory`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs) | Builds real HTTP requests. See [Server requests](#server-requests). | Arena client, cheats, `WebStorage` |
| [`DebugCommands`](../Assets/ModulesPrivate/Security/Scripts/DebugCommands.cs) | A console code is six digits and is valid for one command ID on one device. `GetHashCode` derives the device value from `SystemInfo.deviceUniqueIdentifier`. In the editor, `Decode` accepts the plain command number. | [Developer console](gui.md#debug-console), [`Cheats`](../Assets/Scripts/Legacy/Debug/Cheats.cs), editor window [`CodeGenerator`](../Assets/Editor/CodeGenerator.cs) |
| [`StringExtensions`](../Assets/ModulesPrivate/Security/Scripts/StringEncryptor.cs) | A fixed character substitution over printable ASCII. `Encrypt` throws for other characters. | `JavaWrappers`, `CertificateChecker`, editor window [`StringEncryptorWindow`](../Assets/Editor/StringEncryptorWindow.cs) |
| [`BundleValidator.IsValidBundle`](../Assets/ModulesPrivate/Security/Scripts/BundleValidator.cs) | On Android, compares a hash of `Application.identifier` with the expected value. Elsewhere `true`. | `GoogleAdsManager`, which is not compiled with the current defines |

`IapPurchaseProcessor` is in `SecurityStub` for open-source builds, but its commercial version lives in `ServicesPrivate`, not in `Security`.

Any client that can decrypt mods must contain the private key, so a key extracted from one build decrypts every mod ([MP19](issues/modules_private.md#mp19-mod-confidentiality-depends-on-a-key-inside-the-client)).

What the assembly does not do: it does not sign server requests, and it does not validate store receipts. See [MP15](issues/modules_private.md#mp15-server-requests-are-not-authenticated) and [MP16](issues/modules_private.md#mp16-purchases-are-not-validated).

## Accounts

`IAccount.Id` is the player ID on the arena server. Each implementation adds a platform prefix, so IDs from different platforms cannot collide.

| Class | Platform guard | Sign-in | `Id` | `DisplayName` | Icon |
| --- | --- | --- | --- | --- | --- |
| [`GoogleAccount`](../Assets/ModulesPrivate/Services/Scripts/Account/GoogleAccount.cs) | `UNITY_ANDROID && !NO_GPGS` | `SignIn` calls `PlayGamesPlatform.Authenticate`. Reading `Status` re-checks `IsAuthenticated` and drops to `NotConnected` when the session ended. | `G:` + Play Games user ID | Play Games name | Downloads `GetUserImageUrl()` |
| [`GameCenterAccount`](../Assets/ModulesPrivate/Services/Scripts/Account/GameCenterAccount.cs) | none (bound only on iOS) | `SignIn` calls `GameCenterManager.Init` once. `OnAuthFinished` sets the status. A second `SignIn` does nothing ([MP7](issues/modules_private.md#mp7-game-center-sign-in-cannot-be-retried)). | `i:` + Game Center player ID | Game Center alias | `Player.LoadPhoto` (small) |
| [`SteamAccount`](../Assets/ModulesPrivate/Services/Scripts/Account/SteamAccount.cs) | `UNITY_STANDALONE` | None. `Status` is `Connected` when `SteamManager.Initialized`. `CanSignIn` is `false`. | `s:` + Steam ID | Steam persona name | None |

All three write `GameSettings.SignedIn` (Google and Game Center) and fire `AccountStatusChangedSignal` on a change. `SteamAccount` never fires it ([MP8](issues/modules_private.md#mp8-steamaccount-never-reports-a-status-change)). No implementation supports `SignOut`.

[`GooglePlayServices`](../Assets/ModulesPrivate/Services/Scripts/GooglePlayGames/GooglePlayGamesService.cs) calls `PlayGamesPlatform.Activate()` at start-up. `ServicesInstaller` binds it together with `GoogleCloudStorage` on Android players.

[`SteamManager`](../Assets/ModulesPrivate/Services/Scripts/Steamworks.NET/SteamManager.cs) is the Steamworks.NET template (version 1.0.13). The first read of `SteamManager.Initialized` creates a `DontDestroyOnLoad` object that calls `SteamAPI.Init` and runs `SteamAPI.RunCallbacks` every frame. The call to `RestartAppIfNecessary` still passes the template value `AppId_t.Invalid`. The Steamworks.NET package itself is in `Assets/com.rlabrecque.steamworks.net/`; see [Plugins](plugins.md).

## Local and cloud saves

The save backends are documented in [GameSession: Storage](game_session/storage.md): file layout, header formats, backup order, platform folders, cloud format, Google and Apple slot behaviour, export and import, and failure behaviour. Their defects are [GS5.19–GS5.26](issues/game_session.md#gs5-smaller-defects), [GS7](issues/game_session.md#gs7-a-cloud-save-to-a-new-slot-skips-the-write-back), and [GS9](issues/game_session.md#gs9-cloud-saves-do-not-store-the-mod-id).

Summary of the classes:

| Class | Contract | Notes |
| --- | --- | --- |
| [`LocalStorage`](../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs) | `IDataStorage` | Base class. One obfuscated file per database, plus `.bak` and `.err`. |
| [`WindowsLocalStorage`](../Assets/ModulesPrivate/Services/Scripts/Storage/WindowsLocalStorage.cs), [`MacLocalStorage`](../Assets/ModulesPrivate/Services/Scripts/Storage/MacLocalStorage.cs) | `IDataStorage` | `SavesDir` next to the game, with the old `persistentDataPath` files as backups. |
| [`ExportableLocalStorage`](../Assets/ModulesPrivate/Services/Scripts/Storage/ExportableLocalStorage.cs) | `IDataStorage`, `ISavegameExporter` | Export and import through NativeFilePicker. iOS and editor. |
| [`AndroidLocalStorage`](../Assets/ModulesPrivate/Services/Scripts/Storage/AdnroidLocalStorage.cs) | `IDataStorage`, `ISavegameExporter` | Writes the internal and the external files folder. |
| [`CloudDataAdapter`](../Assets/ModulesPrivate/Services/Scripts/Storage/CloudDataAdapter.cs) | `IDataStorage` (load only) | Cloud byte format for both cloud backends. |
| [`GoogleCloudStorage`](../Assets/ModulesPrivate/Services/Scripts/Storage/GoogleCloudStorage.cs) | `ICloudStorage` | Play Games saved games. Keeps the last download as `savegame.cloud`. |
| [`AppleCloudStorage`](../Assets/ModulesPrivate/Services/Scripts/Storage/AppleCloudStorage.cs) | `ICloudStorage` | Stan's Assets `ISN_GameSaves`. |
| [`GameDataStub`](../Assets/ModulesPrivate/Services/Scripts/Storage/GameDataStub.cs) | `IGameData` | No callers. |

## In-app purchases

[`InAppPurchasing`](../Assets/ModulesPrivate/Services/Scripts/InAppPurchasing/InAppPurchasing.cs) implements `IInAppPurchasing` on Unity IAP 5 (`StoreController`). `GameInstaller` binds it when `IAP_UNITY` is defined; otherwise it binds the public `InAppPurchasingStub`.

### Products

[`IapPurchaseProcessor`](../Assets/ModulesPrivate/Services/Scripts/InAppPurchasing/IapPurchaseProcessor.cs) declares eight product IDs:

- Six consumable star packs that the store offers (50 to 2,000 stars).
- One more consumable star pack that the store does not fetch. Its reward is wrong ([MP4](issues/modules_private.md#mp4-an-unused-star-pack-grants-the-wrong-amount)).
- One non-consumable supporter pack.

`InAppPurchaseProduct` wraps a store product as `IIapItem`. It removes a trailing `(…)` from the localized title (the app name that stores append) and uses the icon `Textures/Gui/InAppPurchases/<product ID>`.

### Flow

1. `Initialize` (Zenject `IInitializable`) creates the store controller, subscribes to its events, and awaits `Connect()`. A failed connection is logged and never retried.
2. After the connection, `FetchProducts` requests the seven offered products. `OnProductsFetched` fires `InAppItemListUpdatedSignal`.
3. `GetAvailableProducts` returns the fetched products that can be bought. It hides the supporter pack when the session already has it.
4. `IIapItem.Buy` calls `PurchaseProduct`.
5. `OnPurchasePending` runs `TryProcessPurchase` for every product in the order, then calls `ConfirmPurchase`, then fires `InAppPurchaseCompletedSignal` and `InAppItemListUpdatedSignal`. [`GameDataManager`](application_layer.md#session-lifecycle) saves the session on `InAppPurchaseCompletedSignal`.
6. `OnPurchaseFailed` fires `InAppPurchaseFailedSignal` with the store's details. A deferred purchase is only logged.

`TryProcessPurchase` adds the stars to both `Purchases.PurchasedStars` and `Resources.Stars`, or sets `Purchases.SupporterPack` and fires `SupporterPackPurchasedSignal`. See [Economy: IapData](game_session/schema/economy.md#iapdata). The order is confirmed even when the processor does not know the product ([MP3](issues/modules_private.md#mp3-an-unknown-product-is-confirmed-without-a-reward)).

`RestorePurchases` asks the store to restore transactions on iOS and Android. On Android it first deletes Unity IAP's local cache folder. The restored non-consumable then arrives through `OnPurchasePending`. `ConfirmRecentPurchases` is empty.

## Purchase backup storage

`IIapStorage` keeps an `IapData` record outside the save. `GameDataManager.RestorePurchases` reads it and adds the missing stars and the supporter pack to the session ([GameSession: Restoring purchases](game_session/storage.md#restoring-purchases)). The main menu and the purchase settings call it together with the store restore.

| Class | Platform | Storage | `Read` | `Write` |
| --- | --- | --- | --- | --- |
| [`KeyChainStorage`](../Assets/ModulesPrivate/Services/Scripts/IapStorage/KeyChainStorage.cs) | iOS player | One JSON value in the iOS Keychain (FSG plugin). The Keychain keeps it after the app is deleted. | Synchronous. Empty value gives an empty record. | Merges with the stored record (`+`), writes, fires `IapDataSavedSignal`. |
| [`WebStorage`](../Assets/ModulesPrivate/Services/Scripts/IapStorage/WebStorage.cs) | Commercial builds without `NO_INTERNET` and `IAP_DISABLED`, except the iOS player. In practice: Android. | One URL-safe Base64 JSON record per account ID on the game server. | Only when the account is connected; otherwise the callback is never called. | Reads the server record, merges, writes the result. |

No code calls `IIapStorage.Write` ([MP1](issues/modules_private.md#mp1-the-purchase-backup-is-never-written)). Both backends therefore only return records that older game versions wrote. `WebStorage` also has request-ordering and error-handling gaps that matter once `Write` is used ([MP6](issues/modules_private.md#mp6-webstorage-drops-callbacks-and-can-shrink-the-server-record)).

## Advertisements

| Class | Define | Status |
| --- | --- | --- |
| [`UnityAdsManager`](../Assets/ModulesPrivate/Services/Scripts/Advetisements/UnityAdsManager.cs) | `ADS_UNITY` | Live on Android and iOS (Unity Ads 4). |
| [`GoogleAdsManager`](../Assets/ModulesPrivate/Services/Scripts/Advetisements/GoogleAdsManager.cs) | `ADS_GOOGLE` | Not compiled with the current defines. Does not compile for iOS or desktop targets ([MP5](issues/modules_private.md#mp5-googleadsmanager-does-not-compile-outside-android)). |
| `AppodealAdsManager` | `ADS_APPODEAL` | Named in `ServicesInstaller`, but no such class exists. |

### Unity Ads

1. `Initialize` starts the SDK with the platform's game ID and test mode off. `OnInitializationComplete` sets `AdsEnabled`.
2. `ShowRewardedVideo`:
   - From `NotLoaded`, `LoadFailed`, or `ShowFailed`: set `Busy`, remember to play on load, and call `Advertisement.Load`.
   - `OnUnityAdsAdLoaded` sets `Ready` and, if remembered, shows the ad.
   - From `Ready`: show the ad.
   - In the editor, a 3-second timer fakes the load and the completion.
3. `OnUnityAdsShowComplete` sets `NotLoaded` and, when the state is `COMPLETED`, fires `RewardedVideoCompletedSignal`. [`IapStore`](../Assets/Scripts/Gui/StarMap/IapStore.cs) gives the ad loot and [`MultiplayerWindow`](../Assets/Scripts/Gui/StarMap/Arena/MultiplayerWindow.cs) adds an arena ticket, each only while its window is active.
4. `ShowInterstitial` runs once at start-up from [`InitializationState`](../Assets/Scripts/GameStateMachine/States/InitializationState.cs) when the player has not removed ads. Before the SDK is ready it defers to `OnInitializationComplete`. It shows the interstitial placement without loading it first ([MP2](issues/modules_private.md#mp2-the-unity-ads-interstitial-path-is-broken)).

`Status` changes fire `AdStatusChangedSignal` directly.

### Google Mobile Ads

`GoogleAdsManager` starts the SDK only when `BundleValidator.IsValidBundle` is true, and marks requests as child-directed. It loads and shows a rewarded or interstitial ad in one step. SDK callbacks may arrive on a background thread, so status changes and rewards go through a `ConcurrentQueue` that `Tick` drains on the main thread.

## Music bundle download

[`AssetLoader`](../Assets/ModulesPrivate/Services/Scripts/Assets/AssetLoader.cs) implements `IAssetLoader` in every commercial build.

| Build | Source |
| --- | --- |
| Standalone, or `NO_INTERNET` | `StreamingAssets/musicbundle`, like the public `LocalAssetLoader`. |
| WebGL, Android, iOS (and the editor for those targets) | Downloads a platform-specific bundle from the game server with `LoadFromCacheOrDownload` and cache version `1`. |

Both paths load the `Playlist` asset (`MusicPlaylist`) and pass it to `MusicPlayer`. On failure they log and never call back ([MP9](issues/modules_private.md#mp9-assetloader-never-reports-a-failure-and-never-refreshes-the-cache)). The download fires `AssetLoaderStatusChangedSignal` (`Busy`, `Idle`). The server side is described in [Server: Asset bundles](server.md#asset-bundles).

## Server requests

[`RequestFactory`](../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs) builds every request to the game server. The arena requests are UniRx `ObservableWWW.Post` calls with a `WWWForm`. The other requests are `UnityEngine.WWW` GET requests with the data in the query string.

| Method | Purpose | Caller |
| --- | --- | --- |
| `CreateGetPlayerInfoRequest(playerId)` | Read the player's arena record. | [`PlayerInfo`](../Assets/Scripts/GameServices/OfflineMultiplayer/PlayerInfo.cs) |
| `CreateGetTimeFromLastFightRequest(playerId)` | Seconds since the last arena fight. | `PlayerInfo` |
| `CreateRegisterPlayerRequest(playerId, name, rating, aiRating, fleet)` | Create or update the arena record with the serialized fleet. Fails locally above 16,384 characters. Sends `AppConfig.versionCode`. | `PlayerInfo` |
| `CreateFindOpponentRequest(playerId, rating)` | Matchmaking. Sends `AppConfig.versionCode`. | `PlayerInfo` |
| `CreateGetFleetRequest(enemyId)` | Download an opponent fleet. | [`EnemyPlayerInfo`](../Assets/Scripts/GameServices/OfflineMultiplayer/EnemyPlayerInfo.cs) |
| `CreateUpdateStatsRequest(playerId, enemyId, result)` | Report a fight result. | `PlayerInfo` |
| `CreateGetItemsRequest(id)`, `CreateSetItemsRequest(id, data)` | Developer item transfer. | [`Cheats`](../Assets/Scripts/Legacy/Debug/Cheats.cs) |
| `CreateGetIapDataRequest(id)`, `CreateSetIapDataRequest(id, data)` | Purchase backup. | `WebStorage` |

The arena client is described in [Application layer: Game services](application_layer.md#game-services-gameservices). The endpoints, field names, and response formats are described in [Server](server.md). The requests carry no authentication ([MP15](issues/modules_private.md#mp15-server-requests-are-not-authenticated)), and two of them encode the player ID twice ([MP12](issues/modules_private.md#mp12-request-fields-are-encoded-inconsistently)).

## iOS plugins

Versions, licenses, and upgrade notes belong to [Plugins](plugins.md). This section lists only how the game uses them.

| Plugin | Used by | Notes |
| --- | --- | --- |
| FSG iOS Keychain ([`Keychain.cs`](../Assets/ModulesPrivate/Plugins/FSG/iOSKeychainPlugin/Scripts/Keychain.cs)) | `KeyChainStorage` | Native `.mm` bridge on iOS and tvOS players. Elsewhere it falls back to an encrypted file in `persistentDataPath`. An editor post-processor adds the native files to the Xcode project. |
| Stan's Assets iOS Native 9.11 | `GameCenterAccount` (`GameCenterManager`), `AppleCloudStorage` (`ISN_GameSaves`, `GK_SavedGame`) | [`IOSNativeSettings.asset`](../Assets/ModulesPrivate/Plugins/StansAssets/Support/Settings/Resources/IOSNativeSettings.asset) enables only the Game Center and CloudKit modules. The native sources are stored as `.mm.txt` in `Support/NativeLibraries/IOS/`. The install or update action of the iOS Native settings inspector copies the enabled ones to `Assets/Plugins/IOS/` as `.mm` files. Git ignores that folder ([MP13](issues/modules_private.md#mp13-the-ios-native-sources-are-not-in-git)). `IOSNativePostProcess` adds frameworks and plist keys to the iOS Deploy settings. |
| Stan's Assets iOS Deploy | Build pipeline only | `ISD_PostProcess` runs after an iOS build. It calls a Python script to add frameworks, libraries, and flags to the Xcode project, then writes the plist variables into `Info.plist`. The script path is wrong ([MP14](issues/modules_private.md#mp14-ios-deploy-calls-a-python-script-that-does-not-exist)). |

The Stan's Assets runtime assembly has no platform filter, so its 227 scripts compile into every player ([MP18](issues/modules_private.md#mp18-the-ios-plugins-compile-into-every-platform)).

## Legacy and unused code

| Code | State |
| --- | --- |
| `CertificateChecker`, `EncryptedStrings`, `JavaClassWrapper`, `JavaObjectWrapper`, `JavaObjectArrayWrapper` | No callers. An Android signing-certificate check that was disconnected. |
| `GameDataStub` | No callers. |
| `UnityEngineThread` | A component in [`CommonGuiScene`](../Assets/Scenes/CommonGuiScene.unity). Only the error paths of `GoogleCloudStorage` use it to reach the main thread. Before the component exists, `Execute` runs the action at once. |
| `GoogleAdsManager`, `BundleValidator` | Compiled only with `ADS_GOOGLE`, which no platform defines. |
| `IapPurchaseProcessor.GetName`, `GetDescription`, `IsConsumable` | No callers. `GetName` and `GetDescription` return the ID. |
| `ModPrivateKey.Export` | Commented out. |

See [MP17](issues/modules_private.md#mp17-dead-code).

## Working on the private services

### Adding or replacing a platform service

1. Extend the contract in `ServicesFacade` and add a stub in `Services` first. See [Services: Adding a platform service](services.md#adding-a-platform-service).
2. Put the implementation in `Assets/ModulesPrivate/Services/Scripts/<Area>/`, namespace `Services.<Area>`.
3. Guard SDK-specific code with the platform symbol (for example `#if UNITY_ANDROID && !NO_GPGS`). `ServicesPrivate` compiles on every platform.
4. If the class needs an SDK assembly, add the reference to `ServicesPrivate.asmdef`.
5. Add the `#if` branch in `ServicesInstaller` after the `LICENSE_OPENSOURCE` branch.
6. Commit inside the submodule, then commit the new submodule pointer in the main repository.

### Adding a public type to `Security`

Add the same public type to `SecurityStub` with safe no-op behaviour. Otherwise open-source builds stop compiling. Callers must not depend on behaviour that only the commercial version has.

### Rotating the mod key

Replace `ModPrivateKey.cs` here and `ModPublicKey.cs` in the database editor together. See [Database Editor: Public key](database/editor.md#public-key).

### What must stay out of public files

Keep these only in the submodule: the mod private key, ad game IDs and unit IDs, store product IDs, server addresses, the expected application hash, and the obfuscation tables.
