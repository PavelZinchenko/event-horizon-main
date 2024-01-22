namespace Combat.Ai.BehaviorTree.Nodes
{
	public class UpdateSecondaryTargets : INode
	{
		private readonly float _cooldown;

		public UpdateSecondaryTargets(float cooldown)
		{
			_cooldown = cooldown;
		}

		public NodeState Evaluate(Context context)
		{
			context.UpdateTargetList(_cooldown);
			return context.SecondaryTargets.Count > 0 ? NodeState.Success : NodeState.Failure;
		}
	}
}
