using EditorDatabase;
using EditorDatabase.DataModel;
using static Analyzer.MainWindow;
using System;
using EditorDatabase.Serializable;
using EditorDatabase.Enums;

namespace Analyzer.Analyzer
{
    public class Statistics
    {
        public Statistics(Database database)
        {
            _database = database;

            _components = new List<Component>(
                _database.GetItemList(typeof(Component)).Select(id => _database.GetComponent(id.Value)));
            _components.Sort((x, y) => x.Id.Value - y.Id.Value);
        }

        public int WeaponCount => _components.Count(item => !item.Weapon.IsNull);

        public IEnumerable<WeaponStats> GetCannonStats()
        {
            foreach (var item in _components)
            {
                if (item.Weapon.IsNull) continue;
                var weapon = _database.GetWeapon(item.Weapon.Value);
                float fireRate = weapon.FireRate.Value;
                float energyRate = weapon.FireRate.Value;
                bool continuous = false;

                switch (weapon.WeaponClass)
                {
                    case WeaponClass.Common:
                    case WeaponClass.Manageable:
                    case WeaponClass.RequiredCharging:
                        break;
                    case WeaponClass.MashineGun:
                        fireRate = weapon.Magazine.Value / (weapon.Magazine.Value * MinCooldown + 1.0f / weapon.FireRate.Value);
                        energyRate = fireRate;
                        break;
                    case WeaponClass.MultiShot:
                        fireRate *= weapon.Magazine.Value;
                        break;
                    case WeaponClass.Continuous:
                        continuous = true;
                        break;
                }

                var ammunition = item.Ammunition.IsNull ? null : _database.GetAmmunition(item.Ammunition.Value);
                var ammunitionOld = item.AmmunitionObsolete.IsNull ? null : _database.GetAmmunitionObsolete(item.AmmunitionObsolete.Value);

                //if (GetAmmoType(ammunition, ammunitionOld) != AmmoType.Projectile) continue;

                var stats = new WeaponStats {
                    id = item.Id.Value,
                    name = item.Name,
                    level = item.Level.Value,
                    size = item.Layout.CellCount,
                    special = item.Availability == Availability.None,
                };

                if (ammunition != null)
                {
                    stats.dps = continuous ? GetContinuousDPS(ammunition) : GetDPS(ammunition, fireRate);
                    stats.eps = continuous ? ammunition.Body.Value.EnergyCost.Value : ammunition.Body.Value.EnergyCost.Value * energyRate;
                    stats.range = ammunition.Body.Value.Range.Value;
                    stats.homing = ammunition.Controller.Value.Type == BulletControllerType.Homing;
                }
                else if (ammunitionOld != null)
                {
                    stats.dps = continuous ? GetContinuousDPS(ammunitionOld) : GetDPS(ammunitionOld, fireRate);
                    stats.eps = continuous ? ammunitionOld.EnergyCost.Value : ammunitionOld.EnergyCost.Value * energyRate;
                    stats.range = ammunitionOld.AreaOfEffect.Value + ammunitionOld.Range.Value;
                    stats.homing = IsHoming(ammunitionOld.AmmunitionClass);
                }

                stats.score = CalculateTotalScore(stats);

                yield return stats;
            }
        }

        private static bool IsHoming(AmmunitionClassObsolete ammunition)
        {
            switch (ammunition)
            {
                case AmmunitionClassObsolete.Rocket:
                case AmmunitionClassObsolete.AcidRocket:
                case AmmunitionClassObsolete.HomingTorpedo:
                case AmmunitionClassObsolete.HomingImmobilizer:
                case AmmunitionClassObsolete.EmpMissile:
                case AmmunitionClassObsolete.ClusterMissile:
                case AmmunitionClassObsolete.HomingCarrier:
                    return true;
                default:
                    return false;
            }
        }

