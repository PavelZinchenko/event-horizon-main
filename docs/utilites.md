# Utilites Module

The Utilites module holds small, dependency-free building blocks: inventory collections, a PCG random number generator, obscured integers that resist memory scanners, and the binary serialization helpers of the save system. The name keeps its original spelling; do not rename it, because assembly names and namespaces depend on it.

| | |
| --- | --- |
| Assembly | `Utilites` ([Utilites.asmdef](../Assets/ModulesShared/Utilites/Utilites.asmdef)). No references. `noEngineReferences: true` ([Engine-free modules](architecture.md#engine-free-modules)). Auto-referenced. |
| Namespaces | `Utilites.Collections` (inventories, `SecureList`), `Utilites` (random, `MathEx`), `Session.Utils` (serialization). `ObscuredInt` and `ObscuredLong` are in the global namespace. |
| Location | [`Assets/ModulesShared/Utilites/`](../Assets/ModulesShared/Utilites/), inside the `Assets/ModulesShared` Git submodule. |
| Source | 25 files, about 2,050 lines, plus 2 test files |
| Assemblies that reference it | BattleSimulator, CommonComponents, GalaxyMap, GameSession, Quests, ShipConstructor, Gui, ShipEditor, and the application (`Assets/Scripts`, through auto-reference). |
| Tests | [`Tests/`](../Assets/ModulesShared/Utilites/Tests/) (`Utilites.Tests`, editor only; also run by `dotnet test`): `CollectionsTests`, `PcgRandomTests`. Serialization tests are in GameSession ([list](game_session.md#tests-and-validation)). |
| Known issues | [Utilites issues](issues/utilites.md) |

## Contents

1. [Shared code constraint](#shared-code-constraint)
2. [Source map](#source-map)
3. [Inventories](#inventories)
4. [SecureList](#securelist)
5. [Random numbers](#random-numbers)
6. [MathEx](#mathex)
7. [Obscured integers](#obscured-integers)
8. [Serialization](#serialization)
9. [Observable collections](#observable-collections)
10. [Tests](#tests)
11. [Change rules](#change-rules)

## Shared code constraint

`Assets/ModulesShared` is a separate Git repository ([event-horizon-modules](https://github.com/PavelZinchenko/event-horizon-modules)). Its README says it holds "components shared between Event Horizon and Frontier". This repository does not contain Frontier, so the effect of a change on Frontier cannot be checked here.

For this module the constraint is strict, because two kinds of output are persistent:

1. **Save bytes.** The serialization helpers define the save format of [GameSession](game_session/serialization.md). A change in the bytes breaks existing saves of every game that uses the helpers.
2. **Random sequences.** `PcgRandom` and `RandomState` turn a seed into a fixed sequence. Quests use seeds stored in saves. A change in the algorithm or in the float conversion changes generated content for existing saves.

`SecureList`, `SecureInventory`, `ILockableInventory`, and `IInventory<T>.Empty` have no user in Event Horizon. Frontier may use them.

## Source map

| File | Contents |
| --- | --- |
| [Collections/IInventory.cs](../Assets/ModulesShared/Utilites/Collections/IInventory.cs) | `IQuantityProvier<T>` (sic), `IReadOnlyInventory<T>`, `IInventory<T>`, `ILockableInventory<T>`, internal `NullInventory<T>`. |
| [Collections/SimpleInventory.cs](../Assets/ModulesShared/Utilites/Collections/SimpleInventory.cs) | Inventory on a `Dictionary<T, int>`. |
| [Collections/SecureInventory.cs](../Assets/ModulesShared/Utilites/Collections/SecureInventory.cs) | Inventory with XOR-masked quantities in a `SecureList<int>`. |
| [Collections/SecureList.cs](../Assets/ModulesShared/Utilites/Collections/SecureList.cs) | `IList<T>` that stores items in shuffled slots behind masked indices. |
| [Random/PcgRandomAlgorithm.cs](../Assets/ModulesShared/Utilites/Random/PcgRandomAlgorithm.cs) | PCG32 step, seeding, and stream selection. |
| [Random/PcgRandom.cs](../Assets/ModulesShared/Utilites/Random/PcgRandom.cs) | `PcgRandom` class: seed and stream. |
| [Math/MathEx.cs](../Assets/ModulesShared/Utilites/Math/MathEx.cs) | `MathEx`: engine-free replacement for the `UnityEngine.Mathf` members that engine-free code uses. |
| [Random/RandomState.cs](../Assets/ModulesShared/Utilites/Random/RandomState.cs), [RandomStateExtensions.cs](../Assets/ModulesShared/Utilites/Random/RandomStateExtensions.cs) | `RandomState` struct and its `Next`, `NextUint`, `NextFloat`, `NextDouble`. |
| [Security/ObscuredInt.cs](../Assets/ModulesShared/Utilites/Security/ObscuredInt.cs), [ObscuredLong.cs](../Assets/ModulesShared/Utilites/Security/ObscuredLong.cs) | Masked `int` and `long` values. |
| [Serialization/](../Assets/ModulesShared/Utilites/Serialization/) (14 files) | Streams, the variable-length integer coder, `SessionDataReader` and `SessionDataWriter`, observable collections, `IDataChangedCallback`. |

## Inventories

An inventory maps an item to a positive quantity.

| Interface | Members |
| --- | --- |
| `IQuantityProvier<T>` | `GetQuantity(item)`: 0 when absent. |
| `IReadOnlyInventory<T>` | `Items`: the keys. |
| `IInventory<T>` | `Add(item, quantity = 1)`, `Remove(item, quantity = 1)`, `Clear()`, static `Empty`. |
| `ILockableInventory<T>` | `Lock(item)`, `Unlock(item)`. No implementation except `NullInventory`. |

Rules in both implementations:

1. `Add` and `Remove` ignore a quantity of 0 or less.
2. `Remove` returns the amount removed. It removes the key when the quantity reaches 0, so `Items` never lists an item with quantity 0.
3. Every change sets `IsDirty`. No code in the module reads or clears it.

`SimpleInventory<T>` is used by [`ShipBuilder`](ship_constructor.md) to count limited components and component tags. The ship editor reads inventories through `IReadOnlyInventory<T>` ([Gui component list](gui.md#component-list)).

`SecureInventory<T>` stores `quantity XOR mask` in a `SecureList<int>`. The mask is the low 32 bits of `DateTime.UtcNow.Ticks` at construction, one per instance. Freed slots are reused.

The game's main inventories do not use these classes. They use [`GameItemCollection`](common_components.md#gameitemcollection) and the session's `ObservableInventory`.

## SecureList

`SecureList<T>` hides the order of items from a memory scan. It keeps two lists:

1. `_items`: the stored values, in slot order.
2. `_indices`: for each list position, the slot number XOR a mask. The mask is the current second count (`Ticks / TicksPerSecond`) at construction.

Setting an item writes the value to a new slot and frees the old slot. Freed slots are reused only when more than 8 are free. So the old value stays in memory for a while; this is the intended "reserve".

`IndexOf` and `CopyTo` have defects; see [UT1](issues/utilites.md#ut1-securelist-indexof-and-copyto-are-wrong).

## Random numbers

The module implements PCG32 (permuted congruential generator, the `XSH RR` output function). A PCG generator keeps a 64-bit state and a 64-bit odd increment. The increment selects one of 2^63 independent streams.

| Type | State | Seeding | Use |
| --- | --- | --- | --- |
| `PcgRandom` (class) | `ulong` state and increment | `PcgRandom()` uses `Environment.TickCount`; `PcgRandom(seed)` uses the default stream; `PcgRandom(seed, sequence)` uses stream `sequence`. | Quests (`QuestCollection.CreateRandomWeighted`, random quest seeds in `NodeBuilder`), `QuestDataProvider`, `DatabaseMusicPlaylist`. |
| `RandomState` (struct) | `ulong Value` only; always the default stream | `FromSeed(seed)`, `FromTickCount()` | `StarLayout` star offsets (save-affecting), `LayeredEffect` in combat, `HolidayManager` (holiday quest seeds), `CommonShip` random colours, the Database expression random source. |

Seeding follows the reference PCG code: start from 0, step once, add the seed, step once.

Output:

| Method | Range | Notes |
| --- | --- | --- |
| `NextUint()` | 0 to 2^32−1 | One step. |
| `Next()` | 0 to 2^31−1 | `NextUint() & 0x7FFFFFFF`. |
| `NextDouble()` | [0, 1) | `NextUint() / 2^32`. 32 bits of precision. |
| `NextFloat()` | [0, **1**] | `(NextUint() % (2^24 + 1)) / 2^24`. Can return exactly 1.0. See [UT2](issues/utilites.md#ut2-nextfloat-can-return-10). |
| `InsideUnitCircle()` | Point with radius ≤ 1 | `r = sqrt(NextFloat())`, `angle = 2π·NextFloat()`; returns `(float x, float y)`. `StarLayout` uses it for star offsets, so a change moves every star in every save. |

`RandomState` is a value type. Pass it by `ref` (the extension methods take `this ref`); a copy continues the same sequence independently.

The project also uses `System.Random` and `UnityEngine.Random` in many places. `PcgRandom` gives the same sequence on every platform and runtime, which matters for seeded quest content. Some users (`QuestDataProvider`, `DatabaseMusicPlaylist`, `LayeredEffect`) seed it from the clock and do not need that property.

## MathEx

[`MathEx`](../Assets/ModulesShared/Utilites/Math/MathEx.cs) replaces `UnityEngine.Mathf` in engine-free code. Each member gives the same result as its `Mathf` counterpart, so a switch from `Mathf` to `MathEx` changes no game output.

| Member | Behavior that must stay |
| --- | --- |
| `Clamp(float)`, `Clamp(int)`, `Clamp01` | Never throws. `System.Math.Clamp` throws when `min > max`, so do not use it as a replacement. |
| `RoundToInt` | `(int)Math.Round(value)`: rounds half to even, like `Mathf.RoundToInt`. |
| `Min`, `Max` (float, int) | Two-argument forms only. |
| `Lerp` | Clamps `t` to [0, 1]. |
| `Approximately` | The `Mathf.Approximately` formula. |
| `Sqrt`, `PI`, `Deg2Rad`, `Rad2Deg` | `float` versions. |

`Sin`, `Cos`, `Atan2`, `DeltaAngle`, and `PerlinNoise` are not in `MathEx`: only engine code uses them. Add a member only when engine-free code needs it, and match `Mathf` exactly. ExpressionParser does not reference Utilites; it has its own internal `MathCompat` ([Expression parser](expression_parser.md)).

## Obscured integers

`ObscuredInt` and `ObscuredLong` keep a value in memory as `value XOR mask`. They make it harder to find a currency value with a memory scanner. They are not encryption.

1. The mask is static: one per type per process. `ObscuredInt` takes it from `System.Random`; `ObscuredLong` uses `DateTime.UtcNow.Ticks`.
2. Each value has seven storage fields. The constructor picks one by `(value XOR mask) % 7`, so equal values use the same field.
3. Implicit conversions go both ways. `++` and `--` create a new value.
4. The save file holds the plain value, never the mask.

Users: [`GameItemCollection`](common_components.md#gameitemcollection) values, session `ResourcesData` (money, stars, fuel, tokens), `ObservableInventory` values, research points, and some application fields. See [GameSession: options that do not change bytes](game_session/serialization.md#options-that-do-not-change-bytes).

`default(ObscuredInt)` is not 0. All fields are 0, so the value reads as the mask. See [UT3](issues/utilites.md#ut3-defaultobscuredint-is-not-zero).

## Serialization

The `Session.Utils` namespace holds the low-level save format. [GameSession: Serialization](game_session/serialization.md) describes it in full. This section lists the parts.

| Type | Role |
| --- | --- |
| `IWriterStream`, `IReaderStream` and their extension methods | Fixed-size little-endian values (`BitConverter`) and length-prefixed UTF-8 strings. |
| `WriterStream`, `ReaderStream` | Adapters for `System.IO.Stream`. The game writes saves through `WriterStream`. |
| `MemoryWriterStream`, `MemoryReaderStream` | Adapters for a `byte[]`. The game reads saves through `MemoryReaderStream`. They do no bounds checks of their own; the array throws. |
| `EliasGammaEncoder`, `EliasGammaDecoder` | A bit-packed variable-length integer code with 4-bit sections (despite the name, not the classic Elias gamma code). |
| `SessionDataWriter`, `SessionDataReader` | Pick `Plain` or `EliasGamma` per value. `float` and `string` are always `Plain`. |
| `EncodingType` | `Plain`, `EliasGamma`. |
| `IDataChangedCallback` | `OnDataChanged()`: the change notification of the session model. |

`ReaderStream` does not detect the end of the stream. See [UT4](issues/utilites.md#ut4-readerstream-does-not-detect-the-end-of-the-stream).

## Observable collections

The generated session model ([code generator](game_session/code_generator.md)) uses five collection types. Each wraps a standard collection and calls `IDataChangedCallback.OnDataChanged()` on its owner after every change. The owner forwards the call up to the session root, which marks the game as changed.

| Type | Wraps | Notes |
| --- | --- | --- |
| `ObservableList<T>` | `List<T>` | `Assign(items, comparer)` changes only differing entries and returns the count. `Sort` does not notify ([GS5.2](issues/game_session.md#gs5-smaller-defects)). |
| `ObservableSet<T>` | `HashSet<T>` | |
| `ObservableMap<TKey, TValue>` | `Dictionary<TKey, TValue>` | `SetValue(key, value, comparer)` skips equal values. |
| `ObservableInventory<T>` | `Dictionary<T, ObscuredInt>` | `Remove` deletes a key at 0. `SetValue` keeps a key with 0 ([GS5.17](issues/game_session.md#gs5-smaller-defects)). |
| `ObservableBitset` | Sorted `List<uint>` of change points | Compact for long runs of set bits. See [GameSession: Bitset](game_session/serialization.md#bitset). |

All five are structs that hold a reference to the inner collection. Copying the struct copies the reference, so both copies change the same data. `ObservableMap` and `ObservableInventory` have a constructor `(original, callback)` that shares the inner dictionary with the original under a new callback. No current code calls it.

`Equals(other, comparer)` compares contents. `ObservableSet.Equals` ignores its comparer argument and uses the set's own comparer.

## Tests

| Test | Checks |
| --- | --- |
| `CollectionsTests.SecureListTest` | Add, set, remove, insert, and enumeration order. |
| `CollectionsTests.SecureInventoryTest` | Add, remove past zero, slot reuse. |
| `PcgRandomTests.PcgRandomStateTest` | Equal seeds give equal sequences; different seeds differ. |
| `PcgRandomTests.PcgSeedDistributionTest` | Spread of first outputs over consecutive seeds, within a tolerance. |
| `PcgRandomTests.PcgRandomTest` | Stream selection: `Sequence` round trip; different streams differ. |

No test fixes reference outputs. A change in the algorithm would pass these tests and still change seeded content. Run them in the Unity Test Runner (EditMode, assembly `Utilites.Tests`) or with `dotnet test Tests~/EngineFree.Tests.csproj`.

## Change rules

1. Do not change the bytes that the serialization helpers write or read. Add a new method instead. See [GameSession: Compatibility rules](game_session/serialization.md#compatibility-rules).
2. Do not change the output of `PcgRandom` or `RandomState` for a given seed. Ask the project owner first, and add a test with fixed reference outputs before any change.
3. Keep the assembly free of references. Every other module depends on it.
4. Check Frontier before you remove a public type, even one with no user here.
