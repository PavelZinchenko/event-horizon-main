# GameSession Binary Format

This page describes the bytes that GameSession writes and reads: the envelope, the bit-level value coding, how each schema type is laid out, how old versions are upgraded, and how the headerless legacy format is recognized. For the module overview, see [GameSession module](../game_session.md). For the code that produces the readers and writers, see [GameSession code generator](code_generator.md).

## Contents

- [Layers](#layers)
- [Envelope](#envelope)
- [Value coding](#value-coding)
- [Layout of schema types](#layout-of-schema-types)
- [Compatibility rules](#compatibility-rules)
- [Load path](#load-path)
- [Version conversions](#version-conversions)
- [Headerless legacy format](#headerless-legacy-format)
- [Failure behavior](#failure-behavior)

## Layers

```text
SessionData.Serialize / TryDeserialize                    Scripts/Facade/SessionData.cs
  └─ SessionSerializer      envelope: header, format, version, EOF marker
       └─ SessionLoader     version dispatch and conversion (generated + partial)
            └─ Model.SaveGameData.Serialize / ctor(reader)  generated, schema order
                 └─ SessionDataWriter / SessionDataReader   per-value encoding switch
                      ├─ EliasGammaEncoder / Decoder        bit-packed variable-length integers
                      └─ IWriterStream / IReaderStream      little-endian fixed-size values
```

| Layer | File |
| --- | --- |
| Envelope | [SessionSerializer.cs](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs) |
| Version dispatch | [SessionLoader.cs](../../Assets/Modules/GameSession/Scripts/GeneratedCode/SessionLoader.cs) (generated), [SessionLoader_v1.cs](../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs) (hand-written) |
| Model | [Scripts/GeneratedCode/Model/](../../Assets/Modules/GameSession/Scripts/GeneratedCode/Model/) |
| Encoding switch | [SessionDataWriter.cs](../../Assets/ModulesShared/Utilites/Serialization/SessionDataWriter.cs), [SessionDataReader.cs](../../Assets/ModulesShared/Utilites/Serialization/SessionDataReader.cs) |
| Variable-length coding | [EliasGammaEncoder.cs](../../Assets/ModulesShared/Utilites/Serialization/EliasGammaEncoder.cs), [EliasGammaDecoder.cs](../../Assets/ModulesShared/Utilites/Serialization/EliasGammaDecoder.cs) |
| Fixed-size values | [IWriterStream.cs](../../Assets/ModulesShared/Utilites/Serialization/IWriterStream.cs), [IReaderStream.cs](../../Assets/ModulesShared/Utilites/Serialization/IReaderStream.cs), [WriterStream.cs](../../Assets/ModulesShared/Utilites/Serialization/WriterStream.cs), [ReaderStream.cs](../../Assets/ModulesShared/Utilites/Serialization/ReaderStream.cs) |
| Collections | [ObservableList.cs](../../Assets/ModulesShared/Utilites/Serialization/ObservableList.cs), [ObservableMap.cs](../../Assets/ModulesShared/Utilites/Serialization/ObservableMap.cs), [ObservableSet.cs](../../Assets/ModulesShared/Utilites/Serialization/ObservableSet.cs), [ObservableInventory.cs](../../Assets/ModulesShared/Utilites/Serialization/ObservableInventory.cs), [ObservableBitset.cs](../../Assets/ModulesShared/Utilites/Serialization/ObservableBitset.cs) |

The helpers in `Assets/ModulesShared/Utilites/Serialization/` live in the `ModulesShared` Git submodule and compile into the `Utilites` assembly. They use the `Session.Utils` namespace.

## Envelope

`SessionSerializer.Serialize` writes this envelope into a `MemoryStream` ([SessionSerializer.cs:41](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L41)):

| Offset | Size | Field | Value |
| --- | --- | --- | --- |
| 0 | 4 | Header | `0x5AFE5AFE` (`uint`). Bytes `FE 5A FE 5A`. |
| 4 | 4 | Serializer format | `1` (`int`). Readers accept `1` only ([line 11](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L11)). |
| 8 | 4 | Schema version | `(major << 16) + minor` (`int`). The current model writes `3.0`, so the value is `0x00030000`. |
| 12 | n | Payload | `SaveGameData.Serialize` through a `SessionDataWriter`. |
| 12 + n | 4 | EOF marker | `0xDEADC0DE` (`uint`). |

All fixed-size values use `BitConverter`, which is little-endian on every platform the game supports. The serializer does not compress or encrypt. Storage backends wrap the bytes; for example, `PlayerPrefsStorage` compresses them with zlib ([PlayerPrefsStorage.cs:37](../../Assets/Modules/Services/Scripts/Storage/PlayerPrefsStorage.cs#L37)). See [Storage](storage.md).

The serializer format and the schema version are separate. The format number covers the envelope. The schema version selects the payload layout. Neither is the `DataVersion` that storage uses to compare saves; see [Change tracking](../game_session.md#change-tracking-and-dataversion).

## Value coding

`SessionDataWriter` and `SessionDataReader` take an `EncodingType` for every value ([SessionDataWriter.cs:5](../../Assets/ModulesShared/Utilites/Serialization/SessionDataWriter.cs#L5)):

| Encoding | Bytes |
| --- | --- |
| `Plain` | Fixed-size little-endian value through `IWriterStream`. |
| `EliasGamma` | Bit-packed variable-length integer through `EliasGammaEncoder`. |

The schema can set `encoding="plain"` or `encoding="elias"` per member. The default is `elias` ([Constants.cs:31](../../Assets/Modules/GameSession/.CodeGenerator/CodeGenerator/Schema/Constants.cs#L31)). **No v1–v3 member sets the attribute, so every generated call uses `EliasGamma`.**

`float` and `string` ignore the argument and always use `Plain` ([SessionDataWriter.cs:113](../../Assets/ModulesShared/Utilites/Serialization/SessionDataWriter.cs#L113), [line 123](../../Assets/ModulesShared/Utilites/Serialization/SessionDataWriter.cs#L123)).

### The variable-length integer code

The coder is called Elias gamma, but it is a nibble-based code. A value is cut into 4-bit sections ([EliasGammaEncoder.cs:46](../../Assets/ModulesShared/Utilites/Serialization/EliasGammaEncoder.cs#L46)):

1. Write `s` one-bits and one zero-bit. `s` is the number of 4-bit sections that the value needs. Zero needs no section, so `0` is a single zero-bit.
2. For a signed value with `s > 0`, write one sign bit: `1` means negative ([line 33](../../Assets/ModulesShared/Utilites/Serialization/EliasGammaEncoder.cs#L33)).
3. Write the absolute value in `4 × s` bits, least significant bit first.

Bits fill each byte from bit 0 upward. A byte is written when it has 8 bits ([line 73](../../Assets/ModulesShared/Utilites/Serialization/EliasGammaEncoder.cs#L73)).

| Value | Unsigned bits | Signed bits |
| --- | --- | --- |
| 0 | 1 | 1 |
| 1…15 | 6 | 7 |
| 16…255 | 11 | 12 |
| 256…4 095 | 16 | 17 |
| up to 2³² − 1 | 41 | 42 |
| up to 2⁶⁴ − 1 | 81 | 82 |

`bool` in this encoding is one bit. `long.MinValue` is written as magnitude 2⁶³ and reads back correctly.

### Switching between encodings

Consecutive `EliasGamma` values share bytes. When the writer switches to `Plain`, it flushes the partial byte, padded with zero-bits ([SessionDataWriter.cs:133](../../Assets/ModulesShared/Utilites/Serialization/SessionDataWriter.cs#L133)). When the reader switches back to `EliasGamma`, it drops the rest of the partial byte ([SessionDataReader.cs:118](../../Assets/ModulesShared/Utilites/Serialization/SessionDataReader.cs#L118)). Both sides start in `Plain`. `SessionDataWriter.Dispose` flushes the last partial byte, so the EOF marker is byte-aligned.

Because every string and float forces a flush, a string costs up to 7 padding bits plus its length prefix.

## Layout of schema types

The generated `Serialize` method writes members in schema order. The reading constructor reads them in the same order. Nothing in the payload names a member or a type: the layout is defined only by the schema version in the envelope.

| Schema type | Layout (all integers `EliasGamma` unless noted) |
| --- | --- |
| `int`, `short`, `sbyte`, `long` | Signed code. |
| `uint`, `ushort`, `byte`, `ulong` | Unsigned code. |
| `bool` | One bit. |
| `single` | 4-byte IEEE float, `Plain`. |
| `string` | `Plain` `int` byte length, then UTF-8 bytes ([IWriterStream.cs:67](../../Assets/ModulesShared/Utilites/Serialization/IWriterStream.cs#L67)). `null` and `""` both write length 0 and read back as `""`. |
| `list` | Signed count, then each element. |
| `set` | Signed count, then each element in `HashSet` order. |
| `map` | Signed count, then key and value pairs in `Dictionary` order. |
| `inventory` | Signed count, then item and signed quantity pairs. |
| `bitset` | Signed number of change points, then each change point as `delta − 1`, unsigned ([ObservableBitset.cs:187](../../Assets/ModulesShared/Utilites/Serialization/ObservableBitset.cs#L187)). |
| struct | Its members, inline. No count, no marker. |
| object | Its members, inline. No count, no marker. |

Sets and maps are written in enumeration order. The same content can therefore produce different bytes after removals. Do not compare saves byte by byte.

### Bitset

`ObservableBitset` stores a sorted list of change points instead of bits ([ObservableBitset.cs:8](../../Assets/ModulesShared/Utilites/Serialization/ObservableBitset.cs#L8)). Each run of set bits is a pair `[start, end)`. When the list has an odd length, the first run starts at 0 and the first `start` is omitted. Example: bits `{1, 2, 5, 6, 7}` are stored as `[1, 3, 5, 8]` and written as count 4, then deltas `0, 1, 1, 2`. Star maps with long runs of explored stars compress well this way.

### Options that do not change bytes

| Option | Effect |
| --- | --- |
| `encrypted` | The in-memory field is `ObscuredInt` or `ObscuredLong`. They XOR the value with a random per-process mask ([ObscuredInt.cs:98](../../Assets/ModulesShared/Utilites/Security/ObscuredInt.cs#L98)) to resist memory scanners. The save holds the plain value. See [GS5.6](../issues/game_session.md#gs5-smaller-defects) for collections. |
| `notimportant` | The setter does not call `OnDataChanged`. The value is still saved, but a change alone does not make the session dirty. |
| `default` | Initial value for a new game only. `timestamp` means `DateTime.UtcNow.Ticks` cast to the member type. |

## Compatibility rules

These rules follow from the layout. They apply to every released schema version.

1. Adding, removing, or reordering a member changes the layout. It needs a new major version. See [Versioning](../game_session.md#versioning-and-migration-workflow).
2. Widening an integer type with the same signedness is compatible in `EliasGamma`, because the code does not store a width. `sbyte` → `int` and `byte` → `uint` read old bytes correctly. The released v1 and v2 snapshots were edited this way: `ShipComponentInfo.Modification` was `sbyte` and `InventoryComponentInfo.Modification` was `byte` (compare the generator's own copy in `.CodeGenerator/Schema/`).
3. Narrowing an integer type truncates through a C# cast without an error.
4. Changing signedness changes the code. It is not compatible.
5. In `Plain` encoding, any change of width is incompatible.
6. Changing `float` or `string` to or from an integer type is incompatible.
7. Options and defaults do not change the layout.

## Load path

`SessionSerializer.TryDeserialize` ([SessionSerializer.cs:21](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L21)):

1. Wrap the buffer at `startIndex` in a `MemoryReaderStream`.
2. Read 4 bytes. If they are not the header, hand the whole buffer to `LegacyDataLoader` ([line 28](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L28)). See [Headerless legacy format](#headerless-legacy-format).
3. Read the format. Reject anything other than 1 ([line 65](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L65)).
4. Read the schema version and call `SessionLoader.Load(reader, major, minor)` ([line 76](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L76)).
5. Read 4 more bytes. If they are not the EOF marker, log an error and return `false` ([line 78](../../Assets/Modules/GameSession/Scripts/Facade/SessionSerializer.cs#L78)).

`SessionLoader.Load` ([SessionLoader.cs:43](../../Assets/Modules/GameSession/Scripts/GeneratedCode/SessionLoader.cs#L43)) reads the model for the stored major and then converts it step by step to the current model:

| Stored major | Read as | Conversions |
| --- | --- | --- |
| 1 | `v1.SaveGameData` | `Convert_1_2`, then `Convert_2_3` |
| 2 | `v2.SaveGameData` | `Convert_2_3` |
| 3 | `Model.SaveGameData` | none |
| other | nothing | returns `null`. See [GS2](../issues/game_session.md#gs2-unknown-schema-versions-are-not-rejected). |

The `v1` and `v2` namespaces contain only the types that changed in a later version. A type that never changed, such as `GameData`, exists once in `Session.Model` and is read by every version.

## Version conversions

Each generated `Convert_X_Y` creates an empty new root, moves unchanged members, and then calls a partial hook `Upgrage_vX_0_to_vY_0` (the misspelling is in the generator). Unchanged objects are moved by reference: the setter re-parents them to the new root. Changed objects with unchanged collections are copied with `Assign`.

### v1.0 → v2.0

Generated part ([SessionLoader.cs:73](../../Assets/Modules/GameSession/Scripts/GeneratedCode/SessionLoader.cs#L73)): moves `Game`, `Achievements`, `Common`, `Fleet`, `Iap`, `Pvp`, `Quests`, `Research`, `Resources`, `Social`, `Statistics`, `Upgrades`, `Wormholes`. Copies `Inventory.Satellites`, `Regions.Factions`, and the unchanged `StarMap` members.

Hand-written part ([SessionLoader_v1.cs:7](../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs#L7)):

| v1 data | v2 data | Rule |
| --- | --- | --- |
| `Bosses[id].LastDefeatTime` (UTC ticks) | `Bosses[id].LastDefeatTime` (`int`) | Whole hours since `Game.StartTime`, minimum 0 ([line 11](../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs#L11)). |
| `Regions.DefeatedFleetCount[region]` | `Regions.CapturedBases`, `Regions.MilitaryPower` | A negative count marks the base captured. Otherwise power = `max(50, min(300 + 5 × distance, 1000) − 10 × count)`, where `distance` is the home star's distance from the center for the game seed ([line 17](../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs#L17)). |
| `Events.CompletedTime[star]` (ticks) | `Events.CompletedTime[star]` (`int`) | Hours since game start ([line 33](../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs#L33)). |
| `StarMap.StarData[star]` (0–3) | `DiscoveredStars`, `SecuredStars`, `EnemiesOnStars` | Every key is discovered. 1 and 2 are secured. 2 and 3 have enemies ([line 39](../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs#L39)). This matches the v2 `Occupant` values. |
| `Shop.Purchases[star][item].Time` (ticks) | `PurchaseInfo.Time` (`int`) | Hours since game start ([line 49](../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs#L49)). |
| `Inventory.Components` keyed by a packed `long` | keyed by `InventoryComponentInfo` | Bits 8–15 level, 16–23 modification, 24–31 quality, 32+ component ID. Bits 0–7 are dropped ([line 63](../../Assets/Modules/GameSession/Scripts/Migration/SessionLoader_v1.cs#L63)). |

The time conversions turn "never" (tick 0) into hour 0, which means "at game start".

### v2.0 → v3.0

Generated part ([SessionLoader.cs:100](../../Assets/Modules/GameSession/Scripts/GeneratedCode/SessionLoader.cs#L100)) moves every v2 member. The only change is the new `ShipPresets` object, which starts empty. The partial hook `Upgrage_v2_0_to_v3_0` has no implementation.

## Headerless legacy format

Saves written before the header existed start with the `int` format ID `1` (`01 00 00 00`). `LegacyDataLoader.TryDeserializeOldData` ([LegacyDataLoader.cs:14](../../Assets/Modules/GameSession/Scripts/Facade/LegacyDataLoader.cs#L14)) handles them:

1. `DatabaseContentObsolete.TryDeserialize` checks the format ID ([DatabaseContent.cs:69](../../Assets/Modules/GameSession/Scripts/Obsolete/DatabaseContent.cs#L69)). Then it reads a section count and, for each section, a name, a byte length, and the bytes. Integers and strings use `GameModel.Serialization.Helpers` ([Helpers.cs](../../Assets/Modules/CommonComponents/Scripts/Serialization/Helpers.cs)).
2. Each section name selects a reader class in `Scripts/Obsolete/Content/`. Unknown names are logged and skipped.
3. The reader classes resolve old string names through `Database.Legacy` name tables and need `IDatabase`, which `ContentFactoryObsolete` provides. The `regions` reader needs the seed of the `game` section, which comes first in every legacy save ([DatabaseContent.cs:109](../../Assets/Modules/GameSession/Scripts/Obsolete/DatabaseContent.cs#L109)).
4. The `Transfer*` methods copy each section into a new `v1.SaveGameData`.
5. `SessionLoader.Convert` runs the normal v1 → v3 conversions ([LegacyDataLoader.cs:46](../../Assets/Modules/GameSession/Scripts/Facade/LegacyDataLoader.cs#L46)).

| Section name | Reader | v1 target |
| --- | --- | --- |
| `game` | `GameData` | `Game` |
| `starmap` | `StarMapData` | `StarMap` |
| `inventory2` | `InventoryData` | `Inventory` |
| `fleet` | `FleetData` | `Fleet` |
| `stores` | `ShopData` | `Shop` |
| `events` | `EventData` | `Events` |
| `boss` | `BossData` | `Bosses` |
| `regions` | `RegionData` | `Regions` |
| `iap` | `InAppPurchasesData` | `Iap` |
| `wormholes` | `WormholeData` | `Wormholes` |
| `achievements` | `AchievementData` | `Achievements` |
| `objects` | `CommonObjectData` | `Common` |
| `research` | `ResearchData` | `Research` |
| `craft` | `StatisticsData` | `Statistics` |
| `resources` | `ResourcesData` | `Resources` |
| `upgrades` | `UpgradesData` | `Upgrades` |
| `pvp` | `PvpData` | `Pvp` |
| `social` | `SocialData` | `Social` |
| `quests` | `QuestData` | `Quests` |

Known transfer defects: [GS4](../issues/game_session.md#gs4-legacy-quest-statistics-get-a-wrong-completion-time) and [GS5.4](../issues/game_session.md#gs5-smaller-defects).

`SessionDataObsolete` and `ISessionDataObsolete` in `Scripts/Obsolete/` are the old runtime session. Nothing references them; only the reader classes and `DatabaseContentObsolete` are live.

## Failure behavior

| Condition | Result |
| --- | --- |
| Header present, format not 1 | `false`, error logged. |
| Unknown major | `false` through the EOF check. See [GS2](../issues/game_session.md#gs2-unknown-schema-versions-are-not-rejected). |
| Payload shorter than expected | `MemoryReaderStream` throws; `false`, exception logged. |
| Payload longer than expected, or misaligned | EOF marker mismatch; `false`, error logged. |
| Header absent, legacy format ID not 1 | `false`. |
| Any exception in legacy reading | `false`, exception logged. |

In every `false` case the `out` model may already be assigned. `SessionData` stores it anyway; see [GS1](../issues/game_session.md#gs1-a-failed-load-replaces-the-live-model).
