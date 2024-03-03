using UnityEngine;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using Combat.Component.Unit.Classification;
using Combat.Ai.BehaviorTree.Utils;
using Combat.Component.Platform;
using Combat.Component.Body;
using Combat.Unit;
using GameDatabase.Enums;

namespace Combat.Ai.Calculations
{
	public static class AimAndAttackHandler
	{
		[System.Flags]
		public enum State
		{
			Failed = 0,
			Aiming = 1,
			Attacking = 2,
		}

        public static bool AttackTurretTargets(IShip ship, ShipWeaponList weaponList, ShipControls controls)
        {
            var activated = 0;
            for (int i = 0; i < weaponList.Count; ++i)
            {
                var id = weaponList.Ids[i];
                var weapon = weaponList.GetWeaponById(id);
                if (!weapon.CanBeActivated) continue;
                var platform = weapon.Platform;

                if (!platform.ActiveTarget.IsActive() && weapon.Info.RequiresAiming)
                    platform.Aim(weapon.Info.BulletSpeed, weapon.Info.Range, weapon.Info.RelativeVelocityEffect);
                if (!platform.ActiveTarget.IsActive())
                    continue;

                if (CanAttack(ship, platform.ActiveTarget, weapon, false))
                {
                    activated++;
                    controls.ActivateSystem(id, weapon.Info.WeaponType != WeaponType.RequiredCharging);
                }
            }

            return activated > 0;
        }

        public static State AttackWhileStandingStill(IShip ship, IShip enemy, bool simpleAiming, ShipWeaponList weaponList, ShipControls controls)
        {
            if (enemy == null || enemy.State != Unit.UnitState.Active)
                return State.Failed;

            var activated = 0;
            for (int i = 0; i < weaponList.Count; ++i)
            {
                var id = weaponList.Ids[i];
                var weapon = weaponList.GetWeaponById(id);
                if (!weapon.CanBeActivated) continue;

                if (CanAttack(ship, enemy, weapon, simpleAiming))
                {
                    activated++;
                    controls.ActivateSystem(id, weapon.Info.WeaponType != WeaponType.RequiredCharging);
                }
            }

            return activated > 0 ? State.Attacking : State.Failed;
        }

        public static State AttackWithAllWeapons(IShip ship, IShip enemy, bool directAttacksOnly, ShipWeaponList weaponList, ShipControls controls)
		{
            if (enemy == null || enemy.State != Unit.UnitState.Active)
                return State.Failed;

            var targetAngle = 0f;
			var targetDeltaAngle = 10000f;
			var setCourse = false;

			var aiming = 0;
			var activated = 0;

			for (int i = 0; i < weaponList.Count; ++i)
			{
				var id = weaponList.Ids[i];
				var weapon = weaponList.GetWeaponById(id);
				if (!weapon.CanBeActivated) continue;

				var caps = weapon.Info.Capability;
				if (caps.HasCapability(WeaponCapability.CaptureDrone) && enemy.Type.Class != UnitClass.Drone) continue;

                var bulletType = weapon.Info.BulletType == AiBulletBehavior.Projectile && directAttacksOnly ? AiBulletBehavior.Beam : weapon.Info.BulletType;
                if (!AttackHelpers.TryGetTarget(weapon, ship, enemy, bulletType, out var target))
					continue;

				aiming++;
				var shotImmediately = weapon.Info.BulletType == AiBulletBehavior.Homing || weapon.Info.BulletType == AiBulletBehavior.AreaOfEffect;
				var shouldTrackTarget = weapon.Info.RequiresAiming;

				var course = weapon.Platform.OptimalShipCourse(target);
                var spread = CalculateSpread(weapon.Platform.Body.WorldPosition(), target, enemy.Body.Scale, _aimingAngleErrorMargin + weapon.Info.Spread/2);
				var delta = Mathf.Abs(Mathf.DeltaAngle(course, ship.Body.Rotation));

				if (weapon.Platform.ActiveTarget == enemy || weapon.Platform.ActiveTarget == null)
					delta -= weapon.Platform.AutoAimingAngle;

				if (delta <= spread || shotImmediately)
				{
					activated++;
					controls.ActivateSystem(id, weapon.Info.WeaponType != WeaponType.RequiredCharging);
				}

				if (delta < 0 || delta > targetDeltaAngle)
					continue;

				targetAngle = course;
				targetDeltaAngle = delta;
				setCourse = shouldTrackTarget;
			}

			if (setCourse)
				controls.Course = targetAngle;

			return activated > 0 ? State.Attacking : aiming > 0 ? State.Aiming : State.Failed;
		}

        private static bool CanAttack(IShip ship, IShip enemy, IWeapon weapon, bool simpleAiming)
        {
            var caps = weapon.Info.Capability;
            if (caps.HasCapability(WeaponCapability.CaptureDrone) && enemy.Type.Class != UnitClass.Drone) return false;

            var bulletType = weapon.Info.BulletType;
            if (!AttackHelpers.TryGetTarget(weapon, ship, enemy, simpleAiming ? AiBulletBehavior.Beam : bulletType, out var target))
                return false;

            if (bulletType == AiBulletBehavior.Homing || bulletType == AiBulletBehavior.AreaOfEffect)
                return true;

            var angle = Mathf.Abs(weapon.Platform.AimingDeltaAngle(target));
            var spread = _aimingAngleErrorMargin + weapon.Info.Spread / 2;
            if (angle > spread)
                spread = CalculateSpread(weapon.Platform.Body.WorldPosition(), enemy.Body.Position, enemy.Body.Scale, spread);

            return angle <= spread;
        }

        private static float CalculateSpread(Vector2 turretPosition, Vector2 enemyPosition, float enemySize, float weaponSpread)
        {
            var distance = Vector2.Distance(enemyPosition, turretPosition);
            weaponSpread += Mathf.Atan2(enemySize, distance) * Mathf.Rad2Deg;
            return weaponSpread;
        }

        private const float _aimingAngleErrorMargin = 5;
    }
}
