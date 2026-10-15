# Build Pipeline

The project has no build script and no CI. A developer makes every player build by hand in the Unity Editor. A few editor scripts take part: one window sets the define symbols and the version, and three build hooks change the version file and the generated native projects. This page describes the settings, hooks, signing, packages, and editor tools that a build depends on.

| | |
| --- | --- |
| Unity version | 6000.0.75f1 ([ProjectVersion.txt](../ProjectSettings/ProjectVersion.txt)) |
| Settings | [ProjectSettings.asset](../ProjectSettings/ProjectSettings.asset), [EditorBuildSettings.asset](../ProjectSettings/EditorBuildSettings.asset) |
| Packages | [Packages/manifest.json](../Packages/manifest.json), [packages-lock.json](../Packages/packages-lock.json) |
| Editor scripts | [`Assets/Editor/`](../Assets/Editor/) (9 C# files, compiled into `Assembly-CSharp-Editor`) |
| Platforms | Android (Google Play), iOS (App Store), Windows/Linux/macOS (Steam), WebGL |
| Outputs | `Builds/` (git-ignored, local) |
| Known issues | [Build issues](issues/build.md) |

## Contents

1. [Terms](#terms)
2. [Build flow](#build-flow)
3. [Platform settings](#platform-settings)
4. [Scenes in the build](#scenes-in-the-build)
5. [Build hooks](#build-hooks)
6. [Signing and platform IDs](#signing-and-platform-ids)
7. [Package dependencies](#package-dependencies)
8. [Editor tools](#editor-tools)
9. [Build outputs](#build-outputs)
10. [How to make a build](#how-to-make-a-build)
11. [How to add a build hook](#how-to-add-a-build-hook)

## Terms

- **Player build** – the output of **File > Build Profiles** for one platform: an APK/AAB, an Xcode project, a desktop executable, or a WebGL folder.
- **Define symbol** – a scripting define in `ProjectSettings.asset` (for example `STORE_GOOGLE`). The installers use it to select service implementations. See [AppConfiguration: Scripting define symbols](app_configuration.md#scripting-define-symbols).
- **Build hook** – an editor method that Unity calls during a build. This project uses `[PostProcessBuild]` and `IPostGenerateGradleAndroidProject`.
- **Gradle template** – a file in `Assets/Plugins/Android/` that replaces the Gradle file Unity would generate. See [Plugins: Android build templates](plugins.md#android-build-templates-and-external-dependency-manager).
- **IL2CPP** – the Unity scripting backend that converts C# to C++ before native compilation.

Values on this page come from the working copy on 2026-10-12. `ProjectSettings.asset` has uncommitted changes from the Unity 6 upgrade, and the committed file still has the older values (version 1.12.1, Android SDK 22/33). See [BL3](issues/build.md#bl3-the-committed-project-settings-are-older-than-the-working-copy).

## Build flow

1. The developer selects the platform. Each platform has its own define symbols in `ProjectSettings.asset`.
2. The developer opens **Window > Application Manager** and checks the store, IAP, ads, and license symbols. The window rewrites the symbols for the selected platform. See [AppConfiguration: Application Manager window](app_configuration.md#application-manager-window).
3. The developer presses **Update config file**. The window regenerates `AppConfig.cs` from the current `PlayerSettings`.
4. For the music bundle, the developer runs **Assets > Build AssetBundles** and uploads the bundle. See [Server: Asset bundles](server.md#asset-bundles).
5. Unity builds the player. On Android, External Dependency Manager resolves the Gradle dependencies first, and `ApplicationBuildProcessor` edits the generated manifests.
6. After the build, `ApplicationManager.OnPostBuild` increases the build number and rewrites `AppConfig.cs`. On iOS, `MissingPushNotificationEntitlementRemover` edits the Xcode project.
7. The developer signs and uploads the output to the store by hand.

Step 6 runs after the scripts are compiled. The build that just finished therefore contains the previous build number and the previous `AppConfig` values ([AC1](issues/app_configuration.md#ac1-the-build-number-is-incremented-after-the-build), [BL1](issues/build.md#bl1-appconfigbundleidentifier-comes-from-the-last-built-platform)).

## Platform settings

Common settings: company `ZipasGames`, product `Event Horizon`, version `1.13.1`, API compatibility `.NET Standard 2.1`, active input handling `Input System Package (New)`, engine code stripping on.

| | Android | iOS | Standalone | WebGL |
| --- | --- | --- | --- | --- |
| Store symbol | `STORE_GOOGLE` | `STORE_APPLE` | `STORE_STEAM` | `STORE_WEB` |
| Application ID | `com.ZipasGames.EventHorizon` | `com.ZipasGames.EventHorizon` | `unity.ZipasGames.Event Horizon` | – |
| Scripting backend | IL2CPP | IL2CPP | IL2CPP | IL2CPP (fixed) |
| Graphics API | Automatic (Vulkan, OpenGL ES 3) | Metal | Automatic (Windows: Direct3D 11) | WebGL 2 |
| Other | Version code `113013`; min SDK 23, target SDK 35; ARMv7 + ARM64; R8 minify in release; custom manifest, main Gradle, properties, settings, and ProGuard files | Target iOS 13.0; manual signing; requires full screen | Full-screen window by default; 1024×768 and resizable when windowed; `steam_appid.txt` for local runs | Template `PROJECT:FullscreenWindow`; compression disabled; memory 32 MB initial, 2048 MB maximum; no threads |

All platforms allow landscape left and right only. Android additionally forces `sensorLandscape` through a build hook ([Build hooks](#build-hooks)).

The define symbols per platform are listed in [AppConfiguration: Scripting define symbols](app_configuration.md#scripting-define-symbols). Every platform uses `LICENSE_COMMERCIAL`, so a build needs the private submodule and the git-ignored plugins ([AC3](issues/app_configuration.md#ac3-checked-in-defines-need-the-private-submodule), [PL1](issues/plugins.md#pl1-the-commercial-build-needs-git-ignored-plugins)).

The `Standalone` application ID is the Unity default pattern. Steam does not use it.

## Scenes in the build

[EditorBuildSettings.asset](../ProjectSettings/EditorBuildSettings.asset) lists 11 scenes, all enabled. Index 0 is the entry scene.

| Index | Scene |
| --- | --- |
| 0 | `Assets/Scenes/LoaderScene.unity` |
| 1 | `Assets/Scenes/CommonGuiScene.unity` |
| 2 | `Assets/Scenes/MainMenuScene.unity` |
| 3 | `Assets/Scenes/StarMapScene.unity` |
| 4 | `Assets/Scenes/SkillTreeScene.unity` |
| 5 | `Assets/Scenes/ConfigureControlsScene.unity` |
| 6 | `Assets/Scenes/CombatScene.unity` |
| 7 | `Assets/Scenes/ExplorationScene.unity` |
| 8 | `Assets/Scenes/EhopediaScene.unity` |
| 9 | `Assets/ModulesShared/ShipEditor/Scenes/ShipEditorScene.unity` |
| 10 | `Assets/Scenes/SettingsScene.unity` |

The same file sets the Input System settings asset (`com.unity.input.settings`). `BaseTestScene` is not in the build. For the game states that load each scene, see [Application layer](application_layer.md).

## Build hooks

| Hook | Type | Platform | What it does |
| --- | --- | --- | --- |
| [`ApplicationManager.OnPostBuild`](../Assets/Editor/ApplicationManager.cs#L331) | `[PostProcessBuild]` | All | Adds 1 to `buildNumber` and regenerates `AppConfig.cs` from the current `PlayerSettings`. See [AppConfiguration: Post-build hook](app_configuration.md#post-build-hook). |
| [`ApplicationBuildProcessor`](../Assets/Editor/ApplicationBuildProcessor.cs) | `IPostGenerateGradleAndroidProject` | Android | Finds every `AndroidManifest.xml` in the generated Gradle project. Replaces each `android:screenOrientation` value with `sensorLandscape`. |
| [`MissingPushNotificationEntitlementRemover`](../Assets/Editor/MissingPushNotificationEntitlementRemover.cs) | `[PostProcessBuild]` | iOS | Replaces `define UNITY_USES_REMOTE_NOTIFICATIONS 1` with `0` in `Classes/Preprocessor.h`. This removes the App Store warning about a missing push entitlement. No game code registers for notifications; the StansAssets notification controllers are unused. |
| External Dependency Manager | Package | Android | Resolves the Gradle dependencies and writes them into the Gradle templates. See [Plugins](plugins.md#android-build-templates-and-external-dependency-manager). |

`sensorLandscape` uses the sensor to choose between the two landscape directions, and ignores the device rotation lock. The hook changes every manifest in the Gradle project, including library manifests ([BL4](issues/build.md#bl4-the-android-orientation-hook-edits-every-manifest)).

[`TexturePostProcessor.cs`](../Assets/Editor/TexturePostProcessor.cs) is an import hook, not a build hook. It dithers textures to RGBA4444 when their sprite packing tag ends with `.dither`. No texture has such a tag ([BL6](issues/build.md#bl6-dead-editor-files)).

## Signing and platform IDs

No passwords or keys are in the repository.

| Platform | Mechanism |
| --- | --- |
| Android | Custom keystore `license.keystore` in the project root (`{inproject}` path in `ProjectSettings.asset`). `.gitignore` excludes the file. Unity does not save the keystore and key passwords; the developer enters them once per Editor session. |
| iOS | Manual signing. The developer team ID is in `ProjectSettings.asset`; the provisioning profile is selected in Xcode. |
| Steam | `steam_appid.txt` in the project root, git-ignored. Steamworks.NET writes it with the test app ID when it is missing. See [Plugins: Steamworks.NET](plugins.md#steamworksnet). |
| Google Play Games | App ID in [GooglePlayGameSettings.txt](../ProjectSettings/GooglePlayGameSettings.txt). The setup window generates the git-ignored `GooglePlayGamesManifest.androidlib`. |

`cloudProjectId` in `ProjectSettings.asset` links the project to Unity Cloud services (Ads, IAP, and in the past Cloud Build).

## Package dependencies

[manifest.json](../Packages/manifest.json) lists 11 packages and 32 built-in modules. One scoped registry (OpenUPM) serves the `com.google` scope.

| Package | Version | Used by |
| --- | --- | --- |
| `com.google.external-dependency-manager` | 1.2.187 | Android Gradle dependency resolution. See [Plugins](plugins.md#android-build-templates-and-external-dependency-manager). |
| `com.unity.2d.sprite` | 1.0.0 | Sprite Editor (editor only). |
| `com.unity.ads` | 4.17.0 | Unity Ads (`ADS_UNITY`). See [Plugins](plugins.md). |
| `com.unity.ide.visualstudio` | 2.0.27 | IDE integration; generates the `.csproj` files. |
| `com.unity.inputsystem` | 1.19.0 | All input. See [GameServices](game_services.md). |
| `com.unity.multiplayer.center` | 1.0.0 | Nothing. The Unity 6 upgrade added it ([BL8](issues/build.md#bl8-unused-packages-and-modules)). |
| `com.unity.purchasing` | 5.3.1 | Unity IAP (`IAP_UNITY`). See [Plugins](plugins.md). |
| `com.unity.test-framework` | 1.6.0 | Editor tests (`*.Tests` assemblies, Unity Test Runner). |
| `com.unity.toolchain.win-x86_64-linux-x86_64` | 2.0.11 | IL2CPP Linux builds from a Windows Editor. |
| `com.unity.ugui` | 2.0.0 | All game UI. See [Gui module](gui.md). |
| `com.unity.ui` | 2.0.0 | UI Toolkit. Only `SystemFontLocator` imports `UnityEngine.UIElements` (it creates `FontAsset` objects). |

Most game plugins (Zenject, UniRx, Google Play Games, Steamworks.NET, BouncyCastle, NativeFilePicker) are copies in `Assets/`, not packages. See [Plugins](plugins.md).

## Editor tools

| Tool | Menu or trigger | Purpose | Details |
| --- | --- | --- | --- |
| [`ApplicationManager`](../Assets/Editor/ApplicationManager.cs) | **Window > Application Manager** | Define symbols, version, `AppConfig.cs`, ship prefab generation. | [AppConfiguration](app_configuration.md#application-manager-window) |
| [`CreateAssetBundles`](../Assets/Editor/CreateAssetBundles.cs) | **Assets > Build AssetBundles** | Builds all asset bundles into `Assets/StreamingAssets/`. | [Server: Asset bundles](server.md#asset-bundles) |
| [`CodeGenerator`](../Assets/Editor/CodeGenerator.cs) | **Window > Code Generator** | Shows the developer console codes for a device ID. | [ModulesPrivate](modules_private.md), [Services](services.md) |
| [`StringEncryptorWindow`](../Assets/Editor/StringEncryptorWindow.cs) | **Window > String Encryptor** | Encrypts and decrypts obfuscated string constants. | [ModulesPrivate](modules_private.md) |
| [`FindMissingScriptsRecursively`](../Assets/Editor/FindMissingScripts.cs) | **Window > FindMissingScriptsRecursively** | Logs every component with a missing script in the selected objects and their children. | – |
| [`SkillTreeNodeEditor`](../Assets/Editor/SkillTree/SkillTreeNodeEditor.cs) | Inspector of `SkillTreeNode` | Buttons **Connect**, **Disconnect**, **Clear**, **Refresh** for the links between selected skill nodes in `SkillTreeScene`. Draws the links as dotted lines in the Scene view. | [Application layer](application_layer.md) |
| [`ColorTable.colors`](../Assets/Editor/ColorTable.colors) | Color picker | A swatch library with 6 preset colors for the Unity color picker. | – |
| [`PolygonCollider2DOptimizer`](../Assets/Tools/PolygonCollider2DOptimizer/Editor/PolygonCollider2DOptimizer.cs) | **Window > PolygonColliderOptimizer** | Reduces the points of a `PolygonCollider2D`. | [Plugins](plugins.md) |
| Database editor | `Assets/Modules/Database/editor.cmd` | External editor for the game database. | [Database editor](database/editor.md) |

[`mod_pbxproj.py`](../Assets/Editor/mod_pbxproj.py) and its compiled `mod_pbxproj.pyc` are a Python 2 library that edits Xcode projects. No script calls them ([BL6](issues/build.md#bl6-dead-editor-files)).

## Build outputs

`Builds/` is git-ignored. The local convention is one folder per target:

| Folder | Content |
| --- | --- |
| `Android/` | `.apk` and `.aab` files, R8 mapping files, `adb` helper scripts. |
| `iOS/` | The generated Xcode project. |
| `Windows/`, `Windows-release/`, `Windows.mono/` | Desktop builds. By its name, `Windows.mono` is a Mono backend build. |
| `Linux/`, `Mac/` | Desktop builds. |
| `Web/` | WebGL build. |

IL2CPP writes a `*_BackUpThisFolder_ButDontShipItWithYourGame` folder next to each output. It holds debug symbols for crash reports. Keep it, but do not upload it to a store.

## How to make a build

1. Initialize the submodules, including `Assets/ModulesPrivate`. For a public clone, switch the license to `LICENSE_OPENSOURCE` in **Window > Application Manager** ([AC3](issues/app_configuration.md#ac3-checked-in-defines-need-the-private-submodule)).
2. Put the git-ignored plugins in place for a commercial build ([PL1](issues/plugins.md#pl1-the-commercial-build-needs-git-ignored-plugins)).
3. Select the platform in **File > Build Profiles**.
4. In **Window > Application Manager**, check the symbols and press **Update config file**. Do this after you switch platforms, so `AppConfig.bundleIdentifier` matches the platform ([BL1](issues/build.md#bl1-appconfigbundleidentifier-comes-from-the-last-built-platform)).
5. Android: enter the keystore passwords in **Player Settings > Publishing Settings**. Run **Assets > External Dependency Manager > Android Resolver > Resolve** after a plugin change.
6. Build into the platform folder under `Builds/`.
7. Commit the regenerated `AppConfig.cs` only when you release that build.

## How to add a build hook

1. Put the script in `Assets/Editor/` (or in an `Editor/` folder of a module with an editor-only assembly).
2. Prefer `IPreprocessBuildWithReport` or `IPostprocessBuildWithReport` over the `[PostProcessBuild]` attribute. The interfaces give the build report and a `callbackOrder`.
3. Check the platform first and return early for the others.
4. For Android manifests, edit only the launcher or `unityLibrary` manifest, not every manifest in the Gradle project.
5. Catch exceptions and log them with `Debug.LogException`, as `ApplicationBuildProcessor` does. A thrown exception fails the build.
6. Add the hook to the table in [Build hooks](#build-hooks).
