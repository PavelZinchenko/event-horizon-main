# ComponentMod

A component modification is a random "quality" property of a component item, for example "+20 % damage" or "−30 % weight". An item has at most one modification and one [quality](enums.md#modificationquality). The quality selects which column of the modification's values applies. [Component](Component.md) lists the modifications it can roll in `PossibleModifications`. Loot, shops, crafting, enemy upgrades, the ship builder, and saves use modification records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `12` |
| Kind | collection |
| Folder | [`Resources/Database/Component/Modifications/`](../../../Assets/Modules/Database/Resources/Database/Component/Modifications/) |
| Records in the built-in catalog | 21 |
| Schema | [ComponentMod.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/ComponentMod.xml) |
| Generated class | `GameDatabase.DataModel.ComponentMod` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/ComponentMod.cs), [hand-written part](../../../Assets/Modules/Database/Scripts/DataModel/ComponentMod.cs)) |
| Runtime wrapper | `ComponentModification` ([ComponentModification.cs](../../../Assets/Modules/ShipConstructor/Scripts/Modification/ComponentModification.cs)): the modification plus the item quality |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Description` | string | | Localization key of the item text. The text receives one format argument per row of `Modifications`, in row order: `{0}`, `{1}`, … The game formats each value as a signed percent, or as a signed number for `ExtraHitPoints` ([ComponentModification.cs:20](../../../Assets/Modules/ShipConstructor/Scripts/Modification/ComponentModification.cs#L20)). |
| `Modifications` | struct list [StatModification](structures.md#statmodification) | | The stat changes. Each row names a stat and gives one value per quality. An empty list gives no effect and an empty text. |

## Notes

### Quality

| Quality | Column | Item quality in the UI | Price factor |
| --- | --- | --- | ---: |
| `N3` | `Gray3` | Low | 50 % |
| `N2` | `Gray2` | Low | 75 % |
| `N1` | `Gray1` | Low | 90 % |
| `P1` | `Green` | Medium | 150 % |
| `P2` | `Purple` | High | 200 % |
| `P3` | `Gold` | Perfect | 300 % |

An item without a modification is Common quality and uses the base price ([ComponentInfo.cs:227](../../../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs#L227)). The game resets the quality of such an item to `N3`, which has no effect.

Sources of qualities:

| Source | Quality | Code |
| --- | --- | --- |
| Random loot and shops | From the level difference, see [Component: Random selection](Component.md#random-selection) | [ComponentQuality.cs:49](../../../Assets/Modules/ShipConstructor/Scripts/ComponentQuality.cs#L49) |
| Workshop crafting | `P1`, `P2`, or `P2`…`P3` | [Technology.cs:204](../../../Assets/Scripts/Domain/Technologies/Technology.cs#L204) |
| Enemy builds of a difficulty class | `P1`…`P3` | [IComponentConverter.cs:60](../../../Assets/Modules/ShipConstructor/Scripts/Builder/IComponentConverter.cs#L60), [ShipBuild: Difficulty class](ShipBuild.md#difficulty-class) |
| Christmas rewards and shop | `P2`…`P3` | [LootGenerator.cs:318](../../../Assets/Scripts/Domain/Economy/LootGenerator.cs#L318) |
| Arena shop | `P3` | [ArenaInventory.cs:49](../../../Assets/Scripts/Legacy/Game/Quests/Inventory/ArenaInventory.cs#L49) |
| Builds (`InstalledComponent`) | `Quality` field | [structures.md](structures.md#installedcomponent) |

The modification itself is a random entry of the component's `PossibleModifications` in all random cases.

### Effects

`ComponentModification` applies each row to a part of the component ([ComponentModification.cs:45](../../../Assets/Modules/ShipConstructor/Scripts/Modification/ComponentModification.cs#L45)). `v` is the value of the row's quality column. A type that a part does not know has no effect on that part.

| Type | Passive stats | Device | Weapon | Drone bay |
| --- | --- | --- | --- | --- |
| `WeaponDamage`, `WeaponRange`, `WeaponFireRate`, `WeaponBulletSpeed`, `WeaponAoe` | | | × `v` | |
| `WeaponBulletMass` | | | weight × `v` (impulse and recoil) | |
| `DroneAttack`, `DroneDefense`, `DroneSpeed` | the component's own drone bonus × `v` | | | multiplier + `(v − 1)` |
| `DroneRange` | the component's own drone range bonus × `v` | | | range × `v` |
| `EnergyCapacity`, `ShieldPoints`, `ShieldRechargeRate`, `ArmorPoints`, `Mass` | × `v`, only when the value is positive | | | |
| `EnergyRechargeRate`, `ArmorRepairRate` | × `v` | | | |
| `Resistance` | each positive resistance × `v` | | | |
| `EnginePower`, `EngineTurnRate` | × `v`, with and without energy | | | |
| `DeviceCooldown`, `DeviceRange`, `DevicePower` | | × `v` | | |
| `EnergyCost` | consumption × `v` | activation energy × `v`, when positive | energy cost × `v` | launch energy × `v`, when positive |
| `ExtraHitPoints` | armor + `v` | | | |

Notes on the table:

1. Passive stats are the [ComponentStats](ComponentStats.md) after the per-cell multiplier, plus the passive energy of the device and the drone bay ([CommonComponent.cs:21](../../../Assets/Modules/ShipConstructor/Scripts/Component/CommonComponent.cs#L21)). `EnergyCost` therefore also scales that passive energy.
2. A drone bonus on passive stats changes only a bonus that the component already has: a `DroneAttack` value of 1.5 turns +10 % into +15 % ([StatMultiplier.cs:27](../../../Assets/Modules/CommonComponents/Scripts/DataTypes/StatMultiplier.cs#L27)).
3. Weapons with [AmmunitionObsolete](AmmunitionObsolete.md) apply the same types to the old ammunition stats ([ComponentModification.cs:152](../../../Assets/Modules/ShipConstructor/Scripts/Modification/ComponentModification.cs#L152)).
4. `WeaponDamage` and `DevicePower` do not change passive stats. A damage modification on a passive module does nothing.

### Validity and saves

An item's modification is valid when it is empty or in the component's `PossibleModifications` ([ComponentInfo.cs:100](../../../Assets/Modules/ShipConstructor/Scripts/ComponentInfo.cs#L100)). An invalid one is replaced on load (see [Component: Saves](Component.md#saves)).

The session stores the modification ID and the quality, not the values. A changed value changes every existing item with that modification. A removed record makes the loader return the empty modification, so saved items silently lose their modification.

### Built-in records

| ID | Description | Types |
| ---: | --- | --- |
| 1 | `$WeightMod` | `Mass` |
| 2 | `$EnergyCostMod` | `EnergyCost` |
| 3 | `$DefenseMod` | `ArmorPoints`, `Resistance`, `ShieldPoints` |
| 4 | `$HitPointsMod` | `ExtraHitPoints` (−3…+5) |
| 5, 17 | `$DamageMod` | `WeaponDamage`; `DroneAttack` |
| 6 | `$CooldownMod` | `DeviceCooldown`, `WeaponFireRate` |
| 7, 20 | `$RangeMod` | `WeaponRange`, `DeviceRange`; `DroneRange` |
| 8 | `$BulletVelocityMod` | `WeaponBulletSpeed` |
| 9 | `$EnergyMod` | `EnergyCapacity` |
| 10 | `$RepairRateMod` | `ArmorRepairRate`, `DevicePower` |
| 11 | `$EnginePowerMod` | `EnginePower`, `EngineTurnRate` |
| 12 | `$EnergyRechargeMod` | `EnergyRechargeRate`, `ShieldRechargeRate` |
| 13 | `$BulletVelocityMod2` | `WeaponBulletSpeed`, `WeaponDamage` |
| 14 | `$AreaOfEffectMod` | `WeaponAoe` |
| 15 | `$ShieldPowerMod` | `DevicePower`, `ShieldPoints` |
| 16 | `$DamageMod2` | `WeaponDamage`, `DeviceCooldown`, `WeaponFireRate` |
| 18 | `$DefenseMod` | `DroneDefense` |
| 19 | `$DroneSpeedMod` | `DroneSpeed` |
| 21 | `$RecoilMod` | `WeaponBulletMass` |

### Data rules

1. Make positive qualities better. For cost-like stats (`Mass`, `EnergyCost`, `DeviceCooldown`) use values below 1 in `Green`, `Purple`, and `Gold`.
2. Use multipliers (`1.2` = +20 %) for every type except `ExtraHitPoints`, which takes an absolute number of armor points.
3. Keep the `{n}` placeholders of the description in the same order as the rows.
4. Give the modification only to components with the matching part. A `DroneAttack` row has no effect on a component without drone bonuses or a drone bay.
5. Do not change or remove a record that players may own. Saves keep the ID and read the values from the current database.
