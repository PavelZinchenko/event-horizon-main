using System;
using System.Collections.Generic;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;

namespace Combat.Integration.Core
{
    // Maps one weapon with new-format ammunition to a core cannon or beam (phase 4, step 3a, "Ship mapping (3a)",
    // rows 3-5). A weapon with any part the core cannot run is inert as a whole (D1); the reason names that part.
    // Phase 4, step 4b: cannons also take recoil (E1), push and several damage effects (E4).
    // Phase 5, step 2: SpawnBullet and Detonate triggers, with child bullets mapped as cannon bullets. A child the core
    // cannot run makes the weapon inert with the child's own reason.
    // Phase 5, step 3: HitAllTargets and DamageOverTime impacts, falloff and radial push effects, bullets without a
    // prefab (the legacy area bullet) and bullets attached to their ship.
    // Phase 5, step 4a: MultiShot and MashineGun weapons, the Rocket controller. Step 4b: Manageable weapons, the
    // StickyMine controller.
    public static class CoreWeaponMapper
    {
        // A child bullet the mapper made: the ship mapper records its view source.
        public delegate void ChildBullet(BulletDefinition bullet, Ammunition ammunition, ColorData color);

        public static DeviceDefinition Map(IWeaponData weapon, int barrelIndex, float armorMultiplier, out string inertReason) =>
            Map(weapon, barrelIndex, armorMultiplier, null, out inertReason);

        public static DeviceDefinition Map(IWeaponData weapon, int barrelIndex, float armorMultiplier, ChildBullet children,
            out string inertReason)
        {
            var stats = weapon.Weapon.Stats;
            var ammunition = weapon.Ammunition;
            var modifier = weapon.Stats;

            if (!TryCheckTriggers(ammunition, out var hasGameplayTriggers, out inertReason))
                return null;
            if (!TryGetEffects(ammunition, hasGameplayTriggers, out var effects, out inertReason))
                return null;

            var cooldown = Cooldown(stats.FireRate, modifier.FireRateMultiplier.Value);
            if (float.IsInfinity(cooldown))
                return Inert("fire rate blocked by modifiers", out inertReason);

            var body = ammunition.Body;
            var energyCost = body.EnergyCost * modifier.EnergyCostMultiplier.Value;
            var range = body.Range * modifier.RangeMultiplier.Value;

            switch (stats.WeaponClass)
            {
                case WeaponClass.Common:
                case WeaponClass.MultiShot:
                case WeaponClass.MashineGun:
                case WeaponClass.Manageable:
                    var mode = ToCannonMode(stats.WeaponClass);
                    var magazine = mode == CannonMode.MultiShot || mode == CannonMode.MachineGun ? stats.Magazine : 1;
                    if (magazine < 1)
                        return Inert($"weapon class {stats.WeaponClass} without magazine", out inertReason);
                    var context = new Context(armorMultiplier, children);
                    var bullet = MapBullet(ammunition, modifier, effects, 0, context, out inertReason);
                    if (bullet == null)
                        return null;
                    context.Commit();
                    return new CannonDefinition(barrelIndex, cooldown, energyCost, weapon.Weapon.Stats.Spread * DegreesToRadians, bullet,
                        mode, magazine);
                case WeaponClass.Continuous:
                    if (hasGameplayTriggers)
                        return Inert($"beam with trigger {FirstGameplayTrigger(ammunition)}", out inertReason);
                    if (effects.Damage.Count > 1)
                        return Inert("beam with several damage effects", out inertReason);
                    if (effects.Push != 0f || effects.RadialPush != 0f || effects.RadialPushWithFalloff != 0f)
                        return Inert("beam with push", out inertReason);
                    if (effects.HasFalloff)
                        return Inert("beam with falloff", out inertReason);
                    if (ammunition.Controller.Type != BulletControllerType.Beam)
                        return Inert($"continuous weapon with controller {ammunition.Controller.Type}", out inertReason);
                    if (ammunition.ImpactType != BulletImpactType.DamageOverTime)
                        return Inert($"beam with impact {ammunition.ImpactType}", out inertReason);
                    if (!(range > 0f))
                        return Inert("beam without range", out inertReason);

                    var damage = effects.Damage[0];
                    inertReason = null;
                    return new BeamDefinition(barrelIndex, cooldown, energyCost * body.Lifetime, energyCost, range,
                        damage.Amount * modifier.DamageMultiplier.Value, damage.DamageType);
                default:
                    return Inert($"weapon class {stats.WeaponClass}", out inertReason);
            }
        }

