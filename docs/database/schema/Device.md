# Device

A device is an active or passive ship system: a shield, a teleporter, a cloak, repair bots, a self-destruct charge, and so on. A [Component](Component.md) brings a device through `DeviceId`. A hull can also have built-in devices (`Ship.Features.BuiltinDevices`). `DeviceClass` selects the implementation, and the class decides what the numeric fields mean. The ship builder and combat read device records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `2` |
| Kind | collection, legacy (one `DeviceStats` struct) |
| Folder | [`Resources/Database/Device/`](../../../Assets/Modules/Database/Resources/Database/Device/) |
| Records in the built-in catalog | 36 in 23 classes. 4 `Teleporter`, 4 `Retribution`, 3 `RepairBot`, 3 `ClonningCenter`. No record uses `WormTail`. |
| Schema | [Device.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Device.xml) |
| Generated class | `GameDatabase.DataModel.Device` with the struct `DeviceStats` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Device.cs)) |
| Runtime | [`DeviceFactory`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs) creates one system per device. Behavior per class: [BattleSimulator: Devices](../../battle_simulator.md#devices). |

## Fields

The record also has the [common fields](index.md#common-fields). The meaning of `Power`, `Range`, `Size`, `Cooldown`, and `Lifetime` depends on the class (see [Device classes](#device-classes)).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `DeviceClass` | enum [DeviceClass](enums.md#deviceclass) | `Accelerator` | The implementation. |
| `EnergyConsumption` | float | 0…999999999 | Energy per use or per second, by class. See [Energy](#energy). |
| `PassiveEnergyConsumption` | float | 0…999999999 | Permanent consumption while the component is installed. It lowers the ship's energy recharge. |
| `ScaleEnergyWithShipSize` | bool | `true` | Multiplies both energy fields by `0.05 × n`, where `n` is the hull cell count. Only 2 built-in records turn it off. |
| `Power` | float | | Main strength. No clamp. |
| `Range` | float | 0…1000 | Distance or radius. |
| `Size` | float | 0…1000 | Size of the created object or effect, or a count. |
| `Cooldown` | float | 0…1000 | Seconds before the next use. Most classes start it when the effect ends. |
| `Lifetime` | float | 0…1000 | Duration or delay. |
| `Offset` | vector | | Position of a partial shield, or joint offsets of a `WormTailV2` tail. |
| `ActivationType` | enum [ActivationType](enums.md#activationtype) | `None` | How the device is triggered. See [Activation](#activation). |
| `Color` | color | white | Color of the created object, effect, or ship tint. |
| `Sound` | audio | | Played when the device activates. |
| `EffectPrefab` | prefab (`Effect`) | | Effect prefab. The shield object of `EnergyShield` when `Prefab` is empty. |
| `VisualEffect` | ref [VisualEffect](VisualEffect.md) | | Composite effect. It wins over `EffectPrefab` where both apply. |
| `ObjectPrefab` | prefab (`Object`) | | Tail segment of `WormTail` and `WormTailV2` when `Prefab` is empty. Other classes ignore it. |
| `Prefab` | ref [GameObjectPrefab](GameObjectPrefab.md) | | Code-built object: the shield of `EnergyShield`, the tail segment of the worm tails. |
| `AmmunitionId` | ref [Ammunition](Ammunition.md) | | The bullet of `Retribution`. |
| `ControlButtonIcon` | image (`ActionButton`) | | Icon of the combat button. |

## Device classes

Each row lists the fields that the class reads ([DeviceFactory.cs:35](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs#L35)). A field that a row does not name has no effect for that class. *Energy* is `EnergyConsumption` after ship-size scaling. *Scale* is the ship's body scale. *Damage multiplier* and *shield multiplier* are the ship's stat multipliers.

| Class | Power | Range | Size | Cooldown | Lifetime | Energy | Other fields |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `Accelerator` | Speed and acceleration multiplier. Throttle is forced to full. | | Flash size × scale | After release | | Per second: `max(1, energy × engine consumption)` ([AcceleratorDevice.cs:19](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/AcceleratorDevice.cs#L19)) | A flash at each engine, only when `EffectPrefab` is set. `Color`. |
| `Brake` | Extra deceleration `Power / √weight` while throttle is 0 ([BrakeDevice.cs:15](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/BrakeDevice.cs#L15)) | | | | | none | Always passive. |
| `ClonningCenter` | | | | Between clones. The first clone is ready after 20 % of it. | | Per clone | `EffectPrefab` at the clone. The clone has 90 % size, damage, and defense, and no drone bays ([ClonningDevice.cs:29](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/ClonningDevice.cs#L29)). Drones never get one. |
| `Decoy` | Number of decoys (rounded) | Launch speed | | After use | Decoy life × 0.75…1.25 | Per use | `Color`. Each decoy has `10 × armor multiplier` hit points ([DeviceFactory.cs:60](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs#L60)). |
| `Detonator` | Damage per hull cell, × damage multiplier | Radius × √scale ([DetonatorDevice.cs:27](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/DetonatorDevice.cs#L27)) | | | Countdown | Per use | Heat explosion, then the ship is destroyed. The ship blends to `Color` during the countdown. |
| `DroneCamouflage` | Chance 0…1 that drones ignore the ship | | | | | none | Always passive. |
| `EnergyShield` | Damage that one energy point absorbs, × shield multiplier | | Shield diameter, relative to the ship | After switch-off | | On switch-on. Hits on the shield then cost energy. | `Prefab`, else `EffectPrefab`, else the default shield. `Color`. The ship is invulnerable while the shield is on. |
| `Fortification` | Resistance in percent. `0` means 50 %. | | | After switch-off | | Per second | Ship tint `Color`. All resistances move toward 1 by `Power / 100` ([FortificationDevice.cs:23](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/FortificationDevice.cs#L23)). |
| `Ghost` | | | Effect size × scale | After it ends | Maximum duration | On start and per second | Invulnerable. Blocks other systems except stealth. Ship tint `Color`. Flash from `VisualEffect` or `EffectPrefab`. |
| `GravityGenerator` | Push impulse × √target weight | Radius | Effect size = `Size × 2 × Range` | After use | Effect duration | Per use | Destroys missiles, energy bolts, and area effects in the radius. |
| `Jammer` | | | | | | none | Always passive. Enemies prefer the ship as a target. |
| `MissileCamouflage` | Chance 0…1 that missiles ignore the ship | | | | | none | Always passive. |
| `PartialShield` | Like `EnergyShield` | | Shield size | After switch-off | | Per second | `Offset` places the shield. `Color`. The prefab is fixed. |
| `PointDefense` | Damage per hit on drones, × damage multiplier | | Radius beyond half the ship scale | After switch-off, and between intercepts | | On switch-on, and per intercept | `Color`. Disarms enemy missiles and bolts in the radius. |
| `RepairBot` | Percent of maximum armor repaired per second | Flight radius beyond half the ship scale | Bot size and bot hit points | After switch-off | Rebuild delay after the bot is destroyed | Per second | Beam `Color`. `Sound` plays on the bot. Not on bionic hulls. |
| `Retribution` | Damage multiplier of the bullet, × damage multiplier | | | | | none | Fires `AmmunitionId` when the ship is destroyed. Always passive. |
| `Stealth`, `SuperStealth` | | | | After it ends | | Per use | Enemies cannot target the ship. A hit ends `Stealth` but not `SuperStealth`. Activating most other systems ends both ([StealthDevice.cs:46](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/StealthDevice.cs#L46)). No time limit. |
| `Teleporter` | | Jump distance forward | Effect size × scale | After use | | Per use | Effect from `VisualEffect` or `EffectPrefab`, `Color`. The ship stops after the jump. |
| `TeleporterV2` | Flight speed. The flight lasts `Range / Power` seconds ([WarpDrive.cs:33](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/WarpDrive.cs#L33)). | Flight distance | Effect size × scale | After the flight | | Per use | Invulnerable and transparent. Releasing the key after 0.1 s stops early. A ship with a `WormTail` device jumps instantly. |
| `TimeMachine` | | | Effect size × scale | Between uses | Invulnerability after a revive | none | Revives the ship when it dies and the cooldown is over, or on a key press. Restores energy, armor, and shield ([InfinityStone.cs:45](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/InfinityStone.cs#L45)). Ship tint `Color`. |
| `ToxicWaste` | Corrosive damage per second, × damage multiplier | | Cloud radius = `scale × Size + 1` | | Cloud duration | none | Creates the cloud when the ship is destroyed ([ToxicWaste.cs:35](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/ToxicWaste.cs#L35)). Always passive. `Color`. |
| `WormTail` | Segment hit points as a fraction of maximum armor | | Number of segments | | | none | Segment from `Prefab` or `ObjectPrefab`. Resistance grows with the intact segments. Always passive. |
| `WormTailV2` | Segment hit points as a fraction of maximum armor | | Tail length. Segment weight is `ship weight / Size`. | Segment regrowth. A destroyed tail returns after `1.5 × Cooldown`. | | none | Segment from `Prefab` or `ObjectPrefab`. `Offset` sets the joints ([WormTailDeviceV2.cs:73](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Devices/WormTailDeviceV2.cs#L73)). Always passive. |

## Notes

### Energy

1. A component device adds `PassiveEnergyConsumption` to the component's consumption ([CommonComponent.cs:130](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L130)). The ship builder installs such a component only when the ship's recharge covers it (see [Component: Stats and energy](Component.md#stats-and-energy)).
2. With `ScaleEnergyWithShipSize`, both fields are multiplied by `0.05 × n` ([CommonComponent.cs:97](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L97)). A 20-cell hull pays the listed value, a 100-cell hull pays five times as much.
3. Built-in hull devices skip both rules. They have no passive consumption and no size scaling ([ShipBuilder.cs:85](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L85)).
4. A [modification](ComponentMod.md#effects) of the component multiplies `Cooldown`, `Range`, `Power`, and a positive `EnergyConsumption`.

### Activation

The key binding of the installed component passes through the device's `ActivationType` ([ActivationType.cs:14](../../../Assets/Modules/ShipConstructor/Scripts/Component/ActivationType.cs#L14)):

| `ActivationType` | Key binding |
| --- | --- |
| `None` | Always `-1`: the device runs automatically with no energy reserve. |
| `Manual` | A negative key becomes `0`. The player or the AI must press it. |
| `Mixed` | As set on the component: a key, or automatic with a reserve. |

Automatic systems are explained in [BattleSimulator: Ship systems](../../battle_simulator.md#ship-systems). A hull's built-in device gets key 5 when it is `Manual`, otherwise it is automatic. `Brake`, `Jammer`, the camouflages, `ToxicWaste`, the worm tails, and `Retribution` ignore the key and have no button.

AI behavior trees use devices by class: the `ActivateDevice` node and the `HasDevice` requirement. See [BehaviorTree](BehaviorTree.md).

### Effects and prefabs

- `Sound` plays on activation. `Ghost` also plays it on deactivation. `PointDefense` and `RepairBot` pass it to their units ([DeviceFactory.cs:178](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs#L178)).
- `VisualEffect` wins over `EffectPrefab` ([DeviceFactory.cs:184](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DeviceFactory.cs#L184)). The `Accelerator` creates its flash only when `EffectPrefab` is set, even when `VisualEffect` is set.
- Built-in records set `ObjectPrefab` on a decoy, a partial shield, the toxic waste, and the repair bots, and `EffectPrefab` on the detonator, the point defense, and the repair bots. These classes use fixed prefabs and ignore the values.

### Compatibility

A component with a device does not fit a hull that already has a built-in device of the same class. A `RepairBot` does not fit a bionic hull ([CompatibilityChecker.cs:32](../../../Assets/Modules/ShipConstructor/Scripts/Component/CompatibilityChecker.cs#L32)). The built-in data limits most device kinds to one per ship with a [ComponentGroupTag](ComponentGroupTag.md).

Device 18 (toxic waste) is [hard-coded](../../database.md#hard-coded-ids): the `Infected` perk adds it to the ship.

### Data rules

1. Set `DeviceClass`. The default `0` is `Accelerator`.
2. Turn off `ScaleEnergyWithShipSize` when the cost should not grow with the hull. The default makes the listed cost exact only for 20-cell hulls.
3. Give `TeleporterV2` a `Power` above 0. With `0`, the flight lasts until the key is released.
4. Give `Fortification` a `Power` other than 0 when 50 % is not intended.
5. Give `Retribution` an `AmmunitionId`, and the worm tails a `Prefab` or an `ObjectPrefab`.
6. Do not change or remove device 18.
