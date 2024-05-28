using UnityEngine;
using Combat.Component.Ship;
using Combat.Ai.BehaviorTree.Utils;
using Combat.Unit;
using Combat.Ai.BehaviorTree;

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
            for (int i = 0; i < weaponList.List.Count; ++i)
            {
                var data = weaponList.List[i];
                var weapon = data.Weapon;
                var platform = weapon.Platform;

                if (!platform.ActiveTarget.IsActive() && data.Aiming != WeaponWrapper.AimingStrategy.NoAiming)
                    platform.Aim(weapon.Info.BulletSpeed, weapon.Info.Range, weapon.Info.RelativeVelocityEffect);
                if (!platform.ActiveTarget.IsActive())
                    continue;

                var aimingData = AimAndAttack(ship, platform.ActiveTarget, data);
                if (aimingData.ReadyToFire)
                {
                    if (aimingData.InRange) activated++;
                    data.Activate(controls);
                }
            }

            return activated > 0;
        }

        public static State AttackWhileStandingStill(IShip ship, IShip enemy, ShipWeaponList weaponList, ShipControls controls)
        {
            if (enemy == null || enemy.State != Unit.UnitState.Active)
                return State.Failed;

            var activated = 0;
            for (int i = 0; i < weaponList.List.Count; ++i)
            {
                var data = weaponList.List[i];
                var weapon = data.Weapon;

                var aimingData = AimAndAttack(ship, enemy, data);
                if (aimingData.ReadyToFire)
                {
                    if (aimingData.InRange) activated++;
                    data.Activate(controls);
                }
            }

            return activated > 0 ? State.Attacking : State.Failed;
        }

        public static State AttackWithAllWeapons(IShip ship, IShip enemy, ShipWeaponList weaponList, ShipControls controls)
		{
            if (enemy == null || enemy.State != Unit.UnitState.Active)
                return State.Failed;

            var targetAngle = 0f;
			var targetDeltaAngle = float.MaxValue;
			var setCourse = false;

			var aiming = 0;
			var activated = 0;

            var shipRotation = ship.Body.Rotation;

			for (int i = 0; i < weaponList.List.Count; ++i)
			{
                var data = weaponList.List[i];
                if (!data.IsReadyToFire()) continue;
				if (!data.IsValidTarget(enemy)) continue;

                var aimingData = new AimingData();
                data.Aim(ship, enemy, ref aimingData);
                data.Attack(ship, enemy, ref aimingData);

                if (aimingData.ReadyToFire)
                {
                    if (aimingData.InRange) activated++;
                    data.Activate(controls);
                }

                if (aimingData.Angle.HasValue)
                {
                    aiming++;
                    var delta = Mathf.Abs(Mathf.DeltaAngle(aimingData.Angle.Value, shipRotation));
                    if (delta <= targetDeltaAngle)
                    {
                        targetAngle = aimingData.Angle.Value;
                        targetDeltaAngle = delta;
                        setCourse = true;
                    }
                }
			}

			if (setCourse)
				controls.Course = targetAngle;

			return activated > 0 ? State.Attacking : aiming > 0 ? State.Aiming : State.Failed;
		}

        private static AimingData AimAndAttack(IShip ship, IShip enemy, WeaponWrapper data)
        {
            var aimingData = new AimingData();
            if (!data.IsReadyToFire()) return aimingData;
            if (!data.IsValidTarget(enemy)) return aimingData;

            data.Aim(ship, enemy, ref aimingData);
            data.Attack(ship, enemy, ref aimingData);
            return aimingData;
        }
    }
}