        // Legacy WeaponFactory: MultishotCannon, MachineGun and ManageableCannon; every other class with projectiles is a
        // CommonCannon.
        private static CannonMode ToCannonMode(WeaponClass weaponClass)
        {
            switch (weaponClass)
            {
                case WeaponClass.MultiShot:
                    return CannonMode.MultiShot;
                case WeaponClass.MashineGun:
                    return CannonMode.MachineGun;
                case WeaponClass.Manageable:
                    return CannonMode.Manageable;
                default:
                    return CannonMode.Single;
            }
        }

        // A projectile of a cannon (level 0) or a child bullet (level > 0, legacy BulletStats with isNested). Legacy
        // BulletFactory: damage x DamageMultiplier, push x EffectPowerMultiplier; recoil x WeightMultiplier
        // (BulletStats.Recoil), none for a bullet attached to its parent (ApplyRecoil). Power level 1: no charge. A child
        // also scales its speed and range by AoeRadiusMultiplier.
        private static BulletDefinition MapBullet(Ammunition ammunition, WeaponStatModifier modifier, in Effects effects,
            int level, Context context, out string inertReason)
        {
            var body = ammunition.Body;
            var nested = level > 0;

            var homing = false;
            var magnetic = false;
            var rocket = false;
            var stickyLifetime = 0f;
            var startingVelocity = 1f;
            switch (ammunition.Controller)
            {
                case BulletController_Projectile _:
                    break;
                case BulletController_Homing controller:
                    // SmartAim is deferred (owner, 2026-11-03, H4): no built-in ammunition sets it.
                    if (controller.SmartAim)
                        return InertBullet("homing with smart aim", out inertReason);
                    homing = true;
                    magnetic = controller.IgnoreRotation;
                    startingVelocity = controller.StartingVelocityModifier;
                    break;
                case BulletController_Rocket controller:
                    // Legacy RocketController does nothing while the bullet rides on its parent.
                    rocket = !body.AttachedToParent;
                    startingVelocity = controller.StartingVelocityModifier;
                    break;
                case BulletController_StickyMine controller:
                    if (body.AttachedToParent)
                        return InertBullet("sticky bullet attached to the parent", out inertReason);
                    if (!(controller.Lifetime > 0f))
                        return InertBullet("sticky bullet without lifetime", out inertReason);
                    stickyLifetime = controller.Lifetime;
                    break;
                default:
                    return InertBullet($"cannon with controller {ammunition.Controller.Type}", out inertReason);
            }

            // Phase 5, step 3: area and over-time impacts. A damage-over-time contact is one interval at a fixed rate.
            var impact = ToImpact(ammunition.ImpactType);
            if (impact == BulletImpact.OverTime)
            {
                if (effects.Push != 0f || effects.RadialPush != 0f || effects.RadialPushWithFalloff != 0f)
                    return InertBullet("push over time", out inertReason);
                if (effects.HasFalloff)
                    return InertBullet("falloff over time", out inertReason);
                if (effects.Damage.Count > 1)
                    return InertBullet("several damage types over time", out inertReason);
            }

            // Without a prefab legacy uses the area-of-effect bullet (BulletFactory.BulletShape: Mine).
            var shape = body.BulletPrefab != null ? body.BulletPrefab.Shape : BulletShape.Mine;
            if (!TryGetColliderRadius(shape, out var colliderRadius))
                return InertBullet(IsBeamShape(shape) ? "moving beam bolt" : $"bullet shape {shape}", out inertReason);

            // Physics attaches bullets to bodies only; legacy attaches a child to its parent bullet.
            if (body.AttachedToParent && nested)
                return InertBullet("bullet attached to a bullet", out inertReason);
            if (body.AttachedToParent && homing)
                return InertBullet("homing bullet attached to the parent", out inertReason);

            // Legacy BulletStats.BodySize: an area with effects also scales by AoeRadiusMultiplier. A positive
            // Body.Lifetime replaces range / speed (GetBulletLifetime).
            var aoe = nested ? modifier.AoeRadiusMultiplier.Value : 1f;
            var isAoe = ammunition.ImpactType != BulletImpactType.HitFirstTarget && ammunition.Effects.Count > 0;
            var scale = body.Size * (isAoe ? modifier.AoeRadiusMultiplier.Value : 1f);
            var radius = colliderRadius * scale;
            var speed = body.Velocity * modifier.VelocityMultiplier.Value * aoe;
            var range = body.Range * modifier.RangeMultiplier.Value * aoe;
            var lifetime = body.Lifetime > 0f ? body.Lifetime : 0f;
            if (!(radius > 0f) || !(speed >= 0f) || !(range >= 0f) || !(lifetime > 0f || (speed > 0f && range > 0f)))
                return InertBullet("bullet without size, speed or range", out inertReason);

            var triggers = MapTriggers(ammunition, modifier, level, context, out inertReason);
            if (triggers == null)
                return null;

            var damageMultiplier = modifier.DamageMultiplier.Value;
            DamageAmount[] extraDamage = null;
            if (effects.Damage.Count > 1)
            {
                extraDamage = new DamageAmount[effects.Damage.Count - 1];
                for (var i = 0; i < extraDamage.Length; ++i)
                {
                    var item = effects.Damage[i + 1];
                    extraDamage[i] = new DamageAmount(item.DamageType, item.Amount * damageMultiplier, item.Falloff);
                }
            }

            // A bullet with triggers may have no damage effect (a carrier).
            var first = effects.Damage.Count > 0 ? effects.Damage[0] : default;
            var bullet = new BulletDefinition(
                radius,
                speed,
                range,
                first.Amount * damageMultiplier,
                weight: body.Weight * modifier.WeightMultiplier.Value,
                parentVelocityEffect: body.ParentVelocityEffect,
                hitPoints: body.HitPoints * context.ArmorMultiplier * modifier.HitPointsMultiplier.Value,
                homing: homing,
                startingVelocityMultiplier: startingVelocity,
                damageType: first.DamageType,
                recoil: body.AttachedToParent || nested ? 0f : body.Recoil * modifier.WeightMultiplier.Value,
                push: effects.Push * modifier.EffectPowerMultiplier.Value,
                extraDamage: extraDamage,
                triggers: triggers,
                detonateWhenDestroyed: body.DetonateWhenDestroyed,
                canBeDisarmed: body.CanBeDisarmed,
                impact: impact,
                damageFalloff: first.Falloff,
                radialPush: effects.RadialPush * modifier.EffectPowerMultiplier.Value,
                radialPushWithFalloff: effects.RadialPushWithFalloff * modifier.EffectPowerMultiplier.Value,
                lifetime: lifetime,
                scale: scale,
                attachedToParent: body.AttachedToParent,
                rocket: rocket,
                stickyLifetime: stickyLifetime,
                magnetic: magnetic,
                hasDirection: shape.HasDirection());

            inertReason = null;
            return bullet;
        }

