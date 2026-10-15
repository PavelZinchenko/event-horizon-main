# AppConfiguration Module Issues

This file records problems in `Assets/Modules/AppConfiguration/` and in its generator [`Assets/Editor/ApplicationManager.cs`](../../Assets/Editor/ApplicationManager.cs). For the design, see [AppConfiguration module](../app_configuration.md).

The analysis was done by reading the code on 2026-10-12. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain.
- **Confirmed (latent)** – the code path is certain, but current data or settings do not trigger it.
- **Plausible** – the code is wrong, but the failure needs specific conditions.

## Defects

### AC1. The build number is incremented after the build

**Severity:** Low. **Status:** Confirmed.

**Where.** `ApplicationManager.OnPostBuild` ([ApplicationManager.cs:330](../../Assets/Editor/ApplicationManager.cs#L330)) calls `GenerateAppConfigFile(..., increaseBuildNumber: true, ...)`.

**Failure.**
1. The build that just finished shows the old number. Build N reports "build N-1".
2. Every player build leaves `AppConfig.cs` modified in the working tree. This is why `git status` lists the file.
3. The hook passes the *compiled* `AppConfig` values. Window fields that were changed but not saved with **Update config file** are lost.

**Fix.** Increment and regenerate in an `IPreprocessBuildWithReport` hook, before the scripts compile. Or read the build number from CI and do not store it in source.

### AC2. `enableCheats` is `true` in the checked-in configuration

**Severity:** Low. **Status:** Confirmed.

**Where.** [AppConfig.cs:9](../../Assets/Modules/AppConfiguration/Scripts/Generated/AppConfig.cs#L9).

**Failure.** The hidden developer console in the main menu stays in the scene in every build made from the repository. A triple click opens the keypad. In open-source builds, `Security.DebugCommands.Decode` from `SecurityStub` returns the typed number as is, so any player can run any cheat by its number. In commercial builds the codes depend on a device hash, so the risk is lower.

**Fix.** Default to `false`. Set it to `true` only in development builds, or tie it to `Debug.isDebugBuild`.

### AC3. Checked-in defines need the private submodule

**Severity:** Medium. **Status:** Confirmed.

**Where.** `scriptingDefineSymbols` in [ProjectSettings.asset](../../ProjectSettings/ProjectSettings.asset) sets `LICENSE_COMMERCIAL` for Android, Standalone, WebGL, and iOS.

**Failure.** `SecurityStub` compiles only with `LICENSE_OPENSOURCE` (its `defineConstraints`). With `LICENSE_COMMERCIAL`, the installers reference private types (`GoogleAccount`, `WindowsLocalStorage`, `AssetLoader`, and others). A public clone without `Assets/ModulesPrivate` does not compile until a contributor finds and switches the license in **Window > Application Manager**. Nothing in `README.md` says this.

**Fix.** Document the switch in `README.md`, or detect the missing private assembly with an `asmdef` `versionDefines` entry and select the license automatically.

### AC4. `testMode` and the Yandex options are dead

**Severity:** Low. **Status:** Confirmed.

**Where.** `AppConfig.testMode`; `IapType.Yandex`, `AdsType.Yandex`, `StoreType.Yandex` in [ApplicationManager.cs](../../Assets/Editor/ApplicationManager.cs).

**Failure.** No code reads `testMode`. No code checks `IAP_YANDEX`, `ADS_YANDEX`, or `STORE_YANDEX`. Selecting them silently gives a build with IAP and ads disabled.

**Fix.** Remove the options, or restore implementations.

### AC5. `RemoveDefineSymbol` and `AddDefineSymbol` call `Debug.Break`

**Severity:** Low. **Status:** Confirmed.

**Where.** [ApplicationManager.cs:228](../../Assets/Editor/ApplicationManager.cs#L228) and [line 246](../../Assets/Editor/ApplicationManager.cs#L246).

**Failure.** When the current symbol set does not match what the window expects (for example after a manual edit of the player settings), the window logs an error and calls `Debug.Break()`. The change is skipped, and the window state no longer matches the defines. The window reads its state from `#if` blocks, so it stays wrong until the next recompile.

**Fix.** Rebuild the full symbol list from the window state instead of patching it, and drop `Debug.Break()`.

## Structural issues

### AC6. A `const` class is the wrong carrier for build data

**Problem.** Version, build number, and flags are compile-time constants in source. They duplicate `PlayerSettings` (`Application.version` already returns the bundle version), and any change needs a recompile and a commit.

**Evidence.** `version` duplicates `PlayerSettings.bundleVersion`; `bundleIdentifier` duplicates `Application.identifier`.

**Direction.** Use `Application.version` and `Application.identifier` at run time. Keep only true flags (`enableCheats`, `alternativeTitle`) in a ScriptableObject that the build pipeline sets.
