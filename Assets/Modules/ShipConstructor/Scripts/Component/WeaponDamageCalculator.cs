using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Constructor.Component
{
    public class WeaponDamageCalculator : IBulletTriggerFactory<BulletTrigger_SpawnBullet>
    {
        public WeaponInfo CalculateWeaponDamage(WeaponStats weapon, AmmunitionObsoleteStats ammunition)
        {
            GetWeaponData(ref weapon, out var fireRate, out var damageMultiplier, out var continuous);

            var damage = continuous ? new DamageInfo() : GetDamage(ref ammunition);
            var dps = continuous ? GetContinuousDPS(ref ammunition) : GetDPS(ref ammunition, fireRate);

            damage *= damageMultiplier;
            dps *= damageMultiplier;

            return new WeaponInfo
            {
                Damage = damage,
                Dps = dps,

                Magazine = weapon.Magazine,
                Continuous = continuous,
                EnergyCost = ammunition.EnergyCost,
                FireRate = weapon.FireRate,
                Energy = ammunition.EnergyCost,
                Range = ammunition.Range > 0 ? ammunition.Range : ammunition.LifeTime * ammunition.Velocity,
                BulletVelocity = ammunition.Velocity,
                Impulse = ammunition.Impulse,
                AreaOfEffect = ammunition.AreaOfEffect,
            };
        }

        public WeaponInfo CalculateWeaponDamage(WeaponStats weapon, Ammunition ammunition, WeaponStatModifier statModifier)
        {
            GetWeaponData(ref weapon, out var fireRate, out var damageMultiplier, out var continuous);

            if (ammunition.ImpactType == BulletImpactType.DamageOverTime && ammunition.Body.Lifetime >= 1f / fireRate)
                continuous = true;

            damageMultiplier *= statModifier.DamageMultiplier.Value;
            fireRate *= statModifier.FireRateMultiplier.Value;
            // TODO: statModifier.LifetimeMultiplier.Value

            var damage = continuous ? new DamageInfo() : GetDPS(ammunition, 1.0f);
            var dps = continuous ? GetContinuousDPS(ammunition) : GetDPS(ammunition, fireRate);

            damage *= damageMultiplier;
            dps *= damageMultiplier;

            return new WeaponInfo
            {
                Damage = damage,
                Dps = dps,

                Magazine = weapon.Magazine,
                Continuous = continuous,
                EnergyCost = ammunition.Body.EnergyCost * statModifier.EnergyCostMultiplier.Value,
                FireRate = weapon.FireRate * statModifier.FireRateMultiplier.Value,
                Energy = ammunition.Body.EnergyCost * statModifier.EnergyCostMultiplier.Value,
                Range = ammunition.Body.Range * statModifier.RangeMultiplier.Value,
                BulletVelocity = ammunition.Body.Velocity * statModifier.VelocityMultiplier.Value,
                Impulse = ammunition.Body.Weight * statModifier.WeightMultiplier.Value,
                //AreaOfEffect = 
            };
        }

        public WeaponInfo CalculateWeaponDamage(GameDatabase.DataModel.Component item)
        {
            if (item.Weapon == null)
                throw new System.ArgumentException("CalculateWeaponDamage: component is not weapon");

            if (item.Ammunition != null) 
                return CalculateWeaponDamage(item.Weapon.Stats, item.Ammunition, new WeaponStatModifier());
            else
                return CalculateWeaponDamage(item.Weapon.Stats, item.AmmunitionObsolete.Stats);
        }

        private static void GetWeaponData(ref WeaponStats weapon, out float fireRate, out float damageMultiplier, out bool continuous)
        {
            fireRate = weapon.FireRate;
            damageMultiplier = 1.0f;
            continuous = false;

            switch (weapon.WeaponClass)
            {
                case WeaponClass.Common:
                case WeaponClass.Manageable:
                case WeaponClass.RequiredCharging:
                    break;
                case WeaponClass.MashineGun:
                    //fireRate = weapon.Magazine / (weapon.Magazine * MinCooldown + 1.0f / weapon.FireRate);
                    break;
                case WeaponClass.MultiShot:
                    //damageMultiplier = weapon.Magazine;
                    break;
                case WeaponClass.Continuous:
                    continuous = true;
                    break;
            }
        }

        //private static bool IsHoming(AmmunitionClassObsolete ammunition)
        //{
        //    switch (ammunition)
        //    {
        //        case AmmunitionClassObsolete.Rocket:
        //        case AmmunitionClassObsolete.AcidRocket:
        //        case AmmunitionClassObsolete.HomingTorpedo:
        //        case AmmunitionClassObsolete.HomingImmobilizer:
        //        case AmmunitionClassObsolete.EmpMissile:
        //        case AmmunitionClassObsolete.ClusterMissile:
        //        case AmmunitionClassObsolete.HomingCarrier:
        //            return true;
        //        default:
        //            return false;
        //    }
        //}

        private DamageInfo GetDamage(ref AmmunitionObsoleteStats ammunition)
        {
            switch (ammunition.AmmunitionClass)
            {
                case AmmunitionClassObsolete.PlasmaWeb:
                case AmmunitionClassObsolete.DamageOverTime:
                case AmmunitionClassObsolete.Acid:
                    return new DamageInfo();
                default:
                    return GetDPS(ref ammunition, 1.0f);
            }
        }

        private DamageInfo GetDPS(ref AmmunitionObsoleteStats ammunition, float fireRate)
        {
            var damage = new DamageInfo();

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
                    return damage.Add(ammunition.Damage * fireRate, ammunition.DamageType);
                case AmmunitionClassObsolete.Aura:
                case AmmunitionClassObsolete.PlasmaWeb:
                case AmmunitionClassObsolete.DamageOverTime:
                case AmmunitionClassObsolete.Acid:
                    return damage.Add(ammunition.Damage, ammunition.DamageType);
                case AmmunitionClassObsolete.RepairRay:
                case AmmunitionClassObsolete.TractorBeam:
                case AmmunitionClassObsolete.EnergyBeam:
                case AmmunitionClassObsolete.EnergySiphon:
                case AmmunitionClassObsolete.VampiricRay:
                case AmmunitionClassObsolete.SmallVampiricRay:
                case AmmunitionClassObsolete.LaserBeam:
                    if (ammunition.LifeTime >= 0.5f)
                        return damage.Add(ammunition.Damage * ammunition.LifeTime * 0.5f * fireRate, ammunition.DamageType);
                    else
                        return damage.Add(ammunition.Damage * ammunition.LifeTime * fireRate, ammunition.DamageType);
                case AmmunitionClassObsolete.AcidRocket:
                    return damage.Add(ammunition.Damage * ammunition.LifeTime * fireRate, ammunition.DamageType);
                case AmmunitionClassObsolete.Fireworks:
                    return damage.Add(ammunition.Damage * /*20 **/ fireRate, ammunition.DamageType);
                case AmmunitionClassObsolete.ClusterMissile:
                case AmmunitionClassObsolete.FragBomb:
                    return damage.Add(ammunition.Damage * /*20 **/ fireRate, ammunition.DamageType);
                case AmmunitionClassObsolete.Carrier:
                case AmmunitionClassObsolete.HomingCarrier:
                    return damage.Add(ammunition.Damage * fireRate, ammunition.DamageType);
                default:
                    return damage;
            }
        }

        private DamageInfo GetContinuousDPS(ref AmmunitionObsoleteStats ammunition)
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
                case AmmunitionClassObsolete.PlasmaWeb:
                    return new DamageInfo().Add(ammunition.Damage, ammunition.DamageType);
                default:
                    return new DamageInfo();
            }
        }

        private DamageInfo GetSpawnDPS(Ammunition ammunition, float fireRate, float powerMultiplier = 1.0f, int nestingLevel = 0)
        {
            var damage = new DamageInfo();

            if (nestingLevel >= 100) return damage;

            foreach (var item in ammunition.Triggers)
            {
                if (item.EffectType != BulletEffectType.SpawnBullet) continue;
                var trigger = item.Create(this);

                if (trigger.MaxNestingLevel > 0 && nestingLevel > trigger.MaxNestingLevel) continue;

                var bulletPower = trigger.PowerMultiplier > 0 ? trigger.PowerMultiplier : 1.0f;
                var quantity = trigger.Quantity > 0 ? trigger.Quantity : 1;
                var spawnRate = GetSpawnRate(ammunition, trigger, fireRate);

                if (trigger.Ammunition != null)
                    damage += GetDPS(trigger.Ammunition, bulletPower /** quantity*/, spawnRate, nestingLevel + 1) * powerMultiplier;
            }

            return damage;
        }

        private static float GetSpawnRate(Ammunition ammunition, BulletTrigger_SpawnBullet trigger, float fireRate)
        {
            if ((ammunition.ImpactType == BulletImpactType.HitAllTargets ||
                ammunition.ImpactType == BulletImpactType.DamageOverTime) &&
                trigger.Condition == BulletTriggerCondition.Hit)
            {
                return trigger.Cooldown > 0 ? 1.0f / trigger.Cooldown : fireRate;
            }

            return 1.0f;
        }

        private DamageInfo GetContinuousDPS(Ammunition ammunition, float powerMultiplier = 1.0f, int nestingLevel = 0)
        {
            var damage = GetSpawnDPS(ammunition, 0.0f, powerMultiplier, nestingLevel);

            var impactDamage = new DamageInfo();
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
                    case ImpactEffectType.RestoreLifetime:
                    default:
                        continue;
                }

                impactDamage.Add(effect.Power * powerMultiplier, effect.DamageType);
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

        private DamageInfo GetDPS(Ammunition ammunition, float fireRate, float powerMultiplier = 1.0f, int nestingLevel = 0)
        {
            var damage = GetSpawnDPS(ammunition, fireRate, powerMultiplier, nestingLevel);

            var impactDamage = new DamageInfo();
            foreach (var effect in ammunition.Effects)
            {
                switch (effect.Type)
                {
                    case ImpactEffectType.Damage:
                    case ImpactEffectType.SiphonHitPoints:
                    case ImpactEffectType.Devour:
                        break;
                    case ImpactEffectType.DrainEnergy:
                    case ImpactEffectType.SlowDown:
                    case ImpactEffectType.CaptureDrones:
                    case ImpactEffectType.Repair:
                    case ImpactEffectType.RestoreLifetime:
                    default:
                        continue;
                }

                impactDamage.Add(effect.Power * powerMultiplier, effect.DamageType);
            }

            switch (ammunition.ImpactType)
            {
                case BulletImpactType.HitFirstTarget:
                case BulletImpactType.HitAllTargets:
                    damage += impactDamage * fireRate;
                    break;
                case BulletImpactType.DamageOverTime:
                    damage += impactDamage * fireRate * ammunition.Body.Lifetime;
                    break;
            }

            return damage;
        }

        public BulletTrigger_SpawnBullet Create(BulletTrigger_None content) { return null; }
        public BulletTrigger_SpawnBullet Create(BulletTrigger_PlaySfx content) { return null; }
        public BulletTrigger_SpawnBullet Create(BulletTrigger_SpawnBullet content) { return content; }
        public BulletTrigger_SpawnBullet Create(BulletTrigger_Detonate content) { return null; }
        public BulletTrigger_SpawnBullet Create(BulletTrigger_SpawnStaticSfx content) { return null; }
        public BulletTrigger_SpawnBullet Create(BulletTrigger_GravityField content) { return null; }

        private const float MinCooldown = 0.04f;

        public struct DamageInfo
        {
            public DamageInfo Add(float damage, DamageType type)
            {
                switch (type)
                {
                    case DamageType.Impact:
                        Kinetic += damage; break;
                    case DamageType.Heat:
                        Heat += damage; break;
                    case DamageType.Energy:
                        Energy += damage; break;
                    case DamageType.Direct:
                        Direct += damage; break;
                }
                return this;
            }

            public static DamageInfo operator+(DamageInfo first, DamageInfo second)
            {
                return new DamageInfo { 
                    Kinetic = first.Kinetic + second.Kinetic, 
                    Energy = first.Energy + second.Energy,
                    Heat = first.Heat + second.Heat,
                    Direct = first.Direct + second.Direct };
            }

            public static DamageInfo operator *(DamageInfo first, float second)
            {
                return new DamageInfo
                {
                    Kinetic = first.Kinetic * second,
                    Energy = first.Energy * second,
                    Heat = first.Heat * second,
                    Direct = first.Direct * second
                };
            }

            public bool Any => Kinetic > 0 || Heat > 0 || Energy > 0 || Direct > 0;

            public float Kinetic;
            public float Heat;
            public float Energy;
            public float Direct;
        }

        public struct WeaponInfo 
        {
            public DamageInfo Damage;
            public DamageInfo Dps;
            public int Magazine;
            public bool Continuous;
            public float EnergyCost;
            public float FireRate;
            public float Energy;
            public float Range;
            public float BulletVelocity;
            public float Impulse;
            public float AreaOfEffect;
        }
    }
}
