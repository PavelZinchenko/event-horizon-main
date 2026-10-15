# Database Editor

The database editor is a standalone Windows tool for browsing and editing the JSON game database, upgrading old databases to the current schema, and packing a database folder into an encrypted mod file that the game can load. A companion Analyzer reports statistics and broken references.

Both tools live in the `Assets/Modules/Database/.Editor/` Git submodule. Most of their data model is generated from the same XML schema as the game; see [Database Code Generator](code_generator.md). For the runtime side of loading mods, see [Database Module](../database.md).

## Solution Layout

`.Editor/GameDatabase.sln` contains five projects. All of them target `net10.0-windows7.0`.

| Project | Path | Output | Purpose |
| --- | --- | --- | --- |
| `DatabaseEditor` | `GameDatabase/` | `DatabaseEditor.exe` (WinForms) | Main editor UI, mod builder, and mod encryption. |
| `DatabaseModel` | `DatabaseModel/` | `DatabaseModel.dll` | Editor-side data model: generated DTOs, models, migration code, plus hand-written storage and helpers. |
| `Analyzer` | `Analyzer/` | `Analyzer.exe` (WinForms) | Statistics, dead-link and ship-barrel reports. |
| `IonicZlib` | `IonicZlib/` | `IonicZlib.dll` | Vendored DotNetZip zlib implementation; the same format the game decompresses. |
| `Cyotek.Windows.Forms.ColorPicker` | `Cyotek.Windows.Forms.ColorPicker/` | DLL | Vendored color-picker control. |

NuGet dependencies are `Newtonsoft.Json` 13.0.3 (editor, model, analyzer) and `BouncyCastle.Cryptography` 2.7.0 (editor only, for mod encryption).

`.Editor/Database/` holds a sample copy of the game database, and `.Editor/Release/` holds packaging scripts. Neither is the authoritative content; the game's source of truth is `Assets/Modules/Database/Resources/Database/`.

### DatabaseModel

| Path | Kind | Contents |
| --- | --- | --- |
| `GeneratedEditorCode/` | Generated | `EditorDatabase` namespace: `Database`, `DatabaseContent`, `Enums/`, `Serializable/`, `DataModel/`, storage interfaces. |
| `GeneratedMigrationCode/` | Generated | `DatabaseMigration` namespace: the version dispatcher and `v1/` DTOs. |
| `Migration/v1/` | Hand-written | Editor copies of the minor-version migration steps. |
| `DataModel/` | Hand-written | Partial classes that extend generated models (`Barrel`, `BulletTrigger`, `Component`). |
| `Model/` | Hand-written | Value types the generated code relies on: `ItemId<T>`, `NumericValue<T>`, `Layout`, `Vector2`, `ImageData`, `AudioClipData`, wrappers. |
| `Storage/` | Hand-written | `DatabaseStorage` (folder storage, version detection) and `JsonSerializer`. |
| `Helpers/` | Hand-written | `DataAdapter`/`IProperty` reflection layer used by the UI, and `TooltipText`. |
| `Database.cs` | Hand-written | `Database.MigrateFrom(storage)`. |

Never hand-edit the two `Generated*` folders. Regenerate them with `rebuild_editor.cmd`.

## Building and Launching

Run these from `Assets/Modules/Database`:

```bat
rem Regenerate the editor model + migration code, then build the solution in Release
rebuild_editor.cmd

rem Open the built-in game database in the editor
editor.cmd

rem Open the built-in game database in the analyzer
analyzer.cmd
```

`rebuild_editor.cmd` does two things:

1. It runs `DatabaseCodeGenerator.exe .Schema\versions.xml .Editor\DatabaseModel\DatabaseModel editor upgrade_editor`, which writes `GeneratedEditorCode/` and `GeneratedMigrationCode/`.
2. It builds `.Editor\GameDatabase.sln` in `Release` using an `MSBuild.exe` path hard-coded in the script. Adjust that path for your machine. `dotnet build .Editor\GameDatabase.sln -c Release` also works if the .NET 10 SDK with Windows Desktop support is installed.

The launchers start `bin\Release\net10.0-windows7.0\DatabaseEditor.exe` and `Analyzer.exe`, passing `Resources\Database` as the database path.

