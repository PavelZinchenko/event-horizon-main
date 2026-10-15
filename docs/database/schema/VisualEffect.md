# VisualEffect

A visual effect record is a composite effect: a list of elements (flashes, shock waves, smoke, sparks, lightning, camera shake) on a shared timeline. Combat creates a new `CompositeEffect` object each time the effect plays. Ammunition triggers, weapons (muzzle flash), devices, and ship explosions refer to visual effect records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `26` |
| Kind | collection |
| Folder | [`Resources/Database/Ammunition/Effects/`](../../../Assets/Modules/Database/Resources/Database/Ammunition/Effects/) |
| Records in the built-in catalog | 24, with 62 elements. Record 4 is unused. No element uses `Sprite`, `Offset`, or `Rotation`. |
| Schema | [VisualEffect.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Weapon/VisualEffect.xml), [VisualEffectElement.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Weapon/VisualEffectElement.xml) |
| Generated class | `GameDatabase.DataModel.VisualEffect` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/VisualEffect.cs)) |
| Runtime | [`CompositeEffect`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs) with element prefabs from [`PrefabCache.GetEffectPrefab`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L151) |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Elements` | struct list [VisualEffectElement](#visualeffectelement) | | The parts of the effect. An empty list shows nothing and lasts 0 s. |

## Structures

### VisualEffectElement

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Type` | enum [VisualEffectType](enums.md#visualeffecttype) | `Flash` | The element kind and its base prefab. See [Element types](#element-types). |
| `Image` | image (`Effect`) | | Sprite or texture of the element. `Shake` ignores it. |
| `ColorMode` | enum [ColorMode](enums.md#colormode) | `TakeFromOwner` | How `Color` combines with the color that the caller gives the effect (bullet color, device color). |
| `Color` | color | white | Element color. |
| `Quantity` | int | 1, 1…100 | Copies of the element. The copies share all values. Only elements with random motion (`Smoke`, `Spark`, `Lightning`) look different. |
| `Size` | float | 1, 0.001…100 | Element size, relative to the effect size. `Spark`: also the speed of the sparks. `Shake`: the strength. |
| `GrowthRate` | float | 0, −1…100 | Size change over the element's life: size = `Size × (1 + GrowthRate × (1 − life))` ([CompositeEffect.cs:196](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs#L196)). −1 shrinks to 0. |
| `TurnRate` | float | 0, −1000…1000 | Total rotation in degrees over the element's life, not per second ([CompositeEffect.cs:198](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs#L198)). |
| `StartTime` | float | 0…1000 | Start of the element on the timeline. |
| `Lifetime` | float | 1, 0…1000 | Duration of the element on the timeline. Must be above 0 (see [Data rules](#data-rules)). |
| `ParticleSize` | float | 1, 0.001…100 | `Smoke`, `SmokeAdditive`: strength of the random variation in position, size, color, and spin. `Spark`: scale of each spark. `Lightning`, `LightningStrike`: line thickness. Other types ignore it. |
| `Offset` | vector | | Position of the element inside the effect. |
| `Rotation` | float | 0…360 | Start rotation of the element in degrees. |
| `Loop` | bool | | The element repeats every `Lifetime` from `StartTime` until the effect ends. |
| `Inverse` | bool | | The element's life runs from 0 to 1 instead of 1 to 0: it fades in and reverses `GrowthRate` and `TurnRate`. |
| `UseRealTime` | bool | | The element uses seconds since the effect started, not the stretched timeline (see [Timeline](#timeline)). |

## Notes

### Timeline

1. The timeline length *T* is the largest `StartTime + Lifetime` of all elements ([CompositeEffect.cs:101](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs#L101)).
2. The caller plays the effect for a duration *L*. A *L* of 0.01 or less means *T* ([CompositeEffect.cs:75](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs#L75)).
3. The effect stretches the timeline to *L*: at real time *t*, the timeline time is `t × T / L` ([CompositeEffect.cs:169](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs#L169)). `StartTime` and `Lifetime` are therefore relative values unless the caller passes 0.
4. An element is visible from `StartTime` to `StartTime + Lifetime`, or from `StartTime` on with `Loop` ([CompositeEffect.cs:203](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/CompositeEffect.cs#L203)).
5. An element's life goes from 1 to 0 over its `Lifetime`. Most element prefabs fade with their life.

Callers and their duration *L*:

| Caller | Duration | Size | Color |
| --- | --- | --- | --- |
| Ammunition trigger `PlaySfx`, `SpawnStaticSfx` | trigger `Lifetime`, or the bullet's remaining life with `SyncLifetimeWithBullet` | trigger `Size` | trigger color ([Ammunition: BulletTrigger](Ammunition.md#bullettrigger-switch)) |
| Weapon muzzle flash | flash time of the ammunition | ammunition size × `EffectSize` | bullet color ([Weapon: Muzzle flash](Weapon.md#muzzle-flash)) |
| Device effect | device lifetime or 0.5 s | device size | device color ([Device: Effects and prefabs](Device.md#effects-and-prefabs)) |
| Ship explosion | the largest element `Lifetime`, without `StartTime`, so elements with a `StartTime` are compressed ([ShipExplosionAction.cs:34](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Triggers/ShipExplosionAction.cs#L34)) | ship scale | none: each element keeps its own `Color`, and `ColorMode` has no effect ([Ship: Combat object](Ship.md#combat-object), [ShipSettings](ShipSettings.md)) |

### Element types

Base prefabs: [PrefabCache.cs:151](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L151).

| `Type` | Base prefab | Notes |
| --- | --- | --- |
| `Flash`, `FlashAdditive` | `Flash`, `FlashAdditive` | Sprite flash. The additive version brightens. |
| `Shockwave` | `Wave` | Ring that expands from 0 to full size over its life, drawn with the image texture. |
| `Smoke`, `SmokeAdditive` | `SmokeSimple`, `SmokeSimpleAdditive` | Random offset, size, tint, and spin by `ParticleSize` ([SmokeEffect.cs:28](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/SmokeEffect.cs#L28)). |
| `Shake` | none | Shakes the camera each time the element becomes visible. Strength: `min(2 × effect size × Size / √(distance + 20), 5)` ([ShakeEffect.cs:24](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/ShakeEffect.cs#L24)). |
| `Spark` | `Spark` | Sparks fly in random directions at speed `Size`, spin, and swirl by `TurnRate` ([SparkEffect.cs:24](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/SparkEffect.cs#L24)). |
| `Lightning` | `LightningStrike` | Animated lightning that changes direction every frame. |
| `LightningStrike` | `LightningStrike` | One lightning bolt that fades ([LightningEffect.cs:41](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Effects/LightningEffect.cs#L41)). |
| `Sprite` | `Sprite` | Plain sprite. |

The cache keeps one element prefab per `Type` and `Image`. `Spark`, `Smoke`, and `Lightning` prefabs store more values at creation, so elements that share a type and an image also share these values. See [DB15.19](../../issues/database.md#db15-smaller-defects).

### Data rules

1. Give every element a `Lifetime` above 0. A non-looping element with 0 is visible only at the exact start time, so it almost never shows. Built-in effect 9 (`ShockWave`) has a `Shake` element with 0, so its shake never plays. A looping element with 0 gets an invalid life. See [DB15.5](../../issues/database.md#db15-smaller-defects).
2. Elements with the same `Type` and `Image` must use the same `Size`, `TurnRate`, `ParticleSize`, `Color`, and `ColorMode`, or use different images ([DB15.19](../../issues/database.md#db15-smaller-defects)). Built-in data breaks it three times: effect 15 against eight other effects (`Smoke` with `CloudWhite`), 12 against 19 (`Lightning` with `beam`), and 9, 13, and 14 (`LightningStrike` with `beam`).
3. Use `UseRealTime` for elements that must keep their speed when a caller stretches the effect, for example a looping glow.
4. `Quantity` copies only differ for random types. Use several elements with different `Offset` or `Rotation` for fixed patterns.
5. Each play creates new objects (not pooled). Avoid many elements on effects that play every frame.
