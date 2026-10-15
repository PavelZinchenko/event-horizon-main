# Localization Module

The Localization module turns localization keys into text in the player's language. It loads XML string catalogs from Unity Resources and from the active database or mod. It keeps one dictionary for the selected language and one English dictionary as a fallback. It expands parameters, nested keys, and plural forms, and it fires a signal when the language changes. The module also gives display names for input-control paths and contains a disabled system-font fallback.

| | |
| --- | --- |
| Assembly | `Localization` ([Localization.asmdef](../Assets/ModulesShared/Localization/Localization.asmdef)). References: AppConfiguration, CommonComponents, Database, Diagnostics, Unity.InputSystem, Zenject. |
| Namespace | `Services.Localization` |
| Location | [`Assets/ModulesShared/Localization/`](../Assets/ModulesShared/Localization/), inside the `Assets/ModulesShared` Git submodule. |
| Source | [`Scripts/Localization/`](../Assets/ModulesShared/Localization/Scripts/Localization/) |
| Catalogs | [`Resources/Localization/`](../Assets/ModulesShared/Localization/Resources/Localization/) |
| Assemblies that use it | BattleSimulator, Quests, ShipConstructor, Gui, ShipEditor, and the application (`Assets/Scripts`). |
| Tests | None. |
| Known issues | [Localization issues](issues/localization.md) |

## Contents

