# GameSession Module Issues

This file records problems in `Assets/Modules/GameSession/`, in its code generator, in the serialization helpers it uses from `Assets/ModulesShared/Utilites/Serialization/`, and in application code that loads or writes session data. For a description of the current design, see [GameSession module](../game_session.md).

The analysis was done by reading the code and by running the generator on 2026-10-12. No issue was reproduced in Play Mode. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain. The failure follows directly from the code.
- **Confirmed (latent)** – the code path is certain, but no current caller or schema triggers it.
- **Plausible** – the code is wrong, but the failure needs specific data or conditions.

Structural issues use Problem / Evidence / Effect / Direction. Defects use Where / Failure / Fix.

### GS1. A failed load replaces the live model

**Severity:** High. **Status:** Confirmed.

**Where.** `SessionData.TryDeserialize` passes its own `_content` field as the `out` argument of `SessionSerializer.TryDeserialize` ([SessionData.cs:95](../../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L95)). The serializer assigns the argument before it decides that the load failed:

- When the EOF marker is missing, `content` holds the rejected model ([SessionSerializer.cs:76](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L76)–[82](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L82)).
- When the schema version is unknown, `SessionLoader.Load` returns `null` (see [GS2](#gs2-unknown-schema-versions-are-not-rejected)), and `content` is `null`.
- When reading throws, `content` is `null` ([SessionSerializer.cs:36](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L36), [LegacyDataLoader.cs:52](../../Assets/Modules/GameSession/Scripts/Facade/LegacyDataLoader.cs#L52)).

In all three cases the content facades still wrap the old model, because `InitializeContent` does not run ([SessionData.cs:98](../../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L98)). If `InitializeContent` itself throws, some facades wrap the new model and some wrap the old one.

**Failure.**
1. The player loads a cloud save that was written by a newer game version, or a truncated one. Gameplay keeps writing to the old model through the facades. `Serialize` writes `_content`: the rejected model, or a `NullReferenceException` when it is `null` ([SessionData.cs:88](../../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L88)). Progress made after the failed load is not saved.
2. The player switches to a mod whose save does not load. `GameDataManager` then calls `CreateNewGame` ([GameDataManager.cs:95](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L95)). `CreateNewGame` copies the purchases from `_content` ([SessionData.cs:129](../../Assets/Modules/GameSession/Scripts/Facade/SessionData.cs#L129)). When `_content` is `null`, `RemoveAds`, `SupporterPack`, and purchased stars are not carried over.

**Fix.** Deserialize into a local variable. Build the facades into locals. Assign `_content` and the facades only after both steps succeed.

### GS2. Unknown schema versions are not rejected

**Severity:** Medium. **Status:** Confirmed.

**Where.** `SessionLoader.Load` reads only the majors 1, 2, and 3. For any other major it reads nothing and returns `null` ([SessionLoader.cs:43](../../Assets/Modules/GameSession/Scripts/GeneratedCode/SessionLoader.cs#L43)–[69](../../Assets/Modules/GameSession/Scripts/GeneratedCode/SessionLoader.cs#L69)). The version check in the template is commented out ([SessionLoaderTemplate.tt:124](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/SessionLoaderTemplate.tt#L124)). The minor version is read but not used.

**Failure.** A save from a newer build fails only because the serializer then reads the first payload bytes as the EOF marker. The log says `EoF marker was not found` instead of naming the version. The rejected load then triggers [GS1](#gs1-a-failed-load-replaces-the-live-model).

**Fix.** Before reading the payload, reject a major above `SaveGameData.VersionMajor` or below 1, and log both versions.

### GS3. Migration and the envelope have no regression tests

**Severity:** Medium. **Status:** Confirmed.

**Problem.** A broken conversion loses player progress, but no test loads an old save.

**Evidence.** [GameSessionTests.cs](../../Assets/Modules/GameSession/Tests/GameSessionTests.cs) covers the stream helpers, the Elias gamma coder, bitsets, and one round trip of `BossData`. Nothing tests `SessionSerializer`, `SessionLoader` conversions, `SessionLoader_v1`, or `LegacyDataLoader`. The repository has no save fixtures.

**Effect.** [GS4](#gs4-legacy-quest-statistics-get-a-wrong-completion-time) and [GS5.4](#gs5-smaller-defects) were not caught. A future schema version has nothing to compare against.

**Direction.** Add one binary fixture per supported format (headerless, v1, v2, v3). Load each fixture through `SessionSerializer` and compare selected fields. Add a round trip of a fully populated v3 model.

### GS4. Legacy quest statistics get a wrong completion time

**Severity:** Low. **Status:** Confirmed.

**Where.** `TransferQuestData` passes `item.Value.CompletionCount` as the `lastCompletionTime` argument ([LegacyDataLoader.cs:218](../../Assets/Modules/GameSession/Scripts/Facade/LegacyDataLoader.cs#L218)–[219](../../Assets/Modules/GameSession/Scripts/Facade/LegacyDataLoader.cs#L219)). The obsolete reader has the real value in `LastCompletionTime` ([QuestData.cs:413](../../Assets/Modules/GameSession/Scripts/Obsolete/Content/QuestData.cs#L413)).

**Failure.** After a headerless save is converted, the `TimeSinceLastCompletion` requirement computes `TotalPlayTime − completionCount` ([TimeSinceLastCompletion.cs:47](../../Assets/Modules/Quests/Scripts/Requirements/TimeSinceLastCompletion.cs#L47)). A repeatable quest with a cooldown becomes available at once. The conversion runs once per save, so the effect ends after the next completion.

**Fix.** Pass `item.Value.LastCompletionTime`.

### GS5. Smaller defects

| ID | Where | Problem | Status |
| --- | --- | --- | --- |
| GS5.1 | [ObservableList.cs:138](../../Assets/ModulesShared/Utilites/Serialization/ObservableList.cs#L138) | `RemoveRange` calls `TrimExcess` and fires the callback, but removes nothing. No caller today. | Confirmed (latent) |
| GS5.2 | [ObservableList.cs:26](../../Assets/ModulesShared/Utilites/Serialization/ObservableList.cs#L26) | `Sort` changes the serialized order without calling `OnDataChanged`, so the new order is saved only with the next other change. No caller today. | Confirmed (latent) |
| GS5.3 | [GameData.cs:23](../../Assets/Modules/GameSession/Scripts/Facade/Content/GameData.cs#L23) | The `Counter` getter post-increments the stored value. Every read changes the save and marks it dirty. A debugger watch or a log line changes later random seeds. | Confirmed |
| GS5.4 | [ShipComponentInfo.cs:40](../../Assets/Modules/GameSession/Scripts/Model/ShipComponentInfo.cs#L40) | The legacy constructor casts `modification` to `sbyte`. The schema field is `int`. A component modification ID above 127 is corrupted when a headerless save is converted. | Confirmed (latent) |
| GS5.5 | [ShipPresetsData.cs:25](../../Assets/Modules/GameSession/Scripts/Facade/Content/ShipPresetsData.cs#L25) | Preset lists get `_data.Fleet` as their change callback instead of `_data.ShipPresets`. The root still becomes dirty, but `FleetData.DataChanged` is set for preset changes and `ShipPresetsData.DataChanged` never is. | Confirmed |
| GS5.6 | [UtilsClassMember.tt:198](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsClassMember.tt#L198) | `options="encrypted"` works only on scalar `int` and `long`. On a collection it is ignored without an error: `ResearchData.ResearchPoints` is a plain `ObservableMap<int, int>`. Inventory values are always `ObscuredInt`, with or without the option. | Confirmed |
| GS5.7 | [DataSchema.cs:109](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs#L109) | The "value cannot be empty" check tests `item.type` instead of `item.value`, so it never fires. A collection without `value` fails later with `Invalid type - `. The "no xml files" check is inside the loop ([line 88](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs#L88)), so an empty folder is not reported. | Confirmed |
| GS5.8 | [Program.cs:35](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Program.cs#L35) | The generator catches every exception, prints it, and exits with code 0. `generate_code.cmd` cannot detect a failure. An error after `DeleteGeneratedFiles` leaves `Scripts/GeneratedCode/` empty or partial. | Confirmed |
| GS5.9 | [BuilderContext.cs:119](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/BuilderContext.cs#L119) | Version history is keyed by the major number. Two versions with the same major throw, and namespaces are `v{major}`. Minor versions therefore cannot be used, although the templates emit minor upgrade hooks ([SessionLoaderTemplate.tt:33](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/SessionLoaderTemplate.tt#L33)). | Confirmed (latent) |
| GS5.10 | [BuilderContext.cs:121](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/BuilderContext.cs#L121) | `ApplySchema` removes entries from `_current.Objects` while it enumerates the same dictionary. On .NET Framework 4.8 this throws `InvalidOperationException` as soon as a new version drops a type. No version has dropped a type yet. | Confirmed (latent) |
| GS5.11 | [CodeWriter.cs:24](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Utils/CodeWriter.cs#L24) | Every run deletes the whole `GeneratedCode` folder, including the Unity `.meta` files, and rewrites every file. Unity can assign new GUIDs to the recreated scripts, which shows as `.meta` churn in Git and forces a full recompile. The Database generator writes only changed files. | Plausible |
| GS5.12 | [`.CodeGenerator/.gitignore`](../../Assets/Modules/GameSession/.CodeGenerator/.gitignore) | `bin/` is ignored, so a fresh clone has no `CodeGenerator.exe` and must build the .NET Framework 4.8 solution first. The submodule's own `Schema/` copy has only v1 and v2 and differs from `.Schema/` (`ShipComponentInfo.Modification`, `InventoryComponentInfo.Modification`, v2 `RegionData`). | Confirmed |
| GS5.13 | [UtilsClassMember.tt:107](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsClassMember.tt#L107) | An `object` member whose type is a `struct` gets a setter with `if (_x == value) return;`. Generated structs define no `==` operator, so the code does not compile. Structs work only inside collections or other structs. No current object has a direct struct member. | Confirmed (latent) |
| GS5.14 | [ShipComponentInfo.cs:17](../../Assets/Modules/GameSession/Scripts/Model/ShipComponentInfo.cs#L17) | The conversion from `IntegratedComponent` writes `UpgradeLevel = 0` (the real value is commented out). Inventory components converted from v1 saves keep their level ([SessionLoader_v1.cs:66](../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs#L66)). When the player installs such a component, the level is lost at the next fleet save, and removing it returns a level-0 component. If upgrades are retired on purpose, also reset the level in the inventory conversion; otherwise write `component.Info.Level`. | Plausible |
| GS5.15 | [ShipEditorState.cs:117](../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L117) | `PresetStorage` keeps the `null` that `ToShipPreset` returns for an unknown hull. `GetPresets` then throws on `item.Ship` ([line 134](../../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L134)), and `Dispose` throws inside `new ShipPresetInfo(null, …)` ([ShipPresetInfo.cs:14](../../Assets/Modules/GameSession/Scripts/Model/ShipPresetInfo.cs#L14)), so presets are not saved. Happens after a mod removes a hull that has a preset. Fix: skip `null` presets on load. | Plausible |
| GS5.16 | [Skills.cs:53](../../Assets/Scripts/Domain/Skills/Skills.cs#L53) | The "max ship level" skill requirement reads `ShipInfo.Experience` from the session. `PlayerFleet` writes the live experience back only before a save, so the skill unlocks only after the next save. Fix: read `PlayerFleet.Ships`. | Plausible |
| GS5.17 | [PlayerResources.cs:46](../../Assets/Scripts/Domain/Player/PlayerResources.cs#L46) | The snowflakes setter uses `ObservableInventory.SetValue`, which keeps an entry with quantity 0. `PlayerResources.Resources` then lists quest item 25, and [`CargoHoldInventory`](../../Assets/Scripts/Legacy/Game/Quests/Inventory/CargoHoldInventory.cs#L37) creates a product for it with quantity 0. Fix: remove the key when the value is 0. | Plausible |
| GS5.18 | [LocalEvent.cs:20](../../Assets/Scripts/Domain/Galaxy/StarContent/LocalEvent.cs#L20) | The event cooldown is `5 + completedTime % 25` days. Since v2, `completedTime` is `StartTime + hours × TicksPerHour`, and `TicksPerHour` is a multiple of 25. So the modulo equals `StartTime % 25` for every star, and all events in one game have the same cooldown. Fix: take the modulo of the stored hour value, or of a star-based hash. | Confirmed |
| GS5.19 | [LocalStorage.cs:26](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L26) | `TryDecrypt` changes the buffer in place ([line 206](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L206)). The `.err` file and the Android old-version copy ([line 224](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L224)) therefore receive decrypted (or, after a checksum error, scrambled) bytes, not the original file. Neither can be loaded again or compared with the source. Fix: decrypt a copy, or copy the file on disk. | Confirmed |
| GS5.20 | [AppleCloudStorage.cs:287](../../Assets/ModulesPrivate/Services/Scripts/Storage/AppleCloudStorage.cs#L287) | `SetLastError` takes `Substring(0, 150)` when neither regex matches. A shorter message throws. In `ActionGameSaved` and `ActionDataLoaded` the call runs before the status returns to `Idle` ([line 199](../../Assets/ModulesPrivate/Services/Scripts/Storage/AppleCloudStorage.cs#L199), [line 225](../../Assets/ModulesPrivate/Services/Scripts/Storage/AppleCloudStorage.cs#L225)), so the storage stays `Saving` or `Loading`, and every later cloud call returns at once until restart. | Plausible |
| GS5.21 | [AppleCloudStorage.cs:235](../../Assets/ModulesPrivate/Services/Scripts/Storage/AppleCloudStorage.cs#L235) | A failed load returns before it removes the slot from `_loadingGames`. The next load of the same slot throws on `Add` ([line 139](../../Assets/ModulesPrivate/Services/Scripts/Storage/AppleCloudStorage.cs#L139)) and reports a failure; only the third attempt works. | Confirmed |
| GS5.22 | [AppleCloudStorage.cs:246](../../Assets/ModulesPrivate/Services/Scripts/Storage/AppleCloudStorage.cs#L246) | The bad-save message uses the key `BadSavegameError` without the `$` prefix. `Localization.TryGetString` returns such a key as literal text, so the player sees `BadSavegameError`. Google uses `$BadSavegameError`. | Confirmed |
| GS5.23 | [CloudDataAdapter.cs:89](../../Assets/ModulesPrivate/Services/Scripts/Storage/CloudDataAdapter.cs#L89) | The repacked legacy cloud save declares 13 sections but contains 12. The legacy reader reads a 13th name past the end, throws, and returns `null`, so every cloud save in cloud format 0 or 1 fails to load. | Confirmed |
| GS5.24 | [GoogleCloudStorage.cs:150](../../Assets/ModulesPrivate/Services/Scripts/Storage/GoogleCloudStorage.cs#L150) | The slot metadata uses `TimeSpan.FromTicks(TimePlayed)`, but `TimePlayed` is in seconds. Google shows almost zero play time. `UseLongestPlaytime` still orders saves correctly. Fix: `TimeSpan.FromSeconds`. | Confirmed |
| GS5.25 | [ExportableLocalStorage.cs:7](../../Assets/ModulesPrivate/Services/Scripts/Storage/ExportableLocalStorage.cs#L7) | Export copies the main file without a save first. Changes since the last save trigger, including domain state that waits for the write-back, are not in the exported file. Fix: run the normal save (signal and `Save`) before the export. | Plausible |
| GS5.26 | [LocalStorage.cs:55](../../Assets/ModulesPrivate/Services/Scripts/Storage/LocalStorage.cs#L55) | The skip check compares only `GameId` and `DataVersion`. Two devices that continue the same save reach the same pair with different data. After a cloud load of the other device's save, the local save is skipped until the next change; if the game closes first, the next start loads the old local state. | Plausible |
| GS5.27 | [DailyReward.cs:85](../../Assets/Scripts/Domain/Player/DailyReward.cs#L85) | `GetRewardSizeAndUpdate` accepts `daysLeft == 0`: a second collection on the same day gives a reward of size 1 and restarts the streak. `CollectReward` returns `null` when there is no internet time or the clock moved back, and [`DailyRewardState`](../../Assets/Scripts/GameStateMachine/States/DailyRewardState.cs#L35) iterates the result without a check. Today only [`MainMenuState.CheckDailyReward`](../../Assets/Scripts/GameStateMachine/States/MainMenuState.cs#L71) opens the state, after `IsRewardExists` (which requires `daysLeft > 0`), and a suspended main menu cannot open it twice. Fix: return 0 for `daysLeft <= 0`, and return an empty list instead of `null`. | Confirmed (latent) |

### GS6. An unknown satellite in the inventory deletes stored satellites

**Severity:** Medium. **Status:** Plausible.

**Where.** `PlayerInventory.Load` adds `_database.GetSatellite(id)` to a dictionary-backed collection ([PlayerInventory.cs:59](../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L59)). For an ID that the database does not have, `GetSatellite` returns `null` (`Satellite.DefaultValue`), and the dictionary throws `ArgumentNullException`. The signal dispatcher logs the exception and continues with the next handler ([SmartWeakEvent.cs:135](../../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs#L135)), so `Load` stops before it clears the dirty flags ([PlayerInventory.cs:62](../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L62)).

**Failure.**
1. A mod update removes a satellite that the player has in storage.
2. On load, the satellites read before the unknown one are in the live inventory. The rest are not.
3. Both collections are still dirty, so the next `SessionAboutToSaveSignal` clears `Inventory.Satellites` and writes only the partial list ([PlayerInventory.cs:77](../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L77)).
4. Every satellite after the unknown one in dictionary order is deleted from the save. Switching back to the old mod version does not restore them.

Unknown components do not have this problem: `ToComponentInfo` returns `ComponentInfo.Empty`, and the loop skips it.

**Fix.** Skip `null` satellites with a log entry, as the component loop does.

### GS7. A cloud save to a new slot skips the write-back

**Severity:** Medium. **Status:** Confirmed.

**Where.** `GameDataManager.SaveGameToCloud(string)` calls `IDataStorage.Save` and `ICloudStorage.Save` without `SessionAboutToSaveSignal` ([GameDataManager.cs:114](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L114)). The overload for an existing slot fires the signal ([line 124](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L124)). `PlayerFleet`, `PlayerInventory`, and `GameTime` copy their live state into the session only on that signal. The method also does not check `IsGameStarted`.

**Failure.**
1. On the star map, the player buys a component. Money changes in the session at once; the component stays in the live `PlayerInventory`.
2. The player opens the main menu settings and saves to a new cloud slot. No save trigger ran in between.
3. The slot has the lower money but not the component. Fleet changes and `TotalPlayTime` are also from the previous save, so quest start times in the slot can be later than the saved play time.
4. Loading that slot on another device loses the component.

**Fix.** Route both overloads through one method that checks `IsGameStarted`, fires the signal, saves locally, and then saves to the cloud.

### GS8. PlayerPrefs storage writes every mod into the built-in save

**Severity:** High (open-source builds with mods). **Status:** Confirmed.

**Where.** `PlayerPrefsStorage.Save` always writes the key `savegame` ([PlayerPrefsStorage.cs:40](../../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs#L40)). `TryLoad` reads `savegame.<mod>` for a mod ([line 64](../../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs#L64)). The header has no mod field, so a load cannot detect the wrong data. Every `LICENSE_OPENSOURCE` build uses this backend, and the standalone and Android builds can load mods.

**Failure.**
1. The player switches to mod `X`. `TryLoad` finds no `savegame.X`. An editable mod imports `savegame`; another mod starts a new game.
2. The first save of the mod session overwrites `savegame`. The built-in game's progress is lost.
3. Switching back to the built-in database loads the mod session's data into the built-in database. Ships, components, and quests with mod-only IDs are dropped or break.

**Fix.** Write to the same key that `TryLoad` reads (`savegame.<ModId>` for a mod), and add the mod ID to the header with a new format number.

### GS9. Cloud saves do not store the mod ID

**Severity:** Medium. **Status:** Confirmed.

**Where.** `CloudDataAdapter.Serialize(data, mod = null)` writes the `mod` argument, not `data.ModId` ([CloudDataAdapter.cs:117](../../Assets/ModulesPrivate/Services/Scripts/Storage/CloudDataAdapter.cs#L117)). Both callers omit it ([GoogleCloudStorage.cs:134](../../Assets/ModulesPrivate/Services/Scripts/Storage/GoogleCloudStorage.cs#L134), [AppleCloudStorage.cs:109](../../Assets/ModulesPrivate/Services/Scripts/Storage/AppleCloudStorage.cs#L109)). `TryLoad` rejects a stored mod that differs from the current database ([CloudDataAdapter.cs:37](../../Assets/ModulesPrivate/Services/Scripts/Storage/CloudDataAdapter.cs#L37)).

**Failure.**
1. In a mod, the player saves to the cloud. The slot is marked as a built-in save.
2. Loading any slot while the mod is active fails with "Invalid mod id". Mods cannot use cloud saves.
3. Loading the mod's slot with the built-in database succeeds. The session gets an empty `ModId`, mod-only content is dropped, and the next save overwrites the built-in `savegame` file.

**Fix.** Pass `data.ModId` in `Serialize`. Optionally show the mod in the slot list, because one list holds the saves of every database.
