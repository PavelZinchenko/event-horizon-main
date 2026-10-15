# ShipModSettings

Ship modification settings hold the values of the ship modifications (perks) and hide some of them from the shipyard. A ship modification changes the whole ship, not one component. The player buys modifications in a shipyard. A [ShipBuild](ShipBuild.md) can carry up to three of them as `Perks`.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `105` |
| Kind | settings (singleton) |
| File | [`Resources/Database/Settings/ShipModifications.json`](../../../Assets/Modules/Database/Resources/Database/Settings/ShipModifications.json). The built-in file sets no field, so every value is the schema default. |
| Schema | [ShipModSettings.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Settings/ShipModSettings.xml) |
| Generated class | `GameDatabase.DataModel.ShipModSettings` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/ShipModSettings.cs)), read as `IDatabase.ShipModSettings` |
| Consumer | [`ModificationFactory`](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationFactory.cs), [`InfectedModification`](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/InfectedModification.cs) |

## Fields

The record has the `ItemType` [common field](index.md#common-fields) and no `Id`.

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `RemoveWeaponSlotMod` | bool | | The shipyard does not offer `WeaponClass`. |
| `RemoveUnlimitedRespawnMod` | bool | | No effect. The code that reads it is commented out ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)). |
| `RemoveEnergyRechargeCdMod` | bool | | The shipyard does not offer `EnergyRechargeCooldown`. |
| `RemoveShieldRechargeCdMod` | bool | | The shipyard does not offer `ShieldRechargeCooldown`. |
| `RemoveBiggerSatellitesMod` | bool | | The shipyard does not offer `SatelliteSize`. |
| `HeatDefenseValue` | float | 0.5, 0…10 | Heat resistance bonus of `HeatDefense`. |
| `KineticDefenseValue` | float | 0.5, 0…10 | Kinetic resistance bonus of `KineticDefense`. |
| `EnergyDefenseValue` | float | 0.5, 0…10 | Energy resistance bonus of `EnergyDefense`. |
| `RegenerationValue` | float | 0.01, 0…1 | Armor repair of `Infected`, as a fraction of armor per second. |
| `RegenerationArmor` | float | 0.85, 0…1 | Armor multiplier of `Infected`. `0.85` means −15 % armor. |
| `WeightReduction` | float | 0.8, 0…1 | Hull weight multiplier of `LightWeight`. `0.8` means −20 % hull weight. The name suggests the reduction, but the value is the multiplier. |
| `AttackReduction` | float | 0.2, 0…1 | Damage reduction of `WeaponClass`. Damage is multiplied by `1 − value`. |
| `EnergyReduction` | float | 0.5, 0…1 | Energy reduction of `EnergyRechargeCooldown`. Energy is multiplied by `1 − value`. |
| `ShieldReduction` | float | 0.5, 0…1 | Shield reduction of `ShieldRechargeCooldown`. Shield is multiplied by `1 − value`. |

The removal flags change only the shipyard list ([ModificationFactory.cs:53](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationFactory.cs#L53)). Ship build perks and modifications already on saved ships keep working.

## Notes

### Modifications

`ShipPerkType` (the database enum) maps to the runtime `ModificationType` ([ModificationType.cs:27](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationType.cs#L27)). The two enums use different numbers. Saved games store `ModificationType`.

| Perk | Effect | Settings | Shipyard rule |
| --- | --- | --- | --- |
| `AutoTargeting` | Every barrel gets an aiming arc of at least 20°. | | Only when a hull barrel has arc 0. Once per ship. |
| `HeatDefense`, `KineticDefense`, `EnergyDefense` | Adds the value to the resistance bonus (see [ShipSettings: Stat formulas](ShipSettings.md#stat-formulas)). | `...DefenseValue` | Can repeat. |
| `LightWeight` | Multiplies the hull weight. | `WeightReduction` | Can repeat. |
| `Infected` | Adds armor repair, multiplies armor, and adds Device 18 (toxic waste) as a built-in device. The ship does not become bionic. | `RegenerationValue`, `RegenerationArmor` | Once per ship. |
| `UnlimitedRespawn` | Not implemented. Creating the ship throws ([DB1](../../issues/database.md#db1-the-unlimitedrespawn-perk-crashes-ship-creation)). | | Never offered. |
| `WeaponClass` | Removes the slot restriction of every barrel and lowers damage. | `AttackReduction` | Only when a hull barrel has a restriction. Hidden by `RemoveWeaponSlotMod`. |
| `SatelliteSize` | Raises the maximum satellite class by one (at least `Cruiser`) and the maximum satellite scale by 0.4. | | Only on hulls smaller than `Titan`. Hidden by `RemoveBiggerSatellitesMod`. |
| `EnergyRechargeCooldown` | Lowers energy and multiplies the energy recharge delay by 0.01. | `EnergyReduction` | Hidden by `RemoveEnergyRechargeCdMod`. |
| `ShieldRechargeCooldown` | Lowers shield and removes the shield recharge delay. | `ShieldReduction` | Hidden by `RemoveShieldRechargeCdMod`. |

Each modification costs 10 stars in the shipyard. Removing one is free ([ModificationType.cs:91](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationType.cs#L91)). The number of modification slots grows with [shipyard upgrades](Ship.md#shipyard-upgrades). The shipyard checks the rules in [ModificationType.cs:46](../../../Assets/Modules/ShipConstructor/Scripts/Ships/Modification/ModificationType.cs#L46). Ship build perks skip these checks.

### Data rules

1. Keep Device 18 in the catalog while `Infected` is in use. A mod with `UnloadOriginalDatabase` must provide it ([DB15.10](../../issues/database.md#db15-smaller-defects)).
2. A mod settings record replaces the whole record ([DB7](../../issues/database.md#db7-settings-records-cannot-be-partly-overridden)).
