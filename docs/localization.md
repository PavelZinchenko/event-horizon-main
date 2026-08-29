# Localization Module

The Localization module loads Event Horizon's XML string catalogs, resolves keys and parameters at runtime, applies language-specific plural rules, falls back to English, and publishes language changes. It also localizes selected input-control names and contains the current system-font fallback integration.

The module lives in `Assets/ModulesShared/Localization/` inside the `Assets/ModulesShared` Git submodule and compiles as the `Localization` assembly. Runtime APIs use the `Services.Localization` namespace. The assembly references AppConfiguration, CommonComponents, Database, Diagnostics, Zenject, and Unity's Input System.

## Runtime Flow

```text
AppConfig.localizationFile -> Resources/Localization/Languages*.xml
                                      |
GameSettings.Language or system language
                    |
                    v
       LocalizationManager.Initialize(language, IDatabase)
                    |
          +---------+----------+
          |                    |
Resources/Localization/   IDatabase.GetLocalization
    <language>/*.xml          (active database/mod)
          |                    |
          +---------+----------+
                    v
       active dictionary -> English fallback dictionary
                    |
        GetString / Localize / plural expansion
                    |
          LocalizationChangedSignal
```

`Assets/Scripts/Installers/ServicesInstaller.cs` binds `ILocalization` to a singleton `LocalizationManager`, binds `IKeyNameLocalizer`, and registers `LocalizationChangedSignal` plus its trigger. `InitializationState` initializes localization after the selected database or mod has loaded and before session loading begins.

## Directory Map

| Path | Purpose |
| --- | --- |
| `Scripts/Localization/ILocalization.cs` | Public lookup/initialization API and XML DTOs. |
| `Scripts/Localization/LocalizationManager.cs` | Active/default language selection, inline key parsing, fallback, and change notification. |
| `Scripts/Localization/Localization.cs` | Per-language dictionary lookup, parameters, nested keys, enumeration, and plural expansion. |
| `Scripts/Localization/ResourceLoader.cs` | Language-list and XML catalog loading from Unity Resources and `IDatabase`. |
| `Scripts/Localization/KeyParser.cs` | Parser for `$` keys and pipe-delimited parameters embedded in text. |
| `Scripts/Localization/PluralForms.cs` | Parser and matcher for per-language numeric plural rules. |
| `Scripts/Localization/KeyNameLocalizer.cs` | Input System path display names and special keyboard labels. |
| `Scripts/Localization/FontTable.cs`, `SystemFontLocator.cs` | Platform font table and UI Toolkit fallback-font integration. |
| `Resources/Localization/Languages*.xml` | Build-channel language catalogs used by `AppConfig.localizationFile`. |
| `Resources/Localization/<Language>/*.xml` | String catalogs grouped by language and feature area. |

The default `Languages.xml` currently lists English, Russian, German, Spanish, Polish, French, Portuguese, Korean, Turkish, Italian, Chinese, Armenian, Hungarian, and Czech. `Languages.Web.xml` and `Languages.yandex.xml` intentionally expose smaller subsets. The generated `AppConfig.localizationFile` selects the list name without the `.xml` extension; its current value is `Languages`.

## XML Catalogs and Load Precedence

Language lists use this shape:

```xml
<languages>
    <string name="English" iso="en" folder="English" icon="english" />
</languages>
```

Each language folder contains one or more resource files:

```xml
<resources>
    <string name="Loading">Loading...</string>
    <string name="FreeSpace">$1 ${1|cell|cells} free</string>
</resources>
```

Unity loads every `TextAsset` under `Resources/Localization/<language>`. Files are organizational only; all entries are merged into one case-sensitive dictionary. Resource loading happens before `IDatabase.GetLocalization(language)`. If a key is duplicated across resource files or database-provided XML, the first loaded value is kept and the duplicate is logged. Consequently, database/mod localization can currently add missing keys but cannot override a built-in resource key.

Do not rely on `Resources.LoadAll` file ordering. Keep each key unique across the entire language folder. XML deserialization errors are logged per resource and loading continues with the remaining files.

English is always loaded as the default dictionary. A requested non-English language is loaded separately; missing keys fall back to English. If the requested language cannot be loaded at all, lookups use English. Missing keys in both dictionaries are logged and returned as their key text so UI remains diagnosable.

## Lookup and Formatting Syntax

`GetString` distinguishes a localization key from literal text by its first character:

- `GetString("$Loading")` resolves `Loading`.
- `GetString("&Loading")` resolves `Loading` and uppercases the result.
- `GetString("Loading")` returns the literal text `Loading` without a dictionary lookup.

`Localize` scans an arbitrary string and expands embedded `$Key` expressions. Key names may contain letters, digits, and underscores. Pipe-delimited values following an embedded key become its parameters; prefixing an embedded parameter with `$` localizes that parameter before passing it to the outer key.

Inside a translated value:

