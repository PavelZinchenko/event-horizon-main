using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Constructor.Component
{
    [System.Flags]
    public enum WeaponSpecialEffect
    {
        None = 0,
        ShieldPierce = 1,
        ProgressiveDamage = 2,
        TeleportTarget = 4,
        DisruptDrones = 8,
        SlowTarget = 16,
    }

    public class WeaponDamageCalculator : IBulletTriggerFactory<BulletTrigger_SpawnBullet>
    {
        public WeaponInfo CalculateWeaponDamage(WeaponStats weapon, AmmunitionObsoleteStats ammunition)
        {
            var fireRate = weapon.FireRate;
            var continuous = weapon.WeaponClass == WeaponClass.Continuous;
            var damage = continuous ? new DamageInfo() : GetDamage(ref ammunition);
            var dps = continuous ? GetContinuousDPS(ref ammunition) : GetDPS(ref ammunition, fireRate);

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
            var damageMultiplier = statModifier.DamageMultiplier.Value;
            var fireRate = weapon.FireRate * statModifier.FireRateMultiplier.Value;
            var isDot = IsDotWeapon(weapon, ammunition);
            // TODO: statModifier.LifetimeMultiplier.Value

            var damage = isDot ? new DamageInfo() : GetDPS(ammunition, 1.0f);
            var dps = isDot ? GetContinuousDPS(ammunition) : GetDPS(ammunition, fireRate);

            damage = DamageInfo.Boost(damage, damageMultiplier, false);
            dps = DamageInfo.Boost(dps, damageMultiplier, false);

            if (damage.EnergyDrain < 0)
                damage.EnergyDrain *= statModifier.EnergyCostMultiplier.Value;
            if (dps.EnergyDrain < 0)
                dps.EnergyDrain *= statModifier.EnergyCostMultiplier.Value;

            return new WeaponInfo
            {
                Damage = damage,
                Dps = dps,

                Magazine = weapon.Magazine,
                Continuous = weapon.WeaponClass == WeaponClass.Continuous,
                EnergyCost = ammunition.Body.EnergyCost * statModifier.EnergyCostMultiplier.Value,
                FireRate = weapon.FireRate * statModifier.FireRateMultiplier.Value,
                Energy = ammunition.Body.EnergyCost * statModifier.EnergyCostMultiplier.Value,
                Range = ammunition.Body.Range * statModifier.RangeMultiplier.Value,
                BulletVelocity = ammunition.Body.Velocity * statModifier.VelocityMultiplier.Value,
                Impulse = ammunition.Body.Weight * statModifier.WeightMultiplier.Value,
                AreaOfEffect = GetAoE(ammunition) * statModifier.AoeRadiusMultiplier.Value,
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

        private static bool IsDotWeapon(in WeaponStats weapon, in Ammunition ammunition)
        {
            if (ammunition.ImpactType != BulletImpactType.DamageOverTime) return false;

            switch (weapon.WeaponClass)
            {
                case WeaponClass.Common:
                    return ammunition.Body.Lifetime >= 1f / weapon.FireRate;
            }

            return true;
        }

        //private static void GetWeaponData(in WeaponStats weapon, out float fireRate, out float damageMultiplier)
        //{
        //    fireRate = weapon.FireRate;
        //    damageMultiplier = 1.0f;

        //    switch (weapon.WeaponClass)
        //    {
        //        case WeaponClass.Common:
        //        case WeaponClass.Manageable:
        //        case WeaponClass.RequiredCharging:
        //            break;
        //        case WeaponClass.MashineGun:
        //            //fireRate = weapon.Magazine / (weapon.Magazine * MinCooldown + 1.0f / weapon.FireRate);
        //            break;
        //        case WeaponClass.MultiShot:
        //            //damageMultiplier = weapon.Magazine;
        //            break;
        //        case WeaponClass.Continuous:
        //            break;
        //    }
        //}

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
                    case ImpactEffectType.Devour:
                        impactDamage.Add(effect.Power * powerMultiplier, effect.DamageType);
                        break;
                    case ImpactEffectType.ProgressiveDamage:
                        impactDamage.Add(effect.Power * powerMultiplier, effect.DamageType);
                        impactDamage.Effects |= WeaponSpecialEffect.ProgressiveDamage;
                        break;
                    case ImpactEffectType.Repair:
                        impactDamage.Repair += effect.Power * powerMultiplier;
                        //impactDamage.Add(effect.Power * effect.Factor * powerMultiplier, effect.DamageType);
                        break;
                    case ImpactEffectType.DrainShield:
                        impactDamage.Shield += effect.Power * powerMultiplier;
                        break;
                    case ImpactEffectType.RechargeShield:
                        impactDamage.Shield -= effect.Power * powerMultiplier;
                        break;
                    case ImpactEffectType.RechargeEnergy:
                        impactDamage.EnergyDrain -= effect.Power * powerMultiplier;
                        break;
                    case ImpactEffectType.DrainEnergy:
                        impactDamage.EnergyDrain += effect.Power * powerMultiplier;
                        break;
                    case ImpactEffectType.Teleport:
                        impactDamage.Effects |= WeaponSpecialEffect.TeleportTarget;
                        break;
                    case ImpactEffectType.DriveDronesCrazy:
                    case ImpactEffectType.CaptureDrones:
                        impactDamage.Effects |= WeaponSpecialEffect.DisruptDrones;
                        break;
                    case ImpactEffectType.IgnoreShield:
                        impactDamage.Effects |= WeaponSpecialEffect.ShieldPierce;
                        break;
                    case ImpactEffectType.SlowDown:
                        impactDamage.Effects |= WeaponSpecialEffect.SlowTarget;
                        break;
                    case ImpactEffectType.Push:
                    case ImpactEffectType.Pull:
                    case ImpactEffectType.RestoreLifetime:
                    default:
                        break;
                }
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
                        impactDamage.Add(effect.Power * powerMultiplier, effect.DamageType);
                        break;
                    case ImpactEffectType.ProgressiveDamage:
                        impactDamage.Effects |= WeaponSpecialEffect.ProgressiveDamage;
                        impactDamage.Add(effect.Power * powerMultiplier, effect.DamageType);
                        break;
                    case ImpactEffectType.Repair:
                        impactDamage.Repair += effect.Power * powerMultiplier;
                        //impactDamage.Add(effect.Power * effect.Factor * powerMultiplier, effect.DamageType);
                        break;
                    case ImpactEffectType.DrainShield:
                        impactDamage.Shield += effect.Power * powerMultiplier;
                        break;
                    case ImpactEffectType.RechargeShield:
                        impactDamage.Shield -= effect.Power * powerMultiplier;
                        break;
                    case ImpactEffectType.RechargeEnergy:
                        impactDamage.EnergyDrain -= effect.Power * powerMultiplier;
                        break;
                    case ImpactEffectType.DrainEnergy:
                        impactDamage.EnergyDrain += effect.Power * powerMultiplier;
                        break;
                    case ImpactEffectType.Teleport:
                        impactDamage.Effects |= WeaponSpecialEffect.TeleportTarget;
                        break;
                    case ImpactEffectType.DriveDronesCrazy:
                    case ImpactEffectType.CaptureDrones:
                        impactDamage.Effects |= WeaponSpecialEffect.DisruptDrones;
                        break;
                    case ImpactEffectType.IgnoreShield:
                        impactDamage.Effects |= WeaponSpecialEffect.ShieldPierce;
                        break;
                    case ImpactEffectType.SlowDown:
                        impactDamage.Effects |= WeaponSpecialEffect.SlowTarget;
                        break;
                    case ImpactEffectType.Push:
                    case ImpactEffectType.Pull:
                    case ImpactEffectType.RestoreLifetime:
                    default:
                        break;
                }
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

        private float GetAoE(Ammunition ammunition, int nestingLevel = 0)
        {
            if (ammunition == null) return 0;

            float aoe = 0;
            var shape = ammunition.Body.BulletPrefab != null ? ammunition.Body.BulletPrefab.Shape : BulletShape.Mine;
            switch (shape)
            {
                case BulletShape.Wave:
                case BulletShape.BlackHole:
                case BulletShape.CircularSaw:
                    aoe = ammunition.Body.Size;
                    break;
                case BulletShape.Mine:
                case BulletShape.Projectile:
                case BulletShape.Spark:
                case BulletShape.Rocket:
                    if (nestingLevel > 0)
                        aoe = UnityEngine.Mathf.Max(ammunition.Body.Range, ammunition.Body.Size);
                    else if (ammunition.ImpactType != BulletImpactType.HitFirstTarget)
                        aoe = ammunition.Body.Size;
                    break;
                case BulletShape.LaserBeam:
                case BulletShape.EnergyBeam:
                case BulletShape.LightningBolt:
                    aoe = nestingLevel > 0 ? ammunition.Body.Range : 0;
                    break;
                //case BulletShape.Harpoon:
            }

            if (nestingLevel >= 100) return aoe;

            foreach (var item in ammunition.Triggers)
            {
                if (item.EffectType != BulletEffectType.SpawnBullet) continue;
                var trigger = item.Create(this);
                if (trigger.MaxNestingLevel > 0 && nestingLevel > trigger.MaxNestingLevel) continue;
                
                var sizeMultiplier = trigger.Size > 0 ? trigger.Size : 1.0f;
                var spawnAoe = GetAoE(trigger.Ammunition, nestingLevel+1) * sizeMultiplier;
                if (spawnAoe > aoe) 
                    aoe = spawnAoe;
            }

            return aoe;
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
                    case DamageType.Corrosive:
                        Corrosive += damage; break;
                }
                return this;
            }

            public static DamageInfo operator+(DamageInfo first, DamageInfo second)
            {
                return new DamageInfo {
                    Kinetic = first.Kinetic + second.Kinetic,
                    Energy = first.Energy + second.Energy,
                    Heat = first.Heat + second.Heat,
                    Corrosive = first.Corrosive + second.Corrosive,
                    Repair = first.Repair + second.Repair,
                    Shield = first.Shield + second.Shield,
                    EnergyDrain = first.EnergyDrain + second.EnergyDrain,
                    Effects = first.Effects | second.Effects,
                };
            }

            public static DamageInfo operator *(DamageInfo first, float second) => Boost(first, second, true);

            public static DamageInfo Boost(in DamageInfo damage, float multiplier, bool boostEnergyDrain)
            {
                return new DamageInfo
                {
                    Kinetic = damage.Kinetic * multiplier,
                    Energy = damage.Energy * multiplier,
                    Heat = damage.Heat * multiplier,
                    Corrosive = damage.Corrosive * multiplier,
                    Repair = damage.Repair * multiplier,
                    Shield = damage.Shield * multiplier,
                    EnergyDrain = boostEnergyDrain ? damage.EnergyDrain * multiplier : damage.EnergyDrain,
                    Effects = damage.Effects,
                };
            }

            public bool Any => Kinetic > 0 || Heat > 0 || Energy > 0 || Corrosive > 0 || Shield > 0 || Repair > 0;
            public float Total => Kinetic + Heat + Energy + Corrosive + Repair + (Shield > 0 ? Shield : -Shield);

            public float Kinetic;
            public float Heat;
            public float Energy;
            public float Corrosive;
            public float Repair;
            public float Shield;
            public float EnergyDrain;
            public WeaponSpecialEffect Effects;
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
            public float TotalDamage => Damage.Total > 0 ? Damage.Total : Dps.Total;
            public WeaponSpecialEffect Effects => Damage.Effects | Dps.Effects;
        }
    }

    public static class WeaponSpecialEffectExtension
    {
        public static bool Contains(this WeaponSpecialEffect effect, WeaponSpecialEffect value) => (effect & value) == value;
    }
}
