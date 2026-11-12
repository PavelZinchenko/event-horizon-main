# GameObjectPrefab

A game object prefab record configures a combat object that code builds from a base prefab: a worm-tail segment, an energy shield circle, or a frontal shield. [Device](Device.md) refers to it through `Prefab` for the classes `EnergyShield`, `WormTail`, and `WormTailV2`, and in the new combat simulator also `PartialShield`. When a device has no `Prefab`, it uses its old prefab name field instead.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `29` |
| Kind | collection, switch on `Type` |
| Folder | [`Resources/Database/Prefabs/`](../../../Assets/Modules/Database/Resources/Database/Prefabs/) |
| Records in the built-in catalog | 4: 1 `WormTailSegment` (used by the `WormTailV2` device 20), 2 `CircularSpriteObject` (energy shield device 6), 3 `CircularOutlineObject` (starbase shield device 31), 4 `ColliderSpriteObject` (frontal shield device 8, new combat simulator only) |
| Schema | [GameObjectPrefab.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/GameObjectPrefab.xml) |
| Generated class | Abstract `GameDatabase.DataModel.GameObjectPrefab` with `GameObjectPrefab_WormTailSegment`, `GameObjectPrefab_CircularSpriteObject`, `GameObjectPrefab_CircularOutlineObject`, `GameObjectPrefab_ColliderSpriteObject` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/GameObjectPrefab.cs)) |
| Runtime | [`PrefabCache.LoadPrefab`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L100) and [`CustomPrefabLoader`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/CustomPrefabLoader.cs) |

## Fields

