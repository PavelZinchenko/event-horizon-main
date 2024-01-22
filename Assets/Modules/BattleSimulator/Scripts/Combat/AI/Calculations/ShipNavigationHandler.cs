using UnityEngine;
using Combat.Component.Ship;

namespace Combat.Ai.Calculations
{
	public static class ShipNavigationHandler
	{
		public static bool KeepDistance(IShip ship, IShip target, float min, float max, int spin, ShipControls controls)
		{
			var minDistance = min + ship.Body.Scale/2 + target.Body.Scale/2;
			var maxDistance = minDistance - min + max;

			var direction = ship.Body.Position.Direction(target.Body.Position);
			var alpha = RotationHelpers.Angle(direction);

			var distance = direction.magnitude;
			if (distance >= minDistance && distance <= maxDistance)
				return true;

			if (controls.MovementLocked)
				return false;

			var delta = 0f;
			if (distance > maxDistance)
			{
				var sina = minDistance / distance;
				delta = Mathf.Asin(sina) * Mathf.Rad2Deg;
			}
			else
			{
				delta = 90f + 45f * (minDistance - distance) / minDistance;
			}

			if (spin < 0)
				alpha += delta;
			else if (spin > 0)
				alpha -= delta;
			else if (Mathf.DeltaAngle(ship.Body.Rotation, alpha) < 0)
				alpha += delta;
			else
				alpha -= delta;

			if (Vector2.Dot(ship.Body.Velocity, RotationHelpers.Direction(alpha)) < ship.Engine.MaxVelocity * 0.95f)
			{
				controls.Course = alpha;
				if (Mathf.Abs(Mathf.DeltaAngle(alpha, ship.Body.Rotation)) < 30)
					controls.Thrust = 1;
			}

			return false;
		}

		public static float ShortenDistance(IShip ship, IShip target, float distance, ShipControls controls)
		{
			var currentDistance = Helpers.Distance(ship, target);
			if (currentDistance <= distance) return currentDistance;

			if (controls.MovementLocked)
				return currentDistance;

			var direction = ship.Body.Position.Direction(target.Body.Position).normalized;
			var course = RotationHelpers.Angle(direction);
			if (Vector2.Dot(ship.Body.Velocity, direction) < ship.Engine.MaxVelocity * 0.95f)
			{
				controls.Course = course;
				if (Mathf.Abs(Mathf.DeltaAngle(course, ship.Body.Rotation)) < 30)
					controls.Thrust = 1;
			}

			return currentDistance;
		}

		public enum Status
		{
			NoTarget,
			Following,
			Chasing,
			FullThrottle,
			AboutToHit,
		}

		public static Status ChaseAndRam(IShip ship, IShip enemy, ShipControls controls)
		{
			if (AttackHelpers.CantDetectTarget(ship, enemy)) return Status.NoTarget;
			if (controls.MovementLocked) return Status.Chasing;

			var enemyVelocity = enemy.Body.Velocity;
			var enemyPosition = enemy.Body.Position;
			var shipPosition = ship.Body.Position;
			var maxVelocity = Mathf.Max(ship.Body.Velocity.magnitude, ship.Engine.MaxVelocity);

			var canHit = Geometry.GetTargetPosition(
				enemyPosition,
				enemyVelocity,
				shipPosition,
				maxVelocity,
				out var target,
				out var timeInterval);

			if (!canHit)
				target = enemyPosition + enemyVelocity;

			var status = canHit ? Status.Chasing : Status.Following;

			var direction = ship.Body.Position.Direction(target);
			var course = RotationHelpers.Angle(direction);
			controls.Course = course;

			if (Mathf.Abs(Mathf.DeltaAngle(ship.Body.Rotation, course)) < 30)
			{
				status = Status.FullThrottle;
				controls.Thrust = 1.0f;
			}

			if (timeInterval < 1f)
				status = Status.AboutToHit;

			return status;
		}
	}
}
