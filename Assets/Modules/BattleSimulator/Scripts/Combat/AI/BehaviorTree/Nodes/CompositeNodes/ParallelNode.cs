using System.Collections.Generic;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ParallelNode : INode
	{
		public List<INode> Nodes { get; } = new();

		public NodeState Evaluate(Context context)
		{
			int completed = 0;
			int failed = 0;
			int running = 0;

			foreach (var node in Nodes)
			{
				switch (node.Evaluate(context))
				{
					case NodeState.Running:
						running++;
						break;
					case NodeState.Success:
						completed++;
						break;
					case NodeState.Failure:
						failed++;
						break;
				}
			}

			if (completed > 0) 
				return NodeState.Success;
			if (running > 0)
				return NodeState.Running;

			return NodeState.Failure;
		}
	}
}
