# CommonComponents Module

The CommonComponents module contains small domain types and low-level helpers shared by otherwise independent Event Horizon assemblies. It owns common identifiers, economy values, galaxy coordinate math, signals, serialization helpers, collection utilities, and a small set of legacy Unity helpers. It should remain a dependency leaf: feature-specific workflows belong in their owning modules, not here.

The module lives in `Assets/Modules/CommonComponents/` and compiles as the `CommonComponents` assembly. It has no root namespace, so its APIs currently appear under several established namespaces as well as the global namespace. The assembly references Zenject, AppConfiguration, Utilites, and UnityEngine.

## Directory Map

| Path | Purpose |
| --- | --- |
| `Scripts/DataTypes/` | Resource identifiers and `StatMultiplier`. |
| `Scripts/Economy/` | `Money`, `Price`, currencies, item quality, and the common item-type contract. |
| `Scripts/Galaxy/` | Stable star/region ID mapping, positions, rings, distance, and seeded layout helpers. |
| `Scripts/Math/` | Geometry, mesh primitives, rotations, vectors, and atomic float updates. |
| `Scripts/Serialization/` | Primitive byte conversion, URL-safe Base64 conversion, and stream helpers. |
| `Scripts/Services/` | Storage-facing `IGameData` and `ISerializableGameData` contracts. |
| `Scripts/Signals/` | Strong and weak signal types plus Zenject binding helpers. |
| `Scripts/Utils/` | Collections, lazy values, random/enumerable helpers, formatting, and compact numeric wrappers. |
| `Scripts/Legacy/` | Background threading, GameObject helpers, deterministic name generation, and a generic weak reference. |
| `Tests/` | Editor-only NUnit tests; currently focused on `Money` arithmetic. |

## Shared Data Types

`GameDatabase.Model.AudioClipId` and `SpriteId` are lightweight references used across database, resource, audio, and UI code. `AudioClipId` removes the first `*` from an authored name and records that the clip should loop. `SpriteId` carries both a string ID and a category such as ship, component, ammunition, or GUI icon. Their implicit Boolean conversions only indicate whether an ID is present; they do not verify that the asset exists.

`StatMultiplier` stores the delta from one rather than the full multiplier. Constructing `new StatMultiplier(0.25f)` represents a value of `1.25`, while `StatMultiplier.FromValue(0.25f)` represents a 75% reduction. Addition combines deltas, multiplication combines full multiplier values, and `AmplifyDelta` scales only the bonus or penalty.

## Economy Values

`CommonComponents.Money` is a non-negative, saturating value type backed by a `long`. Negative construction and subtraction clamp to zero; positive addition and multiplication saturate at `long.MaxValue` on overflow. Conversion from `double` rounds upward before clamping. Its stored field is XOR-obscured with a process-local mask, so use the explicit numeric conversions instead of inspecting or serializing the backing field.

`Economy.Price` pairs `Money` with a `Currency`. `Common`, `Premium`, and `Tokens` construct the usual price kinds. Under `IAP_DISABLED`, `Premium` converts the amount to credits with the current fixed multiplier. Division keeps a positive price at a minimum amount of one.

`Economy.ItemType.IItemType` is the shared UI/economy-facing contract for named, priced, consumable or withdrawable items. Concrete factories and inventory behavior belong in the application or feature module that owns the item. `UnknownItem` is the neutral fallback.

## Signals and Dependency Injection

Signals separate read access from publication. Consumers receive the signal and subscribe to its `Event`; producers receive the nested `Trigger` and call `Fire`.

```csharp
public sealed class InventoryChangedSignal
    : SmartWeakSignal<InventoryChangedSignal, int> { }

Container.BindSignal<InventoryChangedSignal>();
Container.BindTrigger<InventoryChangedSignal.Trigger>();
```

`Signal<T...>` uses a normal C# event and therefore holds strong subscriber references. Subscribers with shorter lifetimes must unsubscribe explicitly. `SmartWeakSignal<T...>` uses `SmartWeakEvent` and does not keep instance targets alive. Its weak-event implementation rejects anonymous methods that capture a closure; subscribe with an instance method or use a strong signal when a capturing callback is required. Exceptions thrown by weak-event handlers are logged and do not stop the remaining handlers.

`SignalExtensions` provides the `BindSignal` and `BindTrigger` Zenject helpers used by scene installers. Bind both sides in the same container scope unless publication or observation is intentionally inherited from a parent container.

