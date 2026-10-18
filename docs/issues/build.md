# Build Pipeline Issues

This file records problems in the build settings, the build hooks in [`Assets/Editor/`](../../Assets/Editor/), and the package setup. For the design, see [Build pipeline](../build.md). Problems of the Application Manager window are in [AppConfiguration issues](app_configuration.md); problems of the Android templates and plugin copies are in [Plugin issues](plugins.md).

The analysis was done by reading the code and settings on 2026-10-12. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain.
- **Confirmed (latent)** – the code path is certain, but current data or settings do not trigger it.
- **Plausible** – the code is wrong, but the failure needs specific conditions.

## Defects

### BL1. `AppConfig.bundleIdentifier` comes from the last built platform

**Severity:** Medium. **Status:** Confirmed.

**Where.** `GenerateAppConfigFile` writes `PlayerSettings.applicationIdentifier` for the selected platform ([ApplicationManager.cs:346](../../Assets/Editor/ApplicationManager.cs#L346)). The post-build hook calls it after every build ([AC1](app_configuration.md#ac1-the-build-number-is-incremented-after-the-build)). [`AndroidRateGameService`](../../Assets/Scripts/Services/RateGame/RateGameService.cs#L23) builds the store URL from the constant.

**Failure.**
1. A Standalone build regenerates `AppConfig.cs` with `unity.ZipasGames.Event Horizon`. The working copy has this value now.
2. The next Android build compiles that value before its own post-build hook runs.
3. The **Rate** button in that Android build opens `market://details?id=unity.ZipasGames.Event Horizon`. Google Play shows "item not found".

**Fix.** Use `Application.identifier` at run time and remove `bundleIdentifier` from `AppConfig`. Until then, press **Update config file** after every platform switch.

### BL2. The iOS rate URL uses the bundle ID

**Severity:** Low. **Status:** Confirmed.

**Where.** [`IosRateGameService.RateGame`](../../Assets/Scripts/Services/RateGame/RateGameService.cs#L33) opens `itms-apps://itunes.apple.com/app/` + `AppConfig.bundleIdentifier`.

**Failure.** App Store URLs need the numeric Apple ID (`.../app/id<number>`). With `com.ZipasGames.EventHorizon`, the App Store opens no product page.

**Fix.** Store the numeric App Store ID in configuration and use `itms-apps://itunes.apple.com/app/id<number>?action=write-review`. Or use `UnityEngine.iOS.Device.RequestStoreReview()`.

### BL3. The committed project settings are older than the working copy

**Severity:** Medium. **Status:** Confirmed.

**Where.** [ProjectSettings.asset](../../ProjectSettings/ProjectSettings.asset). `git diff` shows 92 changed lines after the Unity 6 upgrade commit.

**Failure.** A fresh clone builds with the committed values:

| Setting | Committed | Working copy |
| --- | --- | --- |
| `bundleVersion` / Android version code | `1.12.1` / `112100` | `1.13.1` / `113013` |
| Android min / target SDK | 22 / 33 | 23 / 35 |
| Android R8 minify (release) | off | on |
| Custom ProGuard file | off | on |
| WebGL compression | Brotli | disabled |
| iOS target | 12.0 | 13.0 |

Google Play rejects new uploads with target SDK 33. With the custom ProGuard file off, the keep rules in `proguard-user.txt` are not used.

**Fix.** Review the diff and commit `ProjectSettings/`. Decide on the WebGL compression on purpose: "disabled" avoids server headers but makes the download several times larger.

### BL4. The Android orientation hook edits every manifest

**Severity:** Low. **Status:** Plausible.

**Where.** [ApplicationBuildProcessor.cs:13–21](../../Assets/Editor/ApplicationBuildProcessor.cs#L13) searches all subfolders of the Gradle project for `AndroidManifest.xml` and replaces every `android:screenOrientation` value.

**Failure.**
1. Library manifests (Play Games, Unity Ads, and other AARs unpacked into the project) get `sensorLandscape` too. An activity of a library that needs portrait shows in landscape.
2. `sensorLandscape` ignores the device rotation lock. The Player Settings (auto-rotation, landscape left and right) would respect it.
3. The hook duplicates the Player Settings orientation, so the two can disagree without notice.

**Fix.** Remove the hook and rely on the Player Settings. If the hook is needed, edit only `unityLibrary/src/main/AndroidManifest.xml` and only the Unity activity.

### BL5. The push notification remover is probably a no-op

**Severity:** Low. **Status:** Plausible.

**Where.** [MissingPushNotificationEntitlementRemover.cs](../../Assets/Editor/MissingPushNotificationEntitlementRemover.cs).

**Failure.**
1. The `UnityPre550Remover` branch is dead: `UNITY_5_5_OR_NEWER` is always defined.
2. Recent Unity versions probably write `UNITY_USES_REMOTE_NOTIFICATIONS 1` only when the project uses remote notifications. No game code does. If so, the regex finds nothing, and the hook only logs two lines.

**Fix.** Make an iOS build without the hook and check `Classes/Preprocessor.h`. If the value is `0`, delete the script.

### BL6. Dead editor files

**Severity:** Low. **Status:** Confirmed.

| File | Why it is dead |
| --- | --- |
| [`TexturePostProcessor.cs`](../../Assets/Editor/TexturePostProcessor.cs) (class `CollisionMapGenerator`) | Acts only on textures whose sprite packing tag ends with `.dither`. No `.meta` file has such a tag. The packing tag belongs to the legacy sprite packer. The class name does not match the file or the behavior. |
| [`mod_pbxproj.py`](../../Assets/Editor/mod_pbxproj.py), `mod_pbxproj.pyc` | A Python 2 library for Xcode projects. No script calls it. The compiled `.pyc` file is tracked in Git. |

**Fix.** Delete the files and their `.meta` files.

## Structural issues

### BL7. No reproducible build

**Severity:** Medium. **Status:** Confirmed.

**Evidence.**
1. No build script, no CI configuration, and no `BuildPipeline.BuildPlayer` call exist in the repository. The Unity Cloud Build webhook on the server is probably broken ([SV9](server.md#sv9-the-cloud-build-webhook-is-unauthenticated-and-probably-broken)).
2. A build depends on git-ignored files: the keystore, `steam_appid.txt`, the commercial plugins ([PL1](plugins.md#pl1-the-commercial-build-needs-git-ignored-plugins)), and the music bundle ([S4](services.md#s4-open-source-builds-have-no-music-until-the-bundle-is-built)).
3. The version changes after the build, not before ([AC1](app_configuration.md#ac1-the-build-number-is-incremented-after-the-build)), and depends on the last built platform ([BL1](#bl1-appconfigbundleidentifier-comes-from-the-last-built-platform)).
4. Define symbols change through a window, not per build profile.

**Direction.** Add one editor build method per platform that sets the define symbols, generates `AppConfig.cs` before compilation, and calls `BuildPipeline.BuildPlayer`. Unity 6 build profiles can hold the define symbols per profile. Run the methods from the command line (`-executeMethod`) on CI.

### BL8. Unused packages and modules

**Severity:** Low. **Status:** Confirmed.

**Where.** [manifest.json](../../Packages/manifest.json).

**Evidence.**
1. `com.unity.multiplayer.center` has no users. The Unity 6 upgrade added it, and with it the untracked `ProjectSettings/MultiplayerManager.asset`.
2. `com.unity.ui` (UI Toolkit) has one possible user: `SystemFontLocator` imports `UnityEngine.UIElements`. The UI Toolkit migration is suspended.
3. Built-in modules with no users in the code: `ai`, `cloth`, `director`, `terrain`, `terrainphysics`, `tilemap`, `umbra`, `vehicles`, `video`, `vr`, `wind`, `xr`.

Engine code stripping removes unused modules from IL2CPP builds, so the cost is editor import time and noise.

**Direction.** Remove `com.unity.multiplayer.center` and the unused modules in **Package Manager > Built-in**. Check whether `SystemFontLocator` compiles without `com.unity.ui` before you remove it.

### BL9. Stale `.csproj` files in the project root

**Severity:** Low. **Status:** Confirmed.

**Where.** The project root, on a developer machine. `.gitignore` excludes `*.csproj`, so the files are not in Git.

**Evidence.** These project files have no matching `.asmdef`: `CombatSimulator`, `GameEconomy`, `GameLogic`, `ShipConstructor2`, `ShipModelGenerator`, `StarMap`, `Unity.ScriptableBuildPipeline`, `Unity.ScriptableBuildPipeline.Editor`, `UnityEditor.CacheServer`, `VorbisPluginImpl`, `Zenject-UnitTests-Editor`. Unity regenerates project files but does not delete old ones. The IDE still shows them, and they misled the [project structure](../project_structure.md) page, which lists `ShipConstructor2` and `ShipModelGenerator` as modules.

**Direction.** Delete all `*.csproj` files in the root and press **Preferences > External Tools > Regenerate project files**.
