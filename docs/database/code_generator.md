# Database Code Generator

`DatabaseCodeGenerator` turns the XML schema in `Assets/Modules/Database/.Schema/` into C# for three consumers: the Unity game, the Windows [database editor](editor.md), and the version-migration layer that both of them use. Generating every DTO, model, loader, and `IDatabase` member from one schema keeps the game, the editor, and the JSON data in agreement.

The generator lives in the `Assets/Modules/Database/.CodeGenerator/` Git submodule. For how the generated runtime code is used, see [Database Module](../database.md).

## Overview

```text
.Schema/versions.xml ──► VersionList ──► latest SchemaVersion
.Schema/v1/**/*.xml  ──► DatabaseSchema.Load (parse + validate)
                              │
     ┌────────────────────────┼─────────────────────────────┐
     ▼ game                   ▼ editor                      ▼ upgrade_game / upgrade_editor
GameCode.Builder        EditorCode.Builder          MigrationCode.Builder (every version)
     │                        │                             │
     ▼                        ▼                             ▼
Scripts/GeneratedGameCode   .Editor/…/GeneratedEditorCode   …/GeneratedMigrationCode
(namespace GameDatabase)    (namespace EditorDatabase)      (namespace DatabaseMigration)
```

Each builder renders T4 templates and writes the results through `Utils.CodeWriter`.

## Project

| Item | Value |
| --- | --- |
| Solution | `.CodeGenerator/DatabaseCodeGenerator.sln` |
| Target | .NET Framework 4.8 console app |
| Output | `DatabaseCodeGenerator/bin/Release/DatabaseCodeGenerator.exe` |
| Dependencies | NUnit 3.12, used only for `Assert` argument checks; no tests are shipped |
| Templates | `*.tt` files using `TextTemplatingFilePreprocessor` |

| Path | Purpose |
| --- | --- |
| `Program.cs` | Command-line entry point. |
| `Schema/` | XML DTOs (`XmlContent.cs`), loader and validator (`DatabaseSchema.cs`), `VersionList`, type and option constants. |
| `GameCode/` | `Builder`, naming `Utils`, and templates for Unity runtime code. |
| `EditorCode/` | `Builder`, naming `Utils`, and templates for the editor model. |
| `MigrationCode/` | `Builder`, naming `Utils`, and templates for version-specific DTOs and the upgrader. |
| `Utils/CodeWriter.cs` | Writes files only when content changed, and deletes stale `.cs` files. |
| `Schema/` (repository root), `Output/`, `generate.cmd`, `test.cmd` | A local copy of the schema and a scratch output folder for trying the generator in isolation. |

The submodule's `Schema/` copy is **not** the game's schema and can drift from it. The authoritative schema is always `Assets/Modules/Database/.Schema/`.

### Templates Are Preprocessed

Each `.tt` file has a checked-in `.cs` companion that holds a generated `TransformText()` class. The builders call those classes at run time. **Editing a `.tt` file does nothing until its `.cs` is regenerated.** Visual Studio does this when you save the template, or use **Build → Transform All T4 Templates**. Commit both files.

Some `.tt` files are include-only helpers with no `.cs` of their own, such as `WriteDeserializationCode.tt`, `WriteDataClassMember.tt`, `FuncParams.tt`, and `Serialization.tt`. Changing one of them means regenerating every template that includes it.

Constructor parameters for each template are declared in that folder's hand-written `Templates/TemplateClasses.cs`.

## Command Line

```text
DatabaseCodeGenerator.exe <versions.xml> <output dir> [game] [editor] [upgrade_game] [upgrade_editor]
```

- The first argument is the path to `versions.xml`. Schema folders are resolved relative to its directory.
- Each target writes into a fixed subfolder of `<output dir>`:

| Target | Subfolder | Schema used |
| --- | --- | --- |
| `game` | `GeneratedGameCode/` | Latest version only |
| `editor` | `GeneratedEditorCode/` | Latest version only |
| `upgrade_game` | `GeneratedMigrationCode/` | Every version in `versions.xml` |
| `upgrade_editor` | `GeneratedMigrationCode/` | Every version in `versions.xml` |

The process sets `CultureInfo.InvariantCulture`, so float literals always use `.`. Errors are printed to stdout with a stack trace. **The exit code is always 0**, so read the output: a failed run can stop partway after writing only some files.