To build a distributable package, run `Release\UpdateFiles.cmd`. It copies the built binaries into `Release\bin\` and mirrors `Resources\Database\*.json` into `Release\Database\`. `Release\editor.cmd` is the end-user launcher inside that package.

## Opening a Database

`DatabaseEditor.exe [path]` opens the folder given as the first argument, or the current directory if there is none. **File → Load** opens another folder.

`MainWindow.OpenDatabase` runs these steps:

1. `DatabaseStorage.LoadDatabaseVersion()` scans every `*.json` file for the `DatabaseSettings` record and reads `DatabaseVersion` and `DatabaseVersionMinor`. If there is none, it assumes 1.0.
2. The version is compared with the generated `Database.VersionMajor`/`VersionMinor` constants:
   - **Newer than the editor:** opening fails. Rebuild the editor from the current schema.
   - **Older:** the editor offers to upgrade. On confirmation, `Database.MigrateFrom` runs the generated `DatabaseUpgrader` and **immediately saves the upgraded database over the source folder**. Commit or back up before accepting.
   - **Equal:** `new Database(storage)` loads it directly.
3. The left tree mirrors the folder structure and lists `*.json` files.

`DatabaseStorage.LoadContent` loads `.json` records, `.png/.jpg/.jpeg` images keyed by file name without extension, and `.xml` localization keyed by file name without extension. WAV audio is not loaded (it is a TODO in the code), and Ogg audio is ignored.

Selecting a directory shows a summary: the file count, the item type, the first and last ID, and the **first unused ID**. Use that ID when adding a record.

## Editing Records

Selecting a JSON file deserializes its `ItemType` and `Id` and looks up the hydrated model with `Database.GetItem(type, id)`. **Edit** (or a double-click) opens one of these dialogs:

| Item type | Dialog |
| --- | --- |
| `Component` | `ComponentEditorDialog` |
| `Ship`, `Satellite` | `ShipEditorDialog`, which includes the hull layout editor |
| Everything else | `EditorDialog` |

The editor has no per-type forms. `Controls/StructDataEditor` builds the UI by reflection over the public, non-static fields of the generated model, choosing a control from the field type:

| Field type | Control |
| --- | --- |
| enum | Drop-down. Enum value tooltips come from the schema through `[TooltipText]`. |
| `NumericValue<int>` / `NumericValue<float>` | Numeric input clamped to the schema's `minvalue`/`maxvalue`. |
| `string`, `char` | Text box (a `char` field is limited to one character). |
| `bool` | Check box. |
| `Color` | Color button that opens the Cyotek picker. |
| `Layout` | `LayoutEditor` grid painter. |
| `Vector2` | `VectorEditor`. |
| `ItemId<T>` | Drop-down of all records of `T`, plus empty. |
| `IObjectWrapper` | Optional nested object (create/clear). |
| `IDataAdapter` | Nested editor. Used for structs and switch-driven objects. |
| Collections | `CollectionEditor` |

Member tooltips also come from the schema: the text content of a `<member>` element becomes a `[TooltipText]` attribute.

Objects that use a schema `switch` (for example `BulletController`, `NodeAction`, `Requirement`) implement `IDataAdapter`. Changing the switch enum raises `LayoutChangedEvent`, and the editor rebuilds the form to show only the members whose `case` matches. Because this behavior comes from the schema, a new field or a new case usually needs only regeneration, not UI work.

### Saving

- **Save** writes the whole database back to the folder it was opened from.
- **Save As** writes it to another folder.

The generated `Database.Save` serializes every loaded model back to its `*Serializable` DTO, and `DatabaseContent.Save` writes each record through `JsonSerializer`. That serializer:

- Uses Newtonsoft.Json with indented formatting and `\n` line endings.
- Omits null values and fields that equal their default (`DefaultValueHandling.Ignore`). The default is the schema `default` value, emitted as a `[DefaultValue]` attribute, or the C# default when the schema gives none.
- Orders base-class fields before derived ones.

Records keep their original relative file name. Expect whitespace and field-order diffs the first time a hand-written JSON file goes through the editor. A record whose field equals its default disappears from the JSON, and that is intentional.

## Creating a Mod

**File → Create Mod** packs the currently open folder:

1. If the folder has no `id` file, the editor asks for a mod name and writes `id` with two lines: the display name and a new GUID. The GUID is the mod's stable ID; keep the file to keep the same ID across releases.
2. You choose an output file.
3. The database is saved to the folder first, so the pack contains exactly what is on disk.
4. `ModBuilder.Build` serializes the folder, compresses it with zlib, and encrypts it with `ModEncryption.Encrypt` (see below).

Install the result in the game's `Mods/` folder. On PC this is beside the game executable; on Android it is `Android/data/<package>/files/Mods`. Then select the mod in the game settings. The game can also load an unpacked folder from `Mods/` directly, which is useful while iterating.

### Payload Format

This is the plaintext before compression. All integers are little-endian `int32`. Every string is an `int32` byte length followed by UTF-8 bytes; an empty or null string is written as a length of `0`. Every blob is an `int32` length followed by its raw bytes.

```text
int32   payload format     = 1
string  mod name           (line 1 of id)
string  mod id             (line 2 of id)
int32   database major     (DatabaseSettings.DatabaseVersion)
int32   database minor     (DatabaseSettings.DatabaseVersionMinor)

