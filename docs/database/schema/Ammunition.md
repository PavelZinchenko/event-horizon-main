# Ammunition

An ammunition record defines one kind of bullet: its body (size, speed, range, energy cost, look), how it moves, what it does on contact, and what happens at events of its life (hit, expiry, detonation). Bullets include projectiles, missiles, beams, explosions, and clouds. A [Component](Component.md) fires it through `AmmunitionId` together with a [Weapon](Weapon.md). A [Device](Device.md) of class `Retribution` fires it through `AmmunitionId`. A trigger of another ammunition record can spawn it. Combat, the ship editor, and the AI read ammunition records.

Back to the [schema index](index.md).

## Facts

| Fact | Value |
| --- | --- |
| Item type | `25` |
| Kind | collection |
| Folder | [`Resources/Database/Ammunition/`](../../../Assets/Modules/Database/Resources/Database/Ammunition/) (the subfolders hold other tables) |
| Records in the built-in catalog | 56: 35 fired by components, 4 by `Retribution` devices, 16 only spawned by triggers, 1 unused (103). Controllers: 38 `Projectile`, 7 `Homing`, 7 `Beam`, 2 `Harpoon`, 1 `AuraEmitter`, 1 `StickyMine`, no `Parametric`. |
| Schema | [Ammunition.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Weapon/Ammunition.xml), [BulletBody.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Weapon/BulletBody.xml), [BulletController.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Weapon/BulletController.xml), [BulletTrigger.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Weapon/BulletTrigger.xml), [ImpactEffect.xml](../../../Assets/Modules/Database/.Schema/v1/Objects/Weapon/ImpactEffect.xml) |
| Generated class | `GameDatabase.DataModel.Ammunition` ([generated](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/Ammunition.cs)). Switch classes `BulletController_*` and `BulletTrigger_*` ([BulletController.cs](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/BulletController.cs), [BulletTrigger.cs](../../../Assets/Modules/Database/Scripts/GeneratedGameCode/DataModel/BulletTrigger.cs)), with hand-written parts ([BulletController.cs](../../../Assets/Modules/Database/Scripts/DataModel/BulletController.cs), [BulletTrigger.cs](../../../Assets/Modules/Database/Scripts/DataModel/BulletTrigger.cs)) |
| Runtime | [`BulletFactory`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs) creates the bullets. [`BulletStats`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs) scales the values. Pipeline: [BattleSimulator: Bullets](../../battle_simulator.md#bullets). |

## Fields

The record also has the [common fields](index.md#common-fields).

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Body` | struct [BulletBody](#bulletbody) | | Physical values, look, and energy cost. |
| `Controller` | struct [BulletController](#bulletcontroller-switch) | `Projectile` | How the bullet moves. |
| `Triggers` | struct list [BulletTrigger](#bullettrigger-switch) | | Actions at events of the bullet's life: sounds, effects, spawned bullets, detonation, gravity. |
| `ImpactType` | enum [BulletImpactType](enums.md#bulletimpacttype) | `HitFirstTarget` | When the impact effects act: once, once per target, or every frame of contact. It also selects the bullet unit class and the collider mode. See [Impact type](#impact-type). |
| `Effects` | struct list [ImpactEffect](#impacteffect) | | What a contact does to the target. See [Impact effects](#impact-effects). An empty list makes a bullet that does nothing on contact (used for pure trigger carriers). |

## Structures

### BulletBody

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Size` | float | 0…1000 | Body scale. For an area bullet (`HitAllTargets` or `DamageOverTime` with effects) the weapon's area multiplier also scales it ([BulletStats.cs:124](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L124)). It is also the muzzle flash size. |
| `Length` | float | 0…1000 | Length of a moving beam: a beam shape with a non-beam controller and a speed above 0. `0` uses `Size` ([BulletFactory.cs:298](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L298)). Other bullets ignore it. |
| `Velocity` | float | 0…1000 | Start speed in units per second. The `Beam` controller ignores it. The modification `WeaponBulletSpeed` multiplies it. |
| `ParentVelocityEffect` | float | 1, −1000…1000 | Part of the launcher's velocity that the bullet inherits at launch. `0` makes the bullet ignore the ship's motion. |
| `AttachedToParent` | bool | | The bullet moves in the launcher's coordinate space: beams, auras, and charge effects. A spawned bullet attaches to its parent bullet. Attached bullets vanish when the weapon is removed. |
| `Range` | float | 0…999999999 | Maximum distance. Beams use it as the ray length. Homing and harpoon controllers use it as their search and leash range. When `Lifetime` is 0, the lifetime is `Range / Velocity`. |
| `Lifetime` | float | 0…999999999 | Seconds until the bullet expires. `0` derives it from `Range / Velocity`, and gives 0 when either is 0. A `Beam` restores its lifetime every step while the weapon is held, and stays for `Lifetime` after release. |
| `Weight` | float | 0…999999999 | Bullet mass. It sets the physics mass. Homing bullets scale their turn rate and acceleration by `1 / (0.2 + 2 × Weight)` ([BulletFactory.cs:248](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L248)). The modification `WeaponBulletMass` multiplies it. |
| `Recoil` | float | 0…999999999 | Push on the launcher per shot: `−Recoil × (0.5 + 0.5p) ×` weight multiplier `×` the bullet's speed relative to the launcher, as obsolete ammunition. `0` (default) gives no recoil. Attached bullets have none ([BulletFactory.cs](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs), `ApplyRecoil`). The AI reads the same value (see [Stat scaling](#stat-scaling)). |
| `HitPoints` | int | 0…999999999 | Above 0, the bullet is a missile that other bullets can shoot down. Its hit points are `HitPoints ×` the ship's armor multiplier ([WeaponFactory.cs:27](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Systems/WeaponFactory.cs#L27), [ShipFactory.cs:120](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/ShipFactory.cs#L120)). With `HitFirstTarget`, a missile detonates at the target instead of vanishing. |
| `Color` | color | white | Bullet color. A spawned bullet ignores it and takes the color of its trigger (see [Spawned bullets](#spawned-bullets)). |
| `BulletPrefab` | ref [BulletPrefab](BulletPrefab.md) | | The look. `0` makes an invisible area bullet: the base prefab `AreaOfEffect`, treated as a `Mine` shape (no direction, not a beam) ([BulletFactory.cs:318](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L318), [PrefabCache.cs:123](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Services/PrefabCache.cs#L123)). 15 built-in records: explosions, clouds, a rift, and the invisible carriers of the `Retribution` devices. |
| `EnergyCost` | float | 0…999999999 | Energy per shot. For `Beam` controllers, energy per second while held, and `EnergyCost × Lifetime` to start the beam ([BulletStats.cs:108](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L108), [ContinuousCannon.cs:44](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/ContinuousCannon.cs#L44)). See [Weapon: Weapon classes](Weapon.md#weapon-classes). |
| `CanBeDisarmed` | bool | | Point defense can disarm the bullet. |
| `FriendlyFire` | bool | | The bullet is neutral and hits every side, including its owner's allies ([BulletFactory.cs:169](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L169)). 5 built-in records: repair rays, the charger beam, and the EMP explosion and field. |
| `DetonateWhenDestroyed` | bool | `true` | A bullet that is shot down or destroyed by a `Destroy` contact detonates (runs `Detonated` triggers). `false` makes it vanish. |
| `AiBulletBehavior` | enum [AiBulletBehavior](enums.md#aibulletbehavior) | `Projectile` | How the AI and auto-aim use the weapon. `Beam` and `AreaOfEffect` also make the AI hit range `Range`, not `Range + Size` ([BulletStats.cs:103](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L103)). |
| `Type` | enum `BulletTypeObsolete` | obsolete | Old movement type. Migration 1.5 → 1.6 converts it to `AiBulletBehavior`, `ParentVelocityEffect`, `AttachedToParent`, and `Controller` ([DatabaseUpgrader_5_6.cs](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_5_6.cs)). The generated class does not read it. |

### BulletController (switch)

`Type` selects the controller class. The *Case* column lists the `Type` values for which a field applies.

| Field | Type | Case | Default / range | Meaning |
| --- | --- | --- | --- | --- |
| `Type` | enum [BulletControllerType](enums.md#bulletcontrollertype) | | `Projectile` | See the controller table below. |
| `StartingVelocityModifier` | float | `Homing`, `Rocket` | 1, 0…1000 | Multiplies the launch speed. Missiles use 0.1 to start slowly and accelerate ([BulletFactory.cs:202](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L202)). |
| `IgnoreRotation` | bool | `Homing` | | Uses `MagneticController`: the bullet drifts toward the target without turning its nose. Otherwise `HomingController`. |
| `SmartAim` | bool | `Homing` | | Leads the target instead of flying at its current position. |
| `Lifetime` | float | `StickyMine` | 0… | Seconds the mine stays on the target before it detonates. |
| `TrackingAngle` | float | `Beam` | 0…180 | Above 0, the beam turns toward the target it touches, at most this many degrees from the weapon direction (`LookAtTargetController`, as obsolete energy beams with 60). `0` (default) keeps the fixed beam. |
| `X`, `Y` | expression `FloatToFloat(t)` | `Parametric` | `0` | Offset from the launch point at `t` seconds, in the launch frame. |
| `Rotation` | expression `FloatToFloat(t)` | `Parametric` | `0` | Rotation in degrees, added to the launch rotation. |
| `Size` | expression `FloatToFloat(t)` | `Parametric` | `1` | Scale factor of the launch scale. |
| `Length` | expression `FloatToFloat(t)` | `Parametric` | `1` | Ray length for a beam shape. Other shapes ignore it. |

| `Type` | Movement | Code |
| --- | --- | --- |
| `Projectile` | Straight flight with the launch velocity. | none |
| `Homing` | Finds a target in `Range` and turns or drifts toward it. | [BulletFactory.cs:267](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L267) |
| `Beam` | A ray that stays on the weapon while the weapon is active. It is the only *continuous* controller: energy is per second, the collider is a ray of `Range`, and the flash lasts as long as the beam. | [BeamController](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/BeamController.cs) |
| `Parametric` | Moves, turns, scales, and sets the ray length from the expressions. `t` is the time since the first physics step. | [ParametricController.cs:24](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/ParametricController.cs#L24) |
| `Harpoon` | Ropes the bullet to the owner, sticks to the first target, and breaks past 1.5 × `Range`. | [HarpoonController](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/HarpoonController.cs) |
| `AuraEmitter` | Grows to full size, then shrinks over `Body.Lifetime`. It uses the raw `Lifetime`, not the scaled one ([BulletFactory.cs:283](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L283)). | [AuraController](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/AuraController.cs) |
| `StickyMine` | Sticks to the first target and detonates after `Lifetime`. | [StickyController](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/StickyController.cs) |
| `Rocket` | Unguided. Accelerates along its nose to the bullet speed with `speed / (0.1 + weight)`, as the obsolete unguided rocket. | [RocketController](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Controller/RocketController.cs) |

A beam shape with a non-continuous controller and a speed above 0 becomes a moving beam: it grows to its length, then flies ([BulletFactory.cs:296](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L296)).

### BulletTrigger (switch)

`EffectType` selects the trigger class. `Condition` selects the event. The *Case* column lists the `EffectType` values for which a field applies.

| Field | Type | Case | Default / range | Meaning |
| --- | --- | --- | --- | --- |
| `Condition` | enum [BulletTriggerCondition](enums.md#bullettriggercondition) | | `Undefined` | The event. `Undefined` disables the trigger ([BulletFactory.cs:369](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L369)). See [Trigger conditions](#trigger-conditions). |
| `EffectType` | enum [BulletEffectType](enums.md#bulleteffecttype) | | `None` | The action. `None` does nothing. |
| `VisualEffect` | ref [VisualEffect](VisualEffect.md) | `PlaySfx`, `SpawnStaticSfx` | | The effect to show. `0` shows nothing; the sound still plays. |
| `AudioClip` | audio | `PlaySfx`, `SpawnStaticSfx`, `SpawnBullet` | | Sound. For `SpawnBullet`, it plays at each spawn. |
| `Ammunition` | ref [Ammunition](Ammunition.md) | `SpawnBullet` | | The bullet to spawn. `0` disables the trigger. It may be the same record (see [Spawned bullets](#spawned-bullets)). |
| `Color` | color | `PlaySfx`, `SpawnStaticSfx`, `SpawnBullet` | white | Color, combined with the bullet color by `ColorMode`. |
| `ColorMode` | enum [ColorMode](enums.md#colormode) | `PlaySfx`, `SpawnStaticSfx`, `SpawnBullet` | `TakeFromOwner` | How `Color` combines with the bullet color. The default ignores `Color`. |
| `Quantity` | int | `SpawnBullet` | 0…1000 | Bullets per activation. `0` spawns one ([BulletFactory.cs:444](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L444)). |
| `Size` | float | `PlaySfx`, `SpawnStaticSfx`, `SpawnBullet`, `GravityField` | 0…100 | `PlaySfx`: effect size; `0` means 1. `SpawnStaticSfx`: effect size × bullet size; `0` means 1. `SpawnBullet`: multiplies the area, weight, and range of the spawned bullets; `0` keeps them. `GravityField`: field radius × the weapon's area multiplier. |
| `Lifetime` | float | `PlaySfx`, `SpawnStaticSfx` | 0…1000 | Effect duration in seconds. `0` uses the effect's own duration (see [VisualEffect: Timeline](VisualEffect.md#timeline)). |
| `Cooldown` | float | | 0…1000 | Minimum seconds between two activations of this trigger. Required for the `Cooldown` condition, which otherwise fires every physics step. |
| `RandomFactor` | float | `SpawnBullet` | 0…1 | Random spread of speed and lifetime of the spawned bullets: ±`RandomFactor / 2`. |
| `PowerMultiplier` | float | `SpawnBullet`, `GravityField` | 0… | `SpawnBullet`: multiplies the damage and effect power of the spawned bullets; `0` keeps them. `GravityField`: field force × the weapon's area multiplier. |
| `MaxNestingLevel` | int | `SpawnBullet` | 0…100 | Spawn depth limit. `0` means 100. See [Spawned bullets](#spawned-bullets). |
| `OncePerCollision` | bool | `PlaySfx`, `SpawnStaticSfx` | | On `Hit`: the sound plays and a static effect spawns only on the first frame of each contact. A `PlaySfx` effect keeps one effect per target instead of one shared effect. |
| `UseBulletPosition` | bool | `PlaySfx` | | On `Hit`: the effect follows the bullet instead of appearing at the contact point. |
| `SyncLifetimeWithBullet` | bool | `PlaySfx` | | The effect stays attached for the bullet's remaining life and ignores `Lifetime`. |
| `Rotation` | expression `IntToFloat(i)` | `SpawnBullet` | `IF(Quantity <= 1, 0, RANDOM(0, 360))` | Launch angle of bullet `i` (from 0), relative to the parent bullet. |
| `OffsetX`, `OffsetY` | expression `IntToFloat(i)` | `SpawnBullet` | `IF(Quantity <= 1, 0, Size / 2)`, `0` | Launch offset of bullet `i`, rotated by its angle. |

The expressions can use `Quantity`, `Size`, `RandomFactor`, `PowerMultiplier`, `MaxNestingLevel`, and `Cooldown` of the trigger. See [Expressions](../../database.md#expressions).

| `EffectType` | Action | Code |
| --- | --- | --- |
| `PlaySfx` | Plays the sound and shows the effect on the bullet. On `Hit`, the effect appears at the contact point. On `Created`, a non-looping sound plays once at launch. | [BulletFactory.cs:420](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L420) |
| `SpawnStaticSfx` | Plays the sound and leaves the effect in the world. The effect does not follow the bullet. | [BulletFactory.cs:428](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L428) |
| `SpawnBullet` | Spawns `Quantity` bullets of `Ammunition`. | [BulletFactory.cs:436](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L436) |
| `Detonate` | Detonates the bullet. | [BulletFactory.cs:455](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L455) |
| `GravityField` | Creates a gravity field around the bullet. `FriendlyFire` of the body applies. | [BulletFactory.cs:462](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L462) |

### ImpactEffect

| Field | Type | Default / range | Meaning |
| --- | --- | --- | --- |
| `Type` | enum [ImpactEffectType](enums.md#impacteffecttype) | `Damage` | What the contact does. See [Impact effects](#impact-effects). |
| `DamageType` | enum [DamageType](enums.md#damagetype) | `Impact` | Damage type of `Damage`, `ProgressiveDamage`, `SiphonHitPoints`, `Devour`, and the damage that `Repair` deals to enemies. |
| `Power` | float | 0…999999999 | Main strength. Per hit, or per second for `DamageOverTime`. |
| `Factor` | float | 0…999999999 | Second value. Its meaning depends on `Type`. |

## Notes

### Impact type

| `ImpactType` | Effects act | Unit class | Without hit points |
| --- | --- | --- | --- |
| `HitFirstTarget` | Once, on the first new contact. | `EnergyBolt` | The bullet self-destructs after the hit. A missile detonates at the target ([BulletFactory.cs:148](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L148)). |
| `HitAllTargets` | Once per new contact. | `AreaOfEffect` | The bullet stays. |
| `DamageOverTime` | Every physics step of contact, × the step time. The collider reports every frame. | `AreaOfEffect` | The bullet stays. |

A bullet with `HitPoints` above 0 is always a `Missile` unit ([BulletFactory.cs:162](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L162)).

### Impact effects

*Dmg* is the damage multiplier: power level × weapon damage multiplier × trigger `PowerMultiplier`. *Eff* is the effect multiplier: power level × trigger `PowerMultiplier`. The weapon modifications do not change *Eff*. Code: [BulletFactory.cs:102](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L102), actions in [`Collision/Behaviour/Action/`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/).

| `Type` | `Power` | `Factor` | Multiplier |
| --- | --- | --- | --- |
| `Damage` | Damage of `DamageType` | | *Dmg* |
| `ProgressiveDamage` | Damage per second | Seconds until full damage. Only `DamageOverTime` bullets deal it. | *Dmg* |
| `Push`, `Pull` | Impulse × bullet velocity. A bullet without speed (beam, area) does not push. | | *Eff* |
| `PushFromCenter`, `PullToCenter` | Radial impulse | | *Eff* |
| `DrainEnergy` | Energy removed | | *Eff* |
| `SiphonHitPoints` | Damage of `DamageType` | Part of the damage after resistance that repairs the owner | *Dmg* |
| `SlowDown` | Part of the velocity and spin removed (ships only): `Power / √(target weight)`, at most 1 | | *Eff* |
| `CaptureDrones`, `DriveDronesCrazy` | | | |
| `Repair` | Repair of allies | Damage multiplier against enemies | *Dmg* |
| `RechargeShield` | Shield restored on allies | Shield damage multiplier against enemies | *Dmg* |
| `RechargeEnergy` | Energy restored on allies | Energy drain multiplier against enemies | *Eff* × weapon energy cost multiplier |
| `Teleport` | Distance the target jumps forward | | none |
| `DrainShield` | Shield damage | | *Dmg* |
| `Devour` | Damage of `DamageType` to ships, shields, limbs, and space objects. Asteroids and other bullets are destroyed. A hit on a starbase destroys the bullet. | | *Dmg* |
| `IgnoreShield` | | | |
| `RestoreLifetime` | Seconds of lifetime regained per hit on a ship or drone | Part of the lifetime removed at launch | none |
| `DamageWithFalloff` | Damage of `DamageType` × falloff | | *Dmg* |
| `PushFromCenterWithFalloff` | Radial impulse × falloff | | *Eff* |
| `Roll` | Spin on a new contact: angular acceleration `Power × 180 / √(target weight)` in the target's spin direction | | *Eff* |

Falloff (legacy explosions): `clamp01((R + target scale − distance) / R)`, where `R` is half the bullet scale and the distance runs between the centres. It is 1 at the centre and 0 once the target's edge leaves the radius ([ApplyExplosionDamageAction.cs](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Collision/Behaviour/Action/ApplyExplosionDamageAction.cs), `GetFalloff`). Meant for area bullets.

`Repair`, `RechargeShield`, and `RechargeEnergy` decide ally or enemy by the owner's side. Only `FriendlyFire` bullets reach allies.

### Stat scaling

[`BulletStats`](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs) combines the body values with the power level `p` (the charge of a `RequiredCharging` weapon, else 1) and the weapon multipliers (ship bonuses, modification, and for spawned bullets the trigger):

| Value | Formula | Code |
| --- | --- | --- |
| Size | `Size × (0.5 + 0.5p)`, × area multiplier for area bullets | [line 124](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L124) |
| Weight | `Weight × (0.5 + 0.5p) ×` weight multiplier | [line 122](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L122) |
| Range | `Range × p ×` range multiplier; 0 when `p` < 0.1 | [line 136](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L136) |
| Speed | `Velocity ×` speed multiplier, ± random factor | [line 141](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L141) |
| Lifetime | `Lifetime` ± random factor; not scaled by `p` or multipliers. With `Lifetime` 0: scaled range / scaled speed. | [line 148](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L148) |
| Energy cost | `EnergyCost ×` energy cost multiplier | [line 106](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L106) |
| Hit points | `HitPoints ×` ship armor multiplier | [line 137](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L137) |
| Flash | size `Size × (0.5 + 0.5p)`, color = bullet color, duration 0.2 s or the beam lifetime | [line 100](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L100) |
| Recoil (push and AI) | `Recoil × (0.5 + 0.5p) ×` weight multiplier. The AI divides it by the ship weight. | [line 110](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L110) |

`MashineGun` and `MultiShot` weapons set a random factor of 0.2 and 0.3 ([Weapon: Weapon classes](Weapon.md#weapon-classes)). Spawned bullets also scale range, length, and speed by the area multiplier.

### Trigger conditions

| `Condition` | When | Runtime |
| --- | --- | --- |
| `Created` | Once, at launch. | `None` |
| `Hit` | Every reported contact. Sounds and effects run as collision actions. | `OnCollide` |
| `Destroyed` | The bullet is shot down or removed without detonation. | `OnDestroy` |
| `Detonated` | The bullet detonates: hit, missile at target, `Detonate` trigger, or destroyed with `DetonateWhenDestroyed`. | `OnDetonate` |
| `Expired` | The lifetime ends. | `OnExpire` |
| `Disarmed` | Point defense disarms the bullet. | `OnDisarm` |
| `Cooldown` | Every physics step, limited by the trigger `Cooldown`. | `OnCooldown` |
| `OutOfAmmo` | See [Spawned bullets](#spawned-bullets). | the failed spawn conditions |

Code: [BulletFactory.cs:395](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L395). Life cycle: [BattleSimulator: Bullet life cycle](../../battle_simulator.md#bullet-life-cycle).

### Spawned bullets

A `SpawnBullet` trigger creates a nested factory for its `Ammunition` ([BulletFactory.cs:531](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L531)):

1. The spawned bullets inherit the weapon multipliers of the parent and the power level.
2. Their color is `ColorMode` applied to the trigger `Color` and the parent color. Their own `Body.Color` is never used.
3. `PowerMultiplier` above 0 multiplies their damage and effect power. `Size` above 0 multiplies their area, weight, and range.
4. They launch from the parent bullet with the angle and offset of the expressions. An attached spawned bullet moves with the parent.
5. Each factory has a nesting level: 0 for the weapon, +1 per spawn. A trigger whose factory has reached `MaxNestingLevel` (or 100) spawns nothing. Instead, the bullet builds its `OutOfAmmo` triggers with the conditions of these failed spawn triggers ([BulletFactory.cs:383](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletFactory.cs#L383)).

Example: the cluster missile (112) spawns 3 copies of itself when it expires, with `Size` 0.75 and `MaxNestingLevel` 3. The third generation cannot spawn, so its `OutOfAmmo` trigger (`Detonate`) runs on expiry instead.

### Other consumers

| Consumer | Rule | Code |
| --- | --- | --- |
| Ship editor stats | Damage is the first `Damage` or `SiphonHitPoints` effect, else the same search in the first `SpawnBullet` ammunition, and so on. `DamageOverTime` shows it as damage per second. | [ComponentItem.cs:302](../../../Assets/ModulesShared/ShipEditor/Scripts/UI/ComponentItem.cs#L302) |
| Ship editor damage per second | Estimates damage, area, and energy from the effects and triggers. | [WeaponDamageCalculator.cs](../../../Assets/Modules/ShipConstructor/Scripts/Component/WeaponDamageCalculator.cs) |
| Startup check | Rejects a build when `Body.EnergyCost` exceeds the ship's energy. | [ShipBuild: Startup check](ShipBuild.md#startup-check) |
| AI | Reads `AiBulletBehavior`, the capabilities from `Effects` (damage, repair, capture, recharge), range, speed, and recoil. | [BulletStats.cs:48](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Factory/Bullets/BulletStats.cs#L48), [WeaponInfo.cs](../../../Assets/Modules/BattleSimulator/Scripts/Combat/Component/Systems/Weapons/WeaponInfo.cs) |
| `Retribution` device | Fires the device's ammunition when the ship dies. | [Device: Device classes](Device.md#device-classes) |

### Data rules

1. Give every bullet a lifetime: `Lifetime` above 0, or both `Velocity` and `Range` above 0. Otherwise the bullet expires at once.
2. Use the `Beam` controller for beams that stay on the weapon. Set `AttachedToParent` and a beam [BulletPrefab](BulletPrefab.md) shape with it, and remember that `EnergyCost` is per second.
3. Set `Cooldown` on every trigger with the `Cooldown` condition.
4. Give a self-spawning record a `MaxNestingLevel` and an `OutOfAmmo` trigger. With the default of 100, the bullet count grows as `Quantity` to the power 100.
5. Set the color of spawned bullets on the trigger (`Color`, `ColorMode`), not on the spawned record.
6. `DamageOverTime` values are per second. `ProgressiveDamage` works only with `DamageOverTime`.
7. Do not set `Body.Type`. 13 built-in records still carry it next to the converted fields. The game ignores it at version 1.7, but migration from an older version throws when both are set ([DatabaseUpgrader_5_6.cs:27](../../../Assets/Modules/Database/Scripts/Migration/v1/DatabaseUpgrader_5_6.cs#L27)).
8. A bad expression becomes 0 without a load error ([DB6](../../issues/database.md#db6-expression-errors-appear-late-and-become-zero)). Test parametric controllers and spawn patterns in combat.
