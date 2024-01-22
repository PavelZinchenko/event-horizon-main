namespace Combat.Ai.BehaviorTree.Nodes
{
	public class RechargeEnergy : INode
	{
		private readonly float _failIfLess;
		private readonly float _successIfMore;

		public RechargeEnergy(float failIfLess, float successIfMore)
		{
			_failIfLess = failIfLess;
			_successIfMore = successIfMore;
		}

		public NodeState Evaluate(Context context)
		{
			var energy = context.Ship.Stats.Energy.Percentage;
			var restoring = context.RestoringEnergy;

			if (restoring && energy >= _successIfMore)
			{
				context.RestoringEnergy = false;
				return NodeState.Success;
			}

			if (!restoring && energy < _failIfLess)
			{
				context.RestoringEnergy = true;
				return NodeState.Failure;
			}

			return restoring ? NodeState.Failure : NodeState.Success;
		}
	}
}
