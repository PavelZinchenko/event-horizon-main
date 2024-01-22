using UnityEngine;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class StopShipNode : INode
	{
		private const float _threshold = 0.05f;
		private const float _courseThreshold = 10;

		public NodeState Evaluate(Context context)
		{
			var ship = context.Ship;
			var velocity = ship.Body.Velocity;
			var speed = ship.Body.Velocity.sqrMagnitude;
			if (speed < _threshold)
				return NodeState.Success;

			var course = RotationHelpers.Angle(-velocity);
			context.Controls.Course = course;

			if (Mathf.Abs(Mathf.DeltaAngle(ship.Body.Rotation, course)) < _courseThreshold)
				context.Controls.Thrust = speed/ship.Engine.MaxVelocity;

			return NodeState.Running;
		}
	}
}