        private float CalculateTotalScore(WeaponStats stats)
        {
            var score = stats.dps / MathF.Sqrt(stats.size);
            score *= 20f / (20f + stats.eps);
            score *= MathF.Pow(1.0f + stats.range, 0.5f);
            if (stats.homing) score *= 2;

            return score;
        }

        private float GetDPS(AmmunitionObsolete ammunition, float fireRate)
        {
            var lifetime = ammunition.Velocity.Value > 0 && ammunition.Range.Value > 0 ? 
                ammunition.Range.Value / ammunition.Velocity.Value : ammunition.LifeTime.Value;

            switch (ammunition.AmmunitionClass)
            {
                case AmmunitionClassObsolete.Common:
                case AmmunitionClassObsolete.Bomb:
                case AmmunitionClassObsolete.IonBeam:
                case AmmunitionClassObsolete.DroneControl:
                case AmmunitionClassObsolete.Emp:
                case AmmunitionClassObsolete.Immobilizer:
                case AmmunitionClassObsolete.UnguidedRocket:
                case AmmunitionClassObsolete.Singularity:
                case AmmunitionClassObsolete.Fragment:
                case AmmunitionClassObsolete.HomingImmobilizer:
                case AmmunitionClassObsolete.HomingTorpedo:
                case AmmunitionClassObsolete.EmpMissile:
                case AmmunitionClassObsolete.Rocket:
                case AmmunitionClassObsolete.Explosion:
                case AmmunitionClassObsolete.BlackHole:
                    return ammunition.Damage.Value * fireRate;
                case AmmunitionClassObsolete.EnergyBeam:
                case AmmunitionClassObsolete.LaserBeam:
                case AmmunitionClassObsolete.VampiricRay:
                case AmmunitionClassObsolete.EnergySiphon:
                case AmmunitionClassObsolete.RepairRay:
                case AmmunitionClassObsolete.TractorBeam:
                case AmmunitionClassObsolete.Aura:
                case AmmunitionClassObsolete.SmallVampiricRay:
                case AmmunitionClassObsolete.PlasmaWeb:
                    return ammunition.Damage.Value * lifetime * fireRate;
                case AmmunitionClassObsolete.DamageOverTime:
                case AmmunitionClassObsolete.Acid:
                    return ammunition.Damage.Value * fireRate * MathF.Min(lifetime, 1.0f);
                case AmmunitionClassObsolete.AcidRocket:
                    return ammunition.Damage.Value * (1f + lifetime) * fireRate;
                case AmmunitionClassObsolete.Fireworks:
                    return ammunition.Damage.Value * 20 * fireRate;
                case AmmunitionClassObsolete.ClusterMissile:
                case AmmunitionClassObsolete.FragBomb:
                    return ammunition.Damage.Value * (20 + 1) * fireRate;
                case AmmunitionClassObsolete.Carrier:
                case AmmunitionClassObsolete.HomingCarrier:
                    return ammunition.Damage.Value * 2 * fireRate;
                default:
                    return 0.0f;
            }
        }

        private float GetContinuousDPS(AmmunitionObsolete ammunition)
        {
            switch (ammunition.AmmunitionClass)
            {
                case AmmunitionClassObsolete.EnergyBeam:
                case AmmunitionClassObsolete.LaserBeam:
                case AmmunitionClassObsolete.VampiricRay:
                case AmmunitionClassObsolete.EnergySiphon:
                case AmmunitionClassObsolete.RepairRay:
                case AmmunitionClassObsolete.TractorBeam:
                case AmmunitionClassObsolete.Aura:
                case AmmunitionClassObsolete.DamageOverTime:
                case AmmunitionClassObsolete.SmallVampiricRay:
                    return ammunition.Damage.Value;
                default:
                    return 0.0f;
            }
        }