The checked-in wrappers, run from `Assets/Modules/Database`:

| Script | Command |
| --- | --- |
| `generate_code.cmd` | `… .Schema\versions.xml Scripts game` |
| `generate_migration_code.cmd` | `… .Schema\versions.xml Scripts upgrade_game` |
| `rebuild_editor.cmd` | `… .Schema\versions.xml .Editor\DatabaseModel\DatabaseModel editor upgrade_editor`, then builds the editor solution |

Build the generator in `Release` first if `bin\Release\DatabaseCodeGenerator.exe` is missing.

### CodeWriter Behavior

- File paths come from the namespace suffix: `Serializable.Ship` becomes `Serializable/Ship.cs`.
- A file is written only when its content differs, ignoring line-ending differences. Unchanged files keep their timestamps, so Unity does not reimport them.
- New files print `New file:` and changed files print `File updated:`.
- After a builder finishes, `DeleteOldFiles` removes every `.cs` file under the target folder that was not written in this run. It skips symlinked directories. It does **not** delete Unity `.meta` files; Unity removes orphaned metas on the next import, so commit those deletions too.

Never put hand-written files inside a `Generated*` folder: the next run deletes them.

## Schema Format

### versions.xml

```xml
<schema>
    <version name="v1" major="1" minor="7" />
</schema>
```

Each `<version>` names a folder (`name`) and the latest `major.minor` that folder describes. Versions are sorted, and the last one is the current schema. There is **one schema folder per major version**. Minor versions don't get their own folders, so a minor bump means editing `v1/` in place and incrementing `minor`.

### Definition Files

Every `*.xml` file under the version folder, searched recursively, defines exactly one item. The root element is `<data>` and its `type` attribute selects the kind:

| `type` | Meaning | Generated as |
| --- | --- | --- |
| `enum` | Enumeration | C# enum |
| `object` | Top-level record with an `Id`, stored as its own JSON file | Serializable DTO and data class, plus a collection in `IDatabase` |
| `settings` | Singleton record with no `Id` | Serializable DTO and data class, plus a property in `IDatabase` |
| `struct` | Nested value with no `Id` or `typeid` | Serializable DTO and data class |
| `expression` | Typed formula signature | Expression class (game only) |

Folder names under `v1/` (`Enums/`, `Objects/Quests/`, …) are only for organization. Item names must be unique across objects, settings, and structs.

#### Enums

```xml
<data type="enum" name="ItemType">
    <item name="Undefined" value="0" />
    <item name="Component" value="1" />
    <item name="Fleet" value="20">Optional tooltip shown in the editor</item>
</data>
```

`value` is optional for ordinary enums. Inner text becomes a `[TooltipText]` attribute in the editor.

`ItemType` is mandatory and has stricter rules: every item needs a unique integer `value`. Every `object` and `settings` must name an `ItemType` member in `typeid`, and no two items may share one. The JSON `ItemType` field uses these integers, so **never renumber an existing item**.

#### Objects, Settings, and Structs

```xml
<data name="Loot" type="object" typeid="Loot" alias="LootModel">
    <member name="Loot" type="struct" typeid="LootContent" />
</data>

<data name="BulletController" type="struct" switch="Type">
    <member name="Type" type="enum" typeid="BulletControllerType" />
    <member name="Lifetime" type="float" minvalue="0" case="StickyMine" />
    <member name="X" type="expression" typeid="FloatToFloat" arguments="t" default="0" case="Parametric" />
</data>
```

`<data>` attributes:

