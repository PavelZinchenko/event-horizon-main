namespace Combat.Ai.BehaviorTree.Nodes
{
	public static class EmptyNode
	{
		public static readonly INode Success = new SuccessNode();
		public static readonly INode Failure = new FailureNode();

		private class SuccessNode : INode
		{
			public NodeState Evaluate(Context context) => NodeState.Success;
		}

		private class FailureNode : INode
		{
			public NodeState Evaluate(Context context) => NodeState.Failure;
		}
	}
}
