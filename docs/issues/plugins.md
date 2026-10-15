# Plugin Issues

This file records problems with third-party plugins and with the way the game integrates them. For the plugin list, versions, and integration points, see [Plugins](../plugins.md). Expression Parser defects are in [Expression Parser issues](expression_parser.md). Defects in the private services that use the plugins are in [ModulesPrivate issues](modules_private.md) and are not repeated here.

The analysis was done by reading the code and the project files on 2026-10-12. No issue was reproduced on a device. Severity is High, Medium, or Low. Status is **Confirmed** (the code path or file state is certain), **Confirmed (latent)** (certain, but the current settings do not trigger it), **Plausible** (needs conditions that were not tested), or **Needs review** (a decision for the owner, not a code defect).

## Defects

### PL1. The commercial build needs git-ignored plugins

**Severity:** Medium. **Status:** Confirmed.

**Where.** [.gitignore](../../.gitignore) lines 13 to 22 exclude `Assets/GooglePlayGames*`, `Assets/com.rlabrecque.steamworks.net*`, `Assets/GeneratedLocalRepo*`, `Assets/Plugins/Android/*`, and `Assets/Plugins/iOS/`. [ServicesPrivate.asmdef](../../Assets/ModulesPrivate/Services/ServicesPrivate.asmdef) references `Google.Play.Games` and `com.rlabrecque.steamworks.net`.

