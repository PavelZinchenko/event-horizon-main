using System.Collections.Generic;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SequenceNode : INode
	{
		public List<INode> Nodes { get; } = new();

		public NodeState Evaluate(Context context)
		{
			foreach (var node in Nodes)
			{
				switch (node.Evaluate(context))
				{
					case NodeState.Failure:
						return NodeState.Failure;
					case NodeState.Success:
						continue;
					case NodeState.Running:
						return NodeState.Running;
				}
			}

			return NodeState.Success;
		}
	}
}