| Attribute | Purpose |
| --- | --- |
| `name` | Schema name. Also names the DTO: `<name>Serializable`. |
| `typeid` | For `object` and `settings`: the `ItemType` member. Not allowed on `struct`. |
| `alias` | C# name for the data class when it must differ from `name`, for example `Quest` becomes `QuestModel`. |
| `switch` | Name of an enum member that selects which other members apply. See [Switch Objects](#switch-objects). Not allowed on `settings`. |
| `options` | `legacy`. See [Legacy Objects](#legacy-objects). |

`<member>` attributes:

| Attribute | Purpose |
| --- | --- |
| `name` | JSON field name. |
| `type` | One of the member types below. |
| `typeid` | Target enum, object, struct, or expression; or the image/prefab category. |
| `minvalue`, `maxvalue` | Clamp range for `int`, `float`, and numeric expressions. Without them, the range is the full type range. |
| `default` | Default value in the serializable DTO. Enum defaults use the member name; colors use `#RRGGBB` or `#AARRGGBB`. |
| `alias` | Property name in the data class when it differs from the JSON name. Lets you rename in code without migrating data. |
| `case` | For switch objects: the enum values for which the member applies. Separate values with `,` `|` `;` or whitespace. |
| `options` | `obsolete` or `notnull`. |
| `arguments` | For `expression`: names the formula's parameters as seen by content authors, for example `arguments="t"`. |
| Inner text | Editor tooltip. |

A member name may repeat only when the repeats have different `case` values. The serializable DTO gets one field per distinct name.

### Member Types

| Schema `type` | JSON / DTO field | Game data class | Editor data class |
| --- | --- | --- | --- |
| `int` | `int` | `int`, clamped with `Utilites.MathEx.Clamp` | `NumericValue<int>` with min/max |
| `float` | `float` | `float`, clamped with `Utilites.MathEx.Clamp` | `NumericValue<float>` with min/max |
| `bool` | `bool` | `bool` | `bool` |
| `char` | `string` | `char` (first character) | `char` |
| `string` | `string` | `string` | `string` |
| `color` | `string` `#AARRGGBB` | `ColorData` | `System.Drawing.Color` |
| `vector` | `Vector2` | `GameDatabase.Model.Vector2Data` (engine-free, `[Serializable]`, fields `x`/`y`) | `Vector2` |
| `vector_list` | `Vector2Data[]` | `ImmutableCollection<Vector2Data>` | `ValueWrapper<Vector2>[]` |
| `layout` | `string` | `Layout` | `Layout` |
| `image` | `string` | `SpriteId` with category `typeid` | `string` |
| `audioclip` | `string` | `AudioClipId` | `string` |
| `prefab` | `string` | `PrefabId` with category `typeid` | `string` |
| `expression` | `string` formula | Delegate plus an expression class | `string` |
| `enum` | enum | enum | enum (drop-down) |
| `enum_flags` | `enum[]` | `ImmutableSet<enum>` | `ValueWrapper<enum>[]` |
| `object` | `int` ID | Resolved model reference | `ItemId<T>` |
| `object_list` | `int[]` IDs | `ImmutableCollection<T>` | `Wrapper<T>[]` |
| `struct` | Nested DTO | Data class | `ObjectWrapper<T>` |
| `struct_list` | Nested DTO array | `ImmutableCollection<T>` | `T[]` |

`vector_list` takes no `default`; the generator rejects one. The editor model wraps each point in `ValueWrapper` so the collection editor can change it in place.

The categories for `image` and `prefab` map to hand-written enums: `SpriteId.Type` in `Assets/Modules/CommonComponents/Scripts/DataTypes/SpriteId.cs` and `PrefabId.Type` in `Scripts/Types/PrefabId.cs`. A new category must be added there first, or the generated code will not compile.

### Expressions

```xml
<data type="expression" name="IntToFloat" result="float">
    <param name="value" type="int" />
</data>
```

An expression definition declares a formula signature: a `result` type (`float` or `bool`; anything else is generated as `int`) and an ordered list of `<param>` elements, which can be numeric or `enum` with a `typeid`. For each definition, the game target generates `GameDatabase.Expressions.<Name>`, which compiles the JSON string with `CodeWriter.ExpressionParser` the first time it is evaluated. A member of type `expression` exposes a delegate such as `Damage(level)`.

Numeric results are clamped to the member's `minvalue`/`maxvalue`. Inside a formula, other numeric members of the same object can be used as variables through a generated `IVariableResolver`. Expressions that use enums also get an `EnumResolver`, so enum values can be named in formulas. The editor stores formulas as plain strings and does not evaluate them.

### Options

| Option | Scope | Effect |
| --- | --- | --- |
| `obsolete` | member | The field stays in the serializable DTO, so old JSON and migrations can still read it, but it is left out of the game and editor data classes and is not written back by the editor. |
| `notnull` | member | For `object` references only: the game loader throws `DatabaseException` when the referenced record is missing, and the editor throws when the reference is empty. |
| `legacy` | object | Uses the legacy template (below). |

## Generated Code

### Game (`GameDatabase`)

| File or folder | Contents |
| --- | --- |
| `Database.cs` | `IDatabase` (settings properties, `<Name>List`, `Get<Name>(ItemId)`, image/audio/localization lookups), the `Database` partial with ID maps, `VersionMajor`/`VersionMinor` constants, and the nested `Loader`, which builds models, skips `Disabled` records, and resolves references lazily. |
| `Storage/DatabaseContent.cs` | `IContentLoader` implementation. `LoadJson` reads `ItemType`/`Id` through `SerializableItem` and then deserializes the full DTO. The first record for an ID wins. Later duplicates throw while the constructor loads its storage and are skipped silently after it (`LoadParent`, migration export). See [Mods and merge rules](../database.md#merge-rules). |
| `Storage/Interfaces.cs` | `IJsonSerializer`, `IDataStorage`, and `IContentLoader`. |
| `Serializable/` | `SerializableItem` (`ItemType`, `Id`, `Disabled`, and a non-serialized `FileName`) and one `<Name>Serializable` per definition. |
| `DataModel/` | One immutable data class per definition, each with `Create(serializable, loader)`, `DefaultValue`, and the partial hook `OnDataDeserialized`. |
| `Enums/`, `Expressions/` | Enums and expression classes. |

Hand-written behavior belongs in matching `partial` classes under `Scripts/DataModel/`. Use the `OnDataDeserialized` hook to compute derived state.

### Editor (`EditorDatabase`)

This target has the same shape as the game, with mutable, UI-friendly models. Serializable DTOs carry `[DefaultValue]` attributes taken from the schema `default`, which lets the editor omit default-valued fields when it saves. Each data class has `Save(serializable)`, `Database` gains `Save(IDataStorage)`, `GetItem(ItemType, id)`, and `GetItemList(Type)`, and `DatabaseContent.Save` writes each DTO back to its original `FileName`. Expressions are not generated for the editor.

### Migration (`DatabaseMigration`)

| File or folder | Contents |
| --- | --- |
| `DatabaseUpgrader.cs` | Top-level dispatcher, plus a `v<major>.DatabaseUpgrader` partial per version with `UpgradeMinor()` and `partial void Migrate_<n>_<n+1>()` hooks. |
| `v<major>/Serializable/`, `Enums/`, `Storage/DatabaseContent.cs` | Version-specific DTOs that keep **every** member, including obsolete ones, and an `Export(IContentLoader)` that feeds the current loader. |

At run time, `DatabaseUpgrader.Upgrade` works like this:

1. It reads the stored version. Major version `0` is treated as `<first major>.0`.
2. It validates that the version is within the range in `versions.xml`.
3. It loads the data into the matching major's DTOs and runs each minor hook in sequence, for example `Migrate_5_6` and then `Migrate_6_7`.
4. For a later major, it exports into the next major's content, calls `MigrateFrom_v<old>()`, and continues with minor upgrades.
5. It exports the final content into the current game or editor `IContentLoader`.

Because one schema folder serves every minor version of a major, minor migrations operate on the **current** v1 DTOs. A field that a migration still reads must stay in the schema, marked `obsolete`, until no supported version needs it.

The hook bodies are hand-written and live outside the generated folders: the game's are in `Scripts/Migration/v1/DatabaseUpgrader_<a>_<b>.cs`, and the editor keeps its own copies in `.Editor/DatabaseModel/DatabaseModel/Migration/v1/`. Unimplemented `partial void` hooks compile to nothing, so a forgotten migration is silent.

The two contexts differ in small ways. `upgrade_editor` adds `[DefaultValue]` attributes, like the editor DTOs. `upgrade_game` uses `GameDatabase.Model.Vector2Data` and generates no attributes.

### Switch Objects

When `<data>` has a `switch` attribute, the game target generates:

- An `abstract` base class that holds the members without a `case` and the switch field.
- One sealed subclass per value of the switch enum, named `<Class>_<Value>`. Each holds the members whose `case` lists that value; a value with no such members still gets an empty subclass.
- `I<Name>Factory<T>` with one `Create(<Subclass>)` method per value, plus `Create<T>(factory)` on the base. This is a visitor that consumers use instead of `switch` on the type, and adding an enum value breaks every factory until it handles the new case.
- A `Create` factory that picks the subclass from the enum value. An integer that matches no enum value throws `DatabaseException`.

In the editor, a switch object becomes an `IDataAdapter` whose visible properties change with the switch value; see [Editing Records](editor.md#editing-records).

### Legacy Objects

`options="legacy"` (used by `Device`, `Weapon`, `DroneBay`, `AmmunitionObsolete`) generates a data class with a single `Stats` field of type `<Name>Stats`. That struct has public fields for **all** members, obsolete ones included. It exists for API compatibility with old game code and should not be used for new definitions. The editor ignores the option.

## Workflows

### Add a Field to an Existing Record

1. Add a `<member>` to the definition under `.Schema/v1/`, with a `default` if old data should get a non-zero value.
2. Bump `minor` in `versions.xml`, and `DatabaseVersionMinor` in `Resources/Database/Settings/Database.json`.
3. Add `Migrate_<old>_<new>` in **both** migration folders if existing data needs transformation, not just a default.
4. Run `generate_code.cmd`, `generate_migration_code.cmd`, and `rebuild_editor.cmd`.
5. Review the generated diff. Expect the new field in the DTO, the data class, the upgrader's new minor step, and `VersionMinor`.
6. Use the field from game code, via partials if it needs derived logic.

A default-valued field that doesn't change existing semantics doesn't strictly need a version bump. Bump anyway when older games must not load newer data.

### Add a Record Type

1. Add an `ItemType` value with a new, never-used integer.
2. Add the `object` or `settings` definition with that `typeid`.
3. Regenerate all three targets. `IDatabase` gains a list and a getter (or a settings property), and `DatabaseContent` gains a `LoadJson` branch.
4. Add JSON files under `Resources/Database/<Domain>/`.

### Rename or Remove a Field

- **Rename in code only:** set `alias` on the member. The JSON keeps its name.
- **Rename in data:** add the new member, mark the old one `obsolete`, and copy the values in a migration.
- **Remove:** mark it `obsolete` first. Delete it only when no supported version's migration reads it.

### Change the Generator

1. Edit the `.tt` template or the builder in the submodule.
2. Regenerate the `.cs` companion (see [Templates Are Preprocessed](#templates-are-preprocessed)) and build `Release`.
3. Run all three targets and check that the diff contains only the intended changes, in both the game and the editor.
4. Commit in the submodule, then commit the submodule pointer and the regenerated code in the parent repository.

## Validation and Errors

`DatabaseSchema.Load` stops with an `InvalidSchemaException` for any of these:

- An empty or unknown `type`, or an empty `name`.
- A missing `ItemType` enum, or an `ItemType` value that is missing, non-integer, or duplicated.
- An `object` or `settings` with an empty or unknown `typeid`, a duplicate `typeid`, or a `struct` that has a `typeid`.
- A duplicate item name, or a duplicate member name outside of `case` variants.
- A `switch` that points to a missing or non-enum member, a `case` without a `switch`, an unknown `case` value, or a `switch` on `settings`.

Templates also throw when a member's `typeid` points to an unknown enum, object, struct, or expression, or when a member `type` is unknown.

The generator does **not** check that `default` values are valid for their type, and it does not check `image`/`prefab` categories. Those mistakes show up as C# compile errors in Unity or the editor.

## Known Quirks

- The process always exits with code 0. Check the output for `Unable to load` or an exception.
- `options` are matched by substring (`string.Contains`). Object-level `legacy` uses `StringExtensions.ContainsKey`, whose word-boundary check indexes the wrong string and can throw when `options` lists several values. Every current definition uses a single option, so keep it that way or fix the helper first.
- A member with `type="enum"` and a `default` emits the default as a quoted string in the game DTO, which does not compile. Current schemas avoid this; set enum defaults through a migration or the data instead.
- Expression clamping always parses `minvalue`/`maxvalue` as integers, even for `float` results.

## Relation to Mod Encryption

The generator does not take part in mod packing or encryption. The mod container, its payload layout, and the AES-256-GCM / RSA-OAEP envelope are described in [Creating a Mod](editor.md#creating-a-mod). The only schema-dependent part of a mod is its JSON records, which use the generated DTO shapes and are upgraded through the generated `DatabaseUpgrader` when an older mod is loaded.
