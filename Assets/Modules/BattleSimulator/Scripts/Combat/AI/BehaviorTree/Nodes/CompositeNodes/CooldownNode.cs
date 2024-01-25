namespace Combat.Ai.BehaviorTree.Nodes
{
	public class CooldownNode : INode
	{
		private readonly INode _node;
		private readonly float _cooldown;

		private float _lastExecuteTime;

		public static INode Create(INode node, float cooldown)
		{
			if (node == EmptyNode.Failure) return EmptyNode.Failure;
			return new CooldownNode(node, cooldown);
		}

		public NodeState Evaluate(Context context)
		{
			if (context.Time - _lastExecuteTime < _cooldown)
				return NodeState.Failure;

			var result = _node.Evaluate(context);
			if (result == NodeState.Success)
				_lastExecuteTime = context.Time;

			return result;
		}

		private CooldownNode(INode node, float cooldown)
		{
			_node = node;
			_cooldown = cooldown;
			_lastExecuteTime = -_cooldown;
		}
	}
}
