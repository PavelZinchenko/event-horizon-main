using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class KeepDistanceNode : INode
	{
		private readonly float _minDistance;
		private readonly float _maxDistance;

		public KeepDistanceNode(float min, float max)
		{
			_minDistance = min < max ? min : max;
			_maxDistance = max > min ? max : min;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			if (ShipNavigationHandler.KeepDistance(context.Ship, context.TargetShip, _minDistance, _maxDistance, context.Controls))
				return NodeState.Success;

			return NodeState.Running;
		}
	}
}
