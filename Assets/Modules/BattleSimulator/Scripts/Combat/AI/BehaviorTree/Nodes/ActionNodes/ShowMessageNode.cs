namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ShowMessageNode : INode
	{
		private readonly string _message;
		private readonly UnityEngine.Color _color;

		public ShowMessageNode(string message, UnityEngine.Color color)
		{
			_message = message;
			_color = color;
		}

		public NodeState Evaluate(Context context)
		{
			context.Ship.Broadcast(_message, _color);
			return NodeState.Success;
		}
	}
}