        // Legacy BulletFactory.BulletTriggerBuilder.Build: triggers in order, then the OutOfAmmo triggers with the
        // conditions of the spawn triggers that reached the nesting limit. A Detonate trigger acts only on Hit or Expired;
        // on any other condition legacy ignores its result. Behaviour change (owner, 2026-11-02): legacy builds an
        // OutOfAmmo Detonate with the OutOfAmmo condition itself, so it does nothing; the core gives it the conditions too.
        private static List<BulletTriggerDefinition> MapTriggers(Ammunition ammunition, WeaponStatModifier modifier,
            int level, Context context, out string inertReason)
        {
            var result = new List<BulletTriggerDefinition>();
            var outOfAmmo = BulletTriggerConditions.None;
            var hasOutOfAmmo = false;
            foreach (var trigger in ammunition.Triggers)
            {
                if (trigger.Condition == BulletTriggerCondition.Undefined)
                    continue;
                if (trigger.Condition == BulletTriggerCondition.OutOfAmmo)
                {
                    hasOutOfAmmo = true;
                    continue;
                }

                var condition = ToCondition(trigger.Condition);
                switch (trigger)
                {
                    case BulletTrigger_SpawnBullet spawn:
                        if (spawn.Ammunition == null)
                            continue;
                        var limit = spawn.MaxNestingLevel > 0 ? spawn.MaxNestingLevel : MaxNestingLevel;
                        if (level >= limit)
                        {
                            outOfAmmo |= condition;
                            continue;
                        }
                        var child = MapSpawn(spawn, condition, ammunition, modifier, level, context, out inertReason);
                        if (child == null)
                            return null;
                        result.Add(child);
                        break;
                    case BulletTrigger_Detonate _:
                        if ((condition & (BulletTriggerConditions.Hit | BulletTriggerConditions.Expired)) != 0)
                            result.Add(BulletTriggerDefinition.Detonate(condition));
                        break;
                }
            }

            if (hasOutOfAmmo && outOfAmmo != BulletTriggerConditions.None)
            {
                foreach (var trigger in ammunition.Triggers)
                {
                    if (trigger.Condition != BulletTriggerCondition.OutOfAmmo)
                        continue;
                    if (trigger is BulletTrigger_Detonate)
                    {
                        if ((outOfAmmo & (BulletTriggerConditions.Hit | BulletTriggerConditions.Expired)) != 0)
                            result.Add(BulletTriggerDefinition.Detonate(outOfAmmo));
                        continue;
                    }
                    if (!(trigger is BulletTrigger_SpawnBullet spawn) || spawn.Ammunition == null)
                        continue;
                    // Legacy builds it without a nesting check.
                    var child = MapSpawn(spawn, outOfAmmo, ammunition, modifier, level, context, out inertReason);
                    if (child == null)
                        return null;
                    result.Add(child);
                }
            }

            inertReason = null;
            return result;
        }

