namespace Combat.Ai.BehaviorTree.Nodes
{
	public enum NodeState
	{
		Running,
		Success,
		Failure,
	}

	public interface INode
	{
		NodeState Evaluate(Context context);
	}
}
