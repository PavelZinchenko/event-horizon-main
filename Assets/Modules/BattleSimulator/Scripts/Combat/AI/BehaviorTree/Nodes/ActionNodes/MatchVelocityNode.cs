using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MatchVelocityNode : INode
	{
		private readonly float _tolerance;

		public MatchVelocityNode(float tolerance)
		{
			_tolerance = tolerance;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			if (ShipNavigationHandler.TryReachVelocity(context.Ship, context.TargetShip.Body.Velocity, _tolerance, context.Controls))
				return NodeState.Success;

			return NodeState.Running;
		}
	}
}