        // Legacy BulletTriggerBuilder.CreateFactory: the child's modifiers, colour and random factor.
        private static BulletTriggerDefinition MapSpawn(BulletTrigger_SpawnBullet trigger, BulletTriggerConditions condition,
            Ammunition parent, WeaponStatModifier modifier, int level, Context context, out string inertReason)
        {
            var ammunition = trigger.Ammunition;
            if (!TryCheckTriggers(ammunition, out var hasGameplayTriggers, out inertReason) ||
                !TryGetEffects(ammunition, hasGameplayTriggers, out var effects, out inertReason))
                return null;

            // Legacy ActionWithCooldown: a Hit trigger of a bullet that can take several hits waits out its cooldown.
            if (trigger.Cooldown > 0f && condition == BulletTriggerConditions.Hit && parent.Body.HitPoints > 0f)
            {
                inertReason = "SpawnBullet on Hit with cooldown";
                return null;
            }

            var stats = modifier;
            var parentColor = modifier.Color ?? parent.Body.Color;
            stats.Color = trigger.ColorMode.Apply(trigger.Color.ToUnity(), parentColor.ToUnity()).ToData();
            if (trigger.PowerMultiplier > 0f)
            {
                stats.DamageMultiplier *= trigger.PowerMultiplier;
                stats.EffectPowerMultiplier *= trigger.PowerMultiplier;
            }
            if (trigger.Size > 0f)
            {
                stats.AoeRadiusMultiplier *= trigger.Size;
                stats.WeightMultiplier *= trigger.Size;
                stats.RangeMultiplier *= trigger.Size;
            }

            if (!TrySample(trigger, out var pattern, out inertReason))
                return null;

            // Legacy builds children lazily, per spawned bullet; the mapper builds each child once.
            var key = (ammunition, level + 1, stats);
            if (!context.Bullets.TryGetValue(key, out var bullet))
            {
                if (context.Bullets.Count >= MaxChildBullets)
                {
                    inertReason = "too many child bullets";
                    return null;
                }
                bullet = MapBullet(ammunition, stats, effects, level + 1, context, out inertReason);
                if (bullet == null)
                    return null;
                context.Bullets.Add(key, bullet);
                context.Add(bullet, ammunition, stats.Color.Value);
            }

            inertReason = null;
            return BulletTriggerDefinition.Spawn(condition, bullet, pattern, trigger.RandomFactor);
        }

        // Legacy SpawnBulletsAction: Quantity children (at least one); child i turns by Rotation(i) degrees and starts at
        // (OffsetX(i), OffsetY(i)). A RANDOM call becomes a span: the expression is sampled with the call returning its
        // minimum, its maximum and the midpoint. More than one call, RANDOM_INT, or a result that is not linear in the
        // draw cannot be expressed.
        private static bool TrySample(BulletTrigger_SpawnBullet trigger, out BulletSpawnPattern pattern, out string inertReason)
        {
            var count = Math.Max(trigger.Quantity, 1);
            var slots = new SpawnSlot[count];
            for (var i = 0; i < count; ++i)
            {
                var index = i;
                if (!TrySample(() => trigger.Rotation(index), out var rotation, out var rotationSpan) ||
                    !TrySample(() => trigger.OffsetX(index), out var x, out var xSpan) ||
                    !TrySample(() => trigger.OffsetY(index), out var y, out var ySpan))
                {
                    pattern = null;
                    inertReason = "spawn expression with RANDOM";
                    return false;
                }
                slots[i] = new SpawnSlot(rotation * DegreesToRadians, rotationSpan * DegreesToRadians, new Vec2(x, y),
                    new Vec2(xSpan, ySpan));
            }

            pattern = new BulletSpawnPattern(slots);
            inertReason = null;
            return true;
        }