repeat for every file under the folder, recursively, in file-system order:
  byte  entry type
  ...   entry body

byte    0                  (end marker)
```

| Type | Source files | Body |
| ---: | --- | --- |
| `1` Data | `*.json` | `string` file text. No file name is stored. |
| `2` Image | `*.png`, `*.jpg`, `*.jpeg` | `string` file name **with** extension, then a blob with the image bytes. |
| `3` Localization | `*.xml` | `string` file name without extension (the language, e.g. `English`), then `string` XML text. |
| `4` WAV audio | `*.wav` | `string` file name without extension, then a blob. |
| `5` Ogg audio | `*.ogg` | `string` file name without extension, then a blob. |

All other files, including `id`, are skipped. File extensions are matched case-insensitively. The game reads entries until the end of the stream, so the trailing `0` is ignored. JSON entries that fail to parse are logged and skipped, which lets a mod built against a different schema version still load partially.

### Encryption (Format Version 1)

Implemented in `GameDatabase/ModEncryption.cs`. The matching decryptor is `Assets/ModulesPrivate/Security/Crypto/ModEncryption.cs`, which is commercial and not part of open-source builds.

This is hybrid authenticated encryption. Each mod gets a fresh random key that encrypts the content, and only that key is encrypted with RSA:

1. `SecureRandom` generates a 32-byte AES key and a 12-byte nonce.
2. The AES key is wrapped with the game's RSA public key using **RSA-OAEP with SHA-256 as the OAEP hash and SHA-256 for MGF1, with an empty label**. The key is RSA-3072, so the wrapped key is always 384 bytes.
3. The zlib-compressed payload is encrypted with **AES-256-GCM** using a 128-bit tag. The associated data (AAD) is the first 404 bytes of the file: magic, version, wrapped key, and nonce.
4. The file is written in this layout:

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | Magic `0xDADABA5E` (`uint32` LE, bytes `5E BA DA DA`) |
| 4 | 4 | Format version `1` (`int32` LE) |
| 8 | 384 | RSA-OAEP-wrapped AES key |
| 392 | 12 | GCM nonce |
| 404 | 16 | GCM authentication tag |
| 420 | n | AES-GCM ciphertext of the zlib stream |

The tag is stored **before** the ciphertext. Most libraries return `ciphertext || tag`, so you need to split the output and reorder it. Temporary key, nonce, and buffer arrays are zeroed after use.

The game rejects a file with an unknown version, a truncated envelope, a key that fails to unwrap, or a failed GCM authentication, and it does so before decompression. Files that start with the legacy magic `0xDA7ABA5E`, or that have no header at all, use the old obfuscation-only loader. The editor no longer produces either format.

#### Public Key

`GameDatabase/ModPublicKey.cs` embeds:

- `SubjectPublicKeyInfoBase64`: the DER-encoded X.509 `SubjectPublicKeyInfo` of the RSA-3072 key, in Base64.
- `Fingerprint`: the lowercase hex SHA-256 of those DER bytes. The private Security module stores the same fingerprint next to the private key, so you can confirm the two match without decoding the private key:

```bash
grep -o '"MII[^"]*"' ModPublicKey.cs | tr -d '"' | base64 -d | sha256sum
```

To rotate the key pair, replace both files together: `ModPublicKey.cs` here and `ModPrivateKey.cs` in the Security module. Mods built with the old key will then stop loading. The format has no key ID field, so supporting several keys would need a new format version.

#### What It Protects

Encryption hides mod content from casual inspection and detects corruption and tampering after a mod is built. It does **not** authenticate the author. The public key ships inside the editor, so anyone can build a valid mod with any tool. Supporting only official mods would need a signature scheme and a new format version.

#### Writing a Compatible Packer in Another Language

The format only uses standard primitives, so any mainstream crypto library can produce compatible files. In Python, use the `cryptography` package. In Java, use the JCA or Bouncy Castle. Check these points:

- **OAEP parameters.** Both the hash and MGF1 must be SHA-256. Java's `RSA/ECB/OAEPWithSHA-256AndMGF1Padding` in the SunJCE provider defaults MGF1 to SHA-1. Pass `new OAEPParameterSpec("SHA-256", "MGF1", MGF1ParameterSpec.SHA256, PSource.PSpecified.DEFAULT)` explicitly.
- **Tag position.** Write the tag before the ciphertext.
- **AAD.** Use exactly bytes 0 to 403: magic, version, wrapped key, and nonce.
- **Byte order.** Use little-endian everywhere (`struct.pack('<...')` in Python, `ByteOrder.LITTLE_ENDIAN` in Java).
- **Compression.** Use zlib (RFC 1950), not raw deflate or gzip. Python's `zlib.compress` and Java's default `Deflater` both produce it.

Minimal Python reference:

```python
import base64, os, struct, zlib
from cryptography.hazmat.primitives import hashes, serialization
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives.ciphers.aead import AESGCM

