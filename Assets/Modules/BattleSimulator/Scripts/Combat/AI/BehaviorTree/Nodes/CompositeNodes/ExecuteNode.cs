using GameDatabase.Enums;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ExecuteNode : INode
	{
		private readonly INode _node;
		private readonly NodeExecutionMode _mode;
		private readonly NodeState _result;
		private bool _completed;

		public static INode Create(INode node, NodeExecutionMode mode, NodeState result)
		{
			if (node.IsEmpty())
			{
				var nodeResult = node.Evaluate(null);
				if (!mode.IsConditionMet(nodeResult) || result == nodeResult)
					return node;
			}

			return new ExecuteNode(node, mode, result);
		}

		public NodeState Evaluate(Context context)
		{
			if (_completed) 
				return _result;

			var result = _node.Evaluate(context);
			if (_mode.IsConditionMet(result))
				_completed = true;

			return result;
		}

		private ExecuteNode(INode node, NodeExecutionMode mode, NodeState result)
		{
			_node = node;
			_mode = mode;
			_result = result;
		}
	}
}
