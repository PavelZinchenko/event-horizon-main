using UnityEngine;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class AvoidThreatsNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			var ship = context.Ship;
			var target = Vector2.zero;
			var maxVelocity = context.Ship.Engine.MaxVelocity * 3;
			int count = 0;

			for (int i = 0; i < context.Threats.Count; ++i)
			{
				var threat = context.Threats[i];
				if (threat.Body.Velocity.magnitude > maxVelocity)
					continue;

				var dir = ship.Body.Position.Direction(threat.Body.Position).normalized;
				target += dir;
				count++;
			}

			if (count == 0)
				return NodeState.Success;

			var delta = 20 * Mathf.Clamp01(Vector2.Dot(ship.Body.Velocity, RotationHelpers.Direction(ship.Body.Rotation)) / ship.Engine.MaxVelocity);
			if (Mathf.DeltaAngle(RotationHelpers.Angle(target), ship.Body.Rotation) < 0)
				delta = -delta;

			context.Controls.Thrust = 1;
			context.Controls.Course = ship.Body.Rotation + delta;

			return NodeState.Running;
		}
	}
}
