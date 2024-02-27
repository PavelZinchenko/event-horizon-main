using System.Collections.Generic;
using System.Linq;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using Combat.Component.Unit.Classification;
using GameDatabase.Enums;
using UnityEngine;
using BulletEffectType = Combat.Component.Systems.Weapons.BulletEffectType;

namespace Combat.Ai
{
    public class TrackTargetAction : IAction
	{
		public TrackTargetAction(bool trackAlways = false)
		{
			_trackAlways = trackAlways;
		}
		
		public void Perform(Context context, ShipControls controls)
		{
			var ship = context.Ship;
			var enemy = context.Enemy;
			
			foreach (var weapon in ship.Systems.All.OfType<IWeapon>())
			{
                if (!(_trackAlways || weapon.Info.BulletType == AiBulletBehavior.Beam && weapon.Active))
					continue;

                Vector2 target;
				if (!AttackHelpers.TryGetTarget(weapon, ship, enemy, out target))
					continue;

				controls.Course = Helpers.TargetCourse(ship, target, weapon.Platform);
				break;
			}
		}

		private readonly bool _trackAlways;
	}

	public class CommonWeaponsAttackAction : IAction
	{
		public CommonWeaponsAttackAction(IShip ship, bool directOnly, bool secondaryTargets)
		{
			for (var i = 0; i < ship.Systems.All.Count; ++i)
			{
			    var weapon = ship.Systems.All.Weapon(i);
			    if (weapon != null)
			    {
                    if (weapon.Info.BulletEffectType == BulletEffectType.Common || weapon.Info.BulletEffectType == BulletEffectType.DamageOverTime || weapon.Info.BulletEffectType == BulletEffectType.ForDronesOnly)
                        _weapons.Add(i);
                }
			}

		    _secondaryTargets = secondaryTargets;
			_directOnly = directOnly;
		}

	    public CommonWeaponsAttackAction(int weaponId, bool directOnly, bool secondaryTargets)
	    {
	        _weapons.Add(weaponId);

	        _secondaryTargets = secondaryTargets;
	        _directOnly = directOnly;
	    }

	    public void Perform(Context context, ShipControls controls)
	    {
	        var ship = context.Ship;
	        var enemy = context.Enemy;

	        if (_secondaryTargets && context.Targets != null)
	        {
	            var targets = context.Targets.Items;
	            for (var i = 0; i < targets.Count; i++)
	                Perform(ship, targets[i], controls);
	        }
	        else
	        {
                Perform(ship, enemy, controls);
            }
        }

        public void Perform(IShip ship, IShip enemy, ShipControls controls)
        {
            var targetAngle = 0f;
			var targetDeltaAngle = 10000f;
			var setCourse = false;

			foreach (var id in _weapons)
			{
				var weapon = ship.Systems.All.Weapon(id);
				if (!weapon.CanBeActivated) continue;
			    //if (ship.Type.Class == UnitClass.Drone && weapon.Info.WeaponType == WeaponType.RequiredCharging) continue;
                if (weapon.Info.BulletEffectType == BulletEffectType.ForDronesOnly && enemy.Type.Class != UnitClass.Drone) continue;

				Vector2 target;
				if (!AttackHelpers.TryGetTarget(weapon, ship, enemy, weapon.Info.BulletType == AiBulletBehavior.Projectile && _directOnly ? AiBulletBehavior.Beam : weapon.Info.BulletType, out target))
					continue;

			    var shotImmediately = weapon.Info.BulletType == AiBulletBehavior.Homing || weapon.Info.BulletType == AiBulletBehavior.AreaOfEffect;
			    var shouldTrackTarget = weapon.Info.BulletType != AiBulletBehavior.AreaOfEffect;
                
                var course = Helpers.TargetCourse(ship, target, weapon.Platform);
			    var spread = weapon.Info.Spread/2 + Mathf.Asin(0.3f * enemy.Body.Scale / Vector2.Distance(enemy.Body.Position, ship.Body.Position))*Mathf.Rad2Deg;
				var delta = Mathf.Abs(Mathf.DeltaAngle(course, ship.Body.Rotation)) - weapon.Platform.AutoAimingAngle;

                if (delta < spread + 1 || shotImmediately)
                {
                    if (weapon.Info.WeaponType == WeaponType.RequiredCharging)
                    {
                        controls.ActivateSystem(id, false);
                        ship.Systems.All[id].Active = false;
                    }
                    else
                    {
                        controls.ActivateSystem(id, true);
                    }
                }
				if (delta < 0 || delta > targetDeltaAngle)
					continue;

				targetAngle = course;
				targetDeltaAngle = delta;
				setCourse = shouldTrackTarget;
			}

			if (setCourse)
				controls.Course = targetAngle;
		}

