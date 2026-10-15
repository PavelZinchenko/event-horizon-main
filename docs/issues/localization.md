# Localization Module Issues

This file records problems in `Assets/ModulesShared/Localization/`, in its catalogs, and in the application code that drives it. For a description of the current design, see [Localization module](../localization.md). A related storage defect is in [Database issues](database.md):

| ID | Topic |
| --- | --- |
| [DB5](database.md#db5-error-handling-differs-by-storage) | Two catalogs for one language in a mod make the mod fail (`Dictionary.Add`). |

The analysis was done by reading the code and by parsing all catalogs with a script on 2026-10-12. No issue was reproduced in Play Mode. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain. The failure follows directly from the code and the data.
- **Confirmed (latent)** – the code path is certain, but built-in data or current scenes do not trigger it. Mod data or a future change can.
- **Plausible** – the code is wrong, but the failure needs specific UI or platform conditions.

Defects use Where / Failure / Fix. Structural issues use Problem / Evidence / Direction.

All catalog paths below are relative to [`Resources/Localization/`](../../Assets/ModulesShared/Localization/Resources/Localization/).

## Defects

### L1. Polish plural strings have too few forms

**Severity:** Medium. **Status:** Confirmed.

**Where.** The Polish plural rule returns form 1, 2, or 3. These Polish values give only two forms:

| Key | File |
| --- | --- |
| `PlayerExperience` | [Polish/Common.xml:7](../../Assets/ModulesShared/Localization/Resources/Localization/Polish/Common.xml#L7) |
| `OutOfSpaceWarning` | [Polish/Constructor.xml:71](../../Assets/ModulesShared/Localization/Resources/Localization/Polish/Constructor.xml#L71) |
| `SalvageDronesDescription` (two plural blocks) | [Polish/Frontier.xml:93](../../Assets/ModulesShared/Localization/Resources/Localization/Polish/Frontier.xml#L93) |
| `ExplorationCounter` | [Polish/Stars.xml:14](../../Assets/ModulesShared/Localization/Resources/Localization/Polish/Stars.xml#L14) |

**Failure.** For a count that selects form 3 (0, 5–20, 25–30, …), `AddPluralForm` reads `items[3]` and throws `IndexOutOfRangeException` ([Localization.cs:209](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L209)). `GetString` catches it and shows the English text. A Polish player sees "The crew gained 5 experience points" in English. An exception is logged each time.

**Fix.** Add the third form to each value. Add a catalog check that compares form counts with the rule (see [L18](#l18-there-is-no-validation-or-test)).

### L2. Polish and Czech plural rules are wrong

**Severity:** Medium. **Status:** Confirmed.

**Where.** Polish uses the Russian rule `11-14%100:3 1%10:1 2-4%10:2 *:3`. Czech uses the English rule `1:1 *:2`. Both are in the `PluralFroms` key of each language's `Common.xml`.

**Failure.**
1. Polish: `1%10:1` matches 21, 31, 101. Polish uses the "many" form there ("21 punktów", not "21 punkt"). Every Polish plural is wrong for these numbers.
2. Czech: Czech has three forms (1 "bod", 2–4 "body", 5+ "bodů"). The two-form rule shows "2 bodů". The Czech strings also give only two forms.

**Fix.** Polish: `1:1 12-14%100:3 2-4%10:2 *:3`. Czech: `1:1 2-4:2 *:3`, and add the 2–4 form to each Czech plural string. Check the other languages against the CLDR plural rules.

### L3. Malformed placeholders in translations

**Severity:** Medium. **Status:** Confirmed.

**Where and failure.**

| Entry | Text | What the player sees |
| --- | --- | --- |
| [Czech/Common.xml:7](../../Assets/ModulesShared/Localization/Resources/Localization/Czech/Common.xml#L7) `PlayerExperience` | `%{1\|bod\|bodů}` | The literal `%{1\|bod\|bodů}`. |
| [French/Upgrades.xml:18](../../Assets/ModulesShared/Localization/Resources/Localization/French/Upgrades.xml#L18) `ShipLevelsRequirement` | `{1\|vaisseau\|vaisseaux}` without `$` | The literal braces. |
| [Italian/Constructor.xml:66](../../Assets/ModulesShared/Localization/Resources/Localization/Italian/Constructor.xml#L66) `OutOfSpaceWarning` | `${\|cella\|celle}` | No word. An exception is logged. |
| [Armenian/Quests.xml:38](../../Assets/ModulesShared/Localization/Resources/Localization/Armenian/Quests.xml#L38) `SOS_DEADSHIP_ACTION_Repair` | `${1 / դրամ / դրամ}` | No word. An exception is logged. |
| [Armenian/Common.xml:136](../../Assets/ModulesShared/Localization/Resources/Localization/Armenian/Common.xml#L136) `EnergyItem` | `+1$` | Always "+1", not the amount. |
| [Armenian/Frontier.xml:74](../../Assets/ModulesShared/Localization/Resources/Localization/Armenian/Frontier.xml#L74) `AttackBonus` | `+1$%` | Always "+1%". |
| [Armenian/Frontier.xml:86](../../Assets/ModulesShared/Localization/Resources/Localization/Armenian/Frontier.xml#L86), [line 112](../../Assets/ModulesShared/Localization/Resources/Localization/Armenian/Frontier.xml#L112) | `$ 1%`, `1$` | Wrong or missing number. |
| [Armenian/Modifications.xml:28](../../Assets/ModulesShared/Localization/Resources/Localization/Armenian/Modifications.xml#L28) `Ship_Infected` | `$ 1%` | "1%", not the value. |
| [Armenian/Upgrades.xml:14](../../Assets/ModulesShared/Localization/Resources/Localization/Armenian/Upgrades.xml#L14), [line 45](../../Assets/ModulesShared/Localization/Resources/Localization/Armenian/Upgrades.xml#L45) | `1$%`, `$ 1%` | Wrong number. |
| [Czech/Quests.xml:38](../../Assets/ModulesShared/Localization/Resources/Localization/Czech/Quests.xml#L38) `SOS_DEADSHIP_ACTION_Repair` | `(1$ ${1\|…})` | "1" instead of the cost. |
| [Turkish/Common.xml:113](../../Assets/ModulesShared/Localization/Resources/Localization/Turkish/Common.xml#L113) `InvitationReceived` | `$ 1` | "1" instead of the player name. |

`ApplyParameters` drops a `$` that is not followed by a digit, letter, `{`, or `[` ([Localization.cs:162](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L162)), so `$ 1` and `1$` lose the parameter silently.

**Fix.** Correct the entries. Add the patterns `$ `, `N$`, `%{`, and `{N|` without `$` to a catalog check.

### L4. Translations do not match the English parameters

**Severity:** Medium. **Status:** Confirmed.

**Where.** The script found 37 keys where a translation uses a different set of `$N` than English. Two groups:

1. The translation drops `$1`. The number is not shown. Examples: Armenian `EnergyItem`, `HitPointsMod`, `WeightMod`, `ShieldStrengthDesc`; Czech `DefenseBonus`, `RangeMod`, `BulletVelocityMod`, `RepairRateMod`, `EnginePowerMod`; German `MainEnginePowerDesc`, `PlanetaryScannerDesc`; French `UpgradeShipDescription`; Spanish `BetterLootDescription`; Turkish `InvitationReceived`, `StarbaseAttackDescription`; Chinese, Hungarian, and Turkish `Ship_WeaponClassMod`.
2. The translation adds `$1` where English has none. Examples: `SOS_OUTOFFUEL_ACTION_Give` in German ([Quests.xml:42](../../Assets/ModulesShared/Localization/Resources/Localization/German/Quests.xml#L42)), Italian, Korean, Portuguese, and Turkish; `ArenaShipNotAllowed` in seven languages; Turkish `PlayerExperience` adds `$2`.

**Failure.** For group 2, the caller passes no parameters. [QuestAction.cs:29](../../Assets/Scripts/Legacy/GUI/ViewModel/Quests/QuestAction.cs#L29) calls `GetString(action.Text)` for the button `$SOS_OUTOFFUEL_ACTION_Give` ([ShipOutOfFuel.json:24](../../Assets/Modules/Database/Resources/Database/Quests/QuestsBeacon/ShipOutOfFuel.json#L24)). With no parameters, `ApplyParameters` returns the value unchanged ([Localization.cs:100](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L100)). A German player sees the button "$1 ${1|Einheit|Einheiten} Treibstoff geben.". `ArenaShipNotAllowed` has no caller, so it does no harm now.

**Fix.** Make each translation use the English parameter set. Add the comparison to a catalog check.

### L5. Keys used by data and scenes are missing from every catalog

**Severity:** Medium. **Status:** Confirmed.

**Where.**
- Component names in the built-in database: `$GravityBomb` ([GravityBomb.json](../../Assets/Modules/Database/Resources/Database/Component/GravityBomb.json)), `$SuperStealth` ([SuperStealthField.json](../../Assets/Modules/Database/Resources/Database/Component/SuperStealthField.json)), `$Teleporter_Boss` ([Teleporter_Boss1.json](../../Assets/Modules/Database/Resources/Database/Component/Teleporter_Boss1.json) and two more), `$VampiricRay_L`, `$VampiricRay_M`, `$DroneBay_S_91`. English has `VampiricRay_L_1` and `VampiricRay_M_1`, not the names that the records use.
- Ehopedia scene texts under a `CanvasLocalizer`: `$MenuShips`, `$MenuModules`, `$MenuWeapons`, `$MenuSatellites`, `$MenuFactions`, `$NoDescriptionText` ([EhopediaScene.unity](../../Assets/Scenes/EhopediaScene.unity)). Ehopedia development is suspended and it does not ship, so this part is Low and needs action only when Ehopedia resumes.

**Failure.** `GetString` returns the raw key. The player sees `$GravityBomb` wherever the component name shows. Whether these components are visible to the player was not checked.

**Fix.** Add the keys to English, or correct the references. Add a check that every `$Key` in code, scenes, prefabs, and database JSON exists in English. The keys that code builds by concatenation (`$Class`, `$EmptySlot`, `$Object`, `$PlanetType`, `$ThreatLevel`) need an allow-list.

### L6. Duplicate keys with different values

**Severity:** Low. **Status:** Confirmed.

**Where.**
- English and Russian `Constructor.xml` define `EnergyDamage` and `HeatDamage` twice: "Energy damage" at [line 104–106](../../Assets/ModulesShared/Localization/Resources/Localization/English/Constructor.xml#L104), "Energy" at [line 119–120](../../Assets/ModulesShared/Localization/Resources/Localization/English/Constructor.xml#L119).
- Korean defines `ChallengeDescription` twice in `Common.xml` with different texts, and `FuelTank` in both [Components.xml:80](../../Assets/ModulesShared/Localization/Resources/Localization/Korean/Components.xml#L80) and [Constructor.xml:12](../../Assets/ModulesShared/Localization/Resources/Localization/Korean/Constructor.xml#L12) with different texts.
- Same-value duplicates exist in every language except Portuguese: `ChallengeDescription`, `MaxLevelReached` (`Common.xml` and `Frontier.xml`), `HeavyLightningCannon`.

**Failure.** The second value in a file is dead text. A translator who edits it sees no change. For the cross-file Korean `FuelTank`, the result depends on `Resources.LoadAll` order, which Unity does not specify. Each duplicate writes a log line on every load.

**Fix.** Remove the duplicates. If both meanings are needed, use two keys.

### L7. Reload database does not reload localization

**Severity:** Medium. **Status:** Confirmed.

**Where.** Only [`SettingsDatabase.OnDatabaseLoaded`](../../Assets/Scripts/Gui/MainMenu/SettingsDatabase.cs#L62) calls `Initialize(..., forceReload: true)` after a database load. It listens on the messenger of the settings scene. The main-menu button [`MainMenu.ReloadDatabase`](../../Assets/Scripts/Gui/MainMenu/MainMenu.cs#L101) calls `GameDataManager.LoadMod(_database.Id, true)` while the settings scene is not loaded. The same is true for the fallback `_database.LoadDefault()` in [GameDataManager.cs:103](../../Assets/Scripts/GameServices/DataManager/GameDataManager.cs#L103) when that path runs outside the settings scene.

**Failure.** After a main-menu database reload, both dictionaries still hold the strings of the previous database load. New or changed mod keys show as `$Key` or with the old text until the language changes or the game restarts. A later language change reloads only the selected language, not English, so English fallbacks stay old.

**Fix.** Subscribe one long-lived service (for example a small `LocalizationReloader` bound in `ServicesInstaller`) to `IDatabase.DatabaseLoaded` and call `Initialize(..., forceReload: true)`. Remove the call from `SettingsDatabase`. Consider making `Initialize` compare the database identity and reload by itself.

### L8. The language change is forwarded as ArrivedToPlanet

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [SignalsTranslator.cs:265](../../Assets/Scripts/Services/Messenger/SignalsTranslator.cs#L265) handles `LocalizationChangedSignal` with `_messenger.Broadcast<string>(EventType.ArrivedToPlanet, language)`. The intended event `EventType.LocalizationChanged` exists ([EventType.cs:78](../../Assets/Modules/ServicesFacade/Scripts/Messenger/EventType.cs#L78)) and has no sender.

**Failure.** [GameMenu.cs:89](../../Assets/Scripts/Gui/StarMap/GameMenu.cs#L89) listens to `ArrivedToPlanet` as `Callback<int>`. The messenger casts each listener to `Callback<string>` ([MessengerContext.cs:157](../../Assets/Modules/Services/Scripts/Messenger/MessengerContext.cs#L157)) and throws `InvalidCastException` if `GameMenu` is alive. The exception propagates out of `LocalizationManager.Initialize`. Today the language changes only in the main menu, when the star-map scene is not loaded, so the cast does not run.

**Fix.** Broadcast `EventType.LocalizationChanged`, or remove the forwarding.

### L9. The language dropdown ignores the system language

**Severity:** Low. **Status:** Confirmed (display), Plausible (selection).

**Where.** [SettingsGeneral.cs:97](../../Assets/Scripts/Gui/MainMenu/SettingsGeneral.cs#L97) selects the entry whose `folder == GameSettings.Language`. A new player has an empty setting, and the game uses the system language.

**Failure.**
1. A player whose device is set to Russian sees Russian text and "English" in the dropdown.
2. That player selects "English". The dropdown value does not change, so Unity fires no `onValueChanged`, and the language stays Russian. The player must select another language first.
3. The comparison is case-sensitive, while `Initialize` compares without case.
4. If the language list file is missing, `LoadLocalizationList` returns `null` and `InitializeLanguageDropdown` throws `NullReferenceException`.

**Fix.** Select the entry that matches `ILocalization.Language` (case-insensitive). Handle a `null` list.

### L10. The Space key label does not follow the language

**Severity:** Low. **Status:** Confirmed.

**Where.** [`KeyNameLocalizer.TryGetName`](../../Assets/ModulesShared/Localization/Scripts/Localization/KeyNameLocalizer.cs#L36) fills its cache once. The code that clears it on a language change is commented out ([line 13–17](../../Assets/ModulesShared/Localization/Scripts/Localization/KeyNameLocalizer.cs#L13), [line 31](../../Assets/ModulesShared/Localization/Scripts/Localization/KeyNameLocalizer.cs#L31)). It refers to a `LanguageChangedSignal` type that does not exist.

**Failure.** After a language change, the key-binding panel shows "Space" in the first language used. Other key names come from the Input System in English in every language. `KeySpace` exists only in English, Russian, and Turkish.

**Fix.** Subscribe to `LocalizationChangedSignal` and clear the cache, or do not cache the Space label.

### L11. Nested keys have limits

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [Localization.cs:148](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L148) and [line 100](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L100).

**Failure.**
1. Nested keys expand only when the call passes at least one parameter. Without parameters, `$OtherKey` stays as text.
2. The nested value is inserted raw: its `\n` stays as two characters and its own `$` expressions do not expand.
3. The nested key does not fall back to English. A translation that refers to a key it lacks shows `[$Key]`.
4. A nested key name stops at `_`, but `KeyParser` allows `_` in keys.

No built-in value hits these cases now. A translator or modder can.

**Fix.** Run nested values through the same `GetKey` path (with a depth limit), with English fallback, and apply it with or without parameters. Use one key-character rule for both parsers.

### L12. Inline expressions have a narrow grammar

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [KeyParser.cs:30](../../Assets/ModulesShared/Localization/Scripts/Localization/KeyParser.cs#L30).

**Failure.**
1. A parameter ends at the first character that is not a letter, digit, `_`, or `$`. `$Damage|-5` passes an empty parameter and leaves `-5` as text. `$Damage|1.5` passes `1`.
2. A `$` inside the key does not end it. `$A$B` is parsed as the key `A$B` and is not found.
3. A trailing `|` adds an empty parameter.

**Fix.** Document the grammar for data authors (done in [Inline expansion](../localization.md#inline-expansion-localize)). If data needs it, accept a quoted or escaped parameter form, and end a key at `$`.

### L13. Smaller defects

| ID | Where | Defect | Status |
| --- | --- | --- | --- |
| L13.1 | [Localization.cs:113](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L113) | A value that ends with `$` becomes an empty string when the call has parameters. Without parameters it shows as written. | Confirmed (latent) |
| L13.2 | [KeyParser.cs:102](../../Assets/ModulesShared/Localization/Scripts/Localization/KeyParser.cs#L102) | A `null` object parameter throws `NullReferenceException`. `GetString` falls back to English, which throws again, and returns the raw key. | Confirmed (latent) |
| L13.3 | [Localization.cs:198](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L198) | The plural count uses `Convert.ToInt32(string)`. A formatted number ("1,000", "1.5") or a value above `int.MaxValue` throws. The whole string falls back. | Confirmed (latent) |
| L13.4 | [PluralForms.cs:13](../../Assets/ModulesShared/Localization/Scripts/Localization/PluralForms.cs#L13), [Localization.cs:94](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L94) | A malformed `PluralFroms` (double space, missing `:`) throws from the `Localization` constructor and out of `Initialize`. Startup stops. Only a mod-only language can supply its own `PluralFroms`, because built-in values win. | Confirmed (latent) |
| L13.5 | [Localization.cs:91](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L91) | The rule key is spelled `PluralFroms`. Correct spelling silently disables plurals (`[$0]`). | Confirmed |
| L13.6 | [Localization.cs:51](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L51) | `&Key` uppercases with the current culture, not the game language. On a Turkish-culture device, English `i` becomes `İ`. No code or scene uses `&`. | Confirmed (latent) |
| L13.7 | [LocalizationManager.cs:107](../../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs#L107) | The signal carries the requested language, not the loaded one. For `Ukrainian` it carries `Ukrainian` while English is loaded. It fires even when nothing changed or loading failed. | Confirmed |
| L13.8 | [LocalizationManager.cs:71](../../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs#L71) | Every lookup of a missing key logs an error. UI that refreshes each frame floods the log. | Confirmed |
| L13.9 | [Localization.cs:77](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L77) | Every lookup runs two `string.Replace` calls and allocates. The values could be processed once at load time. | Confirmed |
| L13.10 | [ResourceLoader.cs:78](../../Assets/ModulesShared/Localization/Scripts/Localization/ResourceLoader.cs#L78) | A duplicate key is reported with `Debug.Log`, not as a warning, so a mod author does not see that a value was ignored. | Confirmed |
| L13.11 | [ResourceLoader.cs:19](../../Assets/ModulesShared/Localization/Scripts/Localization/ResourceLoader.cs#L19) | `LoadLocalizationList` does not catch XML errors and returns `null` when the file is missing. | Confirmed (latent) |
| L13.12 | [Languages.xml](../../Assets/ModulesShared/Localization/Resources/Localization/Languages.xml) | Display names: "Française" should be "Français"; "Türk" should be "Türkçe". The `iso` attribute is never read. | Confirmed |
| L13.13 | [Localization.cs:115](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L115) | Format errors are logged with `UnityEngine.Debug`, while lookup errors use `GameDiagnostics.Trace`. They do not reach the same sink. | Confirmed |

### L14. Enumerations skip the first item and are unused

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [`AddEnumeration`](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L223) picks `1 + p % (count − 1)`, so item 0 is never chosen. The `Characters.xml` keys `FirstNameHM`, `FirstNameHF`, `LastNameH`, `FirstNameA`, `LastNameA` start with a real name (for example "James"). No code, scene, or database record references these keys.

**Failure.** If code starts to use them, the first name never appears. A negative parameter gives a negative index and throws.

**Fix.** Delete the keys and the `$[...]` syntax, or define the syntax with a selector in item 0 and a non-negative modulo.

## Structural issues

### L15. Mods cannot override built-in text

**Severity:** Medium. **Status:** Confirmed.

**Intended behavior.** A mod value must win over the built-in value for the same key (owner decision, 2026-10-12). The current code does the opposite.

**Problem.** Built-in catalogs load before the database catalog, and the first value wins ([Localization.cs:23](../../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L23), [ResourceLoader.cs:76](../../Assets/ModulesShared/Localization/Scripts/Localization/ResourceLoader.cs#L76)).

**Evidence.**
- A mod's `English.xml` that redefines `Loading` or a built-in item name is ignored, with only a `Debug.Log` line.
- A mod with `UnloadOriginalDatabase` still gets all built-in strings, which is useful, but cannot replace any of them.
- A mod-only language is not in the dropdown. It loads only when the device language matches its file name.

**Direction.**
1. In `Localization.TryLoad`, let database values replace resource values. Keep "first wins" only for duplicates inside one source, and log replaced keys at most once per load.
2. Decide what happens across languages. Lookup order is "selected language, then English". If a mod changes key `X` only in `English.xml`, a Russian player still sees the built-in Russian `X`, so the mod change is invisible to that player. Options:
   - Accept it: the mod must supply each language it wants to change.
   - When a mod redefines a key in English, drop the built-in translations of that key in other languages, so the mod's English text shows. This is the safer default for renamed items.
3. Let the dropdown add languages that the active database provides.
4. Update [Database and mod localization](../localization.md#database-and-mod-localization) after the change.

### L16. The language list does not limit languages

**Problem.** `Languages*.xml` only fills the dropdown. All language folders are in `Resources` and ship in every build.

**Evidence.**
- With `Languages.yandex` (English, Russian, Turkish), a device set to German still loads German through `GetSystemLanguage`, and the dropdown then shows "English".
- The Web list hides Chinese, Korean, and Armenian, but their catalogs are still in the Web build.

**Direction.** Validate the requested language against the active list in `Initialize`, and fall back to English when it is absent. If size matters, move catalogs out of `Resources` (for example into Addressables or per-channel folders).

### L17. Translation coverage is not tracked

**Problem.** No tool reports missing or stale keys.

**Evidence.** See [Catalog statistics](../localization.md#catalog-statistics). Portuguese lacks 903 of 1478 English keys, Korean 868, Italian 745. Korean has 57 and Portuguese 48 keys that English no longer has. These are probably renamed or deleted keys.

**Direction.** Add a report to the catalog check. Remove keys that English does not have. Decide whether low-coverage languages stay in the default list.

### L18. There is no validation or test

**Problem.** The parser and formatter are hand-written and have many edge cases (L1, L3, L4, L11–L14). No test assembly covers them, and no tool checks the catalogs.

**Evidence.** No test references `Services.Localization`. The defects in L1–L6 were found by a one-off script.

**Direction.**
1. Add an EditMode test assembly for `KeyParser`, `PluralForm`, and `Localization.ApplyParameters`. Internal types need `InternalsVisibleTo`.
2. Add an editor or CI check that does the steps in the [validation checklist](../localization.md#validation-checklist): parse, duplicates, missing and extra keys, parameter sets, plural form counts, stray `$`, and unknown references.

### L19. The font fallback is dead code

**Problem.** `SystemFontLocator` and `FontTable` exist and have data, but do nothing.

**Evidence.**
- [`FontTable.GetFontList`](../../Assets/ModulesShared/Localization/Scripts/Localization/FontTable.cs#L31) has its body commented out and yields nothing.
- No scene or prefab contains a `SystemFontLocator`.
- [FontTable.asset](../../Assets/Modules/AppConfiguration/ScriptableObjects/FontTable.asset) lists Chinese, Korean, and Armenian fonts per platform.
- If enabled, `SystemFontLocator` would edit the shared `PanelTextSettings` asset. In the editor that change persists after Play Mode.
- `FontTable` has no `FontList` branch for WebGL or other platforms, so enabling the body would not compile there.

**Context.** The code belongs to the planned move from uGUI to UI Toolkit. That move was suspended because of UI Toolkit limitations at the time. Keep the code until the move is re-evaluated.

**Direction.**
1. First check whether UI Toolkit in Unity 6.0 is ready for the game's UI. This decides whether the font fallback is needed at all.
2. If the move resumes: restore the lookup, add a WebGL branch, place the locator in a persistent scene, and work on a runtime copy of the panel settings.
3. If the move is dropped: delete both classes and the asset.
4. In both cases, verify CJK and Armenian glyphs on each platform. uGUI text does not use this fallback.

### L20. The module mixes unrelated concerns

**Problem.** The assembly holds string lookup, input-control names, and font management.

**Evidence.** It references `Unity.InputSystem` only for `KeyNameLocalizer`, and `Zenject` only for `SystemFontLocator`. It references `AppConfiguration` only for the language list name.

**Direction.** Keep the core (catalogs, lookup, formatting) free of Unity UI and input. Move `KeyNameLocalizer` next to the input settings UI, and the font code to the Gui module. Pass the language list name in from the application.

### L21. A language change needs a scene reload

**Problem.** UI text is localized once. `CanvasLocalizer` overwrites `Text.text` with the result and loses the key ([CanvasLocalizer.cs:33](../../Assets/Scripts/Gui/Utils/CanvasLocalizer.cs#L33)).

**Evidence.** The settings screen fires `ReloadUiSignal`, and `MainMenuState` reloads its scenes. Only `LoadingScreen` reacts to `LocalizationChangedSignal` directly. This is why the language can change only from the main menu.

**Direction.** Keep the key in a component (for example a `LocalizedText` with a serialized key) and refresh it on `LocalizationChangedSignal`. This is part of the UI migration in [Application layer issues](application_layer.md).
