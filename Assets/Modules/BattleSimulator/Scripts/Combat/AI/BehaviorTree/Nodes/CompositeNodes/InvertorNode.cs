namespace Combat.Ai.BehaviorTree.Nodes
{
	public class InvertorNode : INode
	{
		private readonly INode _node;

		public static INode Create(INode node)
		{
			if (node == EmptyNode.Success) return EmptyNode.Failure;
			if (node == EmptyNode.Failure) return EmptyNode.Success;
			return new InvertorNode(node);
		}

		public NodeState Evaluate(Context context)
		{
			switch (_node.Evaluate(context))
			{
				case NodeState.Success: 
					return NodeState.Failure;
				case NodeState.Failure:
					return NodeState.Success;
				default:
					return NodeState.Running;
			}
		}

		private InvertorNode(INode node) => _node = node;
	}
}
