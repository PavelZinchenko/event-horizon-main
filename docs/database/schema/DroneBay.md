# DroneBay

A drone bay launches drones: small ships that fight next to the mother ship. The bay record sets the launch cost, the number of drones, the drone stat multipliers, the operating range, and the drone AI. A [Component](Component.md) brings a bay through `DroneBayId`, and the drone itself through `DroneId`, a [ShipBuild](ShipBuild.md). The ship builder and combat read drone bay records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `5` |
| Kind | collection, legacy (one `DroneBayStats` struct) |
| Folder | [`Resources/Database/Dronebay/`](../../../Assets/Modules/Database/Resources/Database/Dronebay/) |
| Records in the built-in catalog | 6, used by 48 components. All have multipliers 1, launch cost 5, and `Mixed` activation. None sets an AI tree. |
| Schema | [DroneBay.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/DroneBay.xml) |
| Generated class | `GameDatabase.DataModel.DroneBay` with the struct `DroneBayStats` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/DroneBay.cs)) |
| Runtime | [`DroneBay`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs), created by [`DroneBayFactory`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/DroneBayFactory.cs). Overview: [BattleSimulator: Drone bays](../../battle_simulator.md#drone-bays-replicators-and-clones). |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `EnergyConsumption` | float | 0…999999999 | Energy per launch, taken from the ship. |
| `PassiveEnergyConsumption` | float | 0…999999999 | Permanent consumption of the component, × `(0.5 + 0.005 × n)` with `n` = hull cell count ([CommonComponent.cs:29](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L29)). |
| `Range` | float | 1…1000 | Operating range of the drones around the mother ship. The drone AI receives it as its limit. |
| `DamageMultiplier` | float | 0.01…1000 | Drone damage multiplier. |
| `DefenseMultiplier` | float | 0.01…1000 | Drone armor and shield multiplier. |
| `SpeedMultiplier` | float | 0.01…1000 | Drone speed and turn multiplier. Each bay adds a random ±0.2 once per battle ([DroneBay.cs:40](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs#L40)). |
| `BuildExtraCycles` | int | 0…100 | Extra rebuild cycles per drone. A lost drone needs `1 + BuildExtraCycles` cycles. |
| `ImprovedAi` | bool | obsolete, still read | With no AI tree, the drone uses the old hard-coded drone AI in its improved form instead of the `CombatSettings` trees. The editor cannot set it ([DB9](../../issues/database.md#db9-live-fields-are-marked-obsolete)). No built-in record sets it. |
| `Capacity` | int | 1…1000 | Drones in the hangar at the start, and the maximum of drones in the hangar and in space together. |
| `ActivationType` | enum [ActivationType](enums.md#activationtype) | `None` | How launches are triggered. See [Device: Activation](Device.md#activation). |
| `LaunchSound` | audio | | Played on each launch. |
| `LaunchEffectPrefab` | prefab (`Effect`) | | White flash of size 1 on each launch. |
| `ControlButtonIcon` | image (`ActionButton`) | | Icon of the combat button. |
| `DefensiveDroneAI` | ref [BehaviorTree](BehaviorTree.md) | | AI of drones in defensive mode. |
| `OffensiveDroneAI` | ref [BehaviorTree](BehaviorTree.md) | | AI of drones in aggressive mode. |

## Notes

### Launch

A launch needs a drone in the hangar, the bay platform ready, and `EnergyConsumption` in the ship ([DroneBay.cs:45](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs#L45)). All bays of a ship share one launch platform with a 0.4 s cooldown. Drones and clones get no drone bays, so drones never launch drones ([ShipFactory.cs:141](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L141)).

`ShipBuilder` has an `ExtraDroneBayCapacity` property that adds to `Capacity`, but no code sets it ([ShipBuilder.cs:151](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L151)).

### Rebuilding drones

A ship rebuilds drones only when its drone build speed is above 0 ([ShipFactory.cs:155](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L155)). The speed comes from `ShipSettings.BaseDroneReconstructionSpeed`, the components' `DronesBuiltPerSecond` and `DroneBuildTimeModifier`, and the hull's `DroneBuildSpeedBonus` (see [ShipSettings: Stat formulas](ShipSettings.md#stat-formulas)).

1. One replicator serves all bays of the ship. A cycle takes `1 / speed` seconds.
2. Each cycle goes to the first bay that has fewer drones than `Capacity`. It either uses up one extra cycle or puts one drone back into the hangar ([DroneBay.cs:88](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs#L88)).
3. The replicator stops when every bay is full and starts again when a bay loses a drone.

### Drone stats

The drone stats come from four sources, applied in this order:

1. The bay values, changed by the component's [modification](ComponentMod.md#effects): `DroneAttack`, `DroneDefense`, and `DroneSpeed` add `v − 1`, `DroneRange` multiplies the range.
2. The ship bonuses ([ShipBuilder.cs:260](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L260)): the sum of the components' drone modifiers is added to each multiplier. The hull's `DroneAttackBonus` and `DroneDefenseBonus` are added too. The range is multiplied by `1 + Σ DroneRangeModifier`.
3. Damage × the ship's damage bonus, and defense × the ship's armor bonus, from skills and ship level.
4. The bay builds the drone from its `ShipBuild` with these multipliers ([DroneBay.cs:36](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs#L36)). They replace the build's difficulty class boost ([DB15.14](../../issues/database.md#db15-smaller-defects)).

The drone takes the hull, components, `ExtendedLayout`, and `CustomAI` of the build. It ignores perks, satellites, and color.

### Drone AI

The AI is chosen in this order ([DroneBay.cs:33](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/DroneBays/DroneBay.cs#L33), [ControllerFactory.cs:67](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ControllerFactory.cs#L67)):

1. The bay tree for the component's mode: `DefensiveDroneAI` when `InstalledComponent.Behaviour` is 1, else `OffensiveDroneAI`.
2. The `CustomAI` of the drone build.
3. Without `ImprovedAi`: `CombatSettings.OffensiveDroneAI` or `DefensiveDroneAI`.
4. Without any tree: the old hard-coded drone AI.

The player switches the mode in the ship editor. Both modes use the same stats.

### Other consumers

- Random exploration outposts pick drone bays with `Capacity` 1 as extra defenders ([RandomOutpostBuilder.cs:87](../../../Assets/Scripts/Domain/Exploration/RandomOutpostBuilder.cs#L87)).
- The startup check accepts a ship with a drone bay and no weapon ([ShipBuild: Startup check](ShipBuild.md#startup-check)).

### Data rules

1. Keep the multipliers at 1 for normal drones. Use the bonus fields on [ComponentStats](ComponentStats.md) for ship-wide drone upgrades.
2. Pair the bay with a drone build that has weapons, and preferably a hull of type `Drone`. Any build works as a drone.
3. Set an AI tree on the bay only when this bay needs other behavior than all other drones.