        private float GetSpawnDPS(Ammunition ammunition, float fireRate, float powerMultiplier = 1.0f, int nestingLevel = 0)
        {
            float damage = 0f;

            if (nestingLevel >= 100) return damage;

            if (ammunition.Triggers != null)
                foreach (var trigger in ammunition.Triggers.Where(item => item.EffectType == BulletEffectType.SpawnBullet))
                {
					var spawnBullet = trigger.Content as BulletTrigger_SpawnBullet;
                    if (spawnBullet.MaxNestingLevel.Value > 0 && nestingLevel > spawnBullet.MaxNestingLevel.Value) continue;

                    var bulletPower = spawnBullet.PowerMultiplier.Value > 0 ? spawnBullet.PowerMultiplier.Value : 1.0f;
                    var quantity = spawnBullet.Quantity.Value > 0 ? spawnBullet.Quantity.Value : 1;
                    var spawnRate = trigger.Cooldown.Value > 0 ? 1.0f / trigger.Cooldown.Value : fireRate;

                    var bullet = _database.GetAmmunition(spawnBullet.Ammunition.Value);
                    damage += GetDPS(bullet, bulletPower * quantity, spawnRate, nestingLevel + 1) * powerMultiplier;
                }

            return damage;
        }

        private float GetContinuousDPS(Ammunition ammunition, float powerMultiplier = 1.0f, int nestingLevel = 0)
        {
            if (ammunition.Controller.Value.Type != BulletControllerType.Beam) return 0f;

            float damage = GetSpawnDPS(ammunition, 0.0f, powerMultiplier, nestingLevel);

            var impactDamage = 0.0f;
            if (ammunition.Effects != null)
                foreach (var effect in ammunition.Effects)
                {
                    switch (effect.Type)
                    {
                        case ImpactEffectType.Damage:
                        case ImpactEffectType.SiphonHitPoints:
                            break;
                        case ImpactEffectType.DrainEnergy:
                        case ImpactEffectType.SlowDown:
                        case ImpactEffectType.CaptureDrones:
                        case ImpactEffectType.Repair:
                            continue;
                    }

                    impactDamage += effect.Power.Value * powerMultiplier;
                }

            switch (ammunition.ImpactType)
            {
                //case EditorDatabase.Enums.BulletImpactType.HitFirstTarget:
                //case EditorDatabase.Enums.BulletImpactType.HitAllTargets:
                //    damage += impactDamage;
                //    break;
                case BulletImpactType.DamageOverTime:
                    damage += impactDamage;
                    break;
            }

            return damage;
        }

        private float GetDPS(Ammunition ammunition, float fireRate, float powerMultiplier = 1.0f, int nestingLevel = 0)
        {
            float damage = GetSpawnDPS(ammunition, fireRate, powerMultiplier, nestingLevel);

            var impactDamage = 0.0f;
            if (ammunition.Effects != null)
                foreach (var effect in ammunition.Effects)
                {
                    switch (effect.Type)
                    {
                        case ImpactEffectType.Damage:
                        case ImpactEffectType.SiphonHitPoints:
                            break;
                        case ImpactEffectType.DrainEnergy:
                        case ImpactEffectType.SlowDown:
                        case ImpactEffectType.CaptureDrones:
                        case ImpactEffectType.Repair:
                            continue;
                    }

                    impactDamage += effect.Power.Value * powerMultiplier;
                }

            switch (ammunition.ImpactType)
            {
                case BulletImpactType.HitFirstTarget:
                case BulletImpactType.HitAllTargets:
                    damage += impactDamage * fireRate;
                    break;
                case BulletImpactType.DamageOverTime:
                    damage += impactDamage * fireRate * ammunition.Body.Value.Lifetime.Value;
                    break;
            }

            return damage;
        }

        private readonly List<Component> _components;
        private readonly Database _database;

        private const float MinCooldown = 0.04f;

        private enum AmmoType
        {
            Unknown,
            Projectile,
            Continuous,
            Homing,
        }

        public struct WeaponStats
        {
            public int id;
            public int size;
            public string name;
            public int level;
            public float range;
            public float dps;
            public float eps;
            public bool homing;
            public bool special;

            public float score;
        }
    }
}
