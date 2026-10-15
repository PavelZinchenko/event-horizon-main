# BulletPrefab

A bullet prefab record is the look of a bullet: a base shape, an image, a size, and two colors. Combat builds one template object per record from a base prefab and copies it for every bullet. [Ammunition](Ammunition.md) refers to it through `Body.BulletPrefab`. [AmmunitionObsolete](AmmunitionObsolete.md) does not use this table.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `27` |
| Kind | collection |
| Folder | [`Resources/Database/Ammunition/Bullets/`](../../../Assets/Modules/Database/Resources/Database/Ammunition/Bullets/) |
| Records in the built-in catalog | 21. Records 3 and 11 are unused. No record sets `MainColor` or `MainColorMode`. |
| Schema | [BulletPerfab.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Weapon/BulletPerfab.xml) (the file name is misspelled) |
| Generated class | `GameDatabase.DataModel.BulletPrefab` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/BulletPrefab.cs)) |
| Runtime | [`PrefabCache.GetBulletPrefab`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L114) and the initializers in [`Component/Helpers/`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Helpers/) |

## Fields

The record also has the [common fields](index.md#common-fields). What each field does depends on the shape; see [Shapes](#shapes).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Shape` | enum [BulletShape](enums.md#bulletshape) | `Projectile` | The base prefab, the view, and the collider. Beam shapes use a ray collider. |
| `Image` | image (`Ammunition`) | | Sprite of the bullet, or texture of a beam or wave. |
| `Size` | float | 0.01…100 | Sprite scale, or line thickness of a beam. The ammunition `Body.Size` scales the whole object on top of it. |
| `Margins` | float | 0…1 | Shape-specific: engine offset, beam end length, minimum scale, or arc gap. |
| `Deformation` | float | −100…100 | `Wave` only: how much the arc grows over the bullet's life. |
| `MainColor` | color | white | Color of the body. |
| `MainColorMode` | enum [ColorMode](enums.md#colormode) | `TakeFromOwner` | How `MainColor` combines with the bullet color. The default shows the bullet color and ignores `MainColor`. |
| `SecondColor` | color | white | Color of the engine glow, the trail, and the harpoon rope. |
| `SecondColorMode` | enum [ColorMode](enums.md#colormode) | `TakeFromOwner` | How `SecondColor` combines with the bullet color. |

## Notes

### Shapes

The bullet color is the ammunition `Body.Color`, or the trigger color for a spawned bullet. Code: [PrefabCache.cs:125](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L125).

| `Shape` | Base prefab | `Image` | `Size` | `Margins` | Initializer |
| --- | --- | --- | --- | --- | --- |
| `Projectile`, `Rocket` | `CommonProjectile`, `CommonRocket` | sprite | sprite scale | Engine glow at `−Margins`, trail at `−Margins − 0.1` | [ProjectileInitializer.cs:18](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Helpers/ProjectileInitializer.cs#L18) |
| `Harpoon` | `Hook` | sprite | sprite scale | as above; the rope uses `SecondColor` | same |
| `Mine`, `Spark` | `Mine`, `Spark` | spinning sprite | sprite scale | Minimum start and end scale of the spin animation ([RollingSpriteView.cs:21](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/View/RollingSpriteView.cs#L21)) | same |
| `LaserBeam`, `PiercingLaser` | `Laser`, `PiercingLaserBeam` | line texture | line thickness | Length of the faded ends | [BeamInitializer.cs:15](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Helpers/BeamInitializer.cs#L15) |
| `LightningBolt`, `EnergyBeam` | `Lightning`, `EnergyBeam` | line texture | line thickness | | same |
| `Wave` | `Arc` | line texture | | Gap of the arc: it spans 180° × (1 − `Margins`) ([ArcView.cs:21](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/View/ArcView.cs#L21)) | same |
| `BlackHole` | `WormHole` | | particle scale | | [ParticleSystemInitializer.cs](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Helpers/ParticleSystemInitializer.cs) |
| `CircularSaw` | `CircularSaw` | sprite | | | [CircularSawView.cs:28](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/View/CircularSawView.cs#L28) |

`Wave` passes `Deformation` as the growth rate: the arc scales to `1 + Deformation × (1 − life)`, so it grows as the bullet ages. `SecondColor` applies only to projectile-type prefabs that have an engine, a trail, or a rope.

An ammunition record without a bullet prefab uses the invisible base prefab `AreaOfEffect` and behaves as a `Mine` shape ([Ammunition: BulletBody](Ammunition.md#bulletbody)).

`Spark`, `Mine`, and the null prefab have no direction ([BulletShape.cs:7](../../../Assets/Modules/Database/Scripts/Extensions/BulletShape.cs#L7)). A magnetic homing bullet (`IgnoreRotation`) with such a shape searches targets in all directions and does not turn. With other shapes it searches a 30° cone ahead and turns along its velocity ([MagneticController.cs:28](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/MagneticController.cs#L28)). The four beam shapes count as beams ([BulletShape.cs:26](../../../Assets/Modules/Database/Scripts/Extensions/BulletShape.cs#L26)): their view starts at size 0, and with a non-beam controller and a speed above 0 they become moving beams.

### Template cache

`PrefabCache` builds one template per record ID and clears the cache when a database loads ([PrefabCache.cs:199](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L199)). An unknown shape value returns no prefab.

### Data rules

1. Match the shape to the controller: beam shapes for `Beam` controllers and moving beams, `Harpoon` for `Harpoon` controllers.
2. To give a bullet a fixed color regardless of its ammunition, set `MainColor` and `MainColorMode` `UseMyOwn`. Otherwise color the bullet through the ammunition `Body.Color`.
3. Several ammunition records can share one bullet prefab. A change applies to all of them.
