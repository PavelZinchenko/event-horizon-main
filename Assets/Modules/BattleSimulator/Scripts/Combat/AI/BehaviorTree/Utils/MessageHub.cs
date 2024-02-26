using Combat.Component.Ship;
using System.Collections.Generic;

namespace Combat.Ai.BehaviorTree.Utils
{
	public interface IMessageListener
	{
		bool IsAlive { get; }
		bool IsSuitableSender(IShip sender);
		void OnMessageReceived(int id, IShip sender);
	}

	public class MessageHub
	{
		private readonly Dictionary<int, List<IMessageListener>> _messageListeners = new();
		private readonly IdentifiersMap _messageIds;

		public int GetMessageId(string name) => _messageIds.GetMessageId(name);

		public void Subscribe(int id, IMessageListener messageListener)
		{
			if (!_messageListeners.TryGetValue(id, out var listeners))
			{
				listeners = new List<IMessageListener>();
				_messageListeners[id] = listeners;
			}

			listeners.Add(messageListener);
		}

		public bool TrySendMessage(int id, IShip sender)
		{
			if (!_messageListeners.TryGetValue(id, out var listeners)) return false;

			bool result = false;
			int i = 0;
			while (i < listeners.Count)
			{
				var listener = listeners[i];
				if (!listener.IsAlive)
				{
					RemoveListener(listeners, i);
					continue;
				}

				if (listener.IsSuitableSender(sender))
				{
					listener.OnMessageReceived(id, sender);
					result = true;
				}

				i++;
			}

			return result;
		}

		private static void RemoveListener(List<IMessageListener> listeners, int index)
		{
			var last = listeners.Count - 1;
			if (index != last)
				listeners[index] = listeners[last];

			listeners.RemoveAt(last);
		}
	}
}
