# AppConfiguration Module

The AppConfiguration module holds the build configuration of the application: a generated class with version and build flags, and two shared assets (the UI theme and the font table). Code reads the class through static constants. An editor window rewrites the class and the scripting define symbols.

| | |
| --- | --- |
| Assembly | `AppConfiguration` ([AppConfiguration.asmdef](../Assets/Modules/AppConfiguration/AppConfiguration.asmdef)). No references. Auto-referenced, so `Assembly-CSharp` sees it without an explicit reference. |
| Namespace | Global (`AppConfig` has no namespace). |
| Source | [`Scripts/Generated/AppConfig.cs`](../Assets/Modules/AppConfiguration/Scripts/Generated/AppConfig.cs) (1 file, 12 lines, generated) |
| Assets | [`UiTheme.asset`](../Assets/Modules/AppConfiguration/ScriptableObjects/UiTheme.asset), [`FontTable.asset`](../Assets/Modules/AppConfiguration/ScriptableObjects/FontTable.asset) |
| Generator | [`Assets/Editor/ApplicationManager.cs`](../Assets/Editor/ApplicationManager.cs) (menu **Window > Application Manager**) |
| Known issues | [AppConfiguration issues](issues/app_configuration.md) |

## Contents

1. [AppConfig constants](#appconfig-constants)
2. [Consumers](#consumers)
3. [Application Manager window](#application-manager-window)
4. [Scripting define symbols](#scripting-define-symbols)
5. [Assets](#assets)
6. [How to change the configuration](#how-to-change-the-configuration)

## AppConfig constants

`AppConfig` is a static class with `const` fields. The C# compiler copies a `const` value into every assembly that uses it. All consumers are rebuilt together in Unity, so this is safe inside the project.

| Constant | Current value | Meaning |
| --- | --- | --- |
| `bundleIdentifier` | `com.ZipasGames.EventHorizon` | Copy of `PlayerSettings.applicationIdentifier` for the selected platform at generation time. |
| `version` | `1.13.1` | Copy of `PlayerSettings.bundleVersion`. Shown in the main menu and written into save files. |
| `versionCode` | `113013` | Copy of `PlayerSettings.Android.bundleVersionCode`. Sent to the game server with arena requests. |
| `buildNumber` | `1811` | A counter. The post-build hook adds 1 after every player build. Shown in the main menu. |
| `localizationFile` | `Languages` | Name of the `TextAsset` under `Resources/Localization/` that lists the languages. |
| `testMode` | `false` | "Fake in-app purchases". No code reads it (the only use is commented out in the private Unity Ads manager). |
| `enableCheats` | `true` | Allows the developer console commands. |
| `alternativeTitle` | `false` | Shows "Cosmic Horizons" instead of "Event Horizon" on the loading screen. |

## Consumers

| Constant | Read by |
| --- | --- |
| `version`, `buildNumber` | [`AppConfigViewModel`](../Assets/Scripts/Legacy/GUI/ViewModel/MainMenu/AppConfigViewModel.cs) (main menu version label). |
| `version` | [`PlayerPrefsStorage`](../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs) and the private `LocalStorage` write it into the save header. `LocalStorage` compares it on load. |
| `versionCode` | The private `Security.RequestFactory` adds it to arena requests as `v`. |
| `bundleIdentifier` | [`RateGameService`](../Assets/Scripts/Services/RateGame/RateGameService.cs) builds the store URL from it. |
| `localizationFile` | [`ResourceLoader`](../Assets/ModulesShared/Localization/Scripts/Localization/ResourceLoader.cs) in the Localization module. |
| `enableCheats` | [`DevConsoleViewModel`](../Assets/Scripts/Legacy/GUI/ViewModel/MainMenu/DevConsoleViewModel.cs). When it is `false`, the console rejects commands. |
| `alternativeTitle` | [`LoadingScreen`](../Assets/Scripts/Legacy/GUI/LoadingScreen.cs). |

The Localization module is shared with Frontier, so Frontier must also provide an `AppConfig` class with a `localizationFile` constant.

## Application Manager window

[`ApplicationManager`](../Assets/Editor/ApplicationManager.cs) is an `EditorWindow`. It does three things.

1. **Edits player settings.** Fields for bundle ID, bundle version, and Android version code write directly to `PlayerSettings`.
2. **Rewrites `AppConfig.cs`.** The **Update config file** button calls `GenerateAppConfigFile`. The method builds the class text from the current `PlayerSettings` and the window fields, writes it to `Assets/Modules/AppConfiguration/Scripts/Generated/AppConfig.cs`, and refreshes the asset database.
3. **Switches define symbols.** Popups for store, IAP, ads, and license remove the old symbol and add the new one for the selected build target group. Toggles add or remove `NO_GPGS` and `NO_INTERNET`.

The **Use alternative title** toggle also changes `PlayerSettings.productName` to "Event Horizon" or "Cosmic Horizons".

The **Generate ship prefabs** button creates a combat prefab under `Assets/Modules/BattleSimulator/Resources/Combat/Ships/` for every sprite in `Assets/Sprites/Ships` and `Assets/Sprites/Starbases` that has no prefab yet. It copies `Combat/Ships/Default` and adds a `PolygonCollider2D`.

### Post-build hook

`ApplicationManager.OnPostBuild` has the `[PostProcessBuild]` attribute. After a manual player build it calls `GenerateAppConfigFile` with `increaseBuildNumber = true`. The build that just finished therefore contains the old number. The repository gets a modified `AppConfig.cs` after each manual build.

The [automated platform builds](build.md#automated-platform-builds) generate configuration before compilation and increase the number once per run. `CommandLineBuild.IsBuilding` suppresses the post-build increment only while the automated player build runs.

## Scripting define symbols

The window reads its initial state from the compiled symbols (`#if STORE_GOOGLE` and so on). The symbols control which service implementations the installers bind; see [Services](services.md#platform-selection) and [Application layer: Conditional compilation](application_layer.md#conditional-compilation).

| Group | Symbols |
| --- | --- |
| Store | `STORE_YANDEX`, `STORE_GOOGLE`, `STORE_APPLE`, `STORE_WEB`, `STORE_STEAM` |
| In-app purchases | `IAP_DISABLED`, `IAP_UNITY`, `IAP_YANDEX` |
| Ads | `ADS_DISABLED`, `ADS_UNITY`, `ADS_GOOGLE`, `ADS_YANDEX`, `ADS_APPODEAL` |
| License | `LICENSE_OPENSOURCE`, `LICENSE_COMMERCIAL` |
| Switches | `NO_GPGS` (no Google Play Games), `NO_INTERNET` |

Checked-in values in [`ProjectSettings.asset`](../ProjectSettings/ProjectSettings.asset) (`scriptingDefineSymbols`):

| Platform | Symbols |
| --- | --- |
| Android | `UNITY_ASSERTIONS;LICENSE_COMMERCIAL;STORE_GOOGLE;STEAMWORKS_NET;ADS_UNITY;IAP_UNITY` |
| Standalone | `UNITY_ASSERTIONS;NO_GPGS;ADS_DISABLED;IAP_DISABLED;STEAMWORKS_NET;LICENSE_COMMERCIAL;STORE_STEAM` |
| WebGL | `LICENSE_COMMERCIAL;NO_GPGS;ADS_DISABLED;IAP_DISABLED;STORE_WEB;STEAMWORKS_NET` |
| iOS | `NO_GPGS;IAP_UNITY;LICENSE_COMMERCIAL;ADS_UNITY;STORE_APPLE` |

Every platform uses `LICENSE_COMMERCIAL`. A clone without the private submodule must switch to `LICENSE_OPENSOURCE` before it compiles (see [issue AC3](issues/app_configuration.md#ac3-checked-in-defines-need-the-private-submodule)).

`YANDEX` symbols have no implementation in the current code. They are leftovers of an earlier Yandex Games build.

## Assets

| Asset | Type | Used by |
| --- | --- | --- |
| [UiTheme.asset](../Assets/Modules/AppConfiguration/ScriptableObjects/UiTheme.asset) | `Gui.Theme.UiTheme` | `ServicesInstaller._defaultUiTheme` in `LoaderScene` and `BaseTestScene`. It is the default theme; a mod can override it. See [Gui module: Themes](gui.md#themes). |
| [FontTable.asset](../Assets/Modules/AppConfiguration/ScriptableObjects/FontTable.asset) | `Services.Localization.FontTable` | Nothing. The lookup that uses it is disabled. See [Localization: Font fallback](localization.md#font-fallback-systemfontlocator-and-fonttable). |

## How to change the configuration

1. Open **Window > Application Manager**. Leave Play Mode first; the window refuses changes in Play Mode.
2. Change the fields or popups. Each popup change rewrites the define symbols at once and triggers a script recompile.
3. Press **Update config file** to regenerate `AppConfig.cs`.
4. Do not edit `AppConfig.cs` by hand. The next build overwrites it.
