using System.Collections.Generic;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ParallelSequenceNode : INode
	{
		public List<INode> Nodes { get; } = new();

		public NodeState Evaluate(Context context)
		{
			bool anyChildRunning = false;

			foreach (var node in Nodes)
			{
				switch (node.Evaluate(context))
				{
					case NodeState.Failure:
						return NodeState.Failure;
					case NodeState.Success:
						continue;
					case NodeState.Running:
						anyChildRunning = true;
						continue;
				}
			}

			return anyChildRunning ? NodeState.Running : NodeState.Success;
		}
	}
}
