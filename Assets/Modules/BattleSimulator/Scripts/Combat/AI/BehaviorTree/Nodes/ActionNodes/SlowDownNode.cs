using UnityEngine;
using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SlowDownNode : INode
	{
		private readonly float _tolerance;

		public SlowDownNode(float tolerance)
		{
			_tolerance = tolerance;
		}

		public NodeState Evaluate(Context context)
		{
			if (ShipNavigationHandler.TryReachVelocity(context.Ship, Vector2.zero, _tolerance, context.Controls))
				return NodeState.Success;

			return NodeState.Failure;
		}
	}
}
