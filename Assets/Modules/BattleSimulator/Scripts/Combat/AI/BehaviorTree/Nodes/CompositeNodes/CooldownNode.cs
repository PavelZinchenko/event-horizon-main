using GameDatabase.Enums;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class CooldownNode : INode
	{
		private readonly INode _node;
		private readonly NodeExecutionMode _mode;
		private readonly NodeState _result;
		private readonly float _cooldown;
		private float _lastExecutionTime;

		public static INode Create(INode node, NodeExecutionMode mode, float cooldown, NodeState result)
		{
			if (node.IsEmpty())
			{
				var nodeResult = node.Evaluate(null);
				if (!mode.IsConditionMet(nodeResult) || result == nodeResult)
					return node;
			}

			return new CooldownNode(node, mode, result, cooldown);
		}

		public NodeState Evaluate(Context context)
		{
			if (context.Time - _lastExecutionTime < _cooldown)
				return _result;

			var result = _node.Evaluate(context);
			if (_mode.IsConditionMet(result))
				_lastExecutionTime = context.Time;

			return result;
		}

		private CooldownNode(INode node, NodeExecutionMode mode, NodeState cooldownState, float cooldown)
		{
			_node = node;
			_mode = mode;
			_result = cooldownState;
			_cooldown = cooldown;
			_lastExecutionTime = -_cooldown;
		}
	}
}
