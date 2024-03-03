using System.Collections.Generic;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class IfThenElseNode : INode
	{
		public List<INode> Nodes { get; } = new();

		public NodeState Evaluate(Context context)
		{
			switch (Nodes[0].Evaluate(context))
			{
				case NodeState.Success:
                    return Nodes[1].Evaluate(context);
                case NodeState.Failure:
                    return Nodes[2].Evaluate(context);
                case NodeState.Running:
					return NodeState.Running;
			}

			return NodeState.Failure;
		}
	}
}
