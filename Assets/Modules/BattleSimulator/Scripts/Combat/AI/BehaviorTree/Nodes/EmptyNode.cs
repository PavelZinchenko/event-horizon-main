namespace Combat.Ai.BehaviorTree.Nodes
{
	public static class EmptyNode
	{
		public static readonly INode Success = new SuccessNode();
		public static readonly INode Failure = new FailureNode();
		public static readonly INode Running = new RunningNode();

		public static INode FromState(NodeState state) 
		{
			switch (state)
			{
				case NodeState.Running:
					return Running;
				case NodeState.Success:
					return Success;
				case NodeState.Failure:
				default:
					return Failure;
			}
		}

		private class SuccessNode : INode
		{
			public NodeState Evaluate(Context context) => NodeState.Success;
		}

		private class FailureNode : INode
		{
			public NodeState Evaluate(Context context) => NodeState.Failure;
		}

		private class RunningNode : INode
		{
			public NodeState Evaluate(Context context) => NodeState.Running;
		}
	}

	public static class NodeExtensions
	{
		public static bool IsEmpty(this INode node) => node == EmptyNode.Success || node == EmptyNode.Failure || node == EmptyNode.Running;
	}
}
