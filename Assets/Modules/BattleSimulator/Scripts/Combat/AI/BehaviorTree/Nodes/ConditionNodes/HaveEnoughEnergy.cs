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
			var energy = context.Ship.Stats.Energy.Percentage;
			return energy < _failIfLess ? NodeState.Failure : NodeState.Success;
		}
	}
}
