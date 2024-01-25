using Combat.Ai.BehaviorTree.Utils;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SubscribeToMessageNode : INode, IMessageListener
	{
		private readonly MessageHub _messageHub;
		private readonly int _messageId;
		private IShip _ship;
		private IShip _sender;

		public SubscribeToMessageNode(MessageHub messageHub, string message)
		{
			_messageHub = messageHub;
			_messageId = _messageHub.GetMessageId(message);
		}

		public bool IsAlive => _ship.State == Unit.UnitState.Active;

		public bool IsSuitableSender(IShip sender)
		{
			if (sender == _ship) return false;
			return sender.Type.Side == _ship.Type.Side;
		}

		public void OnMessageReceived(int id, IShip sender)
		{
			_sender = sender;
		}

		public NodeState Evaluate(Context context)
		{
			if (_ship == null)
			{
				_ship = context.Ship;
				_messageHub.Subscribe(_messageId, this);
			}

			if (_sender == null)
				return NodeState.Failure;

			context.LastMessageSender = _sender;
			_sender = null;
			return NodeState.Success;
		}
	}
}
