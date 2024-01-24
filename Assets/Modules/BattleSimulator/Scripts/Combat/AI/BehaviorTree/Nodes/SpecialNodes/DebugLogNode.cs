namespace Combat.Ai.BehaviorTree.Nodes
{
	public class DebugLogNode : INode
	{
		private readonly string _message;

		public DebugLogNode(string message)
		{
			_message = message;
		}

		public NodeState Evaluate(Context context)
		{
			GameDiagnostics.Trace.LogError(_message);
			return NodeState.Success;
		}
	}
}
