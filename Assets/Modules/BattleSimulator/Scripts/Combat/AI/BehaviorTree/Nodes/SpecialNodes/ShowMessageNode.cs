namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ShowMessageNode : INode
	{
		private const float _cooldown = 0.5f;
		private readonly string _message;
		private readonly UnityEngine.Color _color;

		public ShowMessageNode(string message, UnityEngine.Color color)
		{
			_message = message;
			_color = color;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.Time - context.LastTextMessageTime < _cooldown)
				return NodeState.Running;

			context.Ship.Broadcast(_message, _color);
			context.LastTextMessageTime = context.Time;
			return NodeState.Success;
		}
	}
}