        private static bool TrySample(Func<float> expression, out float value, out float span)
        {
            value = RandomStateSource.Probe(expression, 0f, out var calls, out var intCalls);
            span = 0f;
            if (intCalls > 0 || calls > 1)
                return false;
            if (calls == 0)
                return true;

            var high = RandomStateSource.Probe(expression, 1f, out _, out _);
            var middle = RandomStateSource.Probe(expression, 0.5f, out _, out _);
            span = high - value;
            return Math.Abs(middle - (value + 0.5f * span)) <= LinearTolerance * Math.Max(1f, Math.Abs(span));
        }

        private static BulletTriggerConditions ToCondition(BulletTriggerCondition condition)
        {
            switch (condition)
            {
                case BulletTriggerCondition.Created: return BulletTriggerConditions.Created;
                case BulletTriggerCondition.Destroyed: return BulletTriggerConditions.Destroyed;
                case BulletTriggerCondition.Hit: return BulletTriggerConditions.Hit;
                case BulletTriggerCondition.Disarmed: return BulletTriggerConditions.Disarmed;
                case BulletTriggerCondition.Expired: return BulletTriggerConditions.Expired;
                case BulletTriggerCondition.Detonated: return BulletTriggerConditions.Detonated;
                default: return BulletTriggerConditions.None;
            }
        }

        // Sound and static visual triggers are ignored (owner, 2026-10-29). SpawnBullet and Detonate run in the core,
        // except on the Cooldown condition; GravityField does not run yet.
        private static bool TryCheckTriggers(Ammunition ammunition, out bool hasGameplayTriggers, out string inertReason)
        {
            hasGameplayTriggers = false;
            foreach (var trigger in ammunition.Triggers)
            {
                switch (trigger.EffectType)
                {
                    case BulletEffectType.None:
                    case BulletEffectType.PlaySfx:
                    case BulletEffectType.SpawnStaticSfx:
                        continue;
                    case BulletEffectType.SpawnBullet:
                        if (trigger.Condition == BulletTriggerCondition.Cooldown)
                        {
                            inertReason = "SpawnBullet on Cooldown";
                            return false;
                        }
                        hasGameplayTriggers = true;
                        continue;
                    case BulletEffectType.Detonate:
                        hasGameplayTriggers = true;
                        continue;
                    default:
                        inertReason = $"trigger {trigger.EffectType}";
                        return false;
                }
            }

            inertReason = null;
            return true;
        }

        private static BulletEffectType FirstGameplayTrigger(Ammunition ammunition)
        {
            foreach (var trigger in ammunition.Triggers)
                if (trigger.EffectType == BulletEffectType.SpawnBullet || trigger.EffectType == BulletEffectType.Detonate)
                    return trigger.EffectType;
            return BulletEffectType.None;
        }

        // Damage effects summed per damage type and falloff in first-seen order, and the summed push powers.
        private readonly struct Effects
        {
            public readonly List<DamageAmount> Damage;
            public readonly float Push;
            public readonly float RadialPush;
            public readonly float RadialPushWithFalloff;

            public Effects(List<DamageAmount> damage, float push, float radialPush, float radialPushWithFalloff)
            {
                Damage = damage;
                Push = push;
                RadialPush = radialPush;
                RadialPushWithFalloff = radialPushWithFalloff;
            }

            public bool HasFalloff
            {
                get
                {
                    if (RadialPushWithFalloff != 0f) return true;
                    foreach (var item in Damage)
                        if (item.Falloff) return true;
                    return false;
                }
            }
        }