1. [Terms](#terms)
2. [Source map](#source-map)
3. [Composition](#composition)
4. [Runtime overview](#runtime-overview)
5. [Language lists](#language-lists)
6. [String catalogs](#string-catalogs)
7. [Loading a dictionary](#loading-a-dictionary)
8. [Initialization and language selection](#initialization-and-language-selection)
9. [Lookup: GetString](#lookup-getstring)
10. [Inline expansion: Localize](#inline-expansion-localize)
11. [Value formatting](#value-formatting)
12. [Plural forms](#plural-forms)
13. [Enumerations](#enumerations)
14. [Error behavior](#error-behavior)
15. [Language change in the application](#language-change-in-the-application)
16. [Database and mod localization](#database-and-mod-localization)
17. [Input-control names: KeyNameLocalizer](#input-control-names-keynamelocalizer)
18. [Font fallback: SystemFontLocator and FontTable](#font-fallback-systemfontlocator-and-fonttable)
19. [Catalog statistics](#catalog-statistics)
20. [Adding or changing text](#adding-or-changing-text)
21. [Validation checklist](#validation-checklist)

## Terms

| Term | Meaning |
| --- | --- |
| Key | The name of a string in a catalog, for example `Loading`. Keys are case-sensitive. |
| Key reference | A key with a prefix character: `$Loading` or `&Loading`. Code, scenes, and database records store key references. Text without a prefix is literal text. |
| Catalog | One XML file with `<resources>` root and `<string name="Key">value</string>` entries. |
| Language folder | A folder under `Resources/Localization/` named after a language, for example `Russian`. The folder name is the language ID in code and settings. |
| Dictionary | A `Localization` object: all keys of one language merged from all catalogs of that language. |
| Active dictionary | The dictionary of the selected language. It is `null` when the selected language is English. |
| Default dictionary | The English dictionary. It is always loaded and is the fallback for every lookup. |
| Language list | An XML file that lists the languages the settings screen offers. |
| Parameter | A value that the caller passes with a key. A value refers to it as `$1`, `$2`, and so on. |
| Plural rule | A rule from the `PluralFroms` key that maps a number to a plural form number. |

## Source map

| File | Contents |
| --- | --- |
| [ILocalization.cs](../Assets/ModulesShared/Localization/Scripts/Localization/ILocalization.cs) | `ILocalization` (public API), XML data classes (`XmlLanguageInfo`, `XmlLocalizationList`, `XmlKeyValuePair`, `XmlLocalization`), `LocalizationException`, and `LocalizationChangedSignal`. |
| [LocalizationManager.cs](../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs) | `LocalizationManager`, the only `ILocalization` implementation. It holds the active and default dictionaries, selects the language, runs `Localize`, and fires the signal. |
| [Localization.cs](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs) | `Localization` (internal): one dictionary, key lookup, parameter expansion, nested keys, plural forms, and enumerations. |
| [ResourceLoader.cs](../Assets/ModulesShared/Localization/Scripts/Localization/ResourceLoader.cs) | `ResourceLoader` (internal, static): reads the language list and the catalogs from Resources and from `IDatabase`. |
| [KeyParser.cs](../Assets/ModulesShared/Localization/Scripts/Localization/KeyParser.cs) | `KeyParser` (internal): finds `$Key|param|param` in free text. `Parameters` (internal): a read-only view over `object` or `string` parameters. The special characters `$`, `&`, and `\|`. |
| [PluralForms.cs](../Assets/ModulesShared/Localization/Scripts/Localization/PluralForms.cs) | `PluralForm` (internal): parses and matches one plural rule. |
| [KeyNameLocalizer.cs](../Assets/ModulesShared/Localization/Scripts/Localization/KeyNameLocalizer.cs) | `IKeyNameLocalizer` and `KeyNameLocalizer`: display names for Input System control paths. |
| [FontTable.cs](../Assets/ModulesShared/Localization/Scripts/Localization/FontTable.cs) | `FontTable` ScriptableObject: per-platform, per-language font candidates. Its lookup is disabled. |
| [SystemFontLocator.cs](../Assets/ModulesShared/Localization/Scripts/Localization/SystemFontLocator.cs) | `SystemFontLocator` MonoBehaviour: adds OS fonts as UI Toolkit fallback fonts on language change. No scene uses it. |
| [Resources/Localization/Languages*.xml](../Assets/ModulesShared/Localization/Resources/Localization/) | Language lists for build channels. |
| `Resources/Localization/<Language>/*.xml` | Catalogs, one folder per language, files grouped by feature. |

External files that belong to the flow:

| File | Role |
| --- | --- |
| [AppConfig.cs](../Assets/Modules/AppConfiguration/Scripts/Generated/AppConfig.cs#L7) | Generated. `AppConfig.localizationFile` names the language list (`Languages`). |
| [ApplicationManager.cs](../Assets/Editor/ApplicationManager.cs#L354) | Editor window that generates `AppConfig.cs`, including `localizationFile`. |
| [FontTable.asset](../Assets/Modules/AppConfiguration/ScriptableObjects/FontTable.asset) | The only `FontTable` instance. |
| [GameSettings.cs](../Assets/Modules/Services/Scripts/Settings/GameSettings.cs#L72) | Stores the selected language folder name under the `language` key. Empty means "use the system language". |

## Composition

[ServicesInstaller.cs:46](../Assets/Scripts/Installers/ServicesInstaller.cs#L46) binds `ILocalization` to a singleton `LocalizationManager`. Line 47 binds `KeyNameLocalizer` to its interfaces as a singleton. [Lines 208–209](../Assets/Scripts/Installers/ServicesInstaller.cs#L208) bind `LocalizationChangedSignal` and its `Trigger`. `LocalizationManager` gets the trigger in its constructor ([LocalizationManager.cs:10](../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs#L10)).

[TestSceneInstaller.cs:55](../Assets/Scripts/Installers/TestSceneInstaller.cs#L55) creates its own `LocalizationManager` for test scenes and initializes it with `"English"` and the default database.

The module does not bind itself. It has no Zenject installer.

## Runtime overview

```text
AppConfig.localizationFile ──► Resources/Localization/<list>.xml ──► settings dropdown only
                                        
GameSettings.Language ("" = system language)
        │
        ▼
LocalizationManager.Initialize(language, IDatabase, forceReload)
        │
        ├── English dictionary   ◄── Resources/Localization/English/*.xml
        │   (first call or forceReload)   + IDatabase.GetLocalization("English")
        │
        ├── selected dictionary  ◄── Resources/Localization/<language>/*.xml
        │   (null for English)            + IDatabase.GetLocalization(<language>)
        │
        └── LocalizationChangedSignal.Fire(language)

GetString("$Key", args)  ──► selected dictionary ──► English dictionary ──► "$Key" + error log
Localize("text $Key|a|b") ──► KeyParser ──► same lookup for each embedded key
```

The two lookup paths:

1. `GetString` takes one key reference and optional parameters. Callers use it for most UI text.
2. `Localize` takes free text and replaces each embedded `$Key` expression. Callers use it for text from data that can mix literal text and keys.

## Language lists

[`ResourceLoader.LoadLocalizationList`](../Assets/ModulesShared/Localization/Scripts/Localization/ResourceLoader.cs#L10) loads `Resources/Localization/<AppConfig.localizationFile>` and deserializes `XmlLocalizationList`:

```xml
<languages>
    <string name="Русский" iso="ru" folder="Russian" icon="russian" />
</languages>
```

| Attribute | Use |
| --- | --- |
| `name` | Display name in the language dropdown. Written in the language itself. |
| `folder` | Language ID. It is the catalog folder name and the value stored in `GameSettings.Language`. |
| `iso` | ISO 639-1 code. No code reads it. |
| `icon` | Flag sprite name under `Resources/Textures/GUI/Flags/` ([SettingsGeneral.cs:94](../Assets/Scripts/Gui/MainMenu/SettingsGeneral.cs#L94)). |

| File | Languages |
| --- | --- |
| [Languages.xml](../Assets/ModulesShared/Localization/Resources/Localization/Languages.xml) | English, Russian, German, Spanish, Polish, French, Portuguese, Korean, Turkish, Italian, Chinese, Armenian, Hungarian, Czech (14). The current `AppConfig.localizationFile` value. |
| [Languages.Web.xml](../Assets/ModulesShared/Localization/Resources/Localization/Languages.Web.xml) | The same without Korean, Chinese, and Armenian (11). |
| [Languages.yandex.xml](../Assets/ModulesShared/Localization/Resources/Localization/Languages.yandex.xml) | English, Russian, Turkish (3). |

The language list only fills the settings dropdown. It does not limit which folders ship in the build or which language `Initialize` can load. All 14 language folders are in `Resources` and ship in every build. See [L16](issues/localization.md#l16-the-language-list-does-not-limit-languages).

## String catalogs

Each language folder holds catalogs with this shape ([`XmlLocalization`](../Assets/ModulesShared/Localization/Scripts/Localization/ILocalization.cs#L50)):

```xml
<resources>
    <string name="Loading">Loading...</string>
    <string name="FreeSpace">$1 ${1|cell|cells} free</string>
</resources>
```

Rules for catalogs:

1. The file name has no meaning. All catalogs of one language merge into one dictionary. Files only group keys by feature: `Common`, `Components`, `Constructor`, `Quests`, and others.
2. A key must be unique in the whole language folder. When a key repeats, the first loaded value wins and the loader writes a `Debug.Log` line ([ResourceLoader.cs:76](../Assets/ModulesShared/Localization/Scripts/Localization/ResourceLoader.cs#L76)). Inside one file, "first" is document order. Across files, "first" depends on the order of `Resources.LoadAll`, which Unity does not specify.
3. Keys are case-sensitive (`Dictionary<string, string>` with the default comparer).
4. The value is the XML text content (`[XmlText]`). XML escaping applies: write `&lt;`, `&gt;`, `&amp;`.
5. Line breaks are written as `\n`. The lookup replaces `\n` with a newline and then removes every other backslash ([Localization.cs:77](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L77)). So `\"` shows as `"` and `\'` shows as `'`. A catalog cannot show a literal backslash.
6. The common catalog of each language must define `PluralFroms`. The misspelling is part of the format ([Localization.cs:91](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L91)).

## Loading a dictionary

[`Localization.TryLoad`](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L16) builds one dictionary:

1. It returns `false` for an empty language name.
2. [`ResourceLoader.TryLoadFromResources`](../Assets/ModulesShared/Localization/Scripts/Localization/ResourceLoader.cs#L24) calls `Resources.LoadAll<TextAsset>("Localization/<language>")`. It deserializes each asset and adds its keys. A file that fails to parse is logged with `Debug.LogError` and skipped. The other files still load.
3. If a database is given, [`ResourceLoader.TryLoadFromDatabase`](../Assets/ModulesShared/Localization/Scripts/Localization/ResourceLoader.cs#L46) calls `IDatabase.GetLocalization(language)`. It parses the returned XML and adds its keys. A parse error is logged and ignored.
4. Keys from step 2 come first. A database key with the same name is skipped. See [Database and mod localization](#database-and-mod-localization).
5. If the dictionary has no keys, `TryLoad` returns `false`.
6. The constructor reads `PluralFroms`, splits it on spaces, and parses each rule ([Localization.cs:88](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L88)). A malformed rule throws out of `TryLoad` and out of `Initialize` ([L13](issues/localization.md#l13-smaller-defects)).

`Resources.LoadAll` keeps the loaded `TextAsset` objects in memory until Unity unloads unused assets. The dictionary keeps the raw values. Processing (`\n` replacement, parameters) runs on every lookup.

## Initialization and language selection

[`LocalizationManager.Initialize(language, database, forceReload)`](../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs#L90) runs these steps:

1. If `language` is empty, it uses [`GetSystemLanguage`](../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs#L115). That method returns `Application.systemLanguage.ToString()`. It maps `Chinese`, `ChineseSimplified`, and `ChineseTraditional` to `Chinese`. Unity has no `Armenian` value, so the system language never selects Armenian.
2. It loads the English dictionary when no English dictionary exists yet or when `forceReload` is `true`. A failure is logged with `Debug.LogError`.
3. If `language` is `English` (case-insensitive), it sets the active dictionary to `null`.
4. Otherwise it loads the active dictionary when there is none, when `forceReload` is `true`, or when `language` differs from the loaded language (case-insensitive). If loading fails, the active dictionary becomes `null` and lookups use English. A language without a folder (for example `Ukrainian` from the system) ends here.
5. It fires `LocalizationChangedSignal` with the requested `language` string. It fires the signal on every call, even when nothing was reloaded and even when loading failed.

`ILocalization.Language` returns the loaded language: the active dictionary's language, or `English`. It is `null` before the first `Initialize`.

The `database` argument is only used when a dictionary actually reloads. A call with a new database and the same language does not pick up the new database's strings unless `forceReload` is `true` ([L7](issues/localization.md#l7-reload-database-does-not-reload-localization)).

Callers of `Initialize`:

| Caller | Arguments | When |
| --- | --- | --- |
| [InitializationState.cs:96](../Assets/Scripts/GameStateMachine/States/InitializationState.cs#L96) | `_settings.Language`, database | At startup, after the active mod or the default database has loaded and before the session loads. |
| [SettingsGeneral.cs:47](../Assets/Scripts/Gui/MainMenu/SettingsGeneral.cs#L47) | the selected folder, database | The player picks a language in the settings dropdown. |
| [SettingsDatabase.cs:65](../Assets/Scripts/Gui/MainMenu/SettingsDatabase.cs#L65) | `_settings.Language`, database, `forceReload: true` | `EventType.DatabaseLoaded` arrives while the settings scene is open. |
| [TestSceneInstaller.cs:58](../Assets/Scripts/Installers/TestSceneInstaller.cs#L58) | `"English"`, default database | Test scenes. |

## Lookup: GetString

[`LocalizationManager.GetString(key, params object[] parameters)`](../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs#L62):

1. It calls `TryGetString` on the active dictionary. On success it returns the result.
2. It calls `TryGetString` on the English dictionary. On success it returns the result.
3. It logs `localized string not found: '<key>'` through `GameDiagnostics.Trace.LogError` and returns `key` unchanged, with its prefix.

[`Localization.TryGetString`](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L34) checks the first character of `key`:

| Input | Result |
| --- | --- |
| `null` or empty | Empty string. Success. |
| `$Key` | The value of `Key`, formatted with the parameters. Fails when `Key` is not in this dictionary. |
| `&Key` | The same, converted with `string.ToUpper()` (current culture). No code or scene uses this prefix. |
| Any other text | The text itself. Success. No lookup. |

Consequences:

- Database records store names as `$Key` for built-in content. A mod can store a plain name, and the UI shows it as is.
- A missing key shows as `$Key` in the UI. [`CanvasLocalizer`](../Assets/Scripts/Gui/Utils/CanvasLocalizer.cs#L19) logs such texts with their hierarchy path in the editor.
- `GetString` does not expand keys inside literal text. `GetString("Cost: $Credits")` returns the text unchanged. Use `Localize` for that.
- An exception during formatting is caught in `TryGetString`. It is logged, and the lookup falls through to English. If English also throws, the caller gets the raw key.

## Inline expansion: Localize

[`LocalizationManager.Localize(text)`](../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs#L17) scans `text` for `$`. At each `$` it runs [`KeyParser.TryParse`](../Assets/ModulesShared/Localization/Scripts/Localization/KeyParser.cs#L14). On success it replaces the expression with the looked-up value. If the text has no expression, the same string instance is returned. Any exception returns the original text and is logged.

`KeyParser` grammar:

```text
expression = "$" letter { letter | digit | "_" | "$" } { "|" param }
param      = [ "$" ] { letter | digit | "_" | "$" }
```

1. A key starts with a letter. `$5` or `$_x` is not an expression and stays as text.
2. The key ends at the first `|` or at the first character that is not a letter, digit, `_`, or `$`.
3. Each `|` starts a parameter. A parameter ends at the next `|` or at the first invalid character. Spaces, `-`, `.`, `,`, `(`, and `)` end it. So an inline parameter cannot hold a negative number, a decimal number, or a phrase ([L12](issues/localization.md#l12-inline-expressions-have-a-narrow-grammar)).
4. A parameter that contains `$` is passed through `GetString` before use. Use `|$OtherKey` to pass a localized word.
5. A `$` inside the key does not end it. `$A$B` is read as one key `A$B` and fails ([L12](issues/localization.md#l12-inline-expressions-have-a-narrow-grammar)).

The lookup for an inline key ([LocalizationManager.cs:75](../Assets/ModulesShared/Localization/Scripts/Localization/LocalizationManager.cs#L75)) tries the active dictionary and then English. It does not support the `&` prefix. A missing key is logged and inserted as the bare key name, without `$`.

Example: [ResearchItem.cs:21](../Assets/Scripts/Domain/Economy/ItemType/ResearchItem.cs#L21) calls `Localize($"$AlienTechnology ({_faction.Name})")`. `(` ends the first key. The faction name, if it is a `$Key`, is expanded as a second expression.

## Value formatting

After lookup, [`Localization.GetKey`](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L68) replaces `\n`, removes other backslashes, and calls [`ApplyParameters`](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L98).

**If the call has no parameters, `ApplyParameters` returns the value unchanged.** Then `$1`, `$OtherKey`, `${...}`, and `$[...]` all stay as literal text. The built-in English catalog has no value that uses nested keys without also using parameters, so built-in text does not hit this. A translation that adds `$1` where English has none does hit it ([L4](issues/localization.md#l4-translations-do-not-match-the-english-parameters)).

With at least one parameter, each `$` in the value is read as follows:

| Syntax | Result |
| --- | --- |
| `$1`, `$2`, … | The parameter with this 1-based number, as `ToString()`. A number outside the range inserts `[$N]`. |
| `$Name` | The raw value of key `Name` from the same dictionary. The name stops at the first character that is not a letter or digit, so `_` ends it. A missing key inserts `[$Name]`. |
| `${N\|form1\|form2…}` | A plural form chosen by parameter `N`. See [Plural forms](#plural-forms). |
| `$[a\|b\|c…]` | An enumeration chosen by parameter 1. See [Enumerations](#enumerations). |
| `$` + any other character | The `$` is removed. The character stays. |
| `$` as the last character | The whole result becomes an empty string and an exception is logged. |

Nested keys (`$Name`) have limits ([L11](issues/localization.md#l11-nested-keys-have-limits)):

1. The nested value is inserted raw. Its `\n` sequences are not replaced, and its own `$` expressions are not expanded.
2. The nested key is looked up only in the same dictionary. It does not fall back to English.
3. Key names with `_` cannot be nested.

Parameters are formatted with plain `ToString()`. The module does not format numbers by culture. Format numbers before passing them when the display needs separators or fixed decimals. Do not format a number that drives a plural form (see below).

## Plural forms

The `PluralFroms` key of each language holds space-separated rules. Each rule has the form:

```text
[min[-max]][%basis]:id      or      *:id
```

[`PluralForm.FromString`](../Assets/ModulesShared/Localization/Scripts/Localization/PluralForms.cs#L10) parses one rule. [`IsMatch`](../Assets/ModulesShared/Localization/Scripts/Localization/PluralForms.cs#L29) takes the number modulo `basis` (when `basis > 0`) and checks `min ≤ n ≤ max`. `*` matches every number.

[`AddPluralForm`](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L183) handles `${N|form1|form2|…}`:

1. It splits the text inside the braces on `|`. The first item must be the parameter number `N`. If it is not an integer, an exception is logged and nothing is inserted.
2. It converts parameter `N` with `Convert.ToInt32(string)`. A non-integer string ("1.5", "1,000") throws `FormatException`. The lookup then falls back to English or returns the raw key.
3. It checks the rules from left to right. The first match gives `id`.
4. It inserts item number `id` (1-based form), after it expands `$` parameters inside that form. So `${1|once|$1 times}` works.
5. If no rule matches, it inserts `[$0]`. A dictionary without `PluralFroms` always inserts `[$0]`.
6. If `id` is larger than the number of forms, `IndexOutOfRangeException` is thrown. `GetString` then falls back to English ([L1](issues/localization.md#l1-polish-plural-strings-have-too-few-forms)).

Current rules:

| Languages | Rule | Forms |
| --- | --- | --- |
| English, German, Spanish, French, Portuguese, Italian, Turkish, Hungarian, Korean, Chinese, Armenian, Czech | `1:1 *:2` | 1 = one, 2 = other |
| Russian, Polish | `11-14%100:3 1%10:1 2-4%10:2 *:3` | 1 = 1, 21, 31…; 2 = 2–4, 22–24…; 3 = the rest |

The Russian rule is correct for Russian. It is wrong for Polish, and Czech needs three forms ([L2](issues/localization.md#l2-polish-and-czech-plural-rules-are-wrong)).

Every `${…}` in a language must have at least as many forms as the highest `id` in that language's rules.

## Enumerations

[`AddEnumeration`](../Assets/ModulesShared/Localization/Scripts/Localization/Localization.cs#L214) handles `$[a|b|c|…]`. It splits the text on `|` and inserts item `1 + (parameter 1) mod (count − 1)`. Item 0 is never chosen, so it acts like the selector slot in a plural form, but the selector is always parameter 1.

The catalogs use this syntax only in `Characters.xml` (`FirstNameHM`, `FirstNameHF`, `LastNameH`, `FirstNameA`, `LastNameA`). Their first item is a real name, which is never chosen. No code or data references these keys ([L14](issues/localization.md#l14-enumerations-skip-the-first-item-and-are-unused)).

## Error behavior

| Situation | Result |
| --- | --- |
| Key not in the active dictionary | English value. No log. |
| Key in no dictionary (`GetString`) | `$Key` returned. `Trace.LogError` on each call. |
| Key in no dictionary (`Localize`) | `Key` (without `$`) inserted. `Trace.LogError` on each call. |
| Parameter number out of range | `[$N]` inserted. |
| Nested key missing | `[$Name]` inserted. |
| Plural selector not an integer | Exception logged. Nothing inserted. |
| Plural parameter not an integer, or too few forms | Exception. `GetString` falls back to English, then to the raw key. `Localize` returns the whole input text unchanged. |
| `null` object parameter | `NullReferenceException` in `Parameters`. Same fallback as above. |
| `$` at the end of a value (with parameters) | Empty string. Exception logged. |
| Catalog XML is invalid | That file is skipped. Error logged. |
| Duplicate key | First value kept. `Debug.Log` line. |
| Language folder missing | English is used. Error logged. |
| `PluralFroms` malformed | Exception from `Initialize`. |

## Language change in the application

Most UI does not react to `LocalizationChangedSignal`. It reads text once and the application reloads the UI scenes after a language change:

1. [`SettingsGeneral.OnLanguageChanged`](../Assets/Scripts/Gui/MainMenu/SettingsGeneral.cs#L40) stores the folder in `GameSettings.Language`, calls `Initialize`, and fires `ReloadUiSignal`.
2. [`MainMenuState.OnReloadUi`](../Assets/Scripts/GameStateMachine/States/MainMenuState.cs#L124) and [`ModalDialogState`](../Assets/Scripts/GameStateMachine/States/ModalDialogState.cs#L33) reload their scenes.
3. On scene start, [`CanvasLocalizer`](../Assets/Scripts/Gui/Utils/CanvasLocalizer.cs#L12) passes every `UnityEngine.UI.Text` under it through `GetString` and overwrites `text.text`. The original key is lost, so a running scene cannot switch language without a reload.

Settings are reachable only from the main menu, so a language change never happens during a game session.

Subscribers of `LocalizationChangedSignal`:

| Subscriber | Action |
| --- | --- |
| [LoadingScreen.cs:41](../Assets/Scripts/Legacy/GUI/LoadingScreen.cs#L41) | Sets the title and the "Loading" text. |
| [SignalsTranslator.cs:265](../Assets/Scripts/Services/Messenger/SignalsTranslator.cs#L265) | Broadcasts `EventType.ArrivedToPlanet` with the language string. This is a defect: the intended event is `EventType.LocalizationChanged` ([L8](issues/localization.md#l8-the-language-change-is-forwarded-as-arrivedtoplanet)). |
| [SystemFontLocator.cs:68](../Assets/ModulesShared/Localization/Scripts/Localization/SystemFontLocator.cs#L68) | Rebuilds fallback fonts. Not used in any scene. |

The settings dropdown ([SettingsGeneral.cs:86](../Assets/Scripts/Gui/MainMenu/SettingsGeneral.cs#L86)) loads the language list once on injection. It selects the entry whose `folder` equals `GameSettings.Language`. When the setting is empty (system language), it shows English even if another language is active ([L9](issues/localization.md#l9-the-language-dropdown-ignores-the-system-language)).

## Database and mod localization

A database or mod can supply one XML catalog per language. The database stores it by language name, case-insensitive ([Database.cs:173](../Assets/Modules/Database/Scripts/GeneratedGameCode/Database.cs#L173), [line 255](../Assets/Modules/Database/Scripts/GeneratedGameCode/Database.cs#L255)).

| Source | How a catalog enters | Key |
| --- | --- | --- |
| Folder mod | Any `*.xml` file in the mod folder or its subfolders ([FolderDatabaseStorage.cs:73](../Assets/Modules/Database/Scripts/Storage/FolderDatabaseStorage.cs#L73)). | File name without extension, for example `English`. |
| Packed mod file | Payload entry type 3 ([FileDatabaseStorage.cs:74](../Assets/Modules/Database/Scripts/Storage/FileDatabaseStorage.cs#L74)). | The string stored in the entry. |
| Default database | None. `Resources/Database` contains no XML files. | — |

One database holds at most one catalog per language. [`DatabaseContent.LoadLocalization`](../Assets/Modules/Database/Scripts/GeneratedGameCode/Storage/DatabaseContent.cs#L575) uses `Dictionary.Add`, so two `English.xml` files in one folder mod make the mod fail to load ([DB5](issues/database.md#db5-error-handling-differs-by-storage)). `Database.Loader` copies the catalogs into the database ([Database.cs:363](../Assets/Modules/Database/Scripts/GeneratedGameCode/Database.cs#L363)).

Precedence inside the Localization module:

1. Built-in `Resources/Localization/<language>/*.xml`.
2. The database catalog for `<language>`.

The first source that defines a key wins. So:

- A mod can add new keys. Its records can reference them with `$Key`.
- A mod cannot change the text of a built-in key. Its value is skipped with a `Debug.Log` line. This is a defect: the intended rule is that the mod value wins ([L15](issues/localization.md#l15-mods-cannot-override-built-in-text)).
- A mod can add a language that has no built-in folder. The language loads only through the system language, because the dropdown shows the built-in list only. The mod catalog must define `PluralFroms`.
- Mod strings for a missing language fall back to the mod's `English.xml`, then to the key.

The database also has a [`LocalizationSettings`](database/schema/LocalizationSettings.md) record. It holds two key references for corrosive damage labels. It is not part of this module.

## Input-control names: KeyNameLocalizer

[`KeyNameLocalizer.Localize(path)`](../Assets/ModulesShared/Localization/Scripts/Localization/KeyNameLocalizer.cs#L20) returns a display name for an Input System control path:

1. On first use it fills a cache: the four arrow keys map to `←`, `→`, `↑`, `↓`, and `<Keyboard>/space` maps to `GetString("$KeySpace")`.
2. A path in the cache returns the cached value.
3. Any other path returns `InputControlPath.ToHumanReadableString(path, OmitDevice)`. This text is English and comes from the Input System, not from the catalogs.

The cache is never cleared. The Space label keeps the language of its first use ([L10](issues/localization.md#l10-the-space-key-label-does-not-follow-the-language)). The language-change subscription is commented out.

Only [KeyBindingPanel.cs:160](../Assets/Scripts/Gui/MainMenu/KeyBindingPanel.cs#L160) uses it.

## Font fallback: SystemFontLocator and FontTable

The design: [`SystemFontLocator`](../Assets/ModulesShared/Localization/Scripts/Localization/SystemFontLocator.cs) indexes OS fonts by file name with `Font.GetPathsToOSFonts()`. On `LocalizationChangedSignal` it asks [`FontTable.GetFontList`](../Assets/ModulesShared/Localization/Scripts/Localization/FontTable.cs#L31) for candidates for the loaded language. It creates a `FontAsset` for each found font, caches it, and puts it into `PanelTextSettings.fallbackFontAssets`. `OnDestroy` destroys the cached assets.

`FontTable` holds one `FontData[]` per platform (Windows, Android, iOS, macOS, Linux) and a list of languages that need no extra fonts. Each entry has `Language` (empty means "any language"), `Filename` (an OS font file), and `BuiltInFont` (a project font). The intended lookup: return entries for the language; if there are none and the language is not in the "no extra fonts" list, return the entries with an empty language. [FontTable.asset](../Assets/Modules/AppConfiguration/ScriptableObjects/FontTable.asset) has entries for Chinese, Korean, Armenian, and a default font per platform.

The current state: this code does nothing.

1. `GetFontList` is commented out and yields nothing.
2. No scene or prefab contains a `SystemFontLocator`.
3. UI Toolkit panels therefore get no OS fallback fonts. Glyph coverage for Chinese, Korean, and Armenian depends on the fonts in the project.

This code was written for the planned move from uGUI to UI Toolkit. The move was suspended because of UI Toolkit limitations at the time, and the font fallback was suspended with it. The game UI uses uGUI, which does not read `PanelTextSettings`.

See [L19](issues/localization.md#l19-the-font-fallback-is-dead-code).

## Catalog statistics

Counted on 2026-10-12 from the XML files. "Missing" is the number of English keys absent from the language. "Extra" is the number of keys absent from English.

| Language | Files | Keys | Missing | Extra | Duplicate keys |
| --- | --- | --- | --- | --- | --- |
| English | 20 | 1478 | — | — | 6 |
| Russian | 20 | 1298 | 181 | 1 | 4 |
| Hungarian | 17 | 1226 | 253 | 1 | 6 |
| Chinese | 17 | 1217 | 262 | 1 | 6 |
| French | 16 | 1165 | 321 | 8 | 4 |
| Czech | 16 | 1149 | 343 | 14 | 2 |
| Armenian | 16 | 1136 | 356 | 14 | 2 |
| Polish | 16 | 1135 | 357 | 14 | 2 |
| Spanish | 16 | 1125 | 354 | 1 | 4 |
| Turkish | 17 | 1012 | 469 | 3 | 4 |
| German | 16 | 961 | 524 | 7 | 1 |
| Italian | 15 | 738 | 745 | 5 | 2 |
| Korean | 12 | 667 | 868 | 57 | 2 |
| Portuguese | 11 | 623 | 903 | 48 | 0 |

Missing keys show in English. See [L17](issues/localization.md#l17-translation-coverage-is-not-tracked).

## Adding or changing text

Change a string:

1. Add or edit the key in the closest feature file under `Resources/Localization/English/`.
2. Search the whole English folder first. The key must not exist in another file.
3. Add the key to each translated language, or accept the English fallback.
4. Keep every `$N`, nested `$Key`, `\n`, and XML escape in translations. Do not add `$N` that English does not have: the caller may pass no parameters, and then `$N` shows as text.
5. For `${N|…}`, give each language as many forms as its plural rule needs.
6. Reference the key as `$Key` from code (`GetString`), from a `Text` component under a `CanvasLocalizer`, or from a database record.

Pass parameters:

- Pass numbers for plural selectors as integers or integer strings. Do not pre-format them.
- Do not pass `null`.
- For inline `Localize` expressions, use only letters, digits, and `_` in parameters.

Add a language:

1. Create `Resources/Localization/<Folder>/` with the catalogs. Define `PluralFroms` in its `Common.xml`.
2. Add an entry to each language list that must offer it.
3. Add a flag sprite to `Resources/Textures/GUI/Flags/`.
4. If `Application.systemLanguage.ToString()` differs from the folder name, add a mapping to `GetSystemLanguage`.
5. Check that the UI fonts cover the script on every platform.

Change the module code:

- Commit in the `Assets/ModulesShared` submodule first. Then update the submodule pointer in the parent repository.
- Lookup, fallback, reload: `LocalizationManager.cs`.
- Formatting syntax: `Localization.cs`, `KeyParser.cs`, `PluralForms.cs`.
- Merge order of resources and mods: `ResourceLoader.cs`. Check mod compatibility.
- Input labels: `KeyNameLocalizer.cs`.
- Fallback fonts: `FontTable.cs`, `SystemFontLocator.cs`, `FontTable.asset`, the panel settings, and a scene that hosts the locator.

## Validation checklist

No automated tests or validation tools exist for this module.

1. Parse every catalog of every language. Report duplicate keys, with their values.
2. Compare each language with English: missing keys, extra keys, and `$N` sets per key.
3. Check every `${…}`: the selector is a valid parameter number, and the form count fits the language's rule.
4. Search for stray `$` (followed by space, `%`, or end of text) and for `{N|` or `%{` without `$`.
5. Check that every `$Key` in code, scenes, prefabs, and database JSON exists in English.
6. Test `GetString` and `Localize` with literal text, `$Key`, parameters, nested keys, and plural boundaries (0, 1, 2, 5, 11, 21, 22, 25, 111).
7. Switch the language in settings. Check the reloaded menu, the loading screen, and the key-binding panel.
8. Load a mod with an `English.xml`. Check that its new keys appear, and note that built-in keys keep their text.
9. Check Chinese, Korean, and Armenian glyphs on each target platform.
