using Combat.Ai.Calculations;
using Combat.Component.Body;
using Combat.Component.Platform;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Ai.BehaviorTree
{
    public struct AimingData
    {
        public bool InRange;
        public bool ReadyToFire;
        public float? Angle;
        public Vector2 Position;
    }

    public static class WeaponWrapperExtensions
    {
        public static bool IsReadyToFire(this WeaponWrapper weapon)
        {
            if (!weapon.Weapon.CanBeActivated) return false;
            if (weapon.BulletType == AiBulletBehavior.Harpoon && weapon.Weapon.ActiveBullet != null) return false; // TODO
            if (weapon.Weapon.Info.WeaponType == WeaponType.RequiredCharging && weapon.Weapon.PowerLevel < 1.0f) return false;
            return true;
        }

        public static bool IsValidTarget(this WeaponWrapper weapon, IShip target)
        {
            var capabilities = weapon.Weapon.Info.Capability;
            if (capabilities.HasCapability(WeaponCapability.CaptureDrone) &&
                target.Type.Class != Component.Unit.Classification.UnitClass.Drone)
                return false;

            return true;
        }

        public static void Sustain(this WeaponWrapper weapon)
        {
            switch (weapon.Controlling)
            {
                case WeaponWrapper.ControllingStrategy.Never:
                    break;
                case WeaponWrapper.ControllingStrategy.Always:
                    break;
                case WeaponWrapper.ControllingStrategy.WhenHitTarget:
                    break;
                case WeaponWrapper.ControllingStrategy.WhenCloseToTarget:
                    break;
                case WeaponWrapper.ControllingStrategy.WhenTargetApproaching:
                    break;
            }
        }

        public static void Aim(
            this WeaponWrapper data,
            IShip ship, 
            IShip target,
            ref AimingData aiming)
        {
            aiming.Angle = null;

            switch (data.Aiming)
            {
                case WeaponWrapper.AimingStrategy.AimDirectly:
                    aiming.InRange = TargetingHelpers.TryGetDirectTarget(data.Weapon, ship, target, out aiming.Position);
                    break;
                case WeaponWrapper.AimingStrategy.AimWithPrediction:
                    aiming.InRange = TargetingHelpers.TryGetProjectileTarget(data.Weapon, ship, target, out aiming.Position);
                    break;
                case WeaponWrapper.AimingStrategy.NoAiming:
                    aiming.InRange = IsInAttackRange(ship, target, data.Weapon);
                    return;
                default:
                    throw new System.InvalidOperationException();
            }

            if (aiming.InRange)
                aiming.Angle = data.Weapon.Platform.OptimalShipCourse(aiming.Position);
        }

        public static void Attack(
            this WeaponWrapper data, 
            IShip ship,
            IShip target,
            ref AimingData aiming)
        {
            const float aimingAngleErrorMargin = 5;
            if (data.Activation == WeaponWrapper.ActivationStrategy.Always) 
                aiming.ReadyToFire = true;
            else if (data.Activation == WeaponWrapper.ActivationStrategy.WhenInRange) 
                aiming.ReadyToFire = aiming.InRange;
            else if (!aiming.Angle.HasValue)
                aiming.ReadyToFire = false;
            else
            {
                var weapon = data.Weapon;
                var preciseAiming = data.Activation == WeaponWrapper.ActivationStrategy.WhenAimedPrecisely;
                var delta = Mathf.Abs(Mathf.DeltaAngle(aiming.Angle.Value, ship.Body.Rotation));
                var spread = CalculateSpread(weapon.Platform.Body.WorldPosition(),
                    aiming.Position, target.Body.Scale, aimingAngleErrorMargin + weapon.Info.Spread / 2);

                if (!preciseAiming)
                    if (weapon.Platform.ActiveTarget == target || weapon.Platform.ActiveTarget == null)
                        delta -= weapon.Platform.AutoAimingAngle;

                if (delta <= spread)
                    aiming.ReadyToFire = true;
            }
        }

        public static void Activate(this WeaponWrapper data, ShipControls controls)
        {
            var activate = data.Weapon.Info.WeaponType != WeaponType.RequiredCharging;
            controls.ActivateSystem(data.SystemId, activate);
        }

        private static bool IsInAttackRange(IShip ship, IShip target, IWeapon weapon)
        {
            var range = weapon.Info.Range;
            var distance = ship.Body.Position.Distance(target.Body.Position) - 0.5f*ship.Body.Scale + 0.5f*target.Body.Scale;
            return distance <= range;
        }

        private static float CalculateSpread(Vector2 turretPosition, Vector2 enemyPosition, float enemySize, float weaponSpread)
        {
            var distance = Vector2.Distance(enemyPosition, turretPosition);
            var delta = Mathf.Atan2(enemySize, distance) * Mathf.Rad2Deg;
            return (weaponSpread + delta) * 0.5f;
        }
    }
}