        // Damage and push effects only (phase 5, step 3: falloff and radial push, legacy BulletFactory). At least one
        // Damage effect, unless the bullet has a gameplay trigger.
        private static bool TryGetEffects(Ammunition ammunition, bool hasGameplayTriggers, out Effects effects,
            out string inertReason)
        {
            effects = default;
            var damage = new List<DamageAmount>(1);
            var push = 0f;
            var radialPush = 0f;
            var radialPushWithFalloff = 0f;
            foreach (var effect in ammunition.Effects)
            {
                switch (effect.Type)
                {
                    case ImpactEffectType.Damage:
                        AddDamage(damage, (int)effect.DamageType, effect.Power, false);
                        break;
                    case ImpactEffectType.DamageWithFalloff:
                        AddDamage(damage, (int)effect.DamageType, effect.Power, true);
                        break;
                    case ImpactEffectType.Push:
                        push += effect.Power;
                        break;
                    case ImpactEffectType.PushFromCenter:
                        radialPush += effect.Power;
                        break;
                    case ImpactEffectType.PullToCenter:
                        radialPush -= effect.Power;
                        break;
                    case ImpactEffectType.PushFromCenterWithFalloff:
                        radialPushWithFalloff += effect.Power;
                        break;
                    default:
                        inertReason = $"effect {effect.Type}";
                        return false;
                }
            }

            if (damage.Count == 0 && !hasGameplayTriggers)
            {
                inertReason = "no damage effect";
                return false;
            }

            effects = new Effects(damage, push, radialPush, radialPushWithFalloff);
            inertReason = null;
            return true;
        }

        private static void AddDamage(List<DamageAmount> damage, int damageType, float amount, bool falloff)
        {
            for (var i = 0; i < damage.Count; ++i)
            {
                if (damage[i].DamageType != damageType || damage[i].Falloff != falloff) continue;
                damage[i] = new DamageAmount(damageType, damage[i].Amount + amount, falloff);
                return;
            }
            damage.Add(new DamageAmount(damageType, amount, falloff));
        }

        private static BulletImpact ToImpact(BulletImpactType impact)
        {
            switch (impact)
            {
                case BulletImpactType.HitAllTargets: return BulletImpact.AllTargets;
                case BulletImpactType.DamageOverTime: return BulletImpact.OverTime;
                default: return BulletImpact.FirstTarget;
            }
        }

        // Legacy WeaponFactory: a base rate of 0 means no cooldown; a positive rate driven to 0 or below is blocked.
        private static float Cooldown(float fireRate, float multiplier)
        {
            if (fireRate > 0f && multiplier <= 0f) return float.PositiveInfinity;
            var rate = fireRate * multiplier;
            return rate > 0f ? 1f / rate : 0f;
        }

        // The circle collider of the legacy bullet prefab at body size 1 (PrefabCache.GetBulletPrefab).
        private static bool TryGetColliderRadius(BulletShape shape, out float radius)
        {
            switch (shape)
            {
                case BulletShape.Projectile:
                case BulletShape.Rocket:
                case BulletShape.Spark:
                    radius = 0.25f;
                    return true;
                case BulletShape.Mine:
                    radius = 0.5f;
                    return true;
                default:
                    radius = 0f;
                    return false;
            }
        }

        private static bool IsBeamShape(BulletShape shape) =>
            shape == BulletShape.LaserBeam || shape == BulletShape.LightningBolt ||
            shape == BulletShape.EnergyBeam || shape == BulletShape.PiercingLaser;

        private static DeviceDefinition Inert(string reason, out string inertReason)
        {
            inertReason = reason;
            return null;
        }

        private static BulletDefinition InertBullet(string reason, out string inertReason)
        {
            inertReason = reason;
            return null;
        }

        // The weapon's mapping state. Child sources reach the ship mapper only when the whole weapon maps.
        private sealed class Context
        {
            public Context(float armorMultiplier, ChildBullet children)
            {
                ArmorMultiplier = armorMultiplier;
                _children = children;
            }

            public float ArmorMultiplier { get; }

            // Child bullets by ammunition, nesting level and modifiers.
            public readonly Dictionary<(Ammunition, int, WeaponStatModifier), BulletDefinition> Bullets =
                new Dictionary<(Ammunition, int, WeaponStatModifier), BulletDefinition>();

            public void Add(BulletDefinition bullet, Ammunition ammunition, ColorData color)
            {
                if (_children != null) _pending.Add((bullet, ammunition, color));
            }

            public void Commit()
            {
                foreach (var (bullet, ammunition, color) in _pending)
                    _children(bullet, ammunition, color);
            }

            private readonly ChildBullet _children;
            private readonly List<(BulletDefinition, Ammunition, ColorData)> _pending = new List<(BulletDefinition, Ammunition, ColorData)>();
        }

        // Legacy BulletFactory.MaxNestingLevel.
        private const int MaxNestingLevel = 100;

        // Distinct child bullets of one weapon. Triggers that change the modifiers differently can branch at every level.
        private const int MaxChildBullets = 1000;

        private const float LinearTolerance = 1e-4f;
        private const float DegreesToRadians = (float)(Math.PI / 180.0);
    }
}
