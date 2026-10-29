using CombatSimulation.Core;
using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Combat.Integration.Core
{
    // Maps one weapon with new-format ammunition to a core cannon or beam (phase 4, step 3a, "Ship mapping (3a)",
    // rows 3-5). A weapon with any part the core cannot run is inert as a whole (D1); the reason names that part.
    public static class CoreWeaponMapper
    {
        public static DeviceDefinition Map(IWeaponData weapon, int barrelIndex, float armorMultiplier, out string inertReason)
        {
            var stats = weapon.Weapon.Stats;
            var ammunition = weapon.Ammunition;
            var modifier = weapon.Stats;

            if (!TryGetDamage(ammunition, out var damage, out inertReason))
                return null;

            var cooldown = Cooldown(stats.FireRate, modifier.FireRateMultiplier.Value);
            if (float.IsInfinity(cooldown))
                return Inert("fire rate blocked by modifiers", out inertReason);

            var body = ammunition.Body;
            var energyCost = body.EnergyCost * modifier.EnergyCostMultiplier.Value;
            var range = body.Range * modifier.RangeMultiplier.Value;
            var power = damage.Power * modifier.DamageMultiplier.Value;
            var damageType = (int)damage.DamageType;

            switch (stats.WeaponClass)
            {
                case WeaponClass.Common:
                    return MapCannon(weapon, barrelIndex, armorMultiplier, cooldown, energyCost, range, power, damageType, out inertReason);
                case WeaponClass.Continuous:
                    if (ammunition.Controller.Type != BulletControllerType.Beam)
                        return Inert($"continuous weapon with controller {ammunition.Controller.Type}", out inertReason);
                    if (ammunition.ImpactType != BulletImpactType.DamageOverTime)
                        return Inert($"beam with impact {ammunition.ImpactType}", out inertReason);
                    if (!(range > 0f))
                        return Inert("beam without range", out inertReason);

                    inertReason = null;
                    return new BeamDefinition(barrelIndex, cooldown, energyCost * body.Lifetime, energyCost, range, power, damageType);
                default:
                    return Inert($"weapon class {stats.WeaponClass}", out inertReason);
            }
        }

        private static DeviceDefinition MapCannon(IWeaponData weapon, int barrelIndex, float armorMultiplier, float cooldown,
            float energyCost, float range, float power, int damageType, out string inertReason)
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

            var bullet = new BulletDefinition(
                radius,
                speed,
                range,
                power,
                weight: body.Weight * modifier.WeightMultiplier.Value,
                parentVelocityEffect: body.ParentVelocityEffect,
                hitPoints: body.HitPoints * armorMultiplier * modifier.HitPointsMultiplier.Value,
                homing: homing,
                startingVelocityMultiplier: startingVelocity,
                damageType: damageType);

            inertReason = null;
            return new CannonDefinition(barrelIndex, cooldown, energyCost, weapon.Weapon.Stats.Spread * DegreesToRadians, bullet);
        }

        // Exactly one Damage effect and only triggers without gameplay effect (owner, 2026-10-29: sound and static
        // visuals are ignored).
        private static bool TryGetDamage(Ammunition ammunition, out ImpactEffect damage, out string inertReason)
        {
            damage = null;
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

            foreach (var effect in ammunition.Effects)
            {
                if (effect.Type != ImpactEffectType.Damage)
                {
                    inertReason = $"effect {effect.Type}";
                    return false;
                }

                if (damage != null)
                {
                    inertReason = "several damage effects";
                    return false;
                }

                damage = effect;
            }

            if (damage == null)
            {
                inertReason = "no damage effect";
                return false;
            }

            inertReason = null;
            return true;
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
