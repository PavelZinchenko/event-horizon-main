using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class DistanceToTargetNode : INode
	{
		private readonly float _maxRange;

		public DistanceToTargetNode(float maxRange)
		{
			_maxRange = maxRange;
		}

		public NodeState Evaluate(Context context)
		{
			var target = context.TargetShip;
			if (!target.IsActive()) 
				return NodeState.Success;

			var currentDistance = Helpers.Distance(context.Ship, target);
			return currentDistance <= _maxRange ? NodeState.Success : NodeState.Failure;
		}
	}
}
