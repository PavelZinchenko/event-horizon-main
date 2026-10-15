# Plugins and Third-Party Code

This page lists every third-party library and SDK in the project: what it is, its version and license, where it lives, which assemblies use it, and how the game integrates it. It describes integration only, not the library APIs. Keys, app IDs, and ad unit IDs are not listed.

Five plugins that the commercial build needs are git-ignored. A fresh clone does not contain them ([PL1](issues/plugins.md#pl1-the-commercial-build-needs-git-ignored-plugins)).

| Plugin | Version | License | Location | In Git | Used by |
| --- | --- | --- | --- | --- | --- |
| [Expression Parser](#expression-parser) | 1.0.1 + fork | MIT | `Assets/ExpressionParser/` | submodule | `Database` |
| [Google Play Games](#google-play-games) | 0.11.01 | Apache 2.0 | `Assets/GooglePlayGames/` | **no** | `ServicesPrivate` (Android) |
| [Google Mobile Ads](#google-mobile-ads-remnants) | not installed | — | `Assets/GoogleMobileAds/` | **no** | nothing compiled |
| [Android build templates](#android-build-templates-and-external-dependency-manager) | Unity 6 templates | — | `Assets/Plugins/Android/` | **no** | Android build |
| [iOS native plugins](#ios-native-plugins) | iOS Native 9.11 | Asset Store; not stated | `Assets/ModulesPrivate/Plugins/`, `Assets/Plugins/iOS/` | submodule; `Plugins/iOS` **no** | `ServicesPrivate` (iOS) |
| [Steamworks.NET](#steamworksnet) | 20.2.0 (SDK 1.57) | MIT; Valve SDK binaries | `Assets/com.rlabrecque.steamworks.net/` | **no** | `ServicesPrivate` (desktop) |
| [NativeFilePicker](#nativefilepicker) | 1.3.5 | MIT (upstream) | `Assets/Plugins/NativeFilePicker/` | yes | `ServicesPrivate`, `Assembly-CSharp` |
| [UniRx](#unirx) | 6.2.2 | MIT | `Assets/Plugins/UniRx/` | yes | services, arena client, combat, state machine |
| [Zenject (Extenject)](#zenject-extenject) | 9.2.0 | MIT | `Assets/Plugins/Zenject/` | yes | almost every assembly |
| [BouncyCastle](#bouncycastle) | 2.7.0 | MIT | `Assets/Plugins/BouncyCastle/` | yes | `Security` |
| [IonicZlib (SimpleZip)](#ioniczlib-simplezip) | 1.0.0.0 | Ms-PL | `Assets/SimpleZip/` | yes | `Database`, `Services`, `Assembly-CSharp` |
| [NVorbis](#nvorbis-and-openwavparser) | 0.10.x + fork | MIT | `Assets/Modules/Database/Plugins/NVorbis/` | submodule | `Database` |
| [OpenWavParser](#nvorbis-and-openwavparser) | 1.0 | not stated | `Assets/Modules/Database/Plugins/eToile/` | yes | `Database` |
| [Collider optimizers](#collider-optimizers) | — | not stated | `Assets/Modules/BattleSimulator/Plugins/`, `Assets/Tools/` | yes | `BattleSimulator`; editor |

Known issues: [Plugin issues](issues/plugins.md) and [Expression Parser issues](issues/expression_parser.md).

## Contents

1. [Terms](#terms)
2. [What a clone contains](#what-a-clone-contains)
3. [Plugin sections](#expression-parser)
4. [Unity packages with plugin integration](#unity-packages-with-plugin-integration)
5. [Other third-party assets](#other-third-party-assets)
6. [Licenses](#licenses)
7. [Working with plugins](#working-with-plugins)

## Terms

| Term | Meaning |
| --- | --- |
| Vendored | Third-party source or binaries copied into `Assets/`, not installed through the Package Manager. |
| Git-ignored plugin | A plugin folder that `.gitignore` excludes. It exists only on a maintainer's machine. |
| EDM | External Dependency Manager for Unity. Its Android Resolver downloads Android libraries and patches the Gradle templates. |
| Commercial build | A build with `LICENSE_COMMERCIAL`. It needs the private submodule. See [ModulesPrivate: Build variants](modules_private.md#build-variants). |
| Open-source build | A build with `LICENSE_OPENSOURCE`. It does not compile `ServicesPrivate`, so it needs none of the platform SDKs. |

## What a clone contains

| Content | Clone without submodules | Clone with submodules | Maintainer machine |
| --- | --- | --- | --- |
| UniRx, Zenject, NativeFilePicker, BouncyCastle, IonicZlib, OpenWavParser, collider optimizers | yes | yes | yes |
| Expression Parser, NVorbis | no | yes | yes |
| Stan's Assets, FSG Keychain | no | only with access to the private submodule | yes |
| Google Play Games, Steamworks.NET, Android templates, `Plugins/iOS` sources, Google Mobile Ads settings, EDM local repository | no | no | yes |
| Gamemaster Audio sound pack | no | no | yes |

The open-source build compiles from a clone with submodules. No public assembly references a git-ignored plugin. The commercial build needs every row.

## Expression Parser

A fork of the CodeWriter Expression Parser, with the Sprache parser library inside. It compiles database formulas. See [Expression Parser](expression_parser.md) for the full description and [its issues](issues/expression_parser.md).

| | |
| --- | --- |
| Assembly | `CodeWriter.ExpressionParser` (every platform). Tests: `CodeWriter.ExpressionParser.Tests` (editor). |
| Integration | `Database.asmdef` references it. [ExpressionBuilder.cs](../Assets/Modules/Database/Scripts/Expression/ExpressionBuilder.cs) and the generated `GameDatabase.Expressions.*` classes are the only callers. |
| Submodule | `Assets/ExpressionParser` → `PavelZinchenko/Expression-Parser` |

## Google Play Games

The Google Play Games plugin for Unity. It gives sign-in, the player ID, and saved games on Android.

| | |
| --- | --- |
| Version | 0.11.01 (`Runtime/Scripts/PluginVersion.cs`) |
| Assemblies | `Google.Play.Games` (Android and editor), `Google.Play.Games.Editor` |
| Native library | `com.google.games:gpgs-plugin-support:0.11.01`. EDM copies it to `Assets/GeneratedLocalRepo/` (git-ignored) and adds it to `mainTemplate.gradle`. |
| Settings | [GooglePlayGameSettings.txt](../ProjectSettings/GooglePlayGameSettings.txt) (tracked). The setup window generates `Assets/Plugins/Android/GooglePlayGamesManifest.androidlib/` (git-ignored) from it. |
| Generated constants | [GPGSIds.cs](../Assets/GPGSIds.cs) (tracked): five achievement IDs. No game code reads them. |

Integration points, all in `ServicesPrivate` and guarded by `UNITY_ANDROID && !NO_GPGS`:

| Class | Use |
| --- | --- |
| `GooglePlayServices` | Calls `PlayGamesPlatform.Activate()` at start-up. |
| `GoogleAccount` | Sign-in, player ID, name, and avatar. See [ModulesPrivate: Accounts](modules_private.md#accounts). |
| `GoogleCloudStorage` | Saved games. See [GameSession: Storage](game_session/storage.md). |

Build notes: iOS, desktop, and WebGL define `NO_GPGS`. The plugin assembly does not compile for them.

## Google Mobile Ads (remnants)

The Google Mobile Ads SDK is not installed. Only these files remain:

- `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset`: the SDK settings object, with the Android app ID. Its script is missing.
- `Assets/GoogleMobileAds/link.xml`: keeps SDK assemblies that do not exist.
- Three `-keep` rules for `com.google.unity`, `com.google.android.gms`, and `com.google.ads` in `Assets/Plugins/Android/proguard-user.txt`. The `gms` rule also protects Play Games.

`GoogleAdsManager` in `ServicesPrivate` needs this SDK. It compiles only with `ADS_GOOGLE`, which no platform defines. See [ModulesPrivate: Advertisements](modules_private.md#advertisements) and [MP5](issues/modules_private.md#mp5-googleadsmanager-does-not-compile-outside-android). The live ad SDK is Unity Ads ([Unity packages](#unity-packages-with-plugin-integration)).

## Android build templates and External Dependency Manager

`Assets/Plugins/Android/` holds Unity's custom Gradle and manifest templates. The whole folder is git-ignored.

| File | Content |
| --- | --- |
| `AndroidManifest.xml` | Main activity; `allowBackup="false"` and `fullBackupContent="false"`, so Android does not back up the save folder; `installLocation="preferExternal"`. |
| `mainTemplate.gradle` | EDM block with two dependencies (`gpgs-plugin-support` and `unity-ads [4.17.0,4.18[`); packaging excludes `armeabi`, `mips`, `mips64`, `x86`, and `x86_64` native libraries; Java 17. |
| `settingsTemplate.gradle` | EDM repositories: the local Maven repository in `Assets/GeneratedLocalRepo/` and `maven.google.com`. |
| `gradleTemplate.properties` | `android.useAndroidX=true`, `android.enableJetifier=true`. |
| `proguard-user.txt` | Keep rules for Google libraries and annotations; `-dontobfuscate`. |
| `GooglePlayGamesManifest.androidlib/` | Generated by the Play Games setup. Holds the Play Games app ID. |
| `*.DISABLED`, `mainTemplate.gradle.backup` to `backup5` | Old copies. Unity does not use them ([PL7](issues/plugins.md#pl7-dead-and-stale-plugin-files)). |

EDM is a Package Manager package (`com.google.external-dependency-manager` 1.2.187 from the OpenUPM registry in [manifest.json](../Packages/manifest.json)). Its tracked settings:

- [AndroidResolverDependencies.xml](../ProjectSettings/AndroidResolverDependencies.xml): the resolved packages, the ABIs (`arm64-v8a`, `armeabi-v7a`), and the patch options.
- [GvhProjectSettings.xml](../ProjectSettings/GvhProjectSettings.xml): auto-resolve on build, Jetifier, template patching, and EDM usage analytics (enabled).

EDM rewrites the blocks between `// Android Resolver ... Start` and `End` markers. Do not edit those blocks by hand.

## iOS native plugins

Both plugins live in the private submodule. [ModulesPrivate: iOS plugins](modules_private.md#ios-plugins) describes how the game uses them. This section adds the plugin facts.

| Plugin | Version | Location | Assembly | Notes |
| --- | --- | --- | --- | --- |
| Stan's Assets iOS Native and iOS Deploy | iOS Native 9.11 | `Assets/ModulesPrivate/Plugins/StansAssets/`, `StansAssetsEditor/` | `StansAssets` (every platform, [MP18](issues/modules_private.md#mp18-the-ios-plugins-compile-into-every-platform)); editor code in `Assembly-CSharp-Editor` | An Asset Store package. Its installer copies native sources into `Assets/Plugins/iOS/` ([MP13](issues/modules_private.md#mp13-the-ios-native-sources-are-not-in-git)). |
| FSG iOS Keychain | not stated | `Assets/ModulesPrivate/Plugins/FSG/` | `iOSKeychainPlugin` (every platform) | Native `.mm` bridge, an Xcode post-processor, and a copy of Unity's XcodeAPI (MIT). |

`Assets/Plugins/iOS/` (git-ignored) holds five files that the Stan's Assets installer copied: `ISN_NativeCore.h`, `ISN_NativeCore.mm`, `ISN_GameCenter.mm`, `ISN_CloudKit.mm`, and `MNP_UIController.mm`. No C# code calls the `MNP_*` functions ([PL7](issues/plugins.md#pl7-dead-and-stale-plugin-files)).

## Steamworks.NET

A C# wrapper for the Steamworks API.

| | |
| --- | --- |
| Version | 20.2.0, Steamworks SDK 1.57 (`Runtime/Version.cs`) |
| Assemblies | `com.rlabrecque.steamworks.net`: editor, Windows (32 and 64 bit), macOS, Linux 64. `com.rlabrecque.steamworks.net.editor`. |
| Native libraries | `Plugins/steam_api.dll`, `steam_api64.dll`, `libsteam_api.so`, `steam_api.bundle` (Valve redistributables) |
| Used by | `SteamManager` and `SteamAccount` in `ServicesPrivate`. See [ModulesPrivate: Accounts](modules_private.md#accounts). |

Editor behaviour of the package:

1. `RedistInstall` runs on every script reload. It writes `steam_appid.txt` with the Steam test app ID 480 to the project root when the file is missing. `.gitignore` excludes that file.
2. `RedistInstall` also adds `STEAMWORKS_NET` to the define symbols of the selected build target. That is why Android, Standalone, and WebGL have this symbol. No game code reads it ([PL7](issues/plugins.md#pl7-dead-and-stale-plugin-files)).
3. `RedistCopy` runs after a Windows or Linux build. It copies the debug files and removes old `steam_api` DLLs from the build folder.

`Assets/ModulesPrivate/Plugins/Steamworks.NET/` is an empty folder. The package itself is in `Assets/com.rlabrecque.steamworks.net/`.

## NativeFilePicker

yasirkula's native file picker for Android and iOS.

| | |
| --- | --- |
| Version | 1.3.5 ([README.txt](../Assets/Plugins/NativeFilePicker/README.txt)) |
| Assemblies | `NativeFilePicker.Runtime` (every platform), `NativeFilePicker.Editor` |
| Native code | `Android/NativeFilePicker.aar`, `iOS/NativeFilePicker.mm` |
| Editor settings | Project Settings > yasirkula > Native File Picker. No `ProjectSettings/NativeFilePicker.json` exists, so the defaults apply: add the iOS frameworks automatically, and do not enable iCloud. |

Callers:

| Caller | Call | Platforms |
| --- | --- | --- |
| [`ExportableLocalStorage`](../Assets/ModulesPrivate/Services/Scripts/Storage/ExportableLocalStorage.cs) | `ExportFile` for save export, `PickFile` for save import | iOS, editor. See [GameSession: Storage](game_session/storage.md). |
| [`SettingsDatabase.FindModFile`](../Assets/Scripts/Gui/MainMenu/SettingsDatabase.cs#L42) | `PickFile` to add a packed mod from outside the mod folder | The **Browse** button in `SettingsScene`. A `PlatformDependentObject` keeps it only in the editor and on Android. |

On Android, `PickFile` copies the chosen file to one fixed path, `temporaryCachePath/pickedFile`, and returns that path. `SettingsDatabase` stores the path in `GameSettings.ExternalMods`. A second mod therefore overwrites the first ([PL2](issues/plugins.md#pl2-android-a-second-picked-mod-replaces-the-first)).

On desktop players, `PickFile` calls back with `null` at once.

## UniRx

Reactive Extensions for Unity by neuecc.

| | |
| --- | --- |
| Version | 6.2.2 ([ReadMe.txt](../Assets/Plugins/UniRx/ReadMe.txt)) |
| Assemblies | `UniRx` (every platform, `autoReferenced`); `UniRx.Examples` (test assembly, not in players) |
| Referenced by | `GameServices`, `Services`, `ServicesPrivate`, `Security`, `SecurityStub`, and `Assembly-CSharp` |

The game uses a small part of the library: `Observable.Return`, `Empty`, `Timer`, `Interval`, `EveryUpdate`, `Create`, `Throw`; `Subject<T>`; `Scheduler.MainThreadIgnoreTimeScale`; and `ObservableWWW.Post`, `GetWWW`, and `LoadFromCacheOrDownload`. `ObservableWWW` carries every arena request and the music bundle download ([ModulesPrivate: Server requests](modules_private.md#server-requests)). It is built on `UnityEngine.WWW`, which Unity marks as obsolete ([PL8](issues/plugins.md#pl8-outdated-libraries)).

Zenject has optional UniRx support behind `ZEN_SIGNALS_ADD_UNIRX`. The project does not define it.

## Zenject (Extenject)

The dependency injection container. [Architecture: Dependency Injection](architecture.md#dependency-injection-zenject) and [Application layer: Composition root](application_layer.md#composition-root) describe how the game uses it.

| | |
| --- | --- |
| Version | Extenject 9.2.0 ([Version.txt](../Assets/Plugins/Zenject/Version.txt)) |
| Assemblies | `Zenject` (every platform); `Zenject-Editor`; optional extras `Zenject-PoolMonitor-Editor`, `Zenject-ReflectionBaking-Editor`, `Zenject-TestFramework` |
| Referenced by | `Assembly-CSharp` and 12 module assemblies; about 320 source files outside the plugin have `using Zenject` |
| Features in use | `SceneContext` with contract names, `MonoInstaller`, `DiContainer`, `IInitializable`, `ITickable`, `IFixedTickable`, `IDisposable`, `IFactory`, `PlaceholderFactory`. No `ProjectContext`, no `SignalBus` (the game has its own signals), no reflection baking. |

[ZenjectDefaultSceneContractConfig.asset](../Assets/Resources/ZenjectDefaultSceneContractConfig.asset) maps the contract `services` to `BaseTestScene`. It is an editor feature: when you enter Play Mode in a scene whose parent contract is not loaded, Zenject first loads `BaseTestScene`, which binds stub services. A player build does not use it ([PL5](issues/plugins.md#pl5-an-editor-only-zenject-asset-is-in-resources)).

## BouncyCastle

The BouncyCastle cryptography library for .NET.

| | |
| --- | --- |
| Version | 2.7.0 (`BouncyCastle.Cryptography.dll`, 4.9 MB; assembly version 2.0.0.0) |
| Import settings | Every platform; `autoReferenced` (not explicitly referenced) |
| Used by | `Security.ModEncryption` only: RSA-OAEP key unwrap and AES-GCM decryption of packed mods. See [ModulesPrivate: Security assembly](modules_private.md#security-assembly). |

The database editor (`Assets/Modules/Database/.Editor/`) is a separate .NET project and has its own BouncyCastle reference.

`Security` compiles only in the commercial build, but the DLL is imported for every build ([PL4](issues/plugins.md#pl4-bouncycastle-is-imported-into-every-build)).

## IonicZlib (SimpleZip)

The zlib part of DotNetZip (`Ionic.Zlib`), as one precompiled DLL. The folder name `SimpleZip` comes from the asset it was taken from.

| | |
| --- | --- |
| Version | `IonicZlib.dll`, assembly version 1.0.0.0. The license file names it `Ionic.Zlib.CF.dll`. |
| License | Microsoft Public License (Ms-PL) ([License.txt](../Assets/SimpleZip/License.txt)) |
| Import settings | Every platform, `autoReferenced` |

Callers. All of them use the zlib stream format (`ZlibStream`), not GZip or raw Deflate.

| Caller | Data | See |
| --- | --- | --- |
| [`FileDatabaseStorage`](../Assets/Modules/Database/Scripts/Storage/FileDatabaseStorage.cs) | Decompresses packed mods. | [Database: Packed mod format](database.md#packed-mod-format-and-encryption) |
| [`PlayerPrefsStorage`](../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs) | Compresses WebGL saves. | [GameSession: Storage](game_session/storage.md) |
| [`FleetWebSerializer`](../Assets/Scripts/GameServices/OfflineMultiplayer/FleetWebSerializer.cs) | Compresses arena fleets. | [Application layer](application_layer.md#game-services-gameservices) |

The database editor has its own copy in `.Editor/IonicZlib/` ([Database Editor](database/editor.md)). Any replacement must read and write the same zlib format, because saves, mods, and server fleets already use it.

## NVorbis and OpenWavParser

Both decode audio in mods. [Database: Media](database.md#media-images-and-audio) covers `AudioClipData` and `OggAudioClip`.

| Library | Version | License | Assembly | Use |
| --- | --- | --- | --- | --- |
| NVorbis (submodule `Assets/Modules/Database/Plugins/NVorbis` → `PavelZinchenko/NVorbis`) | Upstream state of 2022-10-16, plus an assembly definition | MIT, © 2020 Andrew Ward | `NVorbis` (every platform, no engine references) | Ogg Vorbis decoding. Clips over 100 KB stream from memory. |
| OpenWavParser by eToile ([OpenWavParser.cs](../Assets/Modules/Database/Plugins/eToile/OpenWavParser/OpenWavParser.cs)) | 1.0 (PDF manual in the folder) | not stated | compiled into `Database` | `ByteArrayToAudioClip` for WAV files in mods. |

## Collider optimizers

| Tool | Location | License | Use |
| --- | --- | --- | --- |
| Collider2D Optimization (Ramer–Douglas–Peucker) | [BattleSimulator/Plugins/Collider2D Optimization/](../Assets/Modules/BattleSimulator/Plugins/Collider2D%20Optimization/) | not stated; the readme credits a CodeProject article | `ShipFactory` uses the `Optimize` extension. See [BattleSimulator](battle_simulator.md). |
| PolygonCollider2D Optimizer by Unitycoder | [Tools/PolygonCollider2DOptimizer/Editor/](../Assets/Tools/PolygonCollider2DOptimizer/Editor/PolygonCollider2DOptimizer.cs) | not stated | Editor window, **Window > PolygonColliderOptimizer**. Compiles into `Assembly-CSharp-Editor`. No code depends on it. |

## Unity packages with plugin integration

These are Package Manager packages, not vendored code. They are listed because the plugins above depend on them or replace them.

| Package | Version | Integration |
| --- | --- | --- |
| `com.google.external-dependency-manager` | 1.2.187 (OpenUPM) | Resolves Android libraries for Play Games and Unity Ads. See [Android build templates](#android-build-templates-and-external-dependency-manager). |
| `com.unity.ads` | 4.17.0 | `UnityAdsManager` on Android and iOS. See [ModulesPrivate: Advertisements](modules_private.md#advertisements). |
| `com.unity.purchasing` | 5.3.1 | `InAppPurchasing` on Android and iOS. See [ModulesPrivate: In-app purchases](modules_private.md#in-app-purchases). |

## Other third-party assets

These folders hold no code. This page does not audit how the game uses them.

| Folder | In Git | Content |
| --- | --- | --- |
| `Assets/Standard Assets/ParticleSystems/` | yes | Unity Standard Assets particle materials and textures. |
| `Assets/Standard Assets (Mobile)/Shaders/` | yes | Unity mobile shaders. |
| `Assets/Lens Flares/` | yes | One lens flare and its texture. |
| `Assets/Gamemaster Audio - Pro Sound Collection/` | **no** | An Asset Store sound pack (152 files). Four tracked assets use two of its clips ([PL3](issues/plugins.md#pl3-tracked-ui-assets-use-git-ignored-sounds)). |

## Licenses

The project is licensed under GPL-3.0 ([LICENSE](../LICENSE)).

| License | Libraries |
| --- | --- |
| MIT | Expression Parser, Sprache, Steamworks.NET (wrapper), UniRx, Zenject, BouncyCastle, NVorbis, NativeFilePicker (upstream; no license file in the folder), Unity XcodeAPI copy |
| Apache 2.0 | Google Play Games |
| Ms-PL | IonicZlib |
| Proprietary or Asset Store terms | Stan's Assets (private submodule), Gamemaster Audio (git-ignored), Steamworks SDK binaries (git-ignored), Unity packages |
| Not stated | OpenWavParser, Collider2D Optimization, PolygonCollider2D Optimizer, FSG iOS Keychain |

Ms-PL and the unstated licenses in the public repository need a review ([PL9](issues/plugins.md#pl9-some-licenses-need-a-review)).

## Working with plugins

### Setting up the git-ignored plugins

A maintainer machine needs these steps after a fresh clone, for a commercial build:

1. Run `git submodule update --init --recursive`. The private submodule needs access rights.
2. Import Google Play Games 0.11.01 into `Assets/GooglePlayGames/`.
3. Import Steamworks.NET 20.2.0 into `Assets/com.rlabrecque.steamworks.net/`.
4. Restore `Assets/Plugins/Android/` from a maintainer copy. Unity can create new templates (Player Settings > Publishing Settings), but the manifest and ProGuard changes above are then lost.
5. Run the Play Games Android setup. It writes `GooglePlayGamesManifest.androidlib` from `ProjectSettings/GooglePlayGameSettings.txt`.
6. Run the EDM Android Resolver (Force Resolve). It fills `Assets/GeneratedLocalRepo/` and patches the templates.
7. For iOS, run the Stan's Assets install action to copy the native sources to `Assets/Plugins/iOS/`.
8. Import the Gamemaster Audio pack from the Asset Store, if the license is available.

### Updating a vendored plugin

1. Read the plugin's change log for API changes in the parts that this page lists under "Used by" or "Callers".
2. Delete the old folder before you import the new version, so removed files do not stay behind. Keep the `.meta` GUIDs of assemblies that other assemblies reference by GUID.
3. Check the assembly name. Several `.asmdef` files reference plugin assemblies by GUID (for example `ServicesPrivate`).
4. For an Android plugin, run the EDM resolver and check the `Android Resolver` blocks in the templates.
5. Build the open-source variant and each commercial platform that uses the plugin.
6. Update the version on this page.

### Updating a submodule plugin

Commit inside the submodule first, then commit the new submodule pointer in the main repository. This applies to Expression Parser and NVorbis.

### Adding a new plugin

1. Prefer a Package Manager package (registry or Git URL) over a copy in `Assets/`. A package in `Packages/manifest.json` reaches every clone.
2. If you must vendor it, put it under `Assets/Plugins/<Name>/` with its license file, and give it an assembly definition.
3. Restrict the assembly and its native libraries to the platforms that use it.
4. Add a section to this page with the version, the license, and the integration points.