## Galaxy and Math Helpers

`GameModel.StarLayout` maps the unbounded integer grid to stable star IDs and back, lays that grid out as offset hex rows, enumerates stars at a requested ring distance, and calculates hex distance. `GetStarPosition` applies a deterministic per-star offset derived from the game seed. It calls `UnityEngine.Random.InitState`, so callers that also depend on Unity's global random sequence must restore or isolate their random state.

`GameModel.RegionLayout` reuses the star mapping for region IDs and determines each region's home star. Save data and generated galaxy content depend on these mappings remaining stable; changing them is a persistence and world-generation migration, not a cosmetic refactor.

The math folder includes:

- `Geometry` for projectile interception, segment distance/intersection, Bezier curves, collision impulses, and winding checks.
- `Primitives` for procedurally filling Unity meshes with rectangles, subdivided planes, noisy spheres, lines, and circles.
- `RotationHelpers` for cached direction vectors, angle normalization, rotation, arc checks, and rotated bounds.
- `ThreadSafe` for compare-and-swap updates to shared `float` values.
- `VectorExtensions` for direction, distance, squared distance, and minimum-step interpolation.

Most of these types are in the global namespace for compatibility. Avoid adding new global APIs unless moving the existing callers is part of the same change.

## Serialization and Storage Contracts

`Services.Storage.IGameData` describes the metadata expected by local and cloud storage: game ID, played time, data version, and mod ID. `ISerializableGameData` adds binary serialization and deserialization from an offset in an existing byte buffer. GameSession implements these contracts; storage backends consume them without depending on the concrete session model.

The helpers under `GameModel.Serialization` and `Utils` are low-level compatibility utilities rather than a complete file format. Preserve byte ordering and collection layout when changing an existing caller. New persisted formats should own and document their envelope, version, integrity checks, and migration behavior in the responsible module.

## Collections and General Utilities

The utility folder includes observable and quantity-based collections, custom lazy holders, weak events, numeric wrappers, random selection, formatting, and general enumerable operations. Several extension classes are also in the global namespace and can silently compete with framework or LINQ methods. Before adding another general extension, check both this module and `Assets/ModulesShared/Utilites/` and prefer the narrower owning assembly.

`QuickRemove` replaces the removed element with the final element and therefore does not preserve list order. Random-selection helpers accept `System.Random`; pass a seeded instance when the result affects saved or replayed gameplay. `ValueFormatter` is presentation-oriented and should not be used as a persistence representation.

## Legacy Code

`Scripts/Legacy/` remains shared because existing modules still consume it, but it is not the default home for new infrastructure.

- `BackgroundTask` runs work on a dedicated thread outside WebGL and becomes a no-op shell on WebGL. Worker code must not call Unity APIs that require the main thread.
- `GameObjectExtensions` creates meshes and materials and uses immediate destruction in several paths; use it only where that ownership model is expected.
- `NameGenerator` deterministically derives star and planet names from IDs.
- `WeakReference<T>` wraps the non-generic framework weak reference retained by older callers.

When replacing legacy behavior, migrate callers to an owning module or a clearly scoped shared service rather than growing the legacy folder.

## Common Change Locations

- Add a cross-module value object: the closest `DataTypes/`, `Economy/`, or `Galaxy/` folder, with a stable namespace.
- Add a signal family or binding helper: `Signals/`; keep publishing behind its nested trigger.
- Change game/session storage metadata: `Services/`, then verify every storage implementation and GameSession.
- Change deterministic star or region mapping: `Galaxy/`, with save compatibility and seeded-world validation.
- Add a generic collection or algorithm: first verify it is broadly reused and does not belong in `ModulesShared/Utilites` or a feature assembly.
- Add feature-specific orchestration: use the owning feature module instead of CommonComponents.

## Validation Checklist

1. Run `CommonComponents.Tests` in the Unity Test Runner; the current suite verifies `Money` saturation and clamping.
2. Add focused tests for new deterministic math, collection, or serialization behavior rather than relying only on scene testing.
3. For signal changes, verify container bindings, publisher/subscriber lifetimes, unsubscription, and captured-callback behavior.
4. For galaxy changes, round-trip IDs and positions and compare seeded layouts with existing saves.
5. For thread-related changes, test both a threaded standalone/editor path and WebGL's non-threaded conditional path.
