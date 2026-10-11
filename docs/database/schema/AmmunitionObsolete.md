# AmmunitionObsolete

An obsolete ammunition record is a bullet in the old format. One enum value, `AmmunitionClass`, selects fixed code for the movement, the contact effect, and the explosion. The other fields are numbers and assets for that code. Most built-in weapons still use this format. A [Component](Component.md) fires it through `AmmunitionId` together with a [Weapon](Weapon.md). New content should use [Ammunition](Ammunition.md).

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `4` |
| Kind | collection, legacy (one `AmmunitionObsoleteStats` struct) |
| Folder | [`Resources/Database/Ammunition/Obsolete/`](../../../Assets/Modules/Database/Resources/Database/Ammunition/Obsolete/) |
| Records in the built-in catalog | 81: 79 fired by 80 components, 1 used only as a coupled payload, 1 unused (5). See [Classes](#classes) for the count per class. |
| Schema | [AmmunitionObsolete.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/AmmunitionObsolete.xml) |
| Generated class | `GameDatabase.DataModel.AmmunitionObsolete` with the struct `AmmunitionObsoleteStats` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/AmmunitionObsolete.cs)) |
| Runtime | [`BulletFactoryObsolete`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs) and the class rules in [`AmmunitionClassExtensions`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/AmmunitionClassExtensions.cs) |

