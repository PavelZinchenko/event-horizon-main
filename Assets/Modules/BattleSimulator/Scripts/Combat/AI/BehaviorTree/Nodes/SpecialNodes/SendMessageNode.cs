using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SendMessageNode : INode
	{
		private readonly MessageHub _messageHub;
		private readonly int _id;

		public SendMessageNode(MessageHub messageHub, string message)
		{
			_messageHub = messageHub;
			_id = messageHub.GetMessageId(message);
		}

		public NodeState Evaluate(Context context)
		{
			return _messageHub.TrySendMessage(_id, context.Ship) ? NodeState.Success : NodeState.Failure;
		}
	}
}
