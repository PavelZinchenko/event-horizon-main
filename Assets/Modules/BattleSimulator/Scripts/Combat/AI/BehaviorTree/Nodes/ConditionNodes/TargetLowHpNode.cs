using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class TargetLowHpNode : INode
	{
		private readonly float _minValue;

		public TargetLowHpNode(float minValue)
		{
			_minValue = minValue;
		}

		public NodeState Evaluate(Context context)
		{
			var target = context.TargetShip;
			if (!target.IsActive())
				return NodeState.Failure;

			var hp = target.Stats.Armor.Percentage;
			return hp < _minValue ? NodeState.Success : NodeState.Failure;
		}
	}
}