The record also has the [common fields](index.md#common-fields). The JSON names are generic (`Image1`, `Offset1`); the *Model name* column gives the name in the generated class for each case.

| Field | Type | Case | Model name | Default / range | Meaning |
| --- | --- | --- | --- | --- | --- |
| `Type` | enum [ObjectPrefabType](enums.md#objectprefabtype) | | | `Undefined` | The base prefab. `Undefined` throws `InvalidOperationException` when a device loads it ([CustomPrefabLoader.cs:20](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/CustomPrefabLoader.cs#L20)). |
| `Image1` | image (`Satellite`) | `WormTailSegment` | `BodyImage` | | Segment sprite, also used for the wreck. |
| `Image2` | image (`Satellite`) | `WormTailSegment` | `JointImage` | | Joint sprite, also used for the wreck. |
| `Image1` | image (`Effect`) | `CircularSpriteObject`, `CircularOutlineObject`, `ColliderSpriteObject` | `Image` | | Shield sprite or ring texture. `CircularOutlineObject` throws `NullReferenceException` without an image ([CircleView.cs:28](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/View/CircleView.cs#L28)). |
| `ImageScale` | float | `CircularSpriteObject` | `ImageScale` | 1, 0…10 | Sprite size relative to the collision circle. The collider radius is `0.5 / ImageScale` ([CircularObjectInitializer.cs:16](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Helpers/CircularObjectInitializer.cs#L16)). |
| `ImageScale` | float | `CircularOutlineObject` | `ImageScale` | 1, 0…10 | Ring radius: `0.5 × ImageScale`. |
| `ImageScale` | float | `WormTailSegment` | `JointImageScale` | 0…10 | Scale of the joint sprite. `0` hides it. |
| `Thickness` | float | `CircularOutlineObject` | | 0.1, 0…1 | Ring thickness. |
| `AspectRatio` | float | `CircularOutlineObject` | | 1, 0…100 | Width-to-height ratio of the ring. |
| `ImageOffset` | float | `WormTailSegment` | `JointImageOffset` | −1…1 | Position of the joint sprite along the segment. |
| `Length` | float | `WormTailSegment` | `BoneLength` | 0…1 | Distance between two segments. `WormTail` only. |
| `Offset1` | float | `WormTailSegment` | `JointOffset` | 0…1 | Position of the hinge on the segment. `WormTail` only. |
| `Offset2` | float | `WormTailSegment` | `HeadOffset` | −1…1 | Extra distance between the ship and the first segment. `WormTail` only. |
| `Angle1` | float | `WormTailSegment` | `MaxRotation` | 0…180 | Hinge angle limit between segments. `WormTail` only. |
| `Angle2` | float | `WormTailSegment` | `MaxHeadRotation` | 0…180 | Hinge angle limit at the ship. `WormTail` only. |
| `Collider` | struct list [ColliderShape](Ship.md) | `ColliderSpriteObject` | `Collider` | | The shape in the prefab frame (x forward), as `Ship.Collider`. See [Frontal shields](#frontal-shields). |

## Notes

### Base prefabs

| `Type` | Base prefab under `Combat/Objects/` | Initializer |
| --- | --- | --- |
| `WormTailSegment` | `WormSegment` | [WormTailSegmentInitializer.cs:26](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Helpers/WormTailSegmentInitializer.cs#L26) |
| `CircularSpriteObject` | `EnergyShield` | [CircularObjectInitializer.cs:12](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Helpers/CircularObjectInitializer.cs#L12) |
| `CircularOutlineObject` | `EnergyShieldOutline` | [CircleView.cs:22](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/View/CircleView.cs#L22) |
| `ColliderSpriteObject` | `FrontalShield` | none: legacy never loads it for a frontal shield; another device that names it gets the prefab as it is ([CustomPrefabLoader.cs](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/CustomPrefabLoader.cs)) |

`PrefabCache` builds one template per record ID and clears it when a database loads.

### Worm tails

The two worm-tail device classes read different fields ([DeviceFactory.cs:149](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs#L149)):

| Device class | Joint geometry | Code |
| --- | --- | --- |
| `WormTail` | `BoneLength`, `JointOffset`, `HeadOffset`, `MaxRotation`, `MaxHeadRotation` from this record. A prefab without these values logs `Prefab is not suitable for the worm tail` and creates no tail. | [SpaceObjectFactory.cs:248](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Objects/SpaceObjectFactory.cs#L248) |
| `WormTailV2` | `Device.Offset` x and y, fixed angle limits of 40° and 75°, and a fixed head offset of 0.15. The five geometry fields of this record have no effect. | [WormTailDeviceV2.cs:81](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/WormTailDeviceV2.cs#L81), [WormSegmentFactory.cs:21](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/WormTail/WormSegmentFactory.cs#L21) |

The only built-in worm-tail device is `WormTailV2`, so the geometry values of record 1 are not used. Both classes use the images, `JointImageScale`, and `JointImageOffset`.

### Energy shields

`EnergyShield` devices create the shield object from the prefab. The device sets the energy per damage (`1 / (Power ×` shield multiplier`)`), the size (`Size`), and the color (`Color`). Without `Prefab` and `EffectPrefab`, the device uses `Combat/Objects/EnergyShield` ([DeviceFactory.cs:89](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs#L89)). See [Device: Device classes](Device.md#device-classes).

### Frontal shields

New combat simulator only (migration phase 5b, step 7c). A `PartialShield` device with a `ColliderSpriteObject` `Prefab` draws its `Image` and takes its shape from the first `Collider` outline with 3 or more points, as a convex hull. `Device.Size` × model scale scales both; `Device.Offset` places them. A circle, a capsule, or an empty list falls back to the built-in polygon of the `FrontalShield` prefab, and the analyzer (*Dead links*) reports it. Record 4 holds that polygon, so the built-in shape is unchanged. Legacy ignores the record and draws the `FrontalShield` prefab.

### Data rules

1. Never leave `Type` at `Undefined`.
2. Give `CircularOutlineObject` an image.
3. Give `ColliderSpriteObject` an outline of 3 or more points.
3. For `WormTailV2`, set the joint geometry on the device (`Offset`), not here.
