namespace Combat.Ai.BehaviorTree.Nodes
{
	public class IsPlayerControlled : INode
	{
		public NodeState Evaluate(Context context)
		{
			return context.Ship.Controls.DataChanged ? NodeState.Success : NodeState.Failure;
		}
	}
}
