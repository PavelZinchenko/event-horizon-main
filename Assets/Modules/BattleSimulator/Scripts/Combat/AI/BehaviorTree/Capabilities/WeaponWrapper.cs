using Combat.Component.Systems.Weapons;
using GameDatabase.Enums;

namespace Combat.Ai.BehaviorTree
{
    public class WeaponWrapper
    {
        public enum AimingStrategy
        {
            NoAiming,
            AimDirectly,
            AimWithPrediction,
        }

        public enum ActivationStrategy
        {
            Always,
            WhenInRange,
            WhenAimedRoughly,
            WhenAimedPrecisely,
        }

        public enum ControllingStrategy
        {
            Never,
            Always,
            WhenHitTarget,
            WhenCloseToTarget,
            WhenTargetApproaching,
        }

        private readonly int _systemId;
        private readonly IWeapon _weapon;
        private readonly ShipCapabilities _ship;

        public int SystemId => _systemId;
        public IWeapon Weapon => _weapon;

        public WeaponCapability Capability => _weapon.Info.Capability;
        public AiBulletBehavior BulletType => _weapon.Info.BulletType;

        public AimingStrategy Aiming { get; }
        public ActivationStrategy Activation { get; }
        public ControllingStrategy Controlling { get; }
        public bool HasAutoTargeting => _weapon.Platform.AutoAimingAngle >= 5;

        public WeaponWrapper(int systemId, IWeapon weapon, ShipCapabilities ship)
        {
            _systemId = systemId;
            _weapon = weapon;
            _ship = ship;

            Aiming = DetermineAimingStrategy();
            Activation = DetermineActivationStrategy();
            Controlling = DetermineControllingStrategy();
        }

        private AimingStrategy DetermineAimingStrategy()
        {
            var noPrediction = _ship.AiLevel < AiDifficultyLevel.Hard;
            var bulletType = _weapon.Info.BulletType;
            switch (bulletType)
            {
                case AiBulletBehavior.Homing:
                case AiBulletBehavior.AreaOfEffect:
                case AiBulletBehavior.Trap:
                    return AimingStrategy.NoAiming;
                case AiBulletBehavior.Projectile:
                case AiBulletBehavior.Harpoon:
                    return noPrediction ? AimingStrategy.AimDirectly : AimingStrategy.AimWithPrediction;
                case AiBulletBehavior.Beam:
                default:
                    return AimingStrategy.AimDirectly;
            }
        }

        private ActivationStrategy DetermineActivationStrategy()
        {
            //var noRechargeCooldown = _ship.Stats.EnergyRechargeCooldown < 0.1f;
            var bulletType = _weapon.Info.BulletType;
            var needEnergy = _weapon.Info.EnergyCost > 0;
            var firerate = _weapon.Info.Firerate;
            var weaponType = _weapon.Info.WeaponType;

            if (weaponType == WeaponType.Continuous && !needEnergy)
                return ActivationStrategy.Always;

            switch (bulletType)
            {
                case AiBulletBehavior.Trap:
                    return ActivationStrategy.Always;
                case AiBulletBehavior.Homing:
                case AiBulletBehavior.AreaOfEffect:
                    return ActivationStrategy.WhenInRange;
                case AiBulletBehavior.Projectile:
                    return firerate > 1.0f || HasAutoTargeting ? ActivationStrategy.WhenAimedRoughly : ActivationStrategy.WhenAimedPrecisely;
                case AiBulletBehavior.Harpoon:
                    return HasAutoTargeting ? ActivationStrategy.WhenAimedRoughly : ActivationStrategy.WhenAimedPrecisely;
                case AiBulletBehavior.Beam:
                default:
                    return ActivationStrategy.WhenAimedRoughly;
            }
        }

        private ControllingStrategy DetermineControllingStrategy()
        {
            //var noRechargeCooldown = _ship.Stats.EnergyRechargeCooldown < 0.1f;
            var bulletType = _weapon.Info.BulletType;
            var needEnergy = _weapon.Info.EnergyCost > 0;
            //var firerate = _weapon.Info.Firerate;
            var weaponType = _weapon.Info.WeaponType;

            if (weaponType == WeaponType.Common)
                return ControllingStrategy.Never;

            if (bulletType == AiBulletBehavior.Harpoon)
                return ControllingStrategy.WhenTargetApproaching;

            if (weaponType == WeaponType.Continuous)
            {
                if (!needEnergy)
                    return ControllingStrategy.Always;
                else if (bulletType == AiBulletBehavior.AreaOfEffect)
                    return ControllingStrategy.WhenCloseToTarget;
                else
                    return ControllingStrategy.WhenHitTarget;
            }
            
            if (weaponType == WeaponType.Manageable)
                return ControllingStrategy.WhenTargetApproaching;

            if (weaponType == WeaponType.RequiredCharging)
                return ControllingStrategy.Always;

            return ControllingStrategy.Never;
        }
    }
}