- `$1`, `$2`, and later numbers insert positional parameters.
- `$OtherKey` inserts another value from the same language dictionary.
- `${1|singular|plural}` selects a plural form using parameter 1.
- `$[...]` is supported by the internal enumeration formatter and selects from its alternatives using the first parameter modulo the alternative count; there are currently no catalog examples, so prefer established syntax unless extending it with tests.

Plural selection is configured by the intentionally misspelled catalog key `PluralFroms`; changing that spelling would disable the current parser. Rules are checked from left to right. English uses `1:1 *:2`. Russian uses `11-14%100:3 1%10:1 2-4%10:2 *:3`, combining ranges and modulo bases. The selected rule ID indexes the alternatives in `${...}`; every language's string must provide enough forms for its rule set.

Parameters are converted with their normal `ToString` behavior. Format numeric or domain-specific values before passing them when culture-sensitive display is required.

## Language Changes and Integration

Passing an empty language to `Initialize` selects `Application.systemLanguage`; all Chinese system variants map to the `Chinese` folder. Explicit language values are folder names such as `English`, `Russian`, or `Portuguese`, not the localized display names from `Languages.xml`.

Initialization reloads English only on the first call unless `forceReload` is true. The selected dictionary reloads when the language changes or a forced reload is requested. This matters after changing databases: call with `forceReload: true` when localization contributed by the active database must be refreshed without changing the language name.

After initialization, `LocalizationChangedSignal` fires with the requested language string. Consumers that need the effective loaded language should read `ILocalization.Language`, which returns the active language or English fallback.

`KeyNameLocalizer` maps arrow keys to glyphs and the keyboard Space path to `$KeySpace`. Other paths use `InputControlPath.ToHumanReadableString` with the device name omitted. Its localized-key cache is not currently cleared when the language changes, so the cached Space label retains the language from its first use.

## Font Fallback Status

`SystemFontLocator` is designed to rebuild UI Toolkit `PanelTextSettings.fallbackFontAssets` after a localization change. It inventories installed OS fonts, asks `FontTable` for language-specific candidates, creates `FontAsset` instances, caches them, and destroys the cache with its GameObject.

At present, `FontTable.GetFontList` immediately yields no fonts; the platform/language selection implementation is commented out. Therefore this module does not currently add runtime fallback fonts even when `SystemFontLocator` and a populated `FontTable` are present. Re-enabling it requires platform validation and serialized font-table configuration, not only uncommenting the iterator.

## Adding or Updating Translations

For a new or changed key:

1. Add the key once under `Resources/Localization/English/` in the closest feature XML file.
2. Add the same key to supported language folders. Missing translations may temporarily fall back to English, but shipping coverage should be checked explicitly.
3. Preserve parameter numbers, nested keys, line breaks, XML escaping, and the number/order of plural alternatives across translations.
4. Search the entire target language folder for the key to avoid duplicates.
5. Exercise both direct `GetString` calls and any inline `Localize` expressions that use the key.

For a new language:

1. Create `Resources/Localization/<Folder>/` with valid XML catalogs, including `PluralFroms` in its common catalog.
2. Add a language-list entry with display name, ISO code, exact folder name, and icon ID.
3. Add it to each build-channel list that should expose it: `Languages.xml`, `Languages.Web.xml`, and/or `Languages.yandex.xml`.
4. Ensure explicit selection and `Application.systemLanguage.ToString()` both resolve to the intended folder, or add a mapping in `GetSystemLanguage`.
5. Verify fonts and glyph coverage on every supported platform.

Because `Assets/ModulesShared` is a submodule, commit translation/runtime changes in that repository and then update the parent repository's submodule pointer intentionally.

## Common Change Locations

- Add or edit translated text: `Resources/Localization/<Language>/`.
- Change available languages for a build: the selected `Languages*.xml` file and AppConfiguration generation input.
- Change lookup, fallback, or reload behavior: `LocalizationManager.cs` and `Localization.cs`.
- Change XML/database merge behavior: `ResourceLoader.cs`; verify mod compatibility before changing precedence.
- Add input-control labels: `KeyNameLocalizer.cs`, including language-change cache invalidation when needed.
- Re-enable or redesign fallback fonts: `FontTable.cs`, `SystemFontLocator.cs`, the serialized `FontTable` asset, and UI Toolkit panel settings.

## Validation Checklist

There is currently no co-located Localization test assembly, so validate parser behavior with focused tests where possible and verify rendered UI in Unity.

1. Parse every XML file for every enabled language and detect duplicate keys across files.
2. Compare enabled language key sets against English and review intentional fallback gaps.
3. Test literal text, `$Key`, `&Key`, inline `Localize`, nested keys, parameters, and every plural rule boundary.
4. Switch languages at runtime and verify `LocalizationChangedSignal`, missing-key fallback, input labels, and database reload behavior.
5. Load the default database and a mod that supplies localization; confirm the current resource-first duplicate policy is acceptable.
6. Check long strings, multiline strings, non-Latin glyphs, correct UI sizing, and font coverage on each target platform.
