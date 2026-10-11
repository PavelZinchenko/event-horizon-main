# CommonComponents Module

CommonComponents is the leaf assembly that most game assemblies share. It holds small value types (resource IDs, money, prices, stat multipliers), the galaxy coordinate system (star and region IDs), the signal types used for events between services, and a set of general helpers: collections, random selection, binary serialization, geometry, mesh generation, and some legacy Unity code. It has no runtime state of its own and no Zenject installer. Other modules create and own every object it defines.

| | |
| --- | --- |
| Assembly | `CommonComponents` ([CommonComponents.asmdef](../Assets/Modules/CommonComponents/CommonComponents.asmdef)). References: Zenject, AppConfiguration (unused), Utilites. `autoReferenced: true`, so `Assets/Scripts` sees it without a reference. |
| Namespaces | No root namespace. Types use 13 namespaces plus the global namespace. See [Namespaces](#namespaces). |
| Location | [`Assets/Modules/CommonComponents/`](../Assets/Modules/CommonComponents/) (main repository, not a submodule). |
| Source | [`Scripts/`](../Assets/Modules/CommonComponents/Scripts/), 37 files, about 3,100 lines. |
| Assemblies that use it | BattleSimulator, Database, GalaxyMap, GameServices, GameSession, Quests, ResourceLocator, Services, ServicesFacade, ShipConstructor, Gui, Localization, ShipEditor, and the application (`Assets/Scripts`). |
| Tests | [`Tests/CommonComponentsTests.cs`](../Assets/Modules/CommonComponents/Tests/CommonComponentsTests.cs): 7 cases for `Money` add, subtract, and multiply. |
| Known issues | [CommonComponents issues](issues/common_components.md) |

## Contents

1. [Terms](#terms)
2. [Source map](#source-map)
3. [Assembly boundaries](#assembly-boundaries)
4. [Shared data types](#shared-data-types)
5. [Economy](#economy)
6. [Galaxy layout](#galaxy-layout)
7. [Signals](#signals)
8. [Weak events](#weak-events)
9. [Collections](#collections)
10. [Enumerable and random helpers](#enumerable-and-random-helpers)
11. [Value formatting](#value-formatting)
12. [Binary serialization](#binary-serialization)
13. [Storage contracts](#storage-contracts)
14. [Math helpers](#math-helpers)
15. [Mesh primitives](#mesh-primitives)
16. [Legacy code](#legacy-code)
17. [Determinism and save compatibility](#determinism-and-save-compatibility)
18. [Unused code](#unused-code)
19. [Where changes go](#where-changes-go)
20. [Validation checklist](#validation-checklist)

## Terms

| Term | Meaning |
| --- | --- |
| Signal | An object that other objects subscribe to. A signal has one public `Event`. Only its `Trigger` can fire it. |
| Trigger | The nested `Signal.Trigger` class. It holds the signal and has the `Fire` method. Publishers get the trigger, subscribers get the signal. |
| Strong signal | `Signal<...>`. It keeps every subscriber alive, like a normal C# event. |
| Weak signal | `SmartWeakSignal<...>`. It holds subscribers through weak references, so it does not keep them alive. |
| Weak reference | A reference that does not stop the garbage collector (GC) from freeing the target object. |
| Star ID | A non-negative integer that names one cell of the infinite galaxy grid. ID 0 is the cell at (0, 0). |
| Grid position | The integer cell `(x, y)` of a star. It is not the world position. |
| Star position | The world position of a star: the grid position on a hex layout plus a small seeded random offset. |
| Ring | All cells at the same Chebyshev distance `max(|x|, |y|)` from (0, 0). The ID spiral numbers one ring after the other. |
| Hex distance | The number of steps between two stars when each star has six neighbours. |
| Star level | `round(|star position|)`. Game code uses it as the difficulty of a star. |
| Region | A group of stars around a home star. Region IDs use the same spiral as star IDs, offset by one. |
| Obscured value | A number stored XOR-ed with a mask, so memory scanners do not find the plain value. `Money` and `ObscuredInt` (Utilites) do this. It is not encryption. |
| Saturating arithmetic | Arithmetic that stops at the limit (0 or `long.MaxValue`) instead of wrapping around. |

## Source map

All paths are under [`Assets/Modules/CommonComponents/Scripts/`](../Assets/Modules/CommonComponents/Scripts/).

| File | Types | Namespace | Purpose |
| --- | --- | --- | --- |
| [DataTypes/AudioClipId.cs](../Assets/Modules/CommonComponents/Scripts/DataTypes/AudioClipId.cs) | `AudioClipId` | `GameDatabase.Model` | Sound ID with a loop flag. |
| [DataTypes/SpriteId.cs](../Assets/Modules/CommonComponents/Scripts/DataTypes/SpriteId.cs) | `SpriteId`, `SpriteId.Type` | `GameDatabase.Model` | Sprite ID with a category. |
| [DataTypes/StatMultiplier.cs](../Assets/Modules/CommonComponents/Scripts/DataTypes/StatMultiplier.cs) | `StatMultiplier` | `GameDatabase.Model` | Stat bonus stored as "value − 1". |
| [Economy/Money.cs](../Assets/Modules/CommonComponents/Scripts/Economy/Money.cs) | `Money` | `CommonComponents` | Non-negative, saturating, obscured `long`. |
| [Economy/Price.cs](../Assets/Modules/CommonComponents/Scripts/Economy/Price.cs) | `Price` | `Economy` | `Money` plus `Currency`. |
| [Economy/Currency.cs](../Assets/Modules/CommonComponents/Scripts/Economy/Currency.cs) | `Currency` | `Economy` | Currency enum. |
| [Economy/ItemType/IItemType.cs](../Assets/Modules/CommonComponents/Scripts/Economy/ItemType/IItemType.cs) | `IItemType`, `ItemQuality`, `UnknownItem` | `Economy.ItemType` | Contract for anything that can be bought, sold, or rewarded. |
| [Galaxy/StarLayout.cs](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs) | `StarLayout` | `GameModel` | Star ID ↔ grid position, world position, level, rings, hex distance. |
| [Galaxy/RegionLayout.cs](../Assets/Modules/CommonComponents/Scripts/Galaxy/RegionLayout.cs) | `RegionLayout` | `GameModel` | Region ID ↔ grid position, home star. |
| [Signals/ISignal.cs](../Assets/Modules/CommonComponents/Scripts/Signals/ISignal.cs) | `ISignal`, `ITrigger` | `CommonComponents.Signals` | Marker interfaces. |
| [Signals/Signal.cs](../Assets/Modules/CommonComponents/Scripts/Signals/Signal.cs) | `Signal<TSignal[, T1[, T2]]>` | `CommonComponents.Signals` | Strong signals with 0–2 parameters. |
| [Signals/SmartWeakSignal.cs](../Assets/Modules/CommonComponents/Scripts/Signals/SmartWeakSignal.cs) | `SmartWeakSignal<TSignal[, T1[, T2]]>` | `CommonComponents.Signals` | Weak signals with 0–2 parameters. |
| [Signals/SignalExtensions.cs](../Assets/Modules/CommonComponents/Scripts/Signals/SignalExtensions.cs) | `SignalExtensions` | `CommonComponents.Signals` | `BindSignal`, `BindTrigger` for Zenject. |
| [Utils/SmartWeakEvent.cs](../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs) | `SmartWeakEvent<T>` | `SmartWeakEvent` | Weak event list (third-party code, MIT, Daniel Grunwald 2008, modified). |
| [Utils/ObservableCollection.cs](../Assets/Modules/CommonComponents/Scripts/Utils/ObservableCollection.cs) | `IItemCollection<T>`, `ObservableCollection<T>` | `CommonComponents.Utils` | List with change events. |
| [Utils/GameItemCollection.cs](../Assets/Modules/CommonComponents/Scripts/Utils/GameItemCollection.cs) | `IReadOnlyGameItemCollection<T>`, `IGameItemCollection<T>`, `GameItemCollection<T>` | `CommonComponents.Utils` | Item → quantity map with a dirty flag. |
| [Utils/EnumerableExtensions.cs](../Assets/Modules/CommonComponents/Scripts/Utils/EnumerableExtensions.cs) | `EnumerableExtension` | global | LINQ-like helpers, random selection, array helpers. |
| [Utils/RandomExtensions.cs](../Assets/Modules/CommonComponents/Scripts/Utils/RandomExtensions.cs) | `RandomExtension` | global | Extensions on `System.Random`. |
| [Utils/ValueFormatter.cs](../Assets/Modules/CommonComponents/Scripts/Utils/ValueFormatter.cs) | `ValueFormatter`, `DoubleFormatter`, `BigFormat` | `CommonComponents.Utils` | Number → display string. |
| [Utils/FlexInt.cs](../Assets/Modules/CommonComponents/Scripts/Utils/FlexInt.cs) | `FlexInt` | `CommonComponents.Utils` | Integer-like `double` wrapper. Unused. |
| [Utils/Lazy.cs](../Assets/Modules/CommonComponents/Scripts/Utils/Lazy.cs) | `Lazy<T>` | global | Lazy value. Shadows `System.Lazy<T>`. |
| [Utils/Generic.cs](../Assets/Modules/CommonComponents/Scripts/Utils/Generic.cs) | `Generic` | global | `Swap`. |
| [Serialization/Helpers.cs](../Assets/Modules/CommonComponents/Scripts/Serialization/Helpers.cs) | `Helpers` | `GameModel.Serialization` | Byte-level serialize/deserialize for the old save format. |
| [Serialization/StreamExtensions.cs](../Assets/Modules/CommonComponents/Scripts/Serialization/StreamExtensions.cs) | `StreamExtensions` | `CommonComponents.Serialization` | Read `int`, `uint`, string, byte array from a `Stream`. |
| [Serialization/Base64Encoder.cs](../Assets/Modules/CommonComponents/Scripts/Serialization/Base64Encoder.cs) | `Base64Encoder` | `Utils` | Base64 ↔ URL-safe Base64. |
| [Services/IGameData.cs](../Assets/Modules/CommonComponents/Scripts/Services/IGameData.cs) | `IGameData` | `Services.Storage` | Save metadata. |
| [Services/ISerializableGameData.cs](../Assets/Modules/CommonComponents/Scripts/Services/ISerializableGameData.cs) | `ISerializableGameData` | `Services.Storage` | Save metadata plus serialize/deserialize. |
| [Math/Geometry.cs](../Assets/Modules/CommonComponents/Scripts/Math/Geometry.cs) | `Geometry` | global | Intercept, segment distance, Bezier, collision. |
| [Math/RotationHelpers.cs](../Assets/Modules/CommonComponents/Scripts/Math/RotationHelpers.cs) | `RotationHelpers` | global | Cached direction table, angles, arcs. |
| [Math/VectorExtensions.cs](../Assets/Modules/CommonComponents/Scripts/Math/VectorExtensions.cs) | `VectorExtensions` | global | `Vector2` helpers. |
| [Math/ThreadSafe.cs](../Assets/Modules/CommonComponents/Scripts/Math/ThreadSafe.cs) | `ThreadSafe` | global | Lock-free `float` updates. |
| [Math/Primitives.cs](../Assets/Modules/CommonComponents/Scripts/Math/Primitives.cs) | `Primitives` | global | Procedural meshes. |
| [Legacy/BackgroundTask.cs](../Assets/Modules/CommonComponents/Scripts/Legacy/BackgroundTask.cs) | `BackgroundTask` | global | Worker thread base class. |
| [Legacy/GameObjectExtensions.cs](../Assets/Modules/CommonComponents/Scripts/Legacy/GameObjectExtensions.cs) | `GameObjectExtensions` | global | Mesh/material setup and cleanup. |
| [Legacy/NameGenerator.cs](../Assets/Modules/CommonComponents/Scripts/Legacy/NameGenerator.cs) | `NameGenerator` | `Model.Generators` | Star and planet names. |
| [Legacy/WeakReference.cs](../Assets/Modules/CommonComponents/Scripts/Legacy/WeakReference.cs) | `WeakReference<T>` | global | Typed weak reference. Shadows `System.WeakReference<T>`. |

## Assembly boundaries

### References

The module uses Zenject only in [SignalExtensions.cs](../Assets/Modules/CommonComponents/Scripts/Signals/SignalExtensions.cs). It uses Utilites only for `ObscuredInt` in `GameItemCollection`. No file uses AppConfiguration. Many files use `UnityEngine` (`Mathf`, `Vector2`, `Color`, `Mesh`, `Random`, `Debug`), so the assembly cannot run outside Unity.

### Namespaces

| Namespace | Types | Note |
| --- | --- | --- |
| global | `EnumerableExtension`, `RandomExtension`, `Lazy<T>`, `Generic`, `Geometry`, `RotationHelpers`, `VectorExtensions`, `ThreadSafe`, `Primitives`, `BackgroundTask`, `GameObjectExtensions`, `WeakReference<T>` | Visible everywhere without `using`. |
| `GameDatabase.Model` | `AudioClipId`, `SpriteId`, `StatMultiplier` | Same namespace as generated Database types. Readers look for them in the Database module. |
| `CommonComponents` | `Money` | |
| `Economy`, `Economy.ItemType` | `Price`, `Currency`, `IItemType`, `ItemQuality`, `UnknownItem` | The application adds more types to `Economy.ItemType`. |
| `GameModel`, `GameModel.Serialization` | `StarLayout`, `RegionLayout`, `Helpers` | |
| `CommonComponents.Signals`, `.Utils`, `.Serialization` | Signals, collections, formatters, `StreamExtensions` | |
| `Services.Storage` | `IGameData`, `ISerializableGameData` | Same namespace as storage services in other assemblies. |
| `Utils` | `Base64Encoder` | |
| `Model.Generators` | `NameGenerator` | |
| `SmartWeakEvent` | `SmartWeakEvent<T>` | Namespace and class have the same name. |

**Global types shadow framework types.** C# looks for a name in the global namespace before it looks in namespaces imported with `using` at the top of a file. So in every assembly that references CommonComponents, `Lazy<T>` and `WeakReference<T>` mean the CommonComponents types, even in a file with `using System;`. The compiler gives no warning. [BulletFactory.cs:45](../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L45) uses the custom `Lazy<T>`. `UnityEngineThread`, `Console`, and `SharedResources` use the custom `WeakReference<T>`. See [CC-S1](issues/common_components.md#cc-s1-global-types-shadow-framework-types).

## Shared data types

### AudioClipId

[`AudioClipId(string name)`](../Assets/Modules/CommonComponents/Scripts/DataTypes/AudioClipId.cs#L7) removes the first `*` from the name and sets `Loop = true` if it found one. So `"*engine"` becomes ID `"engine"` with looping. The `*` can be anywhere in the string, not only at the start. `None` is the default value (null ID). The implicit `bool` conversion is true when the ID is not empty. It does not check that the clip exists.

### SpriteId

[`SpriteId`](../Assets/Modules/CommonComponents/Scripts/DataTypes/SpriteId.cs) holds a string ID and a [`Type`](../Assets/Modules/CommonComponents/Scripts/DataTypes/SpriteId.cs#L5) category (`Ship`, `Component`, `GuiIcon`, `Ammunition`, …). The resource loader uses the category to choose a folder. `Empty` is the default value. The implicit `bool` is true when the ID is not empty. The struct has no equality operators. `Equals` uses the slow reflection-based `ValueType.Equals`.

### StatMultiplier

[`StatMultiplier`](../Assets/Modules/CommonComponents/Scripts/DataTypes/StatMultiplier.cs) stores the bonus, that is the multiplier minus one. The default value is a multiplier of 1 (no change).

| Expression | Stored bonus | `Value` | `ToString()` |
| --- | --- | --- | --- |
| `new StatMultiplier(0.25f)` | 0.25 | 1.25 | `+25%` |
| `StatMultiplier.FromValue(0.25f)` | −0.75 | 0.25 | `-75%` |
| `a + b` (both +25%) | 0.5 | 1.5 | `+50%` |
| `a * b` (both +25%) | 0.5625 | 1.5625 | `+56%` |
| `a + 0.1f` | 0.35 | 1.35 | |
| `a * 2f` | 1.5 | 2.5 | |
| `a.AmplifyDelta(2f)` | 0.5 | 1.5 | |

1. `+` adds bonuses. Use it to stack bonuses from several sources.
2. `*` multiplies the full values (`Value × Value − 1`). Use it to apply one multiplier after another.
3. `* float` also multiplies the full value. `AmplifyDelta` multiplies only the bonus.
4. `HasValue` is false when the bonus is approximately zero (`Mathf.Approximately`).

ShipConstructor uses `StatMultiplier` for ship stats and BattleSimulator uses it for devices and drone bays.

## Economy

### Money

[`Money`](../Assets/Modules/CommonComponents/Scripts/Economy/Money.cs) is a `readonly struct` around one `long`. Its value is never negative.

**Storage.** The field `_amount` holds `value XOR _mask`. [`_mask`](../Assets/Modules/CommonComponents/Scripts/Economy/Money.cs#L8) is `DateTime.UtcNow.Ticks` when the type loads, so it changes every run. This hides the value from memory scanners. Never persist `_amount`. Persist `(long)money`.

**Conversions.**

| Conversion | Kind | Result |
| --- | --- | --- |
| `int`, `long` → `Money` | implicit | Negative values become 0. |
| `double` → `Money` | implicit | `Math.Ceiling`, then clamp to [0, `long.MaxValue`]. 0.1 becomes 1. |
| `Money` → `long` | explicit | The plain value. |
| `Money` → `int` | explicit | Clamped to [0, `int.MaxValue`]. |

**Operators.**

| Operator | Behavior |
| --- | --- |
| `+` | Saturates at `long.MaxValue`. |
| `-` | Clamps at 0 (the constructor clamps a negative result). |
| `*` (`Money`, `long`) | Returns 0 if a factor is ≤ 0. Saturates at `long.MaxValue` (`checked` multiply, catch overflow). |
| `* double` | Multiplies as `double`, then rounds up (`Math.Ceiling`). Float error can add 1: `100 * 1.1` gives 111. See [CC1](issues/common_components.md#cc1-money--double-rounds-float-error-up). Design rule: a non-zero amount times a positive multiplier must never become 0; rounding up is not required. |
| `/` | Integer division. Division by zero throws `DivideByZeroException`. |
| `==`, `<`, … | Compare plain values. |

`ToString()` uses [`AsMoney`](#value-formatting) with 9 digits: `1,234,567 K` for 1,234,567,890. `GetHashCode` returns the obscured field, which is stable inside one run.

### Price

[`Price`](../Assets/Modules/CommonComponents/Scripts/Economy/Price.cs) pairs `Money` with a [`Currency`](../Assets/Modules/CommonComponents/Scripts/Economy/Currency.cs).

| Factory | Currency |
| --- | --- |
| `Price.Common(m)` | `Credits` |
| `Price.Premium(m)` | `Stars`. With the `IAP_DISABLED` define: `Credits`, amount × 500 ([Price.cs:19](../Assets/Modules/CommonComponents/Scripts/Economy/Price.cs#L19)). |
| `Price.Tokens(m)` | `Tokens` |

1. `* int` and `* float` scale the amount and keep the currency. `* float` uses `Money * double`, so it rounds up.
2. `/ int` returns 0 for a zero price. Otherwise it divides and returns at least 1. Division by 0 throws.
3. `Min(m)` raises the amount to at least `m`. `Max(m)` lowers it to at most `m`. The names describe the bound, not the result.
4. `ToString()` prints only the amount, not the currency.
5. `Price` has no equality operators.

`Currency` values: `None`, `Credits`, `Stars`, `Money` (real money, used by in-app purchase items), `Tokens`, `Snowflakes`. Database and GameSession do not reference the enum, so no saved data depends on its numbers today.

### IItemType

[`IItemType`](../Assets/Modules/CommonComponents/Scripts/Economy/ItemType/IItemType.cs#L15) is the contract for anything the player can buy, sell, receive, or spend: ID, name, description, `SpriteId` icon, color, `Price`, `ItemQuality`, `Consume(amount)`, `Withdraw(amount)`, and the maximum amounts for both. The application implements it (`Assets/Scripts/Domain/Economy/ItemType/`). [`UnknownItem`](../Assets/Modules/CommonComponents/Scripts/Economy/ItemType/IItemType.cs#L31) is a null object: name `???`, zero price, no-op actions.

## Galaxy layout

The galaxy is an infinite grid of cells. Each cell has a star. [`StarLayout`](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs) converts between star IDs and grid cells and computes world positions. Every star property that the session does not store (position, level, name, adjacency) comes from these functions. See [Determinism and save compatibility](#determinism-and-save-compatibility).

### Star ID spiral

[`PositionToId`](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L9) and [`IdToPosition`](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L27) number cells ring by ring. Ring `d` holds the cells with `max(|x|, |y|) = d`. It is the border of a square with side `L = 2d + 1`. It has `8d` cells and uses IDs `(L−2)²` to `L² − 1`. Inside a ring, IDs count down from `L² − 1` in this order:

1. The bottom row (`y = −d`), from left to right.
2. The top row (`y = d`), from left to right.
3. The side cells, from bottom to top, alternating left (even offset) and right (odd offset).

The IDs near the origin (verified with a script):

```
y= 2   19 18 17 16 15
y= 1   10  5  4  3  9
y= 0   12  2  0  1 11
y=-1   14  8  7  6 13
y=-2   24 23 22 21 20
      x=-2 -1  0  1  2
```

The mapping is a bijection: every cell has exactly one ID and every non-negative ID has exactly one cell. `IdToPosition` finds the ring with `floor(sqrt(id)) + 1`, made odd. Negative IDs are not valid and give garbage. `int` overflow starts at about `|x| > 23,170`, far outside any played galaxy.

### Star position and level

[`GetStarPosition(id, seed)`](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L52) places the grid on a hex layout:

1. Column step `DistanceX = 1`. Row step `DistanceY = √3 / 2`.
2. Odd rows (`y % 2 != 0`, including negative odd rows) shift right by 0.5.
3. [`GetStarOffset`](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L161) adds a random offset inside a circle of radius 0.3. It calls `UnityEngine.Random.InitState(seed + id)` and reads `Random.insideUnitCircle`.

Step 3 changes the global Unity random state on every call. After any call, the next `UnityEngine.Random` values depend only on `seed + id`. See [CC3](issues/common_components.md#cc3-getstarposition-reseeds-the-global-unity-random-generator).

[`GetStarLevel(id, seed)`](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L60) is `round(|GetStarPosition(id, seed)|)`. Enemy strength, quest levels, and region levels use it.

### Rings of neighbours

[`GetAdjacentStars(id, distance = 1)`](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L99) returns the `6 × distance` stars at exactly `distance` hex steps from the star. It handles even and odd rows with the `offset` variable. The local variable `odd` is true when `distance` is even, so the name is misleading. The order of the result is fixed and callers depend on it: quest code picks a star with `RandomElement(random)` from this sequence. With `distance = 0`, the method returns the center star four times.

Callers: [StarMap.GetNearestVisited](../Assets/Scripts/Domain/Galaxy/StarMap.cs#L58), [StarMapManager.ExploreAdjacentStars](../Assets/Scripts/Domain/Player/StarMapManager.cs#L80) (scanner range), [QuestGiver](../Assets/Modules/Quests/Scripts/QuestGiver/QuestGiver.cs#L40), [StarMapDataProvider](../Assets/Scripts/Domain/Quests/StarMapDataProvider.cs#L54), and the legacy `StarBorder`.

### Hex distance

[`Distance(star1, star2)`](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L142) converts both IDs to cells and calls [`Distance(x0, y0, x1, y1)`](../Assets/Modules/CommonComponents/Scripts/Galaxy/StarLayout.cs#L149). The formula uses the row parity of the first star, so it agrees with the hex layout of `GetStarPosition` and with `GetAdjacentStars`. Here too, `odd` means "the row is even".

### Regions

[`RegionLayout`](../Assets/Modules/CommonComponents/Scripts/Galaxy/RegionLayout.cs) reuses the spiral for regions:

1. `PositionToId(x, y) = 1 + StarLayout.PositionToId(x, y)`. So region IDs start at 1, and ID 0 means "no region".
2. `IdToPosition(id)` returns (0, 0) for `id ≤ 0`. Region 0 and region 1 both map to (0, 0).
3. [`GetRegionHomeStar(regionId)`](../Assets/Modules/CommonComponents/Scripts/Galaxy/RegionLayout.cs#L27) multiplies the region cell by (6, 4) (`3 × RegionFourthSize`, `2 × RegionFourthSize`, with `RegionFourthSize = 2`) and returns that star ID. Negative regions return star 0.

The application's `RegionMap` and the GameSession v1 loader use these functions.

## Signals

A signal is a class that a module declares once:

```csharp
// BattleSimulator: AiManager.cs
public class CeasefireSignal : Signal<CeasefireSignal, bool> { }

// GameSession: Signals.cs
public class MoneyValueChangedSignal : SmartWeakSignal<MoneyValueChangedSignal, long> { }
```

The self-referencing type parameter (`TSignal`) lets the nested `Trigger` class reach the private event of its own signal type.

### How it works

1. The signal has a private event and a public `Event` with `add` and `remove` ([Signal.cs:10](../Assets/Modules/CommonComponents/Scripts/Signals/Signal.cs#L10)). Anyone with the signal can subscribe. Nobody outside can raise it.
2. The nested `Trigger` takes the signal in its constructor and has `Fire(...)` ([Signal.cs:16](../Assets/Modules/CommonComponents/Scripts/Signals/Signal.cs#L16)).
3. Zenject creates the trigger and injects the signal into it.

### Binding

[`SignalExtensions`](../Assets/Modules/CommonComponents/Scripts/Signals/SignalExtensions.cs) has three methods:

| Method | Binding |
| --- | --- |
| `BindSignal<TSignal>()` | `Bind<TSignal>().AsSingle()` |
| `BindSignal<TSignal>(instance)` | `Bind<TSignal>().FromInstance(instance)`. Used to share one signal object between containers ([ShipEditorState.cs:54](../Assets/Scripts/GameStateMachine/States/ShipEditorState.cs#L54)). |
| `BindTrigger<TTrigger>()` | `Bind<TTrigger>().AsSingle()` |

The project calls these methods about 170 times. Most calls are in [GameInstaller](../Assets/Scripts/Installers/GameInstaller.cs). The trigger resolves the signal from its own container or a parent container. If the signal and the trigger are bound in different containers, a publisher and a subscriber can get different signal objects and the event never arrives.

### Strong and weak signals

| | `Signal<...>` | `SmartWeakSignal<...>` |
| --- | --- | --- |
| Storage | Normal multicast delegate | [`SmartWeakEvent<T>`](#weak-events) |
| Keeps subscribers alive | Yes | No |
| Lambda subscribers | Any | Only lambdas that capture nothing but `this` |
| A handler throws | The exception goes to the caller of `Fire`. Later handlers do not run. | The exception is logged. Later handlers run. |
| Speed | Direct call | Reflection call (`MethodInfo.Invoke`) per handler |
| Count in project | 6 types | 73 types |

Rules for subscribers:

1. Unsubscribe from a strong signal in `Dispose` or `OnDestroy`. Otherwise the signal keeps the subscriber alive.
2. Also unsubscribe from a weak signal when a Unity object is destroyed. A destroyed `MonoBehaviour` stays in memory until the GC runs, and the weak signal calls it until then.
3. Do not subscribe an object to a weak signal if nothing else holds the object. The GC frees it and it stops receiving events without an error.

## Weak events

[`SmartWeakEvent<T>`](../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs) is a modified version of Daniel Grunwald's 2008 "smart weak event" (MIT license, header kept).

1. The static constructor checks that `T` is a delegate type with a `void` return type. Otherwise the first use throws `TypeInitializationException`.
2. [`Add`](../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs#L67) stores the handler's `MethodInfo` and a `WeakReference` to its target (null for static methods). It throws `ArgumentException` when the method's declaring type is compiler-generated. Before it adds, it removes dead entries if the list is full.
3. [`Remove`](../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs#L92) walks the list from the end and removes the last matching entry. It also drops dead entries that it passes.
4. [`Raise`](../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs#L116) copies the list, calls each live handler with `MethodInfo.Invoke`, and removes dead entries at the end. Handlers added during `Raise` run from the next `Raise`.
5. [`TryInvokeMethod`](../Assets/Modules/CommonComponents/Scripts/Utils/SmartWeakEvent.cs#L135) catches every exception and logs it with `Debug.LogException`.

**What the closure check rejects.** The C# compiler puts every lambda that does not use `this` in a compiler-generated class: lambdas that capture local variables and also lambdas that capture nothing. Both throw. A lambda that uses only `this` and fields becomes a method of the subscriber class, so it passes. A named instance or static method always passes. See [CC6](issues/common_components.md#cc6-the-weak-event-rejects-lambdas-that-capture-nothing).

The class is not thread-safe. Call `Add`, `Remove`, and `Raise` on one thread.

## Collections

### ObservableCollection

[`ObservableCollection<T>`](../Assets/Modules/CommonComponents/Scripts/Utils/ObservableCollection.cs) wraps a `List<T>` and implements [`IItemCollection<T>`](../Assets/Modules/CommonComponents/Scripts/Utils/ObservableCollection.cs#L8) (`IList<T>` plus `Assign` and `AsReadOnly`). It has the same name as `System.Collections.ObjectModel.ObservableCollection<T>`. A file that imports both namespaces does not compile.

| Operation | Events, in order |
| --- | --- |
| `Add`, `Insert` | `ItemAddedEvent(item)`, `DataChangedEvent` |
| `Remove` (found), `RemoveAt` | `ItemRemovedEvent(item)`, `DataChangedEvent` |
| `this[i] = value` | `ItemRemovedEvent(old)`, `DataChangedEvent`, `ItemAddedEvent(value)`, `DataChangedEvent` |
| `Clear`, `Assign` | `EntireCollectionChangedEvent`, `DataChangedEvent` (also when the list was already empty) |

1. A collection created with `readOnly: true` throws `InvalidOperationException` on every change.
2. [`AsReadOnly()`](../Assets/Modules/CommonComponents/Scripts/Utils/ObservableCollection.cs#L137) returns one cached read-only view that shares the list. The writable collection forwards events to the view. Nothing calls `AsReadOnly()` today.
3. The events are normal (strong) C# events.

Users: ships and satellites in ShipConstructor (`Components`, `Modifications`), `PlayerFleet`, and the obsolete `FleetData`.

### GameItemCollection

[`GameItemCollection<T>`](../Assets/Modules/CommonComponents/Scripts/Utils/GameItemCollection.cs) maps items to quantities. It stores quantities as `ObscuredInt`.

| Member | Behavior |
| --- | --- |
| `Add(item, amount = 1)` | Adds `amount` to the quantity. A missing item starts at 0. |
| `Remove(item, amount = 1)` | Subtracts. Removes the entry at 0 or below. Returns the amount really removed. Returns 0 for a missing item. |
| `this[item]` get | Quantity, or 0. |
| `this[item]` set | Stores the value as is, including 0 or a negative value. |
| `Assign(...)` | Clears, then copies. A duplicate key throws after the clear. |
| `Clear()` | Clears. Does nothing if already empty. |
| `IsDirty` | Every change sets it to true. Setting it to true fires `CollectionChangedEvent`. The owner sets it to false after it saves. |

The collection does not enforce positive quantities. See [CC12](issues/common_components.md#cc12-gameitemcollection-allows-zero-and-negative-quantities). [PlayerInventory](../Assets/Scripts/Domain/Player/PlayerInventory.cs) uses it for components and satellites and copies dirty collections to the session.

## Enumerable and random helpers

[`EnumerableExtension`](../Assets/Modules/CommonComponents/Scripts/Utils/EnumerableExtensions.cs) and [`RandomExtension`](../Assets/Modules/CommonComponents/Scripts/Utils/RandomExtensions.cs) are in the global namespace. Every file in every dependent assembly sees them.

### Random selection

All helpers take a `System.Random`. Pass a seeded instance when the result goes into a save or a seeded generator.

| Method | Behavior |
| --- | --- |
| `RandomElement(IList)` | One element, or `default` for an empty list. |
| `RandomElement(IEnumerable)` | Same, but enumerates twice (`Count()` then `ElementAt`). |
| `RandomUniqueElements(count[, size])` | Selection sampling: each element is kept with probability `itemsLeft / remaining`. Keeps order. Exact when `size` is the true count. |
| `RandomElements(count[, size])` | Elements may repeat. For each element it makes `itemsLeft` tries with probability `1 / remaining`. The last element takes all unused picks. |
| `RandomUniqueNumbers(min, max, count)` | Selection sampling over the integer range. |
| `Shuffle(IList)` | In-place swap of each position with a random position. This is not Fisher–Yates and some orders are more likely. See [CC15](issues/common_components.md#cc15-shuffle-is-biased). |
| `random.Next(float min, float max)` | `min + (max − min) × Next(1000) / 1000`. 1,000 steps. Never returns `max`. |
| `random.Range(min, max)` | Inclusive int range. Swaps reversed bounds. |
| `random.SquareRange(min, max)` | `Next(min, Next(min, max + 1) + 1)`: biased toward `min`. |
| `Next2(v)`, `Next3(v)` | Nested `Next` calls: biased toward 0. |
| `Percentage(int)` / `Percentage(float)` | `true` with probability `value`% / `value`. Out-of-range values give `false` or `true`. |
| `NextFloat`, `NextFloatSigned` | [0, 1) and [−1, 1). |
| `NextFloatNormal` | The product of two uniform values in [−1, 1]. It is not a normal distribution. Unused. |

### Other helpers

| Method | Note |
| --- | --- |
| `QuickRemove(IList, index)` | Moves the last element into `index`, then removes the last. O(1), does not keep order. Ignores an invalid index. |
| `T[].Add`, `AddIfNotExists` | Return a new array. A null array becomes a one-element array. |
| `T[].RemoveAll(item)` | Returns a new array without `item` (reference equality). It also overwrites the input array. See [CC10](issues/common_components.md#cc10-array-removeall-overwrites-its-input). |
| `SetValue(IList, value, index)` | Grows the list with `default` values up to `index`. |
| `Concat(item)`, `item.Concat(seq)` | Append or prepend one item. |
| `MinValue(selector)` | Element with the smallest key, or `default` for an empty sequence. |
| `Repeat(n)` | Cycles the sequence until it gives `n` items. Stops if the sequence is empty. |
| `FindIndex`, `IndexOf` | For `IList<T>` and `IReadOnlyList<T>`. `List<T>` instance methods win over these. |
| `ToEnumerable()`, `NotNull()` | One-item sequence; filter nulls. |
| `Generic.Swap(ref a, ref b)` | Swap two variables. |

### Lazy and FlexInt

[`Lazy<T>`](../Assets/Modules/CommonComponents/Scripts/Utils/Lazy.cs) runs the initializer on the first `Value` read and caches the result. It is not thread-safe. If the initializer throws, the next read runs it again. `Reset()` clears the cache. It converts implicitly to `T`. It differs from `System.Lazy<T>` in every one of these points and it shadows it ([CC-S1](issues/common_components.md#cc-s1-global-types-shadow-framework-types)).

[`FlexInt`](../Assets/Modules/CommonComponents/Scripts/Utils/FlexInt.cs) stores a `double`. Only the conversion from `double` rounds. Arithmetic results keep fractions. Nothing uses it.

## Value formatting

[`ValueFormatter`](../Assets/Modules/CommonComponents/Scripts/Utils/ValueFormatter.cs) and `DoubleFormatter` make display strings. Do not use them for saved data.

| Method | Example input | Output | Culture |
| --- | --- | --- | --- |
| `AsPercentage(float)` | 0.25 | `25%` | — |
| `AsPercentageChange(float)` | 1.25 / 0.8 | `+25%` / `-20%` | — |
| `AsDecimal(float, digits)` | 1234.5 | `1,234.50` (`N` format) | Current culture |
| `AsMoney(long, maxDigits = 9)` | 1,234,567,890 | `1,234,567 K` | Invariant |
| `AsInteger(float)` | 1,234,567 | `1.23M` (`BigFormat.Decimal`) | Invariant |
| `ToInGameString(Expanded)` | 1,234,567 | `1234K` | Invariant |
| `ToInGameString(Truncated)` | 1,234,567 | `1M` | Invariant |

1. `AsMoney` drops groups of three digits until the number fits in `maxDigits`, then adds `K`, `M`, `B`, or `e12`, `e15`.
2. `BigFormat.Decimal` keeps three significant digits above 100,000. Below 100,000 it prints the integer part.
3. `BigFormat.Expanded` prints the integer below 100,000. Above, it keeps 4–6 digits and adds a suffix.
4. The suffix table ([GetDigitsSuffix](../Assets/Modules/CommonComponents/Scripts/Utils/ValueFormatter.cs#L116)) is hard-coded English and is not localized.
5. `Truncated` and `Expanded` use `Math.Floor`, so negative values round away from zero ([CC17](issues/common_components.md#cc17-smaller-defects)).

## Binary serialization

### Helpers (old save format)

[`GameModel.Serialization.Helpers`](../Assets/Modules/CommonComponents/Scripts/Serialization/Helpers.cs) writes values as `IEnumerable<byte>` and reads them from a `byte[]` with a `ref int index` cursor. The obsolete GameSession content classes (`Obsolete/Content/*Data.cs`), `PlayerPrefsStorage`, `LocalStorage`, `CloudDataAdapter`, `IntegratedComponent`, and `FleetWebSerializer` use it.

| Type | Bytes |
| --- | --- |
| `int`, `short` | Little-endian, written with explicit shifts. |
| `long`, `ulong`, `float` | `BitConverter.GetBytes`: machine byte order (little-endian on all supported platforms). |
| `Vector2` | Two floats. |
| `string` | `int` byte length + UTF-8 bytes. Null and empty both write length 0 and read back as `""`. |
| `byte[]` | `int` length + bytes. Null and empty write length 0. |
| `IEnumerable<int/long/string/byte[]>` | `int` count + items. Null writes count 0. The sequence is enumerated twice. |
| `HashSet<int>`, `Dictionary<int,int>`, `Dictionary<long,int>`, `IGameItemCollection<int>` | `int` count + entries in hash order. Two saves of the same data can differ in byte order. |

The readers do not check lengths. A negative or very large length from a damaged file causes `ArgumentOutOfRangeException`, `IndexOutOfRangeException`, or `OutOfMemoryException`. A duplicate dictionary key throws. The caller must catch these ([CC14](issues/common_components.md#cc14-binary-readers-trust-lengths)). The current GameSession format uses its own generated serializer (see [GameSession module](game_session.md)). These helpers remain for the migration from old saves and for the storage envelopes.

### StreamExtensions

[`StreamExtensions`](../Assets/Modules/CommonComponents/Scripts/Serialization/StreamExtensions.cs) reads from a `Stream`: `ReadInt32`, `ReadUInt32`, `ReadString` (`int` length + UTF-8), `ReadByteArray` (`int` length + bytes). The only user is the mod loader, [FileDatabaseStorage](../Assets/Modules/Database/Scripts/Storage/FileDatabaseStorage.cs#L51). It reads mod content through a decompression stream.

1. `ReadInt32` and `ReadUInt32` do not check how many bytes `Read` returned. At end of stream they return 0 or a partial value.
2. `ReadString` and `ReadByteArray` return an empty value when the stream has fewer bytes than the length. They do not throw.
3. `ReadString` uses `stackalloc` below 255 bytes. A negative length throws `OverflowException`.

So a truncated mod gives empty keys and empty data instead of an error ([CC13](issues/common_components.md#cc13-stream-readers-hide-truncated-data)).

### Base64Encoder

[`Base64Encoder`](../Assets/Modules/CommonComponents/Scripts/Serialization/Base64Encoder.cs) converts standard Base64 to URL-safe Base64 (`+` → `-`, `/` → `_`, no `=` padding) and back. It does not encode or decode bytes. The caller uses `Convert.ToBase64String` and `Convert.FromBase64String`. The only user is the private `WebStorage` for in-app purchase data.

## Storage contracts

[`IGameData`](../Assets/Modules/CommonComponents/Scripts/Services/IGameData.cs) is the metadata that storage services need without knowing the session model: `GameId`, `TimePlayed`, `DataVersion`, `ModId`. [`ISerializableGameData`](../Assets/Modules/CommonComponents/Scripts/Services/ISerializableGameData.cs) adds `Serialize()` and `TryDeserialize(gameId, timePlayed, dataVersion, modId, data, startIndex)`. GameSession implements both. Local, PlayerPrefs, and cloud storage consume them. A change to these interfaces changes every storage implementation, including those in `ModulesPrivate`.

## Math helpers

### Geometry

[`Geometry`](../Assets/Modules/CommonComponents/Scripts/Math/Geometry.cs) is used mostly by BattleSimulator AI and controllers.

| Method | Behavior |
| --- | --- |
| `GetTargetPosition(enemyPos, enemyVel, gunPos, bulletSpeed, out pos, out time)` | Solves `|p0 + v0·t − p| = v·t` for the smallest `t ≥ 0`. Returns false when there is no solution. |
| `GetTargetPosition(..., enemyRadius, ...)` | Same, but treats the enemy as a circle. Returns `time = 0` when the gun is inside the enemy radius. Then shortens `time` by `radius / distance`, but does not move `pos`. |
| `Point2VectorDistance(point, vector)` | Distance from `point` to the segment from the origin to `vector`. |
| `VectorSphereIntersection(vector, center, radius)` | First point where the segment from the origin enters the circle. `Vector2.zero` means "no hit" and also "hit at the origin". Unused. |
| `Bezier`, `BezierTangent` | Quadratic Bezier point and derivative. |
| `ElasticCollision(...)` | Returns velocity changes (not impulses) along the line between centers, only for approaching bodies. Unused. |
| `IsCounterClockwise(p1, p2, p3)` | Sign of the cross product. |

When the target speed equals the bullet speed, the quadratic term is 0 and the solver divides by zero ([CC17](issues/common_components.md#cc17-smaller-defects)).

### RotationHelpers

[`RotationHelpers`](../Assets/Modules/CommonComponents/Scripts/Math/RotationHelpers.cs) is used in 78 files, mostly in combat.

1. [`Direction(angle)`](../Assets/Modules/CommonComponents/Scripts/Math/RotationHelpers.cs#L19) returns a `ref readonly` unit vector from a table of 1,801 entries (0.2° steps), built in the static constructor. It avoids `Sin`/`Cos` per call. A NaN angle gives an invalid index and throws `IndexOutOfRangeException`.
2. `Transform(point, angle)` rotates a point with the same table.
3. `Angle(direction)` returns degrees in [0, 360).
4. `IsRotationInArc(rotation, min, max)` normalizes all three angles to [0, 360). When `min > max` after normalization, the arc crosses 0°. When `min == max`, it returns true for every rotation ([CC17](issues/common_components.md#cc17-smaller-defects)).
5. `BoundingRect(width, height, rotation)` returns the size of the axis-aligned box around a rotated rectangle.

### ThreadSafe

[`ThreadSafe`](../Assets/Modules/CommonComponents/Scripts/Math/ThreadSafe.cs) updates a `float` field with a compare-and-swap loop (`Interlocked.CompareExchange`). The AI thread and the main thread both change energy and hit points through it ([IResourcePoints.cs](../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/HitPoints/IResourcePoints.cs#L69)).

| Method | Behavior |
| --- | --- |
| `Add(ref target, value)` | Atomic add. Unused. |
| `AddClamp(ref target, value, min, max)` | Atomic add with clamp. Returns the change that was applied. |
| `ChangeValue(ref target, func)` | Runs `func` on a copy and writes the result atomically. Returns false if `func` returns false. `func` can run more than once. |

The loops compare the old value with `==`. If the field is NaN, `NaN == NaN` is false and the loop never ends ([CC4](issues/common_components.md#cc4-threadsafe-loops-never-end-on-nan)).

### VectorExtensions

`Direction`, `Distance`, `SqrDistance` between two `Vector2`, and `Lerp(from, to, delta, minStep)`. This `Lerp` moves at least `minStep` world units per call and snaps to `to` when closer than `minStep`.

## Mesh primitives

[`Primitives`](../Assets/Modules/CommonComponents/Scripts/Math/Primitives.cs) fills an existing `Mesh`. Each method calls `mesh.Clear(false)` first.

| Method | Shape | Note |
| --- | --- | --- |
| `CreateRectangle(mesh, w, h, textureScale)` | Centered quad | UVs extend past [0, 1] when `textureScale < 1`. No normals. |
| `CreatePlane(mesh, w, h, n)` | `n × n` quads | 4 vertices per quad (not shared). Normals recalculated. |
| `CreateSphere(mesh, radius, size, freq, power)` | Cube-sphere with Perlin noise | Six faces of `(size+1)²` vertices. The noise offset comes from `UnityEngine.Random.Range`, so the shape depends on the global Unity random state. The noise input uses `radius²`. |
| `CreateLine(mesh, length, thickness, border)` | Strip of 3 parts | The UV `u` uses world units for the border. |
| `CreateCircle(mesh, radius, thickness, segments, aspect)` | Ring | Vertices are on the unit circle. `radius` only sets the inner ratio (`1 − thickness / radius`) and the UV repeat. The caller scales the transform. |

Users: asteroid and wave views in combat, the planet background, and the legacy map objects.

## Legacy code

### BackgroundTask

[`BackgroundTask`](../Assets/Modules/CommonComponents/Scripts/Legacy/BackgroundTask.cs) is the base class of [AiManager](../Assets/Modules/BattleSimulator/Scripts/Combat/AI/AiManager.cs), the only subclass.

1. `StartTask(owner)` starts one new thread. A second call does nothing, also after `StopTask`.
2. The thread loops while the task is not stopped and the owner (weak reference) is alive. Each pass calls `DoWork()`. If `DoWork` returns false, it calls `OnIdle()` (AiManager sleeps one physics step there).
3. Exceptions from `DoWork` are logged, and the loop continues.
4. `Paused = true` makes the thread sleep in 100 ms steps. During the pause it does not check `StopTask`. Nothing sets `Paused` today.
5. The thread is a foreground thread. It is not marked `IsBackground`.
6. On WebGL the class is empty: `StartTask` does nothing. AiManager calls `DoWork()` itself from `FixedTick` there.

`DoWork` runs on a worker thread. It must not call Unity APIs that require the main thread.

### GameObjectExtensions

[`GameObjectExtensions`](../Assets/Modules/CommonComponents/Scripts/Legacy/GameObjectExtensions.cs) sets up meshes and materials for the legacy map objects and the ship editor layout.

| Method | Behavior |
| --- | --- |
| `CreateMeshRenderer`, `CreateMeshFilter` | Get or add the component. |
| `GetMesh` | `MeshFilter.mesh`: creates a mesh instance if needed. |
| `CreateDefaultMaterial(texture, color)` | Loads `Resources/Materials/Default` once, destroys the renderer's current material, assigns the default material, sets color and texture. |
| `CreateMaterial(base, texture, color)` | Same with a new copy of `base`. Returns the copy. |
| `Cleanup` | Destroys the renderer's materials with `DestroyImmediate` (after `renderer.materials` has cloned any shared ones). The mesh branch destroys only a new empty mesh and never the assigned mesh ([CC9](issues/common_components.md#cc9-cleanup-has-a-mesh-branch-that-never-helps)). Only `Star.OnDestroy` calls it. |
| `Move(position)` | Sets local x/y and keeps z. |
| `IsNull` | Tests `GetHashCode() == 0`. This does not detect destroyed objects. Unused. |

### NameGenerator

[`NameGenerator.GetStarName(id)`](../Assets/Modules/CommonComponents/Scripts/Legacy/NameGenerator.cs#L10) seeds `System.Random` with the star ID and builds names like `K7-QX9A` (1–4 characters, a dash, 1–6 characters, sometimes a third part). About one character in three is a digit. The game seed is not used, so a star ID has the same name in every galaxy. Names can repeat. `GetPlanetName(starId, index)` adds a Roman numeral (`index + 1`). An index of 3,999 or more throws.

### WeakReference

[`WeakReference<T>`](../Assets/Modules/CommonComponents/Scripts/Legacy/WeakReference.cs) wraps the non-generic `System.WeakReference` and adds `IsAlive` and a typed `Target`. `System.WeakReference<T>` has neither member (it has `TryGetTarget`). `BackgroundTask` and three legacy singletons depend on this API, so removing the class breaks them.

## Determinism and save compatibility

These outputs go into saves or into seeded content generation. A change to any of them is a save migration, not a refactor.

**Rule.** Never change any of these outputs, or the global random state they use, as part of another fix or refactor. Ask the owner before each such fix, also when the fix looks harmless.

| Output | Source | Depends on |
| --- | --- | --- |
| Star ID ↔ cell | `StarLayout.PositionToId`, `IdToPosition` | Integer formulas. |
| Star position and level | `GetStarPosition`, `GetStarLevel` | `UnityEngine.Random` (Xorshift128 in Unity) seeded with `seed + id`. A change in Unity's generator moves all stars and changes all levels. |
| Neighbour order | `GetAdjacentStars` | Loop order. Quest targets use `RandomElement` on it. |
| Region ID ↔ cell, home star | `RegionLayout` | Spiral + 1 and the (6, 4) scale. |
| Star and planet names | `NameGenerator` | The sequence of `System.Random(int seed)`. A runtime that changes the seeded algorithm renames every star. |
| Seeded selection | `Shuffle`, `RandomElement(s)`, `RandomUniqueElements`, `RandomExtension` | Exact sequence of `Random` calls. |
| Old save bytes | `Helpers` | Byte layout above. |

Only `Money` has tests. None of the outputs above has a test ([CC-S4](issues/common_components.md#cc-s4-deterministic-outputs-have-no-tests)).

## Unused code

No code outside the module calls these members: `FlexInt`, `BoundedSum`, `OddElements`, `IfEmptyThen`, `EnumerableExtension.Empty`, `AddOrUpdate`, `ReplaceAll`, `Next3`, `NextFloatNormal`, `ThreadSafe.Add`, `RotationHelpers.Equals`, `Geometry.VectorSphereIntersection`, `Geometry.ElasticCollision`, `ValueFormatter.AsSignedDecimal`, `BigFormat.Truncated`, `Helpers.DeserializeVector2`, `GameObjectExtensions.IsNull`, `ObservableCollection.AsReadOnly`, and the commented-out `GetAdjacentStars` in `StarLayout`. The search was a text search over `Assets/` (including `ModulesPrivate`); reflection was not checked. These members are marked for review only. Do not delete them until the owner has reviewed each one ([CC-S6](issues/common_components.md#cc-s6-unused-code)).

## Where changes go

| Change | Place | Also check |
| --- | --- | --- |
| New value type shared by several assemblies | `DataTypes/` or `Economy/`, in a named namespace | Equality, `default` value meaning, persistence. |
| New signal | The owning module (signal classes live with their feature) | Bind signal and trigger in the same container. Prefer `SmartWeakSignal`; use `Signal` when subscribers are lambdas with captures. |
| Change to signal infrastructure | `Signals/`, `Utils/SmartWeakEvent.cs` | About 170 bind calls, 79 signal types. |
| Change to star or region math | `Galaxy/` | [Determinism](#determinism-and-save-compatibility). Compare old and new outputs for many IDs and seeds. |
| Change to save metadata | `Services/` | GameSession, `Services`, and `ModulesPrivate` storage implementations. |
| New general helper | First check `Assets/ModulesShared/Utilites/` and LINQ | Do not add to the global namespace. |
| Feature logic | The feature's own module | Not here. |

## Validation checklist

1. Run `CommonComponents.Tests` in the Unity Test Runner (EditMode).
2. For `Money` or `Price` changes, test rounding with `float` multipliers, saturation, and division by zero.
3. For galaxy changes, round-trip `IdToPosition`/`PositionToId` for all IDs in the first 100 rings. Compare `GetStarPosition`, `GetStarLevel`, and `GetAdjacentStars` output with the previous version for several seeds.
4. For signal changes, test subscribe, unsubscribe, a handler that throws, a destroyed `MonoBehaviour` subscriber, and a lambda subscriber.
5. For serialization changes, load an old save and an old mod file.
6. For `BackgroundTask` or `ThreadSafe` changes, run a battle in the Editor (threaded) and in a WebGL build (no thread).
