# Utilites Module Issues

This file records problems in `Assets/ModulesShared/Utilites/`. For the design, see [Utilites module](../utilites.md). Defects in the observable collections that only the session model can trigger are in [GameSession issues](game_session.md#gs5-smaller-defects) (GS5.1, GS5.2, GS5.17).

The analysis was done by reading the code on 2026-10-12. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain.
- **Confirmed (latent)** – the code path is certain, but current data or settings do not trigger it.
- **Plausible** – the code is wrong, but the failure needs specific conditions.

The module is in the `ModulesShared` submodule, which Frontier also uses. Types that have no user here may have users in Frontier.

## Defects

### UT1. `SecureList` `IndexOf` and `CopyTo` are wrong

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [SecureList.cs:98](../../Assets/ModulesShared/Utilites/Collections/SecureList.cs#L98) (`IndexOf`), [line 82](../../Assets/ModulesShared/Utilites/Collections/SecureList.cs#L82) (`CopyTo`).

**Failure.**
1. `IndexOf` returns the storage slot, not the list position. After any set or remove, the slot differs from the position. A caller that passes the result to the indexer or `RemoveAt` reads or removes the wrong item.
2. `CopyTo` checks the free space against the number of slots (`_items.Count`), not against `Count`. After any set or remove there are more slots than items. Then `new List<T>(secureList)`, `ToList()`, and `ToArray()`, which call `CopyTo` with an array of exactly `Count`, throw `ArgumentException`.

`SecureList` is used only by `SecureInventory`, which calls neither method, and by the tests. No Event Horizon code uses `SecureInventory`.

**Fix.** Return `i` from `IndexOf`. Compare against `Count` in `CopyTo`. Add both cases to `CollectionsTests`.

### UT2. `NextFloat` can return 1.0

**Severity:** Low. **Status:** Confirmed.

**Where.** [RandomStateExtensions.cs:11](../../Assets/ModulesShared/Utilites/Random/RandomStateExtensions.cs#L11) and [line 14](../../Assets/ModulesShared/Utilites/Random/RandomStateExtensions.cs#L14).

**Failure.** `NextUint() % (2^24 + 1)` gives 0 to 2^24. Divided by 2^24, the result is in [0, 1], not [0, 1). The chance of exactly 1.0 is about 1 in 16.8 million per call. The modulo by a number that does not divide 2^32 also gives a very small bias. Current callers tolerate 1.0: `QuestCollection.CreateRandomWeighted` subtracts weights with a 0.0001 margin and picks the last quest; `LayeredEffect` uses it for color and position noise. A future caller that computes an index as `(int)(NextFloat() * count)` will go out of range.

**Fix.** Use `(NextUint() >> 8) * (1f / 16777216f)`. **This changes every seeded float sequence**, which changes generated quest content for existing saves. Do not apply it without approval from the project owner. A safe alternative is a new method (`NextFloat01`) for new callers, with the old one kept and documented.

### UT3. `default(ObscuredInt)` is not zero

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [ObscuredInt.cs:37](../../Assets/ModulesShared/Utilites/Security/ObscuredInt.cs#L37) (`Value` getter); same in [ObscuredLong.cs](../../Assets/ModulesShared/Utilites/Security/ObscuredLong.cs).

**Failure.** A struct created without the constructor has all fields at 0. The getter returns `0 XOR mask`, which is the per-process mask, a large random number. This happens for an unassigned `ObscuredInt` field, for `default(ObscuredInt)`, and for the `out` value of a failed `Dictionary.TryGetValue`. All current fields are assigned before they are read, and every `TryGetValue` result is checked. A new field such as `private ObscuredInt _count;` that is read before it is set returns a random value instead of 0.

**Fix.** Keep a `bool _initialized` field that the constructor sets, and return 0 when it is false. The change does not affect saves, because saves hold plain values.

### UT4. `ReaderStream` does not detect the end of the stream

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [ReaderStream.cs:15](../../Assets/ModulesShared/Utilites/Serialization/ReaderStream.cs#L15) and [line 16](../../Assets/ModulesShared/Utilites/Serialization/ReaderStream.cs#L16); [EliasGammaDecoder.cs:47](../../Assets/ModulesShared/Utilites/Serialization/EliasGammaDecoder.cs#L47).

**Failure.**
1. `ReadByte` casts the `-1` end marker of `Stream.ReadByte` to 255.
2. `Read(Span)` ignores the count of bytes actually read. A short read leaves the rest of the buffer at 0.
3. `EliasGammaDecoder.ReadUnary` loops while it reads 1-bits. At the end of a stream, every byte reads as 255, so the loop never ends.

A truncated save read through `ReaderStream` hangs the game. Today the game reads saves through `MemoryReaderStream`, which throws at the end of the array ([GameSession: Failure behavior](../game_session/serialization.md#failure-behavior)).

**Fix.** Throw `EndOfStreamException` in `ReadByte` on `-1` and in `Read` on a short count. Limit `ReadUnary` to 64 sections.

### UT5. `ObservableSet.Equals` ignores its comparer

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [ObservableSet.cs:46](../../Assets/ModulesShared/Utilites/Serialization/ObservableSet.cs#L46).

**Failure.** `Equals(other, equalityComparer)` calls `HashSet.SetEquals`, which uses the set's own comparer. The argument has no effect. The only `ObservableSet` in the session model is `Achievements.Gained` (`int`), where both comparers agree.

**Fix.** Use the comparer, or remove the parameter.

## Structural issues

### UT6. Shared copy constructors alias the inner collection

**Problem.** `ObservableMap(original, callback)` ([ObservableMap.cs:46](../../Assets/ModulesShared/Utilites/Serialization/ObservableMap.cs#L46)) and `ObservableInventory(original, callback)` ([ObservableInventory.cs:90](../../Assets/ModulesShared/Utilites/Serialization/ObservableInventory.cs#L90)) do not copy. They wrap the same dictionary with a different callback. A change through one wrapper notifies only its own owner, so the other owner does not know its data changed and may not save it.

**Evidence.** No current code calls these constructors. The name suggests a copy.

**Direction.** Copy the dictionary, or remove the constructors.

### UT7. No reference values in the random tests

**Problem.** The PCG tests check only that sequences are equal or different. They do not compare against known outputs.

**Evidence.** [PcgRandomTests.cs](../../Assets/ModulesShared/Utilites/Tests/PcgRandomTests.cs). A change in the step function or the float conversion passes all tests and silently changes seeded content.

**Direction.** Add a test with the first outputs for a few seeds and streams, taken from the current code.

### UT8. The module mixes unrelated concerns under a misspelled name

**Problem.** Save serialization, anti-cheat values, random numbers, and inventories share one assembly. The serialization types use the `Session.Utils` namespace, which suggests the GameSession module. `ObscuredInt` and `ObscuredLong` are in the global namespace. `IQuantityProvier` is misspelled.

**Evidence.** Namespaces in the [source map](../utilites.md#source-map).

**Direction.** No action unless a larger refactor happens. Renames break Frontier and every `using` in the project.
