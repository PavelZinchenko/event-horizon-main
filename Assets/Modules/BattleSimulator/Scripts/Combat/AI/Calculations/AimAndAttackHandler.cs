using UnityEngine;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using Combat.Component.Unit.Classification;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.Calculations
{
	public static class AimAndAttackHandler
	{
		public static int AttackWithAllWeapons(IShip ship, IShip enemy, bool directAttacksOnly, ShipWeaponList weaponList, ShipControls controls)
		{
			var targetAngle = 0f;
			var targetDeltaAngle = 10000f;
			var setCourse = false;
			var count = 0;

			for (int i = 0; i < weaponList.Count; ++i)
			{
				var id = weaponList.Ids[i];
				var weapon = weaponList.GetWeaponById(i);
				if (!weapon.CanBeActivated) continue;
				//if (ship.Type.Class == UnitClass.Drone && weapon.Info.WeaponType == WeaponType.RequiredCharging) continue;
				if (weapon.Info.BulletEffectType == BulletEffectType.ForDronesOnly && enemy.Type.Class != UnitClass.Drone) continue;

				Vector2 target;
				if (!AttackHelpers.TryGetTarget(weapon, ship, enemy, weapon.Info.BulletType == BulletType.Projectile && directAttacksOnly ? BulletType.Direct : weapon.Info.BulletType, out target))
					continue;

				var shotImmediately = weapon.Info.BulletType == BulletType.Homing || weapon.Info.BulletType == BulletType.AreaOfEffect;
				var shouldTrackTarget = weapon.Info.BulletType != BulletType.AreaOfEffect;

				var course = Helpers.TargetCourse(ship, target, weapon.Platform);
				var spread = weapon.Info.Spread / 2 + Mathf.Asin(0.3f * enemy.Body.Scale / Vector2.Distance(enemy.Body.Position, ship.Body.Position)) * Mathf.Rad2Deg;
				var delta = Mathf.Abs(Mathf.DeltaAngle(course, ship.Body.Rotation)) - weapon.Platform.AutoAimingAngle;

				if (delta < spread + 1 || shotImmediately)
				{
					count++;
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

			return count;
		}
	}
}
