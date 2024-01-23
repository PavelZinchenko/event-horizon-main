namespace Combat.Ai.BehaviorTree.Nodes
{
	public class HaveEnoughEnergy : INode
	{
		private readonly float _failIfLess;

		public HaveEnoughEnergy(float failIfLess)
		{
			_failIfLess = failIfLess;
		}

		public NodeState Evaluate(Context context)
		{
			return context.EnergyLevelPercentage < _failIfLess ? NodeState.Failure : NodeState.Success;
		}
	}
}
