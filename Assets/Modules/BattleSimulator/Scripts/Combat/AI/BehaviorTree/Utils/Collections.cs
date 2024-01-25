using System.Collections.Generic;

namespace Combat.Ai.BehaviorTree.Utils
{
	public struct IdentifiersMap
	{
		private Dictionary<string, int> _ids;

		public int GetMessageId(string name)
		{
			if (_ids == null)
				_ids = new();

			if (_ids.TryGetValue(name, out int id))
				return id;

			var lastId = _ids.Count;
			_ids.Add(name, lastId);
			return lastId;
		}
	}

	public struct AutoExpandingList<T>
	{
		private List<T> _list;

		public T this[int index]
		{
			get
			{
				if (_list == null) return default(T);
				if (index < 0 || index >= _list.Count) return default(T);
				return _list[index];
			}
			set
			{
				if (index < 0) return;

				if (_list == null)
					_list = new();

				for (int i = _list.Count; i <= index; ++i)
					_list.Add(default(T));

				_list[index] = value;
			}
		}
	}

	public struct BitArray64
	{
		private ulong _value;

		public bool this[int index]
		{
			get
			{
				if (index < 0 || index > 63) return false;
				return ((_value >> index) & 1) == 1;
			}
			set
			{
				if (index < 0 || index > 63) return;

				if (value)
					_value |= 1ul << index;
				else
					_value &= ~(1ul << index);
			}
		}
	}
}
