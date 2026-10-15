# ComponentStats

Component stats are the passive contribution of a [Component](Component.md) to its ship: armor, energy, shield, weight, resistances, engine power, and ship-wide weapon and drone bonuses. Each component references one record. Several components can share a record. The ship builder sums the stats of all installed components into the ship stats.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `11` |
| Kind | collection |
| Folder | [`Resources/Database/Component/Stats/`](../../../Assets/Modules/Database/Resources/Database/Component/Stats/) |
| Records in the built-in catalog | 79: 56 `PerOneCell`, 23 `PerComponent`. Three records serve 188 components (111, 48, and 29). |
| Schema | [ComponentStats.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ComponentStats.xml) |
| Generated class | `GameDatabase.DataModel.ComponentStats` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/ComponentStats.cs)) |
| Runtime form | `ShipEquipmentStats` ([ShipEquipmentStats.cs](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs)), built by `FromComponent` and summed per ship |

## Fields

The record also has the [common fields](index.md#common-fields). All numbers are `float` unless noted. For `PerOneCell` records, every value is multiplied by the component's cell count before use, the bonus fields included.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Type` | enum [ComponentStatsType](enums.md#componentstatstype) | `PerComponent` | `PerOneCell` multiplies every value by the cell count of the component `Layout` ([ShipEquipmentStats.cs:62](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs#L62)). |
| `ArmorPoints` | float | ±1000000 | Added to the armor sum before the armor multipliers. |
| `ArmorRepairRate` | float | ±1000000 | Armor repaired per second, multiplied by the armor multipliers. |
| `ArmorRepairCooldownModifier` | float | −1…1 | Added to the armor repair delay multiplier `1 + Σ`. The delay is `ShipSettings.ArmorRepairCooldown × max(0, multiplier)`. `-0.5` halves the delay. |
| `EnergyPoints` | float | ±1000000 | Added to the energy capacity. |
| `EnergyRechargeRate` | float | ±1000000 | A positive value is recharge per second, multiplied by the energy multipliers. A negative value is a constant consumption, not multiplied ([line 71](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs#L71)). See [Engines and energy](#engines-and-energy). |
| `EnergyRechargeCooldownModifier` | float | −5…5 | Added to the energy recharge delay multiplier, like `ArmorRepairCooldownModifier`. |
| `ShieldPoints` | float | ±1000000 | Added to the shield capacity. A ship with shield points gets a shield bubble in combat. |
| `ShieldRechargeRate` | float | ±1000000 | Shield recharge per second. |
| `ShieldRechargeCooldownModifier` | float | −5…5 | Added to the shield recharge delay multiplier. |
| `Weight` | float | ±1000000 | A positive value is equipment weight, multiplied by the hull's `EquipmentWeightBonus`. A negative value is a weight reduction, added after that multiplier ([line 80](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs#L80)). |
| `RammingDamage` | float | ±1000000 | Ramming value `R`. The ship's ram multiplier is `1 + R / (R + armor)`. See [Ramming and absorption](#ramming-and-absorption). |
| `EnergyAbsorption` | float | ±1000000 | Absorption value `A`, multiplied by the armor bonus. The ship gains `A / (armor + A)` of the energy damage it takes as energy. It does not reduce the damage. |
| `KineticResistance`, `EnergyResistance`, `ThermalResistance` | float | ±1000000 | Resistance values. `ThermalResistance` is heat resistance. The percent formula is on [ShipSettings: Stat formulas](ShipSettings.md#stat-formulas). |
| `EnginePower` | float | 0…2000 | Engine power `P` for speed and acceleration. See [ShipSettings: Engine formulas](ShipSettings.md#engine-formulas). |
| `TurnRate` | float | 0…2000 | Turn power `T`. |
| `Autopilot` | bool | | A player ship with this component gets an extra AI controller in combat: `CombatSettings.AutopilotAI`, or the old autopilot ([ShipFactory.cs:208](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L208)). Only component 216 sets it. |
| `DroneRangeModifier` | float | −50…50 | Bonus to the range of every drone bay: range × `(1 + Σ)`. |
| `DroneDamageModifier`, `DroneDefenseModifier`, `DroneSpeedModifier` | float | −50…50 | Added to the damage, defense, and speed multipliers of every drone bay. See [DroneBay: Drone stats](DroneBay.md#drone-stats). |
| `DronesBuiltPerSecond` | float | 0…100 | Added to `ShipSettings.BaseDroneReconstructionSpeed`. A ship rebuilds drones only when the total is above 0. |
| `DroneBuildTimeModifier` | float | 0…100 | Added to the build time multiplier `1 + Σ`. The rebuild speed is divided by it, so a positive value slows rebuilding ([ShipStatsCalculator.cs:224](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L224)). |
| `WeaponFireRateModifier`, `WeaponDamageModifier`, `WeaponRangeModifier`, `WeaponEnergyCostModifier` | float | −100…100 | Added to ship-wide multipliers `1 + Σ` for every weapon of the ship. The damage multiplier also includes the ship bonuses ([ShipStatsCalculator.cs:192](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L192), [ShipBuilder.cs:500](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L500)). |
| `AlterWeaponPlatform` | int | obsolete | Old turret type. Migration 1.0 → 1.1 converts 1 → `AutoAimingArc` 360, 2 → 80, 3 → 20 ([DatabaseUpgrader_0_1.cs:115](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_0_1.cs#L115)). |
| `AutoAimingArc` | float | 0…360 | Raises the auto-aiming arc of the barrel under the component to at least this value, in degrees. |
| `TurretTurnSpeed` | float | ±1000 | Added to the rotation speed of the barrel under the component, in degrees per second. A barrel without its own speed starts at 270. |

## Notes

### Summing

The ship builder adds the stats of every installed component, satellite components included ([ShipEquipmentStats.cs:125](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs#L125)). Bonus fields add up: two components with `WeaponDamageModifier` 0.1 give `× 1.2`, not `× 1.21`. A [modification](ComponentMod.md#effects) changes the values of its own component before the sum.

`AutoAimingArc` and `TurretTurnSpeed` are not summed. They change only the barrel that the component sits on, so they need a component on weapon cells with a barrel ([CommonComponent.cs:56](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L56)). Only one built-in record sets them.

### Engines and energy

`FromComponent` splits the engine values by energy use ([ShipEquipmentStats.cs:95](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipEquipmentStats.cs#L95)):

1. A record with `EnergyRechargeRate` ≥ 0 also adds its `EnginePower` and `TurnRate` to the engine that works without energy.
2. A record with `EnginePower` > 0 and a negative `EnergyRechargeRate` adds the consumption to the engine energy consumption. The `Accelerator` device multiplies its energy cost by this value ([Device: Device classes](Device.md#device-classes)). The per-cell multiplier is missing here ([DB15.2](../../issues/database.md#db15-smaller-defects)).

When the ship's energy runs out, combat switches to the engine without energy ([ShipFactory.cs:306](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L306)).

### Ramming and absorption

| Value | Formula | Code |
| --- | --- | --- |
| Ram multiplier | `(1 + R / (R + armor))` × ship ram bonuses | [ShipStatsCalculator.cs:205](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L205) |
| Ram damage on a new contact with an enemy | `0.1 × relative speed × min(weights) × multiplier`, only when above 1 | [DefaultCollisionBehaviour.cs:21](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/DefaultCollisionBehaviour.cs#L21) |
| Energy from absorption per hit | `A / (armor + A) × energy damage` | [ShipStats.cs:91](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Stats/ShipStats.cs#L91) |

### Data rules

1. Use `PerOneCell` for stats that should grow with the component size. The bonus fields are multiplied too: a 4-cell `PerOneCell` component with `WeaponDamageModifier` 0.1 gives +40 %.
2. Use a negative `EnergyRechargeRate` for consumers and a positive one for reactors. An engine record with `EnergyRechargeRate` ≥ 0 keeps working when the ship has no energy.
3. A shared record changes every component that uses it. Create a new record before you change one component.
4. Keep `ArmorRepairCooldownModifier` and the other delay modifiers above −1 in total, or the delay becomes 0.
