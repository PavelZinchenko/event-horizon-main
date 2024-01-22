namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ConstantResultNode : INode
	{
		private readonly INode _node;
		private bool _result;

		public static INode Create(INode node, bool result)
		{
			if (node == EmptyNode.Success || node == EmptyNode.Failure) 
				return result ? EmptyNode.Success : EmptyNode.Failure;

			return new ConstantResultNode(node, result);
		}

		public NodeState Evaluate(Context context)
		{
			_node.Evaluate(context);
			return _result ? NodeState.Success : NodeState.Failure;
		}

		private ConstantResultNode(INode node, bool result)
		{
			_node = node;
			_result = result;
		}
	}
}
