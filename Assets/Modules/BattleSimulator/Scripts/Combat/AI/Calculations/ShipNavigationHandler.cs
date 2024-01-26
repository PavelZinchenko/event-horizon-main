using UnityEngine;
using Combat.Component.Ship;

namespace Combat.Ai.Calculations
{
	public static class ShipNavigationHandler
	{
		public static bool FlyAround(IShip ship, IShip target, float min, float max, int spin, ShipControls controls)
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

			float delta;
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

			//if (Vector2.Dot(ship.Body.Velocity, RotationHelpers.Direction(alpha)) < ship.Engine.MaxVelocity * 0.95f)
			{
				controls.Course = alpha;
				if (Mathf.Abs(Mathf.DeltaAngle(alpha, ship.Body.Rotation)) < 30)
					controls.Thrust = 1;
			}

			return false;
		}

		public static bool KeepDistance(IShip ship, IShip target, float min, float max, ShipControls controls)
		{
			var maxVelocity = ship.Engine.MaxVelocity;

			var minDistance = min + ship.Body.Scale / 2 + target.Body.Scale / 2;
			var maxDistance = minDistance - min + max;

			var direction = ship.Body.Position.Direction(target.Body.Position);
			var distance = direction.magnitude;
			if (distance >= minDistance && distance <= maxDistance)
				return true;

			if (controls.MovementLocked)
				return false;

			Vector2 requiredVelocity;
			if (distance < minDistance)
			{
				var excess = (1 + minDistance - distance) / (1 + minDistance);
				requiredVelocity = -direction.normalized * excess * maxVelocity;
			}
			else
			{
				var excess = (1 + distance - maxDistance) / (1 + maxDistance);
				requiredVelocity = direction.normalized * excess * maxVelocity;
			}

			var newVelocity = requiredVelocity - ship.Body.Velocity;
			var course = RotationHelpers.Angle(newVelocity);

			controls.Course = course;
			if (Mathf.Abs(Mathf.DeltaAngle(course, ship.Body.Rotation)) < 10)
				controls.Thrust = newVelocity.magnitude / maxVelocity;

			return false;
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

			var canHit = TryInterceptTarget(ship, enemy, out var target, out var timeToHit);
			var status = canHit ? Status.Chasing : Status.Following;

			var direction = ship.Body.Position.Direction(target);
			var course = RotationHelpers.Angle(direction);
			controls.Course = course;

			if (Mathf.Abs(Mathf.DeltaAngle(ship.Body.Rotation, course)) < 30)
			{
				status = Status.FullThrottle;
				controls.Thrust = 1.0f;
			}

			if (timeToHit < 1f)
				status = Status.AboutToHit;

			return status;
		}

		public static bool TryReachVelocity(IShip ship, Vector2 desiredVelocity, float speedThreshold, ShipControls controls)
		{
			const float maxDeltaAngle = 60;

			var velocity = ship.Body.Velocity;
			var relativeVelocity = velocity - desiredVelocity;

			var speed = relativeVelocity.magnitude;
			if (speed <= speedThreshold)
				return true;

			if (controls.MovementLocked)
				return false;

			var course = RotationHelpers.Angle(-relativeVelocity);
			controls.Course = course;

			var deltaAngle = Mathf.Abs(Mathf.DeltaAngle(ship.Body.Rotation, course));
			if (deltaAngle < maxDeltaAngle)
				controls.Thrust = speed / ship.Engine.MaxVelocity;

			return false;
		}

		private static bool TryInterceptTarget(IShip ship, IShip enemy, out Vector2 target, out float timeToHit)
		{
			var enemyVelocity = enemy.Body.Velocity;
			var enemyPosition = enemy.Body.Position;
			var shipPosition = ship.Body.Position;
			var maxVelocity = Mathf.Max(ship.Body.Velocity.magnitude, ship.Engine.MaxVelocity);

			var canHit = Geometry.GetTargetPosition(
				enemyPosition,
				enemyVelocity,
				shipPosition,
				maxVelocity,
				out target,
				out timeToHit);

			if (!canHit)
			{
				target = enemyPosition + enemyVelocity;
				timeToHit = 60f;
			}

			return canHit;
		}
	}
}