	    private readonly bool _secondaryTargets;
		private readonly bool _directOnly;
		private readonly List<int> _weapons = new List<int>();
	}

	public class DirectAttackAction : IAction
	{
		public DirectAttackAction(int weaponId)
		{
			_weaponId = weaponId;
		}

		public virtual void Perform(Context context, ShipControls controls)
		{
			if (AttackHelpers.CantDetectTarget(context.Ship, context.Enemy))
				return;

            var ship = context.Ship;
			var weapon = ship.Systems.All.Weapon(_weaponId);
			if (weapon.Cooldown > 0) return;

			var enemySize = context.Enemy.Body.Scale;
			var enemyPosition = context.Enemy.Body.Position;

			var distance = Vector2.Distance(ship.Body.Position, enemyPosition) - (ship.Body.Scale + enemySize)*0.4f;

			if (weapon.Info.Range < distance) return;

			var course = Helpers.TargetCourse(ship, enemyPosition, weapon.Platform);
			var spread = weapon.Info.Spread/2 + weapon.Platform.AutoAimingAngle + Mathf.Asin(0.4f * enemySize / Vector2.Distance(enemyPosition, ship.Body.Position))*Mathf.Rad2Deg;
			var fire = Mathf.Abs(Mathf.DeltaAngle(course, ship.Body.Rotation)) < spread;
			
			if (fire)
				controls.ActivateSystem(_weaponId, true);

			controls.Course = course;
		}

		private readonly int _weaponId;
	}

	public class ProjectileAttackAction : IAction
	{
		public ProjectileAttackAction(int weaponId)
		{
			_weaponId = weaponId;
		}
		
		public void Perform(Context context, ShipControls controls)
		{
            var ship = context.Ship;
            var enemy = context.Enemy;

		    var weapon = ship.Systems.All.Weapon(_weaponId);
            if (weapon.Cooldown > 0) return;

            var position = weapon.Platform.Body.WorldPosition();
            var velocity = enemy.Body.Velocity - ship.Body.Velocity * weapon.Info.RelativeVelocityEffect;

            Vector2 target;
            float timeInterval;
            if (!Geometry.GetTargetPosition(
                enemy.Body.Position,
                velocity,
                position,
                weapon.Info.BulletSpeed,
                out target,
                out timeInterval))
            {
                return;
            }

            if (weapon.Info.Range + 0.1f * (ship.Body.Scale + enemy.Body.Scale) < timeInterval * weapon.Info.BulletSpeed) return;

            var course = Helpers.TargetCourse(ship, target, weapon.Platform);
            var spread = weapon.Info.Spread / 4 + weapon.Platform.AutoAimingAngle + Mathf.Asin(0.3f * enemy.Body.Scale / Vector2.Distance(ship.Body.Position, target)) * Mathf.Rad2Deg;
            var fire = Mathf.Abs(Mathf.DeltaAngle(course, ship.Body.Rotation)) < spread;

            if (fire)
                controls.ActivateSystem(_weaponId, true);
            else
                controls.Course = course;
        }

		private readonly int _weaponId;
	}

	public class HomingAttackAction : IAction
	{
		public HomingAttackAction(int weaponId)
		{
			_weaponId = weaponId;
		}
		
