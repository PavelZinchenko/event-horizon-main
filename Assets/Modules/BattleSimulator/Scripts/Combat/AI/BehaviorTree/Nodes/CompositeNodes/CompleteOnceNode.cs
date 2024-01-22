namespace Combat.Ai.BehaviorTree.Nodes
{
	public class CompleteOnceNode : INode
	{
		private readonly INode _node;
		private bool _completed;
		private NodeState _result;

		public static INode Create(INode node, bool result)
		{
			if (node == EmptyNode.Success || node == EmptyNode.Failure)
				return result ? EmptyNode.Success : EmptyNode.Failure;

			return new CompleteOnceNode(node, result);
		}

		public NodeState Evaluate(Context context)
		{
			if (_completed) 
				return _result;

			var result = _node.Evaluate(context);
			if (result == NodeState.Success)
				_completed = true;

			return result;
		}

		private CompleteOnceNode(INode node, bool result)
		{
			_node = node;
			_result = result ? NodeState.Success : NodeState.Failure;
		}
	}
}
