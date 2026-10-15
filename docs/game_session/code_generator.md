# GameSession Code Generator

The GameSession code generator turns the XML save schema in `Assets/Modules/GameSession/.Schema/` into C#: the current save model, read-only models of older versions, and the `SessionLoader` that reads any supported version and converts it to the current one. One schema defines the field order on disk, the change-tracking code, and the conversion code, so they cannot disagree.

The generator is a separate program in the `Assets/Modules/GameSession/.CodeGenerator/` Git submodule ([event-horizon-session-code-generator](https://github.com/PavelZinchenko/event-horizon-session-code-generator)). It is not the [Database code generator](../database/code_generator.md), although the two have a similar structure. For the schema format, see [Save schema](schema/index.md). For the bytes that the generated code writes, see [Binary format](serialization.md).

## Contents

- [Overview](#overview)
- [Project](#project)
- [Running the generator](#running-the-generator)
- [Schema loading and validation](#schema-loading-and-validation)
- [Version tracking](#version-tracking)
- [Templates](#templates)
- [Generated code](#generated-code)
- [The generated SessionLoader](#the-generated-sessionloader)
- [Output handling](#output-handling)
- [Changing the generator](#changing-the-generator)

## Overview

```text
.Schema/versions.xml ─► VersionList (sorted by major, minor)
.Schema/v1/**/*.xml  ─┐
.Schema/v2/**/*.xml  ─┼─► DataSchema.Load (per version) ─► BuilderContext.ApplySchema (cumulative)
.Schema/v3/**/*.xml  ─┘                                          │
                                       ┌─────────────────────────┼────────────────────────┐
                                       ▼                         ▼                        ▼
                               ObjectTemplate             StructTemplate         SessionLoaderTemplate
                           (class per changed object)  (struct per changed type)   (one file)
                                       │                         │                        │
                                       ▼                         ▼                        ▼
                         Scripts/GeneratedCode/Model/  (types whose latest version)   SessionLoader.cs
                         Scripts/GeneratedCode/v1/, v2/ (older type versions)
```

## Project

| Item | Value |
| --- | --- |
| Solution | [`.CodeGenerator/CodeGenerator.sln`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator.sln) |
| Target | .NET Framework 4.8 console application |
| Output | `CodeGenerator/bin/Release/CodeGenerator.exe`. Not tracked in Git ([GS5.12](../issues/game_session.md#gs5-smaller-defects)). |
| Dependencies | NUnit 3.12, used only for `Assert` argument checks in `CodeWriter`. No tests are shipped. |
| Templates | `*.tt` files with the `TextTemplatingFilePreprocessor` custom tool. |

| Path (under `.CodeGenerator/CodeGenerator/`) | Purpose |
| --- | --- |
| [`Program.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Program.cs) | Entry point. |
| [`Schema/XmlContent.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/XmlContent.cs) | XML DTOs: `XmlVersionList`, `XmlTypeInfo`, `XmlClassItem`, `XmlClassMember`. |
| [`Schema/VersionList.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/VersionList.cs) | Reads `versions.xml`. Defines `SchemaVersion`. |
| [`Schema/DataSchema.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs) | Loads and validates one version folder. |
| [`Schema/Constants.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/Constants.cs) | Type names, options, encodings, the `timestamp` default. |
| [`Schema/Helpers.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/Helpers.cs) | Structural equality of types and members; `IsCollectionType`. |
| [`GameCode/Builder.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Builder.cs) | Drives generation. |
| [`GameCode/BuilderContext.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/BuilderContext.cs) | Tracks which version of each type is current. Chooses namespaces. |
| [`GameCode/Utils.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Utils.cs) | Naming rules and the C# names of runtime types. |
| [`GameCode/Templates/`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/) | T4 templates and their preprocessed `.cs` classes. |
| [`Utils/CodeWriter.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Utils/CodeWriter.cs) | Deletes and writes the output folder. |
| `Schema/` (submodule root), `Output/` | An old local copy of the v1–v2 schema and a scratch output folder. **Not** the game's schema; it differs from `.Schema/`. |

## Running the generator

From `Assets/Modules/GameSession/`, run [`generate_code.cmd`](../../Assets/Modules/GameSession/generate_code.cmd):

```bat
.CodeGenerator\CodeGenerator\bin\Release\CodeGenerator.exe .Schema\versions.xml Scripts
```

| Argument | Meaning |
| --- | --- |
| 1 | Path of `versions.xml`. Version folders are resolved relative to its directory. |
| 2 | Output root. The generator writes into `<root>/GeneratedCode/`. |

The paths are relative, so the working directory must be `Assets/Modules/GameSession`. If the executable is missing, build `CodeGenerator.sln` in Release first.

`Program.Main` ([Program.cs:9](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Program.cs#L9)):

1. Sets the invariant culture.
2. Loads `VersionList` and takes the latest version. With no versions it prints a message and stops.
3. Loads the latest schema once. This validates it before anything is deleted. The result is not used further.
4. Runs `Builder.Build`.
5. Catches every exception, prints it, and exits with code 0 ([GS5.8](../issues/game_session.md#gs5-smaller-defects)). Check the console output; the exit code says nothing.

To try a schema change without touching the project, pass a scratch folder as the second argument and diff its `GeneratedCode/` with `Scripts/GeneratedCode/`. On 2026-10-12 this produced output identical to the checked-in code.

## Schema loading and validation

`VersionList` ([VersionList.cs:64](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/VersionList.cs#L64)) reads each `<version name major minor>` entry, skips entries whose numbers do not parse, and sorts by major, then minor. `name` is the folder name.

`DataSchema.Load` ([DataSchema.cs:16](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs#L16)) reads every `*.xml` file under the version folder, recursively. The folder layout (`Objects/`, `Types/`) has no meaning to the generator. Each file holds one `<data>` element. Its `type` attribute decides the kind:

| `type` | Kind |
| --- | --- |
| `object` | Class with a parent link and change tracking. |
| `struct` | Immutable `readonly partial struct`. |
| anything else | `InvalidSchemaException`. |

Checks that run:

| Check | Where |
| --- | --- |
| Type has a name and a kind. | [DataSchema.cs:55](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs#L55) |
| Member has a name and a type. | [line 99](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs#L99) |
| Member name differs from the type name (C# rule). | [line 103](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs#L103) |
| Member names are unique within the type. | [line 105](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs#L105) |
| A `map` has a `key`. | [line 107](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs#L107) |
| Type names are unique across objects and structs. | [line 115](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/DataSchema.cs#L115) |

Checks that do not run: a collection without `value`, and an empty folder ([GS5.7](../issues/game_session.md#gs5-smaller-defects)). Unknown member types, unsupported `encrypted` targets, and invalid encodings are found later, during template rendering.

## Version tracking

`BuilderContext` decides, for every version, which types changed and therefore need their own generated class.

`ApplySchema` ([BuilderContext.cs:116](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/BuilderContext.cs#L116)) runs once per version, in order:

1. Start from a copy of the previous version's type map. Each entry is `(schema, version in which this schema appeared)`.
2. Remove types that the new version does not define ([GS5.10](../issues/game_session.md#gs5-smaller-defects)).
3. For each type in the new version, compare it with the previous one by `Helpers.AreEqual`: same name, kind, member count, and per member the same name, type, key, value, options, encoding, and default, in the same order. A different type gets the new version.
4. `UpdateObjectsVersion` marks a type as changed when any member's type, key, or value type changed in this version. This repeats recursively, so a change in a struct also changes every object and struct that contains it.

Example from the current schema: v2 changed `PurchaseInfo.Time` from `long` to `int`. Step 4 then marked `PurchasesMap` (contains `PurchaseInfo`), `ShopData` (contains `PurchasesMap`), and `SaveGameData` (contains `ShopData`) as changed in v2.

**Namespace rule** ([BuilderContext.cs:53](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/BuilderContext.cs#L53)). A type version that is also the latest version goes into `Session.Model`. An older type version goes into `Session.v{major}`. Each type version is generated once, by the version in which it appeared.

| Namespace | Types (current output) |
| --- | --- |
| `Session.Model` | All 33 current types: the root, 20 objects, and 12 structs. |
| `Session.v1` | `SaveGameData`, `BossData`, `BossInfo`, `EventData`, `Inventory`, `PurchaseInfo`, `PurchasesMap`, `RegionData`, `ShopData`, `StarMapData` |
| `Session.v2` | `SaveGameData` |

Generated classes keep `VersionMajor` and `VersionMinor` constants with the version in which the type last changed. For example, `Model.GameData.VersionMajor` is 1 and `Model.SaveGameData.VersionMajor` is 3. Only the root's constants matter at run time: the serializer writes them into the envelope.

The version history is keyed by major number, so every version needs a new major ([GS5.9](../issues/game_session.md#gs5-smaller-defects)).

## Templates

Each `.tt` file with a `.cs` companion is a preprocessed template: the `.cs` holds a `TransformText()` class that the builder calls. **Editing a `.tt` file has no effect until its `.cs` is regenerated** (save it in Visual Studio, or **Build → Transform All T4 Templates**). Commit both files. Constructors and properties of the template classes are in [`TemplateClasses.cs`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/TemplateClasses.cs).

| Template | Kind | Emits |
| --- | --- | --- |
| [`ObjectTemplate.tt`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/ObjectTemplate.tt) | Preprocessed | One class per changed `object`. |
| [`StructTemplate.tt`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/StructTemplate.tt) | Preprocessed | One `readonly partial struct` per changed `struct`. |
| [`SessionLoaderTemplate.tt`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/SessionLoaderTemplate.tt) | Preprocessed | `SessionLoader.cs`. |
| [`UtilsClassMember.tt`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsClassMember.tt) | Include | Type mapping, fields, properties, initializers, defaults. |
| [`UtilsSerializationCode.tt`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsSerializationCode.tt) | Include | `Serialize` bodies. |
| [`UtilsDeserializationCode.tt`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsDeserializationCode.tt) | Include | Reading constructors. |
| [`UtilsComparisonCode.tt`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsComparisonCode.tt) | Include | Struct `Equals` and comparers. |
| [`UtilsUpgradeCode.tt`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsUpgradeCode.tt) | Include | Member copies between versions and the "manual work" comments. |
| [`GeneratedCodeWarning.tt`](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/GeneratedCodeWarning.tt) | Include | File header. |

Changing an include-only template means regenerating every preprocessed template that includes it.

## Generated code

### Type mapping

| Schema | C# field | Public surface |
| --- | --- | --- |
| `int`, `uint`, `short`, `ushort`, `sbyte`, `byte`, `long`, `ulong`, `bool`, `string` | same type | property |
| `single` | `float` | property |
| `int` / `long` with `encrypted` | `ObscuredInt` / `ObscuredLong` | `int` / `long` property |
| `list` | `ObservableList<T>` | read-only property |
| `set` | `ObservableSet<T>` | read-only property |
| `map` | `ObservableMap<K, V>` | read-only property |
| `inventory` | `ObservableInventory<T>` (values are `ObscuredInt`) | read-only property |
| `bitset` | `ObservableBitset` | read-only property |
| object name | generated class | property with setter |
| struct name | generated struct | read-only property in a struct. In an object the setter does not compile ([GS5.13](../issues/game_session.md#gs5-smaller-defects)); no current object has a direct struct member. |

The rules are in `ConvertType`, `WriteClassMember`, and `WriteProperty` ([UtilsClassMember.tt:4](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsClassMember.tt#L4)).

### Objects

Example: [GameData.cs](../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/GameData.cs).

- `class X : IDataChangedCallback` with a private `_parent` and an `internal Parent` property.
- `X(IDataChangedCallback parent)`: a new instance. Scalars get the schema default; collections and child objects are created with `this` as their callback.
- `X(SessionDataReader reader, IDataChangedCallback parent)`: reads members in schema order. The parent is assigned after reading, so reading does not raise change notifications.
- Scalar setters return early on an equal value, assign, set `child.Parent = this` for object members, and call `OnDataChanged()` unless the member has `notimportant`.
- `Serialize(SessionDataWriter)`: only on the latest version of the type. Older versions are read-only.
- `OnDataChanged()`: sets `DataChanged` and forwards to the parent.

### Structs

Example: [ShipInfo.cs](../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/ShipInfo.cs).

- `readonly partial struct X`. All fields are `readonly`, and all properties are get-only.
- `X(IDataChangedCallback parent)` and `X(SessionDataReader reader, IDataChangedCallback parent)`. A struct has no parent field. It passes `parent` to its collections, so a mutation of a collection inside a struct notifies the object that holds the struct.
- On the latest version only: `Serialize`, `Equals(X other)`, and a nested `EqualityComparer` struct. `Equals` compares collections element by element.
- Constructors with other parameters are hand-written in the matching partial file in [`Scripts/Model/`](../../Assets/Modules/GameSession/Scripts/Model/).

A copy of a struct shares the collection instances of the original. Replacing a struct value in a list or map is how a struct is "changed".

### Defaults

`default` sets the initial value for a new instance ([UtilsClassMember.tt:156](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsClassMember.tt#L156)):

| `default` | Generated initializer |
| --- | --- |
| empty, string member | `string.Empty` |
| empty, other member | `default(T)` |
| `timestamp` | `(T)System.DateTime.UtcNow.Ticks` |
| any other text | inserted literally, so it must be a valid C# expression |

## The generated SessionLoader

[SessionLoaderTemplate.tt](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/SessionLoaderTemplate.tt) emits one `partial class SessionLoader` with:

| Member | Purpose |
| --- | --- |
| `partial void Upgrage_vA_0_to_vB_0(old, new)` | Hook per version step. Its XML comment lists what the generated copy could not transfer. |
| `Convert(v1.SaveGameData)` | Runs every conversion from v1. Used by the legacy loader. |
| `Load(reader, major, minor)` | Reads the stored major and converts it to the latest. |
| `Convert_A_B(old)` | Creates a new root of version B and copies what it can. |

The copy rules for each member of the root ([UtilsUpgradeCode.tt:4](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/GameCode/Templates/UtilsUpgradeCode.tt#L4)):

| Member state | Generated code | Comment in the hook summary |
| --- | --- | --- |
| Removed in B | nothing | `property was removed - …` |
| Added in B | nothing; the new member keeps its default | `property was added - …` |
| Schema attributes differ | nothing | `type was changed - …` |
| Same, and the referenced type has the same version | `new.X = old.X` (moves an object) or `new.X.Assign(old.X)` (collections) | none |
| Same, referenced object type changed | recurse into the object's members | from the recursion |
| Same, referenced struct or collection value changed | nothing | `copy … to …` |

Every comment is work that the hand-written hook must do. The hooks live in [`Scripts/Migration/`](../../Assets/Modules/GameSession/Scripts/Migration/). See [Version conversions](serialization.md#version-conversions) for what the current hooks do.

`Load` does not validate the version ([GS2](../issues/game_session.md#gs2-unknown-schema-versions-are-not-rejected)).

## Output handling

`CodeWriter` ([CodeWriter.cs](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Utils/CodeWriter.cs)):

1. `DeleteGeneratedFiles` deletes `<root>/GeneratedCode/` recursively, including Unity `.meta` files ([GS5.11](../issues/game_session.md#gs5-smaller-defects)).
2. `Write(namespace, name, text)` maps the namespace suffix to a subfolder (`Model`, `v1`, `v2`, or the root for `SessionLoader`) and writes `<name>.cs`.

The delete runs before the per-version schemas load in `Builder.Build`, so an error in an older version folder leaves the output folder empty.

## Changing the generator

1. Edit the generator in the submodule. Regenerate every affected `.tt` companion.
2. Build `CodeGenerator.sln` in Release.
3. Run it into a scratch folder and diff the output with `Scripts/GeneratedCode/`. Every change in the diff must be intended.
4. Run `generate_code.cmd`, then compile in Unity and run `GameSession.Tests`.
5. Commit the submodule, then commit the new submodule pointer and the regenerated code in the main repository together.
