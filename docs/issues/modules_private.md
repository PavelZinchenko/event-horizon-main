# ModulesPrivate Issues

This file records problems in `Assets/ModulesPrivate/`: the `Security` and `ServicesPrivate` assemblies and the vendored iOS plugins. For the design, see [ModulesPrivate](../modules_private.md). Save and cloud storage defects in the same submodule are already recorded in [GameSession issues](game_session.md) (GS5.19–GS5.26, GS7, GS9) and are not repeated here.

The analysis was done by reading the code on 2026-10-12. No issue was reproduced on a device. Severity is High, Medium, or Low. Status is **Confirmed** (the code path is certain), **Confirmed (latent)** (certain, but the current defines or data do not trigger it), or **Plausible** (needs specific conditions or SDK behaviour that was not tested).

This page names no keys, addresses, or product IDs. Some structural items (MP15, MP16, MP19) describe weaknesses of a live service. Decide whether they belong in a public repository before you publish this file.

## Defects

### MP1. The purchase backup is never written

**Severity:** Medium. **Status:** Confirmed.

**Where.** `IIapStorage.Write` ([IIapStorage.cs:37](../../Assets/Modules/ServicesFacade/Scripts/IapStorage/IIapStorage.cs#L37)) has no caller. `GameDataManager` only calls `Read` ([GameDataManager.cs:60](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L60)). The helper that built an `IapData` from the session is commented out ([IIapStorage.cs:10](../../Assets/Modules/ServicesFacade/Scripts/IapStorage/IIapStorage.cs#L10)).

**Failure.**
1. A player buys star packs on Android or iOS.
2. The stars go into the save, but `WebStorage` and `KeyChainStorage` never receive a record.
3. The player reinstalls the game or moves to a new device. Stores do not restore consumable purchases, and the purchase backup holds only what older game versions wrote.
4. The purchased stars are lost. `IapDataSavedSignal` never fires.

**Fix.** After `TryProcessPurchase` succeeds in `OnPurchasePending`, call `IIapStorage.Write` with `PurchasedStars` and `SupporterPack` from the session. Fix [MP6](#mp6-webstorage-drops-callbacks-and-can-shrink-the-server-record) at the same time.

### MP2. The Unity Ads interstitial path is broken

**Severity:** Medium. **Status:** Plausible (show without load); Confirmed (latent) (reward on interstitial).

**Where.** [UnityAdsManager.cs:77](../../Assets/ModulesPrivate/Services/Scripts/Advetisements/UnityAdsManager.cs#L77) and [line 142](../../Assets/ModulesPrivate/Services/Scripts/Advetisements/UnityAdsManager.cs#L142). Caller: [InitializationState.cs:147](../../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L147).

**Failure.**
1. At start-up, a player without remove-ads gets `ShowInterstitial`. The SDK is not ready, so the call is deferred to `OnInitializationComplete`.
2. `ShowInterstitial` then calls `Advertisement.Show` for the interstitial placement. It never calls `Advertisement.Load` for it. Unity Ads 4 requires a load before a show, so the show is expected to fail with "not ready". The interstitial would then never appear.
3. The failure sets the shared `Status` to `ShowFailed`. The rewarded-ad UI sees a failed state although the player has not asked for a rewarded ad yet.
4. `OnUnityAdsShowComplete` does not check the placement. If an interstitial completes while the shop or the arena window is open, `RewardedVideoCompletedSignal` fires and the player gets ad loot or an arena ticket.

**Fix.** Load the interstitial placement and show it in `OnUnityAdsAdLoaded`. Keep a separate status for interstitials. Fire the reward only for the rewarded placement. Verify on a device first.

### MP3. An unknown product is confirmed without a reward

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [InAppPurchasing.cs:190](../../Assets/ModulesPrivate/Services/Scripts/InAppPurchasing/InAppPurchasing.cs#L190). The loop ignores the result of `TryProcessPurchase`, then `ConfirmPurchase` runs for the whole order.

**Failure.** If the store returns a product that `IapPurchaseProcessor` does not know (for example a product added in the store console before the client is updated), the processor logs an exception and gives nothing. The order is still confirmed, so the store treats it as delivered. A consumable is then consumed without a reward and cannot be restored. Today all fetched products are known.

**Fix.** Confirm the order only when every product in it was processed. Otherwise leave it pending, so it is delivered after an update.

### MP4. An unused star pack grants the wrong amount

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [IapPurchaseProcessor.cs:84](../../Assets/ModulesPrivate/Services/Scripts/InAppPurchasing/IapPurchaseProcessor.cs#L84). The smallest star pack adds 50 stars, the same as the next pack. `FetchProducts` does not request this product ([InAppPurchasing.cs:92](../../Assets/ModulesPrivate/Services/Scripts/InAppPurchasing/InAppPurchasing.cs#L92)).

**Failure.** None today. A restored or pending order for this old product would grant 50 stars instead of the amount in its name.

**Fix.** Decide whether the product is retired. If it is, remove it. If an old order can still arrive, grant the intended amount.

### MP5. `GoogleAdsManager` does not compile outside Android

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [GoogleAdsManager.cs:249](../../Assets/ModulesPrivate/Services/Scripts/Advetisements/GoogleAdsManager.cs#L249). The iOS branch declares both constants without a value. The fallback branch declares only the rewarded unit ID, but `LoadInterstitial` uses the interstitial ID too.

**Failure.** With `ADS_GOOGLE` defined, iOS, standalone, and WebGL targets fail to compile. No platform defines `ADS_GOOGLE` today.

**Fix.** Restore the iOS IDs in the private source, and declare both IDs (empty or test IDs) in the fallback branch. Or delete the class if Google Mobile Ads is retired.

### MP6. `WebStorage` drops callbacks and can shrink the server record

**Severity:** Low. **Status:** Confirmed (callback); Confirmed (latent) (record).

**Where.** [WebStorage.cs:21](../../Assets/ModulesPrivate/Services/Scripts/IapStorage/WebStorage.cs#L21), [line 37](../../Assets/ModulesPrivate/Services/Scripts/IapStorage/WebStorage.cs#L37), [line 87](../../Assets/ModulesPrivate/Services/Scripts/IapStorage/WebStorage.cs#L87).

**Failure.**
1. `Read` returns without calling back when the account is not connected. "Restore purchases" then does nothing and shows nothing.
2. `IsBusy` waits only one frame (`if`, not `while`), so two requests can overlap.
3. `Write` reads the server record first. A network error gives an empty text, which is treated as an empty record. The following write then replaces the server record with the local record only. With the `max` merge of `IapData`, a device with fewer purchased stars lowers the stored value.

Items 2 and 3 matter only after [MP1](#mp1-the-purchase-backup-is-never-written) is fixed.

**Fix.** Call back with an empty record or an error when not connected. Use `while (IsBusy)`. Do not write when the read failed (`www.error` is set).

### MP7. Game Center sign-in cannot be retried

**Severity:** Low. **Status:** Plausible.

**Where.** [GameCenterAccount.cs:33](../../Assets/ModulesPrivate/Services/Scripts/Account/GameCenterAccount.cs#L33). `SignIn` calls `GameCenterManager.Init` only when it is not initialized. `Init` sets the flag at once, also when authentication later fails.

**Failure.** After a cancelled or failed Game Center login, the status is `FailedToConnect`. Every later `SignIn` (for example from the arena sign-in dialog) does nothing, and `CanSignIn` still reports `true`. The status changes only if Game Center calls the authentication handler again by itself.

**Fix.** Report `CanSignIn = false` after the first attempt and tell the player to sign in through the iOS settings. Or re-read `GameCenterManager.IsPlayerAuthenticated` when the application becomes active.

### MP8. `SteamAccount` never reports a status change

**Severity:** Low. **Status:** Confirmed.

**Where.** [SteamAccount.cs:26](../../Assets/ModulesPrivate/Services/Scripts/Account/SteamAccount.cs#L26). The status is computed from `SteamManager.Initialized`. The injected `AccountStatusChangedSignal.Trigger` is never fired.

**Failure.** The arena reads the status directly, so start-up works. Listeners that wait for the signal (`OfflineMultiplayer`, `SignInWindow`, `SignalsTranslator`) never see a Steam change. The first read of `Status` also creates the `SteamManager` object as a side effect.

**Fix.** Initialize `SteamManager` explicitly at start-up and fire the trigger once with the result.

### MP9. `AssetLoader` never reports a failure and never refreshes the cache

**Severity:** Low. **Status:** Confirmed.

**Where.** [AssetLoader.cs:20](../../Assets/ModulesPrivate/Services/Scripts/Assets/AssetLoader.cs#L20), [line 36](../../Assets/ModulesPrivate/Services/Scripts/Assets/AssetLoader.cs#L36).

**Failure.**
1. On a failed load or download, the callback is never called. `MusicPlayer` does not retry in the session (same as [S4](services.md#s4-open-source-builds-have-no-music-until-the-bundle-is-built) for the public loader). A player who starts the game offline has no default music until the next start.
2. The download uses cache version `1` for every build. A new bundle on the server reaches players only if the URL changes.

**Fix.** Call back with `null` on failure and let `MusicPlayer` retry later. Use a bundle version or hash from the server, or `UnityWebRequestAssetBundle` with a `Hash128`.

### MP10. The legacy mod stream ignores its checksum

**Severity:** Low. **Status:** Confirmed.

**Where.** [EncryptedStream.cs:22](../../Assets/ModulesPrivate/Security/Scripts/EncryptedStream.cs#L22). The stream stops one byte before the end and sums the bytes into `_checksumm`, but never reads or compares the last byte.

**Failure.** A damaged legacy packed mod is passed to the decompressor and the parser. It fails later with a less useful error, or loads with damaged content.

**Fix.** After the last data byte, read the checksum byte and throw `InvalidDataException` on a mismatch.

### MP11. Two assembly references do not resolve

**Severity:** Low. **Status:** Confirmed.

**Where.** [ServicesPrivate.asmdef](../../Assets/ModulesPrivate/Services/ServicesPrivate.asmdef) lists `GUID:4e20c7306599b442b84e265cb179eddc` and `GUID:1f613da493e88413786955ac61fc9673`. No `.asmdef` in `Assets/`, `Packages/`, or the package cache has these GUIDs.

**Failure.** Unity ignores them. The inspector shows missing references, and a reader cannot tell which SDK they were.

**Fix.** Remove both entries.

### MP12. Request fields are encoded inconsistently

**Severity:** Low. **Status:** Confirmed.

**Where.** [RequestFactory.cs:15](../../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L15) and [line 24](../../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L24) escape the player ID with `WWW.EscapeURL` before `WWWForm` encodes it again. The other arena requests pass the ID and the player name raw ([line 36](../../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs#L36)).

**Failure.** It works only because the server calls `urldecode` once more on every field. That second decode also turns `+` into a space in raw fields. A player name with `+` (or `%` followed by two hex digits) is stored changed.

**Fix.** Do not escape fields that go into a `WWWForm`, and remove the extra `urldecode` on the server in the same release. See [Server](../server.md).

### MP13. The iOS native sources are not in Git

**Severity:** Low. **Status:** Confirmed.

**Where.** Stan's Assets keeps its native code as `.mm.txt`. The iOS Native settings inspector copies the enabled modules to `Assets/Plugins/IOS/`. [.gitignore:22](../../.gitignore#L22) ignores `/Assets/Plugins/iOS/`, and the folder name matches on case-insensitive file systems.

**Failure.** A fresh clone with the submodule has no `ISN_GameCenter.mm`, `ISN_CloudKit.mm`, or `ISN_NativeCore.mm` until someone opens the iOS Native settings and runs the install or update. An iOS build before that step is expected to fail at link time, because the `DllImport("__Internal")` functions have no implementation.

**Fix.** Document the step in the iOS build notes, or track the generated files, or run the installer from a pre-build hook.

### MP14. iOS Deploy calls a Python script that does not exist

**Severity:** Low. **Status:** Confirmed (path); Plausible (impact).

**Where.** [ISD_PostProcess.cs:98](../../Assets/ModulesPrivate/Plugins/StansAssetsEditor/Modules/IOSDeploy/Editor/ISD_PostProcess.cs#L98) builds the path `Assets/ModulesPrivate/Plugins/StansAssets/Modules/IOSDeploy/Scripts/Editor/post_process.py`. The script is in `Plugins/StansAssetsEditor/Modules/IOSDeploy/Editor/`. The code runs `python` and does not check the exit code.

**Failure.** After an iOS build the framework, library, and flag changes from the iOS Deploy settings are not applied, without an error in the Unity console. The plist changes still apply, because they are C#. Modern Xcode may link system frameworks automatically, so the build can still succeed; this was not tested.

**Fix.** Point the path at the real script and check the exit code. Better: replace the Python step with Unity's `UnityEditor.iOS.Xcode.PBXProject` API.

## Structural issues

### MP15. Server requests are not authenticated

**Problem.** The class is named `Security.RequestFactory`, but it only builds plain forms and query strings. Requests carry no signature, token, or platform proof of identity. The server trusts the player ID that the client sends. The purchase-backup requests put the data in a GET query string. All requests use the obsolete `UnityEngine.WWW`.

**Evidence.** [RequestFactory.cs](../../Assets/ModulesPrivate/Security/Scripts/RequestFactory.cs); [Services: SecurityStub](../services.md#securitystub).

**Direction.** Verify the platform identity on the server (Play Games server auth code, Game Center identity verification, Steam session ticket) before arena writes and purchase-backup reads or writes. Move to `UnityWebRequest` and POST bodies. Details belong in [Server](../server.md).

### MP16. Purchases are not validated

**Problem.** `OnPurchasePending` grants the reward as soon as the store reports a pending order. The client does not check the receipt, locally or on a server.

**Evidence.** [InAppPurchasing.cs:186](../../Assets/ModulesPrivate/Services/Scripts/InAppPurchasing/InAppPurchasing.cs#L186).

**Direction.** Validate receipts before `TryProcessPurchase`: Unity IAP's local receipt validation (with the store's public keys) at minimum, or a server check.

### MP17. Dead code

**Problem.** Several private classes have no live caller.

**Evidence.**

| Code | State |
| --- | --- |
| [`CertificateChecker`, `EncryptedStrings`](../../Assets/ModulesPrivate/Security/Scripts/CertificateChecker.cs), [`JavaWrappers`](../../Assets/ModulesPrivate/Security/Scripts/JavaWrappers.cs) | No callers. |
| [`GameDataStub`](../../Assets/ModulesPrivate/Services/Scripts/Storage/GameDataStub.cs) | No callers. |
| `IapPurchaseProcessor.GetName`, `GetDescription`, `IsConsumable` | No callers. |
| [`GoogleAdsManager`](../../Assets/ModulesPrivate/Services/Scripts/Advetisements/GoogleAdsManager.cs), [`BundleValidator`](../../Assets/ModulesPrivate/Security/Scripts/BundleValidator.cs) | Need `ADS_GOOGLE`, which no platform defines. |
| `ADS_APPODEAL` branch in `ServicesInstaller` | Names `AppodealAdsManager`, which does not exist. |
| [`UnityEngineThread`](../../Assets/ModulesPrivate/Services/Scripts/Legacy/UnityEngineThread.cs) | Used only in error paths of `GoogleCloudStorage`. |

**Direction.** Delete the unused classes after the owner reviews each one. If Google Mobile Ads stays as an option, fix [MP5](#mp5-googleadsmanager-does-not-compile-outside-android) first.

### MP18. The iOS plugins compile into every platform

**Problem.** `StansAssets.asmdef` and `iOSKeychainPlugin.asmdef` have no platform filter. About 30,000 lines of iOS-only runtime code compile into Android, Windows, macOS, and WebGL players. `GameCenterAccount` and `AppleCloudStorage` have no `UNITY_IOS` guard, so the filter cannot be added without changing them. Stan's Assets iOS Native is version 9.11 from 2018. Only its Game Center and saved-games parts are used.

**Evidence.** [StansAssets.asmdef](../../Assets/ModulesPrivate/Plugins/StansAssets/StansAssets.asmdef), [IOSNativeSettings.asset](../../Assets/ModulesPrivate/Plugins/StansAssets/Support/Settings/Resources/IOSNativeSettings.asset).

**Direction.** Guard the two classes with `UNITY_IOS`, then limit both plugin assemblies to iOS and the editor. In the long term, replace iOS Native with Apple's Unity GameKit plugin. See [Plugins](../plugins.md).

### MP19. Mod confidentiality depends on a key inside the client

**Problem.** Authenticated packed mods are decrypted on the device, so every commercial build contains the RSA private key in obfuscated form. A key extracted from any build decrypts every mod made for that key. The key cannot be rotated without breaking all existing mods, because the format has no key ID.

**Evidence.** [ModPrivateKey.cs](../../Assets/ModulesPrivate/Security/Scripts/ModPrivateKey.cs); [Database Editor: What it protects](../database/editor.md#what-it-protects).

**Direction.** Treat the encryption as protection against casual copying only, and say so in the mod documentation. Add a key ID to the next format version, so a leaked key can be replaced while old mods still load.