def encrypt_mod(payload: bytes, public_key_b64: str) -> bytes:
    public_key = serialization.load_der_public_key(base64.b64decode(public_key_b64))
    key, nonce = os.urandom(32), os.urandom(12)
    wrapped = public_key.encrypt(key, padding.OAEP(
        mgf=padding.MGF1(hashes.SHA256()), algorithm=hashes.SHA256(), label=None))
    header = struct.pack('<Ii', 0xDADABA5E, 1)
    sealed = AESGCM(key).encrypt(nonce, zlib.compress(payload), header + wrapped + nonce)
    return header + wrapped + nonce + sealed[-16:] + sealed[:-16]
```

To validate a new packer, load its output with the game's loader: run `Database.Tests/PackedModEncryptionTests` with the commercial Security module, or drop the file into `Mods/`. You can also compare its structure with `Assets/Modules/Database/Tests/Fixtures/AuthenticatedModV1.bytes`.

## Analyzer

`Analyzer.exe [path]` loads a database through the same `DatabaseModel` and offers these menu reports:

- **Ammunition** (`Analyzer/Statistics.cs`): a weapon table for balance passes, listing level, size, DPS, energy per second, damage per energy, range (with a homing flag), and a combined score.
- **Dead links** (`Analyzer/IntegrityChecker.cs`): walks every record by reflection, up to 50 levels deep, and reports each `ItemId<T>` that points to a missing record.
- **Ships** (`Analyzer/ShipBarrelAnalyzer.cs`): checks that each ship's `Barrels` array matches the number of barrel cells in its `Layout`, and that each barrel position falls on its own barrel cell.

**Reload** re-reads the database from disk.

## Keeping the Editor in Sync With the Game

The editor and the game compile separate code from one schema. After a schema change:

1. Run `generate_code.cmd` and `generate_migration_code.cmd` for the game, and `rebuild_editor.cmd` for the editor.
2. Mirror any new or changed migration step from `Scripts/Migration/v1/` into `.Editor/DatabaseModel/DatabaseModel/Migration/v1/`. The two sets are hand-maintained copies with different logging (`Console` instead of `GameDiagnostics`) and they are allowed to diverge.
3. Commit inside the `.Editor` submodule, then commit the updated submodule pointer in the parent repository.

Known divergence: the editor has no `Migrate_3_4`. The game's version replaces all component mods with `Resources/DatabaseMigration/v1.3`, but upgrading a 1.3 database in the editor leaves component mods unchanged. Because minor migration hooks are `partial void`, a missing step compiles silently. Compare the two folders when you review a migration.

## Troubleshooting

| Symptom | Cause |
| --- | --- |
| "Database version in not supported" | The database is newer than the generated editor model. Run `rebuild_editor.cmd`. |
| Upgrade prompt on a database you expected to be current | `Database.json` has an older `DatabaseVersionMinor`, or a second `DatabaseSettings` record was found first. |
| Selecting a file shows nothing | The JSON failed to parse or has `ItemType` 0. The exception is swallowed, so open the file manually. |
| A field is missing from the form | It is marked `obsolete` in the schema, or it belongs to a different `case` of a switch object. |
| The game says the mod could not be decrypted | The editor and game keys differ (compare fingerprints), the file was modified, or the build uses the open-source Security stub. |
| A mod loads but some records are missing | Those JSON entries failed to parse in the game, usually because of a schema mismatch. Check the Unity log. |
