using Combat.Unit;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class MothershipLowEnergyNode : INode
	{
		private readonly float _minValue;

		public MothershipLowEnergyNode(float minValue)
		{
			_minValue = minValue;
		}

		public NodeState Evaluate(Context context)
		{
			var mothership = context.Mothership;
			if (!mothership.IsActive())
				return NodeState.Failure;

			var energy = mothership.Stats.Energy.Percentage;
			return energy < _minValue ? NodeState.Success : NodeState.Failure;
		}
	}
}
