# Weapon

A weapon record is the firing behavior of a weapon component: how often it fires, how many bullets it fires, how the shots spread, and its sounds and muzzle flash. The bullet itself, with damage, range, speed, and energy cost, comes from the component's ammunition. A [Component](Component.md) brings a weapon through `WeaponId` together with `AmmunitionId`. The ship builder and combat read weapon records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `3` |
| Kind | collection, legacy (one `WeaponStats` struct) |
| Folder | [`Resources/Database/Weapon/`](../../../Assets/Modules/Database/Resources/Database/Weapon/) (21) and [`Weapon/Obsolete/`](../../../Assets/Modules/Database/Resources/Database/Weapon/Obsolete/) (89) |
| Records in the built-in catalog | 110: 73 `Common`, 20 `Continuous`, 6 `Manageable`, 6 `RequiredCharging`, 4 `MultiShot`, 1 `MashineGun`. Weapons 5, 6, and 18 have no component. |
| Schema | [Weapon.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Weapon/Weapon.xml) |
| Generated class | `GameDatabase.DataModel.Weapon` with the struct `WeaponStats` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Weapon.cs)) |
| Runtime | [`WeaponFactory`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/WeaponFactory.cs) creates one weapon system per installed weapon. Behavior: [BattleSimulator: Weapon creation](../../battle_simulator.md#weapon-creation). |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `WeaponClass` | enum [WeaponClass](enums.md#weaponclass) | `Common` | The firing behavior. See [Weapon classes](#weapon-classes). |
| `FireRate` | float | 0…100 | Rate in uses per second. Its exact meaning depends on the class. The ship's fire rate bonus and the modification multiply it. |
| `Spread` | float | 0…360 | Random direction range in degrees. Each bullet leaves within ±`Spread / 2` of the barrel direction ([BulletFactory.cs:195](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L195)). |
| `Magazine` | int | 0…999999999 | `MashineGun`: shots per burst. `MultiShot`: bullets per shot. Other classes ignore it. |
| `ActivationType` | enum [ActivationType](enums.md#activationtype) | `None` | How the weapon is triggered. `None` makes the weapon fire automatically whenever it can. All built-in weapons are `Manual`. See [Device: Activation](Device.md#activation). |
| `ShotSound` | audio | | Shot sound. When it plays depends on the class. |
| `ChargeSound` | audio | | Sound at the start of charging. Only `RequiredCharging` uses it. |
| `ShotEffectPrefab` | prefab (`Effect`) | | Muzzle flash. |
| `VisualEffect` | ref [VisualEffect](VisualEffect.md) | | Composite muzzle flash. It wins over `ShotEffectPrefab` ([WeaponFactory.cs:125](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/WeaponFactory.cs#L125)). |
| `EffectSize` | float | 0…100 | Multiplies the flash size. `0` keeps the size from the ammunition. |
| `ControlButtonIcon` | image (`ActionButton`) | | Icon of the combat button. |

## Notes

### Weapon classes

*Cost* is the energy cost of the ammunition. Every platform also waits 0.04 s between shots.

| Class | `FireRate` | `Magazine` | Energy | Sounds and flash | Code |
| --- | --- | --- | --- | --- | --- |
| `Common` | Cooldown `1 / FireRate` after each shot. `0` means no cooldown. | | Cost per shot | Sound and flash per shot | [CommonCannon.cs:13](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/CommonCannon.cs#L13) |
| `Manageable` | Cooldown after the bullet is gone | | Cost per shot | Sound on start and end | [ManageableCannon.cs:38](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/ManageableCannon.cs#L38) |
| `Continuous` | Cooldown between beams | | Cost per second while held | Loop sound. Flash on start and while active. | [ContinuousCannon.cs:33](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/ContinuousCannon.cs#L33) |
| `MashineGun` | Cooldown after each burst | Shots per burst | Cost per shot | Sound and flash per shot | [MachineGun.cs:16](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/MachineGun.cs#L16) |
| `MultiShot` | Cooldown after each volley | Bullets per volley | Cost × `Magazine` per volley | Sound and flash per volley | [MultishotCannon.cs:15](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/MultishotCannon.cs#L15) |
| `RequiredCharging` | Full charge takes `1 / FireRate` seconds | | Cost spread over the charge time | `ChargeSound` on start, `ShotSound` on release, a power-level effect | [ChargeableCannon.cs:17](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/ChargeableCannon.cs#L17) |

`Manageable`: releasing the key detonates the bullet. `RequiredCharging`: a release after at least 0.1 s fires with a power equal to the charge fraction. `MashineGun` and `MultiShot` give their bullets a random factor of 0.2 and 0.3 ([WeaponFactory.cs:70](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/WeaponFactory.cs#L70)).

### Multipliers

The weapon fires with `FireRate` × the ship's fire rate multiplier × the modification's `WeaponFireRate` value ([WeaponFactory.cs:29](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/WeaponFactory.cs#L29)). The ship multiplier is the sum of `WeaponFireRateModifier` over all components ([ComponentStats](ComponentStats.md)). Damage, range, and energy cost multipliers go to the ammunition in the same way. Weapons with [AmmunitionObsolete](AmmunitionObsolete.md) apply the multipliers to the old weapon and ammunition stats ([ShipBuilder.cs:415](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L415)).

### Muzzle flash

The flash color, size, and duration come from the ammunition ([WeaponFactory.cs:135](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/WeaponFactory.cs#L135)). `EffectSize` multiplies the size. A weapon without `VisualEffect` and `ShotEffectPrefab` has no flash. 3 built-in weapons use `VisualEffect`.

### Old and new ammunition

The same weapon table serves both bullet pipelines. The component's ammunition decides the pipeline, not the folder:

| Folder | With `AmmunitionObsolete` | With `Ammunition` |
| --- | ---: | ---: |
| `Weapon/` | 0 | 25 components |
| `Weapon/Obsolete/` | 0 | 90 components |

The 80 components with `AmmunitionObsolete` moved to `Ammunition` in phase 4, step 4c ([AmmunitionObsolete: Conversion](AmmunitionObsolete.md#conversion)).

### Other consumers

- The startup check rejects an enemy or player build when one shot costs more energy than the ship has ([ShipBuild: Startup check](ShipBuild.md#startup-check)).
- The ship editor shows damage per second from [`WeaponDamageCalculator`](../../../Assets/Modules/ShipConstructor/Scripts/Component/WeaponDamageCalculator.cs).
- AI reads the class, `Spread`, and `FireRate` through `WeaponInfo` ([WeaponInfo.cs](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/WeaponInfo.cs)).

### Data rules

1. Give `RequiredCharging` weapons a `FireRate` above 0. With `0`, the charge never grows and the shot has no power.
2. Give `MashineGun` and `MultiShot` weapons a `Magazine` of 1 or more. With `0` they fire nothing.
3. `FireRate` `0` on `Common`, `Manageable`, and `Continuous` weapons removes the cooldown. 17 built-in beams use it.
4. Keep `ActivationType` `Manual` unless the weapon should fire on its own.
5. A weapon record can serve several components. 7 built-in records do. A change applies to all of them.
