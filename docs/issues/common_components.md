# CommonComponents Module Issues

This file records problems in `Assets/Modules/CommonComponents/`. For a description of the current design, see [CommonComponents module](../common_components.md).

The analysis was done by reading the code and its callers on 2026-10-12. Numeric claims (spiral IDs, rounding) were checked with a script. No issue was reproduced in Play Mode. Each issue has a severity (High, Medium, or Low) and a status:

- **Confirmed** – the code path is certain. The failure follows directly from the code.
- **Confirmed (latent)** – the code path is certain, but no current caller or data triggers it. A new caller, mod data, or a future change can.
- **Plausible** – the code is wrong, but the failure needs specific runtime conditions.

Defects use Where / Failure / Fix. Structural issues use Problem / Evidence / Direction.

All source paths below are relative to [`Assets/Modules/CommonComponents/Scripts/`](../../Assets/Modules/CommonComponents/Scripts/).

## Summary

| ID | Severity | Status | Topic |
| --- | --- | --- | --- |
| [CC1](#cc1-money--double-rounds-float-error-up) | Medium | Confirmed | `Money * double` adds 1 for float error. |
| [CC2](#cc2-double-to-long-saturation-is-unspecified) | Low | Confirmed (latent) | Huge `double` → `Money` can become 0. |
| [CC4](#cc4-threadsafe-loops-never-end-on-nan) | Medium | Confirmed (latent) | NaN hangs the hit point and energy updates. |
| [CC5](#cc5-the-weak-event-logs-the-same-exception-several-times) | Low | Confirmed | Duplicate exception logs. |
| [CC6](#cc6-the-weak-event-rejects-lambdas-that-capture-nothing) | Low | Confirmed | Wrong closure check. |
| [CC7](#cc7-weak-signals-call-destroyed-objects-and-lose-unrooted-subscribers) | Medium | Plausible | Weak-reference lifetime does not match Unity lifetime. |
| [CC8](#cc8-a-throwing-handler-stops-a-strong-signal) | Low | Confirmed | Strong and weak signals fail differently. |
| [CC9](#cc9-cleanup-has-a-mesh-branch-that-never-helps) | Low | Confirmed | Useless mesh branch in `Cleanup`. |
| [CC10](#cc10-array-removeall-overwrites-its-input) | Low | Confirmed (latent) | Hidden mutation. |
| [CC11](#cc11-the-read-only-view-misses-datachangedevent) | Low | Confirmed (latent) | Event forwarding bug. |
| [CC12](#cc12-gameitemcollection-allows-zero-and-negative-quantities) | Low | Plausible | Invariant not enforced. |
| [CC13](#cc13-stream-readers-hide-truncated-data) | Low | Confirmed (latent) | Truncated mods load partly without an error. |
| [CC14](#cc14-binary-readers-trust-lengths) | Low | Confirmed | No length validation. |
| [CC15](#cc15-shuffle-is-biased) | Low | Confirmed | Non-uniform shuffle. |
| [CC16](#cc16-backgroundtask-cannot-stop-while-paused) | Low | Plausible | Thread lifecycle gaps. |
| [CC17](#cc17-smaller-defects) | Low | Mixed | Smaller defects. |
| [CC-S1](#cc-s1-global-types-shadow-framework-types) | Medium | Confirmed | `Lazy<T>`, `WeakReference<T>` replace the BCL types silently. |
| [CC-S2](#cc-s2-the-module-borrows-other-modules-namespaces) | Low | — | Namespace sprawl. |
| [CC-S3](#cc-s3-the-leaf-assembly-holds-feature-code) | Low | — | Wrong contents for a leaf assembly. |
| [CC-S4](#cc-s4-deterministic-outputs-have-no-tests) | Medium | — | Save-critical code without tests. |
| [CC-S5](#cc-s5-it-duplicates-utilites) | Low | — | Two copies of the same infrastructure. |
| [CC-S6](#cc-s6-unused-code) | Low | Needs review | Dead code. Do not delete before review. |

## Defects

### CC1. `Money * double` rounds float error up

**Severity:** Medium. **Status:** Confirmed.

**Where.** [Money.cs:18](../../Assets/Modules/CommonComponents/Scripts/Economy/Money.cs#L18) multiplies as `double` and [Money.cs:79](../../Assets/Modules/CommonComponents/Scripts/Economy/Money.cs#L79) applies `Math.Ceiling`. `Price * float` ([Price.cs:38](../../Assets/Modules/CommonComponents/Scripts/Economy/Price.cs#L38)) goes through the same path. [CommonProduct.cs:84](../../Assets/Scripts/Domain/Economy/Products/CommonProduct.cs#L84) computes every shop price as `itemType.Price * priceScale`.

**Failure.** Binary floating point cannot store most decimal fractions. The product is then a little above the exact integer, and `Ceiling` adds 1:

| Expression | `double` result | `Money` |
| --- | --- | --- |
| `(Money)100 * 1.1` | 110.00000000000001 | 111 |
| `(Money)100 * 0.1f` | 10.000000149011612 | 11 |

The player pays 1 credit more than the intended price in these cases. The error depends on the value, so prices look inconsistent.

**Design rule (owner decision, 2026-10-12).** A price that is not zero must never become zero after scaling. Rounding up is not required.

**Fix.** Round to the nearest integer. Then, if the original amount and the multiplier are both above zero, return at least 1:

```csharp
var result = Clamp(Math.Round(first.Amount * second));
return first.Amount > 0 && second > 0 && result == 0 ? 1 : result;
```

`Clamp` must not call `Math.Ceiling` any more (see also [CC2](#cc2-double-to-long-saturation-is-unspecified)). The implicit `double` → `Money` conversion also uses `Ceiling`; decide separately if it should follow the same rule. Add test cases for both rows above and for `1 * 0.01` (expected 1).

### CC2. Double-to-long saturation is unspecified

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [Money.cs:81](../../Assets/Modules/CommonComponents/Scripts/Economy/Money.cs#L81) clamps a `double` to `long.MaxValue` and casts it to `long`. [FlexInt.cs:23](../../Assets/Modules/CommonComponents/Scripts/Utils/FlexInt.cs#L23) does the same.

**Failure.** `(double)long.MaxValue` is exactly 2⁶³, which is one more than `long.MaxValue`. In an `unchecked` context, C# leaves the result of an out-of-range conversion unspecified. On x64 (Mono in the Editor, Windows standalone) the cast gives `long.MinValue`, and the `Money` constructor turns it into 0. On ARM64 the hardware conversion saturates to `long.MaxValue` instead. So `Money * 1e30` is 0 on one platform and the maximum on another. A NaN product is also unspecified. No current price reaches 9.2 × 10¹⁸, so nothing fails today.

**Fix.** Compare before the cast: `if (value >= 9.2233720368547758E18) return long.MaxValue; if (!(value > 0)) return 0;`.

### CC4. `ThreadSafe` loops never end on NaN

**Severity:** Medium. **Status:** Confirmed (latent).

**Where.** [ThreadSafe.cs:14](../../Assets/Modules/CommonComponents/Scripts/Math/ThreadSafe.cs#L14), [:34](../../Assets/Modules/CommonComponents/Scripts/Math/ThreadSafe.cs#L34), [:56](../../Assets/Modules/CommonComponents/Scripts/Math/ThreadSafe.cs#L56). The loops exit when `newCurrentValue == currentValue`. Callers: [`Energy` and `HitPoints`](../../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/HitPoints/IResourcePoints.cs#L69).

**Failure.**
1. A NaN argument (for example damage computed as 0/0) makes the new value NaN. The first call stores NaN and exits normally.
2. On the next call, `Interlocked.CompareExchange` compares bits, so it swaps and returns NaN.
3. `NaN == NaN` is false, so the loop repeats forever.

The main thread or the AI thread hangs during a battle. The callers guard against `MaxValue == 0`, but not against NaN input from damage or effect formulas.

**Fix.** Compare bits: `BitConverter.SingleToInt32Bits(newCurrentValue) == BitConverter.SingleToInt32Bits(currentValue)`. Also reject NaN input with a logged error.

### CC5. The weak event logs the same exception several times

**Severity:** Low. **Status:** Confirmed.

**Where.** [SmartWeakEvent.cs:144](../../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs#L144):

```csharp
for (var ex = e; ex != null; ex = ex.InnerException)
    GameDiagnostics.Trace.LogException(e);
```

**Failure.** The loop logs `e` instead of `ex`. `MethodInfo.Invoke` wraps the handler's exception in `TargetInvocationException`, so the chain has at least two levels. Every handler exception is logged at least twice, and the top entry is the wrapper, not the real error.

**Fix.** Log `e.InnerException ?? e` once.

### CC6. The weak event rejects lambdas that capture nothing

**Severity:** Low. **Status:** Confirmed.

**Where.** [SmartWeakEvent.cs:72](../../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs#L72) rejects a delegate when its method's declaring type has `[CompilerGenerated]`.

**Failure.** The C# compiler puts a lambda that captures nothing in a compiler-generated `<>c` class. So `signal.Event += () => Log()` throws "Cannot create weak event to anonymous method with closure", although there is no closure. A lambda that uses `this` passes, because the compiler puts it in the subscriber class. The rule a developer must follow differs from the error message.

**Fix.** Allow `<>c` singletons (their target is a static field, so a weak reference to it is safe). Change the message to "lambdas that capture local variables are not supported".

### CC7. Weak signals call destroyed objects and lose unrooted subscribers

**Severity:** Medium. **Status:** Plausible.

**Where.** [SmartWeakEvent.Raise](../../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs#L116). 73 of 79 signal types are weak.

**Failure.**
1. A destroyed `MonoBehaviour` stays a live managed object until the GC runs. If it did not unsubscribe, the signal calls its handler. The handler then touches destroyed components and throws `MissingReferenceException`, or it changes state of a dead screen.
2. A plain C# object that only the signal references is collected at a random time. After that it stops receiving events, with no error. The bug depends on GC timing, so it appears only on some devices.

**Fix.** Keep explicit unsubscription as the rule for both signal kinds. Optionally skip targets where `target is UnityEngine.Object o && o == null`. Log a warning in development builds when a weak entry dies without `Remove`.

### CC8. A throwing handler stops a strong signal

**Severity:** Low. **Status:** Confirmed.

**Where.** [Signal.cs:20](../../Assets/Modules/CommonComponents/Scripts/Signals/Signal.cs#L20), [:39](../../Assets/Modules/CommonComponents/Scripts/Signals/Signal.cs#L39), [:58](../../Assets/Modules/CommonComponents/Scripts/Signals/Signal.cs#L58).

**Failure.** `Signal.Trigger.Fire` invokes the multicast delegate directly. If one handler throws, later handlers do not run and the exception reaches the publisher. `SmartWeakSignal` logs the exception and continues. Converting a signal from one kind to the other silently changes error behavior.

**Fix.** Invoke each entry of `GetInvocationList()` in a try/catch and log, like the weak version.

### CC9. `Cleanup` has a mesh branch that never helps

**Severity:** Low. **Status:** Confirmed.

**Where.** [GameObjectExtensions.cs:62–78](../../Assets/Modules/CommonComponents/Unity/Legacy/GameObjectExtensions.cs#L62):

```csharp
if (!meshFilter.sharedMesh)
    GameObject.Destroy(meshFilter.mesh);
```

**Failure.**
1. When no mesh is assigned, `meshFilter.mesh` creates a new empty mesh and the code destroys it. This is wasted work.
2. When a mesh is assigned, nothing is destroyed. A procedural mesh made with `GetMesh()` stays in memory until the next `Resources.UnloadUnusedAssets`.
3. `renderer.materials` clones every shared material before the loop destroys the clones. This is also wasted work.

The only live caller is [Star.OnDestroy](../../Assets/Scripts/Legacy/Map/Star.cs#L240). Star does not build a procedural mesh, so nothing leaks today. Do not "fix" the check to destroy `sharedMesh`: for Star that is the prefab's mesh asset.

**Fix.** Delete the mesh branch and read `sharedMaterials`, destroying only materials the object created. Or delete `Cleanup` and the call in `Star`.

### CC10. Array `RemoveAll` overwrites its input

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [EnumerableExtensions.cs:253](../../Assets/Modules/CommonComponents/Scripts/Utils/EnumerableExtensions.cs#L253). The loop writes `target[index] = target[i]` into the caller's array, then copies the result to a new array.

**Failure.** The method looks pure because it returns a new array. But the old array is compacted in place, and its tail holds duplicates. [SkillTreeNode.RemoveLink](../../Assets/Scripts/Legacy/GUI/ViewModel/Skills/SkillTreeNode.cs#L75) replaces its field, but `LinkedNodes` exposes the same array, so a caller that holds the old array sees changed contents.

**Fix.** Count matches first, then copy into a new array without writing to `target`.

### CC11. The read-only view misses `DataChangedEvent`

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [ObservableCollection.cs:149](../../Assets/Modules/CommonComponents/Scripts/Utils/ObservableCollection.cs#L149), [:159](../../Assets/Modules/CommonComponents/Scripts/Utils/ObservableCollection.cs#L159), [:169](../../Assets/Modules/CommonComponents/Scripts/Utils/ObservableCollection.cs#L169).

**Failure.** The writable collection forwards a change to its read-only view only if the view has a subscriber for the specific event (`ItemAddedEvent`, `ItemRemovedEvent`, or `EntireCollectionChangedEvent`). A view with only `DataChangedEvent` subscribers never gets notified. Nothing calls `AsReadOnly()` today.

**Fix.** Always call the view's invoke methods, or delete `AsReadOnly`.

### CC12. `GameItemCollection` allows zero and negative quantities

**Severity:** Low. **Status:** Plausible.

**Where.** [GameItemCollection.cs:56](../../Assets/Modules/CommonComponents/Scripts/Utils/GameItemCollection.cs#L56) (indexer set), [:66](../../Assets/Modules/CommonComponents/Scripts/Utils/GameItemCollection.cs#L66) (`Add`), [:29](../../Assets/Modules/CommonComponents/Scripts/Utils/GameItemCollection.cs#L29) (`Assign`).

**Failure.**
1. `collection[item] = 0` or `Add(item, -n)` stores an entry with 0 or a negative quantity. `Count`, `Keys`, and `Items` include it. [PlayerInventory](../../Assets/Scripts/Domain/Player/PlayerInventory.cs#L72) copies it to the session.
2. `Assign` with a duplicate key throws after it has cleared the collection, so the collection is left half full.
3. `Add` can overflow `int` without a check.

**Fix.** Route the setter and `Add` through one method that removes entries at ≤ 0 and saturates. Build `Assign` into a new dictionary and swap it in.

### CC13. Stream readers hide truncated data

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [StreamExtensions.cs](../../Assets/Modules/CommonComponents/Scripts/Serialization/StreamExtensions.cs). Caller: [FileDatabaseStorage.LoadContent](../../Assets/Modules/Database/Scripts/Storage/FileDatabaseStorage.cs#L36).

**Failure.**
1. `ReadInt32`/`ReadUInt32` ignore the byte count from `Read`. At end of stream they return 0 or a mix of new and zero bytes.
2. `ReadString`/`ReadByteArray` return an empty value when fewer bytes arrive than the length says.
3. A negative string length throws `OverflowException` from `stackalloc`.

For the authenticated mod format, AES-GCM verifies the whole content first, so truncation fails early. For legacy and headerless mods, a truncated file gives an empty JSON string (one logged exception, then the loop ends at EOF) or an image with no bytes. The mod loads partly and the player gets no clear error. The method contract also allows a short read from a healthy stream; the current zlib stream fills the buffer, so this does not occur today.

**Fix.** Read in a loop until the buffer is full and throw `EndOfStreamException` on a short read. `FileDatabaseStorage` already has such a [`ReadExactly`](../../Assets/Modules/Database/Scripts/Storage/FileDatabaseStorage.cs#L218); move it here. Reject negative and oversized lengths.

### CC14. Binary readers trust lengths

**Severity:** Low. **Status:** Confirmed.

**Where.** [Helpers.cs:212–355](../../Assets/Modules/CommonComponents/Scripts/Serialization/Helpers.cs#L212).

**Failure.** A damaged save or cloud blob can contain any count. A large count allocates a huge array (`OutOfMemoryException`). A negative string length throws `ArgumentOutOfRangeException`. Reading past the end throws `IndexOutOfRangeException` or `ArgumentException`. A duplicate dictionary key throws `ArgumentException`. Each caller must catch every type. Also, null strings come back as `""`, so the format cannot round-trip null.

**Fix.** Check `count >= 0 && count <= data.Length - index` before every allocation and throw one exception type (for example `InvalidDataException`).

### CC15. `Shuffle` is biased

**Severity:** Low. **Status:** Confirmed.

**Where.** [EnumerableExtensions.cs:75](../../Assets/Modules/CommonComponents/Scripts/Utils/EnumerableExtensions.cs#L75). Each position swaps with `random.Next(list.Count)`.

**Failure.** This gives `nⁿ` equally likely paths for `n!` orders, so some orders are more likely than others. Callers: the AI `RandomSelectorNode` and fleet generation in Quests and legacy factories.

**Fix.** Use Fisher–Yates (`j = random.Next(i, n)`). The fix changes the output for a given seed. A seeded quest fleet that is already in a save would change composition after the update, so treat it as a [determinism change](../common_components.md#determinism-and-save-compatibility).

**Approval.** Ask the owner before applying this fix.

### CC16. `BackgroundTask` cannot stop while paused

**Severity:** Low. **Status:** Plausible.

**Where.** [BackgroundTask.cs:44–76](../../Assets/Modules/CommonComponents/Scripts/Legacy/BackgroundTask.cs#L44).

**Failure.**
1. The pause loop checks only `_paused`. `StopTask` during a pause does not stop the thread.
2. `_paused` and `_cancelled` are not `volatile`. The C# memory model does not guarantee that the worker sees the change.
3. The thread is a foreground thread. If `Dispose` is not called (for example an exception in scene teardown), the thread keeps running and can keep a standalone process alive.
4. `_started` is never reset, so the task cannot restart after `StopTask`.

Nothing sets `Paused` today, so item 1 is latent.

**Fix.** Use a `CancellationToken` or `volatile` fields, check cancellation inside the pause loop, and set `IsBackground = true`.

### CC17. Smaller defects

1. **Division by zero.** `Money / Money` and `Price / int` throw `DivideByZeroException` for a zero divisor ([Money.cs:15](../../Assets/Modules/CommonComponents/Scripts/Economy/Money.cs#L15), [Price.cs:64](../../Assets/Modules/CommonComponents/Scripts/Economy/Price.cs#L64)).
2. **Ring 0.** `GetAdjacentStars(id, 0)` returns the center star four times. A negative distance returns wrong stars. Negative star IDs give garbage in `IdToPosition`. The `odd` variables in `GetAdjacentStars` and `Distance` mean "even" ([StarLayout.cs:104](../../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L104), [:153](../../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L153)).
3. **Zero-width arc.** `IsRotationInArc` returns true for every rotation when `min == max` after normalization ([RotationHelpers.cs:63](../../Assets/Modules/CommonComponents/Unity/Math/RotationHelpers.cs#L63)). The only caller uses arcs of at least 5°, so this is latent.
4. **Equal speeds.** In `GetTargetPosition`, the quadratic coefficient `a` is 0 when target speed equals bullet speed. The solver divides by zero and returns `true` with an infinite time and position ([Geometry.cs:139](../../Assets/Modules/CommonComponents/Unity/Math/Geometry.cs#L139)). The second overload then makes the time NaN.
5. **Negative formatting.** `ToTruncatedInGameString` and `ToExpandedInGameString` use `Math.Floor`, so −1,234 becomes `-2K` and −123,456.7 becomes `-124K`. `AsDecimal` uses the current culture while the other formatters use the invariant culture. The `K/M/B` suffixes are not localized.
6. **AudioClipId.** The constructor removes the first `*` anywhere in the name, not only a leading one ([AudioClipId.cs:9](../../Assets/Modules/CommonComponents/Scripts/DataTypes/AudioClipId.cs#L9)).
7. **IsNull.** `GameObjectExtensions.IsNull` tests `GetHashCode() == 0`. A destroyed object keeps its instance ID, so the test is always false for it. Unused.
8. **Names.** `NameGenerator` ignores the game seed, so all galaxies share star names. Two stars can get the same name. `GetPlanetName` throws for an index of 3,999 or more.
9. **Double enumeration.** `RandomElement(IEnumerable)`, `RandomElements(count, random)`, `RandomUniqueElements(count, random)`, and `Helpers.Serialize(IEnumerable<…>)` enumerate the sequence twice. A lazy sequence with side effects or a changing source gives wrong results.
10. **Unstable bytes.** `Helpers.Serialize` writes `HashSet` and `Dictionary` entries in hash order. Two saves of equal data can differ byte by byte, which breaks checksums or diffs.
11. **Missing equality.** `Price` and `SpriteId` have no `==` and use reflection-based `ValueType.Equals`. `Price.ToString()` omits the currency.
12. **Random helpers.** `random.Next(float, float)` has only 1,000 steps. `NextFloatNormal` is not a normal distribution. `random.Range(min, int.MaxValue)` overflows.
13. **ObservableCollection events.** `Clear` and `Assign` fire events also when nothing changed. The indexer setter fires `DataChangedEvent` twice.
14. **Price.Min/Max.** `Min(m)` returns at least `m` and `Max(m)` returns at most `m`. The names read as the opposite.

## Structural issues

### CC-S1. Global types shadow framework types

**Severity:** Medium. **Status:** Confirmed.

**Problem.** [`Lazy<T>`](../../Assets/Modules/CommonComponents/Scripts/Utils/Lazy.cs) and [`WeakReference<T>`](../../Assets/Modules/CommonComponents/Scripts/Legacy/WeakReference.cs) are in the global namespace. C# resolves a name in the global namespace before it uses `using` directives at the top of a file. So in all 14 assemblies that reference CommonComponents, these names mean the custom types, even in a file with `using System;`.

**Evidence.**
1. [BulletFactory.cs:45](../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L45) has `using System;` and gets the custom `Lazy<T>`: not thread-safe, re-runs a failed initializer, has `HasValue` instead of `IsValueCreated`.
2. `UnityEngineThread`, `Console`, `SharedResources`, and `BackgroundTask` call `IsAlive` and `Target` on `WeakReference<T>`. `System.WeakReference<T>` has neither, so they depend on the shadow type.
3. A developer who reads `Lazy<T>` expects the BCL semantics. The compiler gives no warning.

**Direction.** Rename the types (`LazyValue<T>`, `TypedWeakReference<T>`) or move them into a namespace. Migrate the callers. For `Lazy<T>`, the BCL type is a drop-in replacement for `BulletFactory`.

### CC-S2. The module borrows other modules' namespaces

**Severity:** Low.

**Problem.** The module spreads its types over 13 namespaces plus the global one. Several names belong to other modules: `GameDatabase.Model` (Database), `Services.Storage` (Services), `Economy.ItemType` (application), `Utils`, `Model.Generators`.

**Evidence.** A reader who sees `GameDatabase.Model.StatMultiplier` looks for it in the Database module. Twelve global types (`Geometry`, `Primitives`, `ThreadSafe`, …) are visible in every file of every dependent assembly, with no `using` to show where they come from.

**Direction.** Set `rootNamespace: CommonComponents` for new files. Move existing types only together with their callers, one folder at a time.

### CC-S3. The leaf assembly holds feature code

**Severity:** Low.

**Problem.** A leaf assembly should hold small, stable types that many assemblies need. This one also holds code that only one feature uses. The engine code (`Primitives`, `GameObjectExtensions`, `Geometry`, `RotationHelpers`, `VectorExtensions`, `SignalExtensions`) moved to `CommonComponents.Unity` in 2026-10; it still has few users each.

| Code | Only user |
| --- | --- |
| `BackgroundTask` | BattleSimulator `AiManager` |
| `ThreadSafe` | BattleSimulator `IResourcePoints` |
| `NameGenerator` | Galaxy / exploration / quests |
| `Helpers` (old save format) | GameSession obsolete content, storage |
| `Base64Encoder` | Private `WebStorage` |

**Evidence.** 14 assemblies reference the module, so any change recompiles all of them. The assembly is now engine-free, and its tests run with `dotnet test`.

**Direction.** Move single-user code into its user's module when that code is next changed. Keep new code out of `Legacy/`.

### CC-S4. Deterministic outputs have no tests

**Severity:** Medium.

**Problem.** Star IDs, star positions, star levels, neighbour order, region home stars, star names, and seeded selection helpers all feed saves or seeded content (see [Determinism and save compatibility](../common_components.md#determinism-and-save-compatibility)). A change to any of them moves stars, changes levels, or changes quest targets in existing saves.

**Evidence.** [CommonComponentsTests.cs](../../Assets/Modules/CommonComponents/Tests/CommonComponentsTests.cs) has only 7 `Money` cases. A fix such as [CC15](#cc15-shuffle-is-biased) cannot be checked for unchanged output. The tests now run with `dotnet test` ([Engine-free modules](../architecture.md#engine-free-modules)), so golden-value tests need no Unity.

**Direction.** Add golden-value tests: a table of (seed, ID) → position, level, name, neighbour list, and region home star, recorded from the current build. Add round-trip tests for `IdToPosition`/`PositionToId` over the first 100 rings. Tests only record current output, so they need no approval. A test failure after a change means the change needs approval.

### CC-S5. It duplicates Utilites

**Severity:** Low.

**Problem.** [`Assets/ModulesShared/Utilites/`](../../Assets/ModulesShared/Utilites/) already has obscured numbers, observable collections, inventories, a seeded random generator, and stream readers. CommonComponents has its own version of each.

**Evidence.**

| CommonComponents | Utilites |
| --- | --- |
| `Money` (own XOR mask) | `ObscuredLong` |
| `ObservableCollection<T>` | `ObservableList`, `ObservableSet`, `ObservableMap` |
| `GameItemCollection<T>` | `SimpleInventory`, `SecureInventory`, `ObservableInventory` |
| `RandomExtension` on `System.Random` | `PcgRandom`, `RandomState` |
| `Helpers`, `StreamExtensions` | `ReaderStream`, `WriterStream`, `SessionDataReader` |

**Direction.** Pick one owner per concept for new code (Utilites for collections, random, and serialization). Do not add more helpers here.

### CC-S6. Unused code

**Severity:** Low.

**Problem.** A text search over `Assets/` finds no caller for: `FlexInt`, `BoundedSum`, `OddElements`, `IfEmptyThen`, `EnumerableExtension.Empty`, `AddOrUpdate`, `ReplaceAll`, `Next3`, `NextFloatNormal`, `ThreadSafe.Add`, `RotationHelpers.Equals`, `Geometry.VectorSphereIntersection`, `Geometry.ElasticCollision`, `AsSignedDecimal`, `BigFormat.Truncated`, `Helpers.DeserializeVector2`, `GameObjectExtensions.IsNull`, `ObservableCollection.AsReadOnly`, and the commented-out `GetAdjacentStars`.

**Evidence.** Dead code with defects ([CC11](#cc11-the-read-only-view-misses-datachangedevent), [CC17](#cc17-smaller-defects) item 7) looks usable to the next developer.

**Direction.** Mark only; the owner must review each member before anything is deleted (owner decision, 2026-10-12). The search included `ModulesPrivate`. Mods contain data, not C# code, so they cannot call these members.
