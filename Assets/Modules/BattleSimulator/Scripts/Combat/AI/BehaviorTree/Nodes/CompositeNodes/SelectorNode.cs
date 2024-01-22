using System.Collections.Generic;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SelectorNode : INode
	{
		public List<INode> Nodes { get; } = new();

		public NodeState Evaluate(Context context)
		{
			foreach (var node in Nodes)
			{
				switch (node.Evaluate(context))
				{
					case NodeState.Failure:
						continue;
					case NodeState.Success:
						return NodeState.Success;
					case NodeState.Running:
						return NodeState.Running;
				}
			}

			return NodeState.Failure;
		}
	}
}
