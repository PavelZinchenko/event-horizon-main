namespace Combat.Ai.BehaviorTree.Nodes
{
	public class HasSecondaryTargetsNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			return context.SecondaryTargets.Count > 0 ? NodeState.Success : NodeState.Failure;
		}
	}
}