		public void Perform(Context context, ShipControls controls)
		{
			if (AttackHelpers.CantDetectTarget(context.Ship, context.Enemy))
				return;

			var ship = context.Ship;
			var enemy = context.Enemy;

			var weapon = ship.Systems.All.Weapon(_weaponId);
			if (weapon.Cooldown > 0) return;

			var distance1 = Vector2.Distance(ship.Body.Position, enemy.Body.Position);
			var distance2 = Vector2.Distance(ship.Body.Position + ship.Body.Velocity, enemy.Body.Position + enemy.Body.Velocity);

			if (weapon.Info.Range < distance1 && weapon.Info.Range < distance2) return;
			if (distance2 - distance1 > weapon.Info.BulletSpeed) return;
			
			controls.Course = Helpers.TargetCourse(ship, enemy.Body.Position, weapon.Platform);
			controls.ActivateSystem(_weaponId, true);
		}
		
		private readonly int _weaponId;
	}

	public class ExplosionAttackAction : IAction
	{
		public ExplosionAttackAction(int weaponId)
		{
			_weaponId = weaponId;
		}
		
		public void Perform(Context context, ShipControls controls)
		{
			var ship = context.Ship;
			var enemy = context.Enemy;

			var weapon = ship.Systems.All.Weapon(_weaponId);
			if (weapon.Cooldown > 0) return;

			var distance = Vector2.Distance(ship.Body.Position, enemy.Body.Position);
			if (weapon.Info.Range > distance)
				controls.ActivateSystem(_weaponId, true);
		}
		
		private readonly int _weaponId;
	}

    public static class AttackHelpers
    {
        public static bool CantDetectTarget(IShip ship, IShip enemy)
        {
            if (enemy == null)
                return true;
			if (ship.Type.Side.IsAlly(enemy.Type.Side))
				return false;
            if (enemy.Features.TargetPriority != TargetPriority.None)
                return false;

            var distance = Helpers.Distance(ship, enemy);
            return distance > 5 + enemy.Body.Scale;
        }

        public static bool TryGetTarget(IWeapon weapon, IShip ship, IShip enemy, AiBulletBehavior type, out Vector2 target)
        {
            if (type == AiBulletBehavior.Projectile)
                return TryGetProjectileTarget(weapon, ship, enemy, out target);
            else
                return TryGetDirectTarget(weapon, ship, enemy, out target);
        }

        public static bool TryGetTarget(IWeapon weapon, IShip ship, IShip enemy, out Vector2 target)
        {
            if (weapon.Info.BulletType == AiBulletBehavior.Projectile)
                return TryGetProjectileTarget(weapon, ship, enemy, out target);
            else
                return TryGetDirectTarget(weapon, ship, enemy, out target);
        }

        public static bool TryGetProjectileTarget(IWeapon weapon, IShip ship, IShip enemy, out Vector2 target)
        {
            if (CantDetectTarget(ship, enemy))
            {
                target = Vector2.zero;
                return false;
            }

            var position = weapon.Platform.Body.WorldPosition();
            var velocity = enemy.Body.Velocity - ship.Body.Velocity * weapon.Info.RelativeVelocityEffect;

            float timeInterval;
            if (!Geometry.GetTargetPosition(
                enemy.Body.Position,
                velocity,
                position,
                weapon.Info.BulletSpeed,
                out target,
                out timeInterval))
            {
                return false;
            }

            return weapon.Info.Range + 0.1f * (ship.Body.Scale + enemy.Body.Scale) >= timeInterval * weapon.Info.BulletSpeed;
        }

        public static bool TryGetDirectTarget(IWeapon weapon, IShip ship, IShip enemy, out Vector2 target)
        {
            if (CantDetectTarget(ship, enemy))
            {
                target = Vector2.zero;
                return false;
            }

            target = enemy.Body.Position;
            var distance = Vector2.Distance(weapon.Platform.Body.WorldPosition(), target) - enemy.Body.Scale * 0.4f;
            return weapon.Info.Range >= distance;
        }
    }
}