`Component.AmmunitionId` points to this table and to [Ammunition](Ammunition.md) with the same number. The built-in ID ranges do not overlap. See [Shared ID field](../../database.md#shared-id-field-componentammunitionid).

## Fields

The record also has the [common fields](index.md#common-fields). *p* is the power level: the charge of a `RequiredCharging` weapon, else 1. The weapon damage, range, and energy multipliers and the modification are applied to the struct before combat ([ShipBuilder.cs:434](../../../Assets/Modules/ShipConstructor/Scripts/Builder/ShipBuilder.cs#L434), [ComponentModification.cs:152](../../../Assets/Modules/ShipConstructor/Scripts/Modification/ComponentModification.cs#L152)).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `AmmunitionClass` | enum `AmmunitionClassObsolete` | `Common` | The behavior. See [Classes](#classes). |
| `DamageType` | enum [DamageType](enums.md#damagetype) | `Impact` | Type of all damage. An explosion of `Impact` damage is a smoke explosion. |
| `Impulse` | float | 0…10 | Bullet mass, × `(0.5 + 0.5p)`. It also sets the push of classes with a direct impulse, the turn rate of homing classes, and disarming: only bullets with less than 0.5 can be disarmed. The modification `WeaponBulletMass` multiplies it. |
| `Recoil` | float | 0…10 | Push on the launcher, × `(0.5 + 0.5p)`: `−Recoil ×` the bullet's speed relative to the ship ([BulletFactoryObsolete.cs:105](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs#L105)). Bound classes and `IgnoresShipVelocity` bullets have no recoil. |
| `Size` | float | 0…1000 | Body scale and muzzle flash size, × `(0.5 + 0.5p)`. The hit effect is 1.5 × this size. |
| `InitialPosition` | vector | | No effect. No code reads it ([DB11](../../issues/database.md#db11-schema-fields-without-runtime-effect)). 23 built-in records set it. |
| `AreaOfEffect` | float | 0…1000 | Radius of the explosion, cloud, web, or EMP, × `(0.5 + 0.5p)`. Size of an `Aura`. Spread range of cluster fragments. `IonBeam` explodes only when it is above 0. The modification `WeaponAoe` multiplies it. |
| `Damage` | float | 0…999999999 | Damage, × *p*. Per second for damage-over-time classes. The payload of carrier and cluster classes takes this value. |
| `Range` | float | 0…1000 | With `Velocity` above 0, the lifetime is `Range / Velocity`, or 1.2 × that for homing classes ([BulletFactoryObsolete.cs:78](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs#L78)). Beams use it as the ray length, homing classes as the search range, `BlackHole` as the gravity radius. Scaled by *p*, and 0 below *p* 0.1. |
| `Velocity` | float | 0…1000 | Launch speed. Classes with an engine launch at 10 % and accelerate. Beams ignore it. `IonBeam` uses `Velocity / 10` as its ray length. |
| `LifeTime` | float | 0…999999999 | Lifetime when `Velocity` or `Range` is 0, × `(0.5 + 0.5p)`. Beams with 0.5 or more deal damage that fades over the lifetime. Also: the flash time of beams, the duration of clouds, webs, and gravity, the slow-down strength of immobilizers, and the spin of `Singularity`. |
| `HitPoints` | int | 0…999999999 | Above 0, the bullet is a missile that other bullets can shoot down, and it detonates at the target. Hit points × the ship's armor multiplier. |
| `IgnoresShipVelocity` | bool | | The bullet does not inherit the ship's velocity and causes no recoil. 2 built-in records. |
| `EnergyCost` | float | 0…999999999 | Energy per shot. Beams: per second. `EnergySiphon` drains 4 × this value per second from the target. An EMP drains 10 × this value. |
| `CoupledAmmunitionId` | ref [AmmunitionObsolete](AmmunitionObsolete.md) | | Payload of `Carrier` and `HomingCarrier` (one bullet), and fragments of `FragBomb` and `ClusterMissile` (20 bullets). Model name `CoupledAmmunition`. The payload takes this record's `Damage` and `DamageType`. 2 built-in records set it. |
| `Color` | color | white | Bullet and explosion color. `Fireworks` mixes it 75 % with a random color per shot. |
| `FireSound` | audio | | Plays at launch. A looping clip plays while the bullet lives. |
| `HitSound` | audio | | Plays when the bullet detonates, and when it expires for classes that detonate on expiry. |
| `HitEffectPrefab` | prefab (`Effect`) | | Effect at each hit, for 0.2 s ([BulletFactoryObsolete.cs:212](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs#L212)). |
| `BulletPrefab` | prefab (`Bullet`) | | Prefab name under `Combat/Bullets/`. A missing name uses `Laser` for beams and `Plasma2` for other classes ([BulletFactoryObsolete.cs:39](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs#L39), [PrefabCache.cs:79](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L79)). |

## Classes

Rules come from [`AmmunitionClassExtensions`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/AmmunitionClassExtensions.cs). *Beam*: bound to the weapon, damage over time, energy per second. *Homing*: seeks a target, launches at 10 % speed. *Expiry*: the bullet also detonates when its lifetime ends.

| Class | Built-in | Movement | On contact | On detonation |
| --- | ---: | --- | --- | --- |
| `Common` | 18 | straight | damage, push | |
| `Fragment` | 1 | straight | damage, push | |
| `Acid` | 1 | area | damage over time | |
| `DamageOverTime` | 3 | area | damage over time | |
| `Aura` | 2 | area bound to the weapon, grows and shrinks | damage over time | |
| `Explosion` | 2 | area bound to the weapon | damage | strong explosion at once |
| `Bomb` | 5 | straight | | explosion; expiry |
| `UnguidedRocket` | 2 | rocket engine, no homing | | explosion; expiry |
| `Rocket` | 9 | homing | | explosion; expiry |
| `AcidRocket` | 3 | homing | push | acid cloud; expiry |
| `HomingTorpedo` | 0 | homing | damage | |
| `HomingImmobilizer` | 1 | homing | damage, slow-down | |
| `Immobilizer` | 3 | straight | damage, slow-down | |
| `Emp` | 2 | straight | | EMP: damage, 25 × damage to shields, energy drain; expiry |
| `EmpMissile` | 0 | homing | push | EMP; expiry |
| `Carrier` | 0 | straight | | spawns the payload; expiry |
| `HomingCarrier` | 0 | homing | | spawns the payload |
| `FragBomb` | 2 | straight | | spawns 20 fragments; expiry |
| `ClusterMissile` | 0 | homing | | spawns 20 fragments; expiry |
| `Fireworks` | 1 | straight, random color | | flash; expiry |
| `PlasmaWeb` | 2 | straight | damage | web that slows and damages; expiry |
| `Singularity` | 1 | straight | damage, push, spin | |
| `BlackHole` | 0 | straight, pulls everything in `Range` | stops a ship and detonates; destroys other bullets | strong explosion |
| `DroneControl` | 1 | straight | damage, captures drones | |
| `IonBeam` | 0 | straight, ray of `Velocity / 10` | damage when `AreaOfEffect` is 0 | explosion when `AreaOfEffect` is above 0 |
| `LaserBeam` | 12 | beam | damage | |
| `EnergyBeam` | 1 | beam that locks onto the target | damage | |
| `EnergySiphon` | 2 | beam that locks onto the target | damage, energy drain | |
| `VampiricRay` | 3 | beam that locks onto the target | damage that repairs the owner 1:1 | |
| `SmallVampiricRay` | 1 | beam that locks onto the target | damage that repairs the owner at 10 % | |
| `RepairRay` | 2 | beam that hits allies | repairs allies, damages enemies | |
| `TractorBeam` | 1 | beam that pulls the target | damage | |

Classes that are not beams or areas act on the first target only, then self-destruct, or detonate when they have hit points. Classes without damage on contact deal damage only through their detonation. A beam with `LifeTime` 0.5 or more deals damage that fades over its lifetime. Code: collision actions [BulletFactoryObsolete.cs:207](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs#L207), detonation actions [BulletFactoryObsolete.cs:252](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs#L252), controllers [BulletFactoryObsolete.cs:184](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactoryObsolete.cs#L184).

## Notes

### Differences from Ammunition

- The obsolete format has no triggers, no `FriendlyFire` (only `RepairRay` hits allies), and no custom visual effects.
- The muzzle flash uses `Size` and `Color` like the new format ([BulletStats.cs:219](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L219)).
- The ship editor and the AI read the class through `GetBehaviorType` and `GetEffectType` ([AmmunitionClassExtensions.cs:31](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/AmmunitionClassExtensions.cs#L31)).
- See [Weapon: Old and new ammunition](Weapon.md#old-and-new-ammunition) for which weapons use which format.

### Data rules

1. Do not create new records. Use [Ammunition](Ammunition.md), which can express every class with triggers.
2. Give `Carrier`, `HomingCarrier`, `FragBomb`, and `ClusterMissile` a `CoupledAmmunitionId`. Without it they spawn nothing.
3. Give a moving bullet both `Velocity` and `Range`, or a `LifeTime`. With none of them, the bullet expires at once.
4. A `BulletPrefab` name that does not exist silently falls back to `Laser` or `Plasma2`. Check the name against `Resources/Combat/Bullets/`.
5. Keep `Impulse` below 0.5 if point defense should be able to disarm the bullet.
