using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MothershipLowHpNode : INode
	{
		private readonly float _minValue;

		public MothershipLowHpNode(float minValue)
		{
			_minValue = minValue;
		}

		public NodeState Evaluate(Context context)
		{
			var mothership = context.Mothership;
			if (!mothership.IsActive())
				return NodeState.Failure;

			var hp = mothership.Stats.Armor.Percentage;
			return hp < _minValue ? NodeState.Success : NodeState.Failure;
		}
	}
}
