# ShipSettings

Ship settings hold the base values of the ship stat formulas, the speed limits, the global switch for cell expansions, and the default explosions. The ship stats calculator, the engine, combat, and the shipyard read them.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `100` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/Ships.json`](../../../Assets/Modules/Database/Resources/Database/Settings/Ships.json) |
| Schema | [ShipSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/ShipSettings.xml) |
| Generated class | `GameDatabase.DataModel.ShipSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/ShipSettings.cs)), read as `IDatabase.ShipSettings` |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`. The *Built-in* column shows the value in the built-in catalog.

| Field | Type | Default / range | Built-in | Meaning |
| --- | --- | --- | --- | --- |
| `DefaultWeightPerCell` | float | 1…1000000 | 20 | Hull weight per cell. Satellites use it too. |
| `MinimumWeightPerCell` | float | 1…1000000 | 10 | Lower weight bound per cell. The bound is at least 5 cells. |
| `BaseArmorPoints` | float | 0…1000000 | 0 | Armor of every ship, before multipliers. |
| `ArmorPointsPerCell` | float | 0…1000000 | 0.5 | Armor per hull cell, before multipliers. |
| `ArmorRepairCooldown` | float | 0…60 | 1 | Seconds of delay after armor damage before armor repair resumes. Components scale it with `ArmorRepairCooldownModifier`; they add no delay of their own. |
| `BaseEnergyPoints` | float | 0…1000000 | 1 | Energy of every ship, before multipliers. |
| `BaseEnergyRechargeRate` | float | 0…1000000 | 0.1 | Energy per second of every ship. `EnergyBonus` does not multiply it. |
| `EnergyRechargeCooldown` | float | 0…60 | 1 | Seconds of delay after energy use before recharge resumes. |
| `BaseShieldRechargeRate` | float | 0…1000000 | 0 | Shield points per second of every ship with a shield. |
| `ShieldRechargeCooldown` | float | 0…60 | 4 | Seconds of delay after shield damage before shield recharge resumes. |
| `BaseDroneReconstructionSpeed` | float | 0…100 | 0 | Drone rebuild speed of every ship with drone bays. `0` means only components rebuild drones. |
| `ShieldCorrosiveResistance` | float | 0.9, 0…1 | 0.9 | Shield resistance to corrosive damage. A shield loses `(1 − value) ×` the corrosive damage it blocks ([Impact.cs:148](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Impact.cs#L148)). |
| `MaxVelocity` | float | 30, 5…100 | 40 | Speed limit in world units per second. |
| `MaxAngularVelocity` | float | 30, 5…100 | 40 | Turn speed limit in turn units. One unit is 30°/s, so 40 is 1200°/s. |
| `MaxAcceleration` | float | 300, 5…1000 | 400 | Acceleration limit. |
| `MaxAngularAcceleration` | float | 300, 5…1000 | 400 | Turn acceleration limit. |
| `DisableCellsExpansions` | bool | | `false` | Hulls with `CellsExpansions` `Default` cannot buy extra cells. See [Ship: Cell expansions](Ship.md#cell-expansions). |
| `ShipExplosionEffect` | ref [VisualEffect](VisualEffect.md) | | 20 | Default explosion of ships with `ModelScale` ≥ 0.9. |
| `ShipExplosionSound` | audio | | `explosion_01` | Default explosion sound of these ships. |
| `DroneExplosionEffect` | ref [VisualEffect](VisualEffect.md) | | 21 | Default explosion of ships with `ModelScale` < 0.9. |
| `DroneExplosionSound` | audio | | `explosion_10` | Default explosion sound of these ships. |

## Notes

### Stat formulas

[`ShipStatsCalculator`](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs) combines four sources:

- `n`: the hull cell count, including bought cells.
- *eq*: the summed stats of the installed components ([ComponentStats](ComponentStats.md)).
- *hull*: the multipliers from [`Ship.Features`](Ship.md#shipfeatures) and the ship's perks.
- *bonus*: the multipliers from skills, ship level, and drone bays (for drones).

| Stat | Formula | Code |
| --- | --- | --- |
| Armor | `(BaseArmorPoints + ArmorPointsPerCell × n + eq.armor) × hull.armor × bonus.armor`. A sum below 1 gives 0. | [line 99](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L99) |
| Energy | `(BaseEnergyPoints + eq.energy) × hull.energy × bonus.energy` | [line 111](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L111) |
| Shield | `eq.shield × hull.shield × bonus.shield` | [line 112](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L112) |
| Energy recharge | `BaseEnergyRechargeRate − eq.consumption + eq.recharge × hull.energy × bonus.energy` | [line 114](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L114) |
| Shield recharge | `(eq.shieldRecharge + BaseShieldRechargeRate) × hull.shield × bonus.shield × bonus.shieldRecharge` | [line 117](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L117) |
| Armor repair | `eq.repair × hull.armor × bonus.armor + armor × regeneration`. Regeneration is 0.01 for bionic hulls plus the `Infected` perk value. | [line 119](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L119) |
| Recharge delays | `(eq.delay + ShipSettings delay) × max(0, eq and hull delay multipliers)` | [line 162](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L162) |
| Resistance value | `R = eq.R × armorMult + (armor + eq.R × armorMult) × (hull feature + perk + skill)` | [line 128](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L128) |
| Resistance percent | `R / (armor + R)` | [line 167](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L167) |
| Weight | `max(MinimumWeightPerCell × max(n × hull.weight, 5), DefaultWeightPerCell × n × hull.weight + eq.weight × hull.equipmentWeight + eq.weightReduction) / 1000` | [line 171](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L171) |
| Drone build speed | `(BaseDroneReconstructionSpeed + eq.speed) × hull.droneBuildSpeed / eq.timeMultiplier` | [line 224](../../../Assets/Modules/ShipConstructor/Scripts/Model/ShipStatsCalculator.cs#L224) |

Combat creates the armor, shield, and energy pools from these values ([ShipStats.cs:33](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Stats/ShipStats.cs#L33)). A ship with less than 0.1 armor logs `Creating ship with zero armor` and has no armor pool. A pool recharges at its rate per second. After the ship spends or loses points, the pool waits for its delay before it recharges ([IResourcePoints.cs:60](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Unit/HitPoints/IResourcePoints.cs#L60)).

### Engine formulas

[`EngineStats`](../../../Assets/Modules/ShipConstructor/Scripts/Model/EngineStats.cs#L28) uses the engine power `P` and turn rate `T` from components (multiplied by `VelocityBonus`, `TurnRateBonus`, and bonuses), the weight `w`, and the cell count `n`:

| Value | Formula |
| --- | --- |
| Speed | `min(MaxVelocity, P × 50 / (√w × n))` |
| Acceleration | `min(MaxAcceleration, P × 100 / (w × n))` |
| Turn speed | `min(MaxAngularVelocity, T × 50 / (√w × n)) × 30` degrees per second |
| Turn acceleration | `min(MaxAngularAcceleration, T × 200 / (w^1.5 × n)) × 30` |

Combat builds a second engine from the engine stats that work without energy. The ship uses it when its energy runs out ([ShipFactory.cs:306](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L306)).

### Explosions

A hull's `VisualEffects` override the defaults. With no effect at all, an old hard-coded explosion plays. See [Ship: Combat object](Ship.md#combat-object).

### Data rules

1. Keep `ArmorPointsPerCell` or `BaseArmorPoints` above 0. Otherwise ships without armor components start with no armor.
2. Keep `BaseEnergyRechargeRate` above 0, or every ship needs an energy component. The startup check rejects builds with an energy recharge of 0 or less ([ShipBuild: Startup check](ShipBuild.md#startup-check)).
3. A mod settings record replaces the whole record. Copy every value, not only the changed one ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
