using System.Collections.Generic;
using CombatSimulation.Core;
using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Combat.Integration.Core
{
    // Maps one weapon with new-format ammunition to a core cannon or beam (phase 4, step 3a, "Ship mapping (3a)",
    // rows 3-5). A weapon with any part the core cannot run is inert as a whole (D1); the reason names that part.
    // Phase 4, step 4b: cannons also take recoil (E1), push and several damage effects (E4).
    public static class CoreWeaponMapper
    {
        public static DeviceDefinition Map(IWeaponData weapon, int barrelIndex, float armorMultiplier, out string inertReason)
        {
            var stats = weapon.Weapon.Stats;
            var ammunition = weapon.Ammunition;
            var modifier = weapon.Stats;

            if (!TryGetEffects(ammunition, out var effects, out inertReason))
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
                    return MapCannon(weapon, barrelIndex, armorMultiplier, cooldown, energyCost, range, effects, out inertReason);
                case WeaponClass.Continuous:
                    if (effects.Damage.Count > 1)
                        return Inert("beam with several damage effects", out inertReason);
                    if (effects.Push != 0f)
                        return Inert("beam with push", out inertReason);
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

        private static DeviceDefinition MapCannon(IWeaponData weapon, int barrelIndex, float armorMultiplier, float cooldown,
            float energyCost, float range, in Effects effects, out string inertReason)
        {
            var ammunition = weapon.Ammunition;
            var modifier = weapon.Stats;
            var body = ammunition.Body;

            var homing = false;
            var startingVelocity = 1f;
            switch (ammunition.Controller)
            {
                case BulletController_Projectile _:
                    break;
                case BulletController_Homing controller:
                    if (controller.SmartAim || controller.IgnoreRotation)
                        return Inert("homing with smart aim or ignored rotation", out inertReason);
                    homing = true;
                    startingVelocity = controller.StartingVelocityModifier;
                    break;
                default:
                    return Inert($"cannon with controller {ammunition.Controller.Type}", out inertReason);
            }

            if (ammunition.ImpactType != BulletImpactType.HitFirstTarget)
                return Inert($"cannon with impact {ammunition.ImpactType}", out inertReason);

            // Without a prefab legacy uses the area-of-effect bullet.
            if (body.BulletPrefab == null)
                return Inert("no bullet prefab", out inertReason);

            var shape = body.BulletPrefab.Shape;
            if (!TryGetColliderRadius(shape, out var colliderRadius))
                return Inert(IsBeamShape(shape) ? "moving beam bolt" : $"bullet shape {shape}", out inertReason);

            var radius = colliderRadius * body.Size;
            var speed = body.Velocity * modifier.VelocityMultiplier.Value;
            if (!(radius > 0f) || !(speed > 0f) || !(range > 0f))
                return Inert("bullet without size, speed or range", out inertReason);

            // Legacy BulletFactory: damage x DamageMultiplier, push x EffectPowerMultiplier; recoil x WeightMultiplier
            // (BulletStats.Recoil), none for a bullet attached to its parent (ApplyRecoil). Power level 1: no charge.
            var damageMultiplier = modifier.DamageMultiplier.Value;
            DamageAmount[] extraDamage = null;
            if (effects.Damage.Count > 1)
            {
                extraDamage = new DamageAmount[effects.Damage.Count - 1];
                for (var i = 0; i < extraDamage.Length; ++i)
                {
                    var item = effects.Damage[i + 1];
                    extraDamage[i] = new DamageAmount(item.DamageType, item.Amount * damageMultiplier);
                }
            }

            var bullet = new BulletDefinition(
                radius,
                speed,
                range,
                effects.Damage[0].Amount * damageMultiplier,
                weight: body.Weight * modifier.WeightMultiplier.Value,
                parentVelocityEffect: body.ParentVelocityEffect,
                hitPoints: body.HitPoints * armorMultiplier * modifier.HitPointsMultiplier.Value,
                homing: homing,
                startingVelocityMultiplier: startingVelocity,
                damageType: effects.Damage[0].DamageType,
                recoil: body.AttachedToParent ? 0f : body.Recoil * modifier.WeightMultiplier.Value,
                push: effects.Push * modifier.EffectPowerMultiplier.Value,
                extraDamage: extraDamage);

            inertReason = null;
            return new CannonDefinition(barrelIndex, cooldown, energyCost, weapon.Weapon.Stats.Spread * DegreesToRadians, bullet);
        }

        // Damage effects summed per damage type in first-seen order, and the summed Push power.
        private readonly struct Effects
        {
            public readonly List<DamageAmount> Damage;
            public readonly float Push;

            public Effects(List<DamageAmount> damage, float push)
            {
                Damage = damage;
                Push = push;
            }
        }

        // At least one Damage effect, any Push effects, and only triggers without gameplay effect (owner, 2026-10-29:
        // sound and static visuals are ignored).
        private static bool TryGetEffects(Ammunition ammunition, out Effects effects, out string inertReason)
        {
            effects = default;
            foreach (var trigger in ammunition.Triggers)
            {
                switch (trigger.EffectType)
                {
                    case BulletEffectType.None:
                    case BulletEffectType.PlaySfx:
                    case BulletEffectType.SpawnStaticSfx:
                        continue;
                    default:
                        inertReason = $"trigger {trigger.EffectType}";
                        return false;
                }
            }

            var damage = new List<DamageAmount>(1);
            var push = 0f;
            foreach (var effect in ammunition.Effects)
            {
                switch (effect.Type)
                {
                    case ImpactEffectType.Damage:
                        AddDamage(damage, (int)effect.DamageType, effect.Power);
                        break;
                    case ImpactEffectType.Push:
                        push += effect.Power;
                        break;
                    default:
                        inertReason = $"effect {effect.Type}";
                        return false;
                }
            }

            if (damage.Count == 0)
            {
                inertReason = "no damage effect";
                return false;
            }

            effects = new Effects(damage, push);
            inertReason = null;
            return true;
        }

        private static void AddDamage(List<DamageAmount> damage, int damageType, float amount)
        {
            for (var i = 0; i < damage.Count; ++i)
            {
                if (damage[i].DamageType != damageType) continue;
                damage[i] = new DamageAmount(damageType, damage[i].Amount + amount);
                return;
            }
            damage.Add(new DamageAmount(damageType, amount));
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

        private const float DegreesToRadians = (float)(System.Math.PI / 180.0);
    }
}