**Failure.**
1. A maintainer clones the repository on a new machine, with access to the private submodule.
2. The checked-in defines select `LICENSE_COMMERCIAL` on every platform ([AC3](app_configuration.md#ac3-checked-in-defines-need-the-private-submodule)).
3. `ServicesPrivate` does not compile: the Play Games and Steamworks.NET types are missing.
4. After the maintainer imports both plugins, the Android build still differs from the release build. Unity uses its default manifest and Gradle files, so `allowBackup="false"`, the native library excludes, and the ProGuard rules are lost. Nothing reports the difference.

The exact plugin versions are recorded only in this documentation.

**Fix.**
1. Install Google Play Games and Steamworks.NET as Package Manager packages from Git URLs or tarballs, with the versions pinned in `Packages/manifest.json`. Both publish packages in that form.
2. Track the hand-written Android templates (`AndroidManifest.xml`, `mainTemplate.gradle`, `settingsTemplate.gradle`, `gradleTemplate.properties`, `proguard-user.txt`). Keep ignoring the generated `.aar`, `.jar`, and `.androidlib` content.
3. Fix [MP13](modules_private.md#mp13-the-ios-native-sources-are-not-in-git) for `Plugins/iOS`.

### PL2. Android: a second picked mod replaces the first

**Severity:** Medium. **Status:** Confirmed.

**Where.** NativeFilePicker copies every picked file on Android to one fixed path, `temporaryCachePath/pickedFile` ([NativeFilePicker.cs:76](../../Assets/Plugins/NativeFilePicker/NativeFilePicker.cs#L76)). [SettingsDatabase.cs:50](../../Assets/Scripts/Gui/MainMenu/SettingsDatabase.cs#L50) ignores a path that is already in `ExternalMods`, and [line 58](../../Assets/Scripts/Gui/MainMenu/SettingsDatabase.cs#L58) stores the cache path. `Database.TryLoad` opens the stored path without checking the mod ID ([Database.cs:124](../../Assets/Modules/Database/Scripts/Database.cs#L124)).

**Failure.**
1. On Android, the player taps **Browse** and picks mod A. The game lists A with the path `…/cache/pickedFile`.
2. The player picks mod B. The plugin overwrites `pickedFile` with B. The path is already in the list, so the game returns without a message. The list still shows A.
3. The player selects A. The game loads B's content under A's ID.
4. At the next start, `InitializationState` reads `pickedFile` again and lists B only. The active mod ID is A, which is no longer found, so the game falls back to the default database.

Android can also delete the cache folder at any time. The mod then disappears from the list at the next start.

**Fix.** In `OnFileSelected`, copy the picked file into the mod folder that `Database.LookForMods` scans (`persistentDataPath/Mods/`), with a unique name, and delete the cache copy. Then `ExternalMods` is not needed on Android.

### PL3. Tracked UI assets use git-ignored sounds

**Severity:** Low. **Status:** Confirmed.

**Where.** `Assets/Gamemaster Audio - Pro Sound Collection/` is git-ignored ([.gitignore:17](../../.gitignore#L17)). Two of its clips are referenced by [ControlsPanel.prefab](../../Assets/Resources/Gui/Settings/ControlsPanel.prefab), [KeySetup.prefab](../../Assets/Resources/Gui/Settings/KeySetup.prefab), [CommonGuiScene.unity](../../Assets/Scenes/CommonGuiScene.unity), and [SettingsScene.unity](../../Assets/Scenes/SettingsScene.unity).

**Failure.** In a clone, these references are missing. Some buttons in the settings and controls screens are silent. A build from the clone ships without the sounds and gives no error.

**Fix.** Copy the two clips into a tracked folder if the Asset Store license allows it, or replace them with clips that the project can distribute. Then remove the pack.

### PL4. BouncyCastle is imported into every build

**Severity:** Low. **Status:** Plausible.

**Where.** [BouncyCastle.Cryptography.dll.meta](../../Assets/Plugins/BouncyCastle/BouncyCastle.Cryptography.dll.meta): every platform enabled, `isExplicitlyReferenced: 0`. Only `Security` uses it, and `Security` compiles only with `LICENSE_COMMERCIAL`.

**Failure.** The 4.9 MB assembly is offered to every assembly and every player, including open-source builds and WebGL. The linker may strip most of it, but the effect on the build size was not measured. Any assembly can start to use it by accident.

**Fix.** Set `isExplicitlyReferenced: 1` and add it to `Security.asmdef` as a precompiled reference. Optionally add `LICENSE_COMMERCIAL` as a define constraint in the plugin import settings.

### PL5. An editor-only Zenject asset is in `Resources`

**Severity:** Low. **Status:** Plausible.

**Where.** [ZenjectDefaultSceneContractConfig.asset](../../Assets/Resources/ZenjectDefaultSceneContractConfig.asset). Its script, `DefaultSceneContractConfig`, is in the `Zenject-Editor` assembly.

**Failure.** Every player build includes all assets under `Resources`. In the player, this asset has no script. Unity may log a missing-script warning at build time. The asset is never loaded at run time.

**Fix.** Move the asset to `Assets/Editor/Resources/`. `Resources.LoadAll` still finds it in the editor, and players do not include it.

### PL6. `.csproj` files in Zenject are ignored, but their `.meta` files are tracked

**Severity:** Low. **Status:** Confirmed.

**Where.** `Assets/Plugins/Zenject/Source/Zenject.csproj.meta` and `Assets/Plugins/Zenject/OptionalExtras/ReflectionBaking/Zenject-ReflectionBaking.csproj.meta` are in Git. The `.csproj` files match `*.csproj` in [.gitignore:58](../../.gitignore#L58).

**Failure.** In a clone, Unity finds two `.meta` files without assets. It logs a warning and deletes them. `git status` then shows two deleted files on every new machine.

**Fix.** Delete both `.csproj` files and their `.meta` files. Unity does not use them.

### PL7. Dead and stale plugin files

**Severity:** Low. **Status:** Confirmed.

| Item | State |
| --- | --- |
| `Assets/Plugins/Android/*.DISABLED` (3 files), `mainTemplate.gradle.backup` to `backup5` | Old template copies with `.meta` files. Unity does not use them. |
| `Assets/GoogleMobileAds/` | Settings and `link.xml` for an SDK that is not installed. |
| `com.google.unity` and `com.google.ads` rules in `proguard-user.txt` | For the same missing SDK. |
| `Assets/Plugins/iOS/MNP_UIController.mm` | Copied by the Stan's Assets installer. No C# code calls it. |
| `Assets/ModulesPrivate/Plugins/Steamworks.NET/` | An empty folder with a tracked `.meta` in the private submodule. |
| `.gitignore` entries `/Assets/Scripts/Steamworks.NET/` and `/Assets/Appodeal*` | No such folders. `ServicesInstaller` still names an `AppodealAdsManager` that does not exist ([ModulesPrivate: Advertisements](../modules_private.md#advertisements)). |
| `STEAMWORKS_NET` define on Android, Standalone, and WebGL | Added by the Steamworks.NET editor script. No game code reads it. |
| [GPGSIds.cs](../../Assets/GPGSIds.cs) | Five achievement IDs. No game code reads them. |
| `Assets/Plugins/UniRx/Examples/` | Sample scripts and two sample scenes. |

**Fix.** Delete the files and the stale `.gitignore` entries. Remove `STEAMWORKS_NET` from the define lists only after you also stop the editor script from adding it again, or it returns on the next script reload.

## Structural issues

### PL8. Outdated libraries

**Severity:** Low. **Status:** Confirmed.

| Library | In the project | Concern |
| --- | --- | --- |
| UniRx | 6.2.2 | The last upstream release is 7.1.0 from 2019. `ObservableWWW` is built on `UnityEngine.WWW`, which Unity marks as obsolete. All arena requests and the music download use it. |
| Zenject (Extenject) | 9.2.0, 2020 | No newer release. It works on Unity 6, but it receives no fixes. |
| Google Play Games | 0.11.01, 2022 | The game calls only `Activate`, `Authenticate`, the user properties, and saved games. Check the upstream release notes for these calls before an upgrade. |
| Steamworks.NET editor script | 20.2.0 | Calls `GetScriptingDefineSymbolsForGroup`, which Unity 6 marks as obsolete. |

**Fix.** Replace `ObservableWWW` with `UnityWebRequest` in `RequestFactory`, `PlayerInfo`, `EnemyPlayerInfo`, and `AssetLoader`. After that, the game's use of UniRx is small enough to replace with plain C# events and coroutines. Test sign-in and saved games on a device after a Play Games upgrade.

### PL9. Some licenses need a review

**Severity:** Medium. **Status:** Needs review.

**Where.** The project is GPL-3.0 ([LICENSE](../../LICENSE)). The public repository ships:

- `IonicZlib.dll` under the Microsoft Public License ([License.txt](../../Assets/SimpleZip/License.txt)). The Free Software Foundation lists Ms-PL as incompatible with the GPL.
- OpenWavParser, Collider2D Optimization, and PolygonCollider2D Optimizer with no license file. Their terms are unknown.
- NativeFilePicker without its license file. Upstream is MIT, which requires the notice to travel with the code.

**Failure.** This is not a runtime failure. It is a legal question for the project owner. This page does not give legal advice.

**Fix.**
1. Replace IonicZlib with `System.IO.Compression.DeflateStream` plus the 2-byte zlib header and the Adler-32 checksum. The format must stay the same, because saves, packed mods, and server fleets already use it. Test with existing files.
2. Find the original source and license of the three unlicensed tools. Replace or remove each one whose license is not compatible.
3. Add the upstream MIT license file to `Assets/Plugins/NativeFilePicker/`.
