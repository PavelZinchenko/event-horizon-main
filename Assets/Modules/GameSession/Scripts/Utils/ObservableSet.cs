using System.Collections.Generic;

namespace Session.Utils
{
	public readonly struct ObservableSet<T>
	{
		private readonly IDataChangedCallback _callback;
		private readonly HashSet<T> _hashset;

		public int Count => _hashset.Count;

		public IEnumerable<T> Items => _hashset;

		public void Assign(ObservableSet<T> other)
		{
			_hashset.Clear();
			_hashset.UnionWith(other._hashset);
			_callback?.OnDataChanged();
		}

		public ObservableSet(IDataChangedCallback callback)
		{
			_callback = callback;
			_hashset = new();
		}

		public bool Contains(T value) => _hashset.Contains(value);
		
		public bool Add(T value)
		{
			if (!_hashset.Add(value)) return false;
			_callback?.OnDataChanged();
			return true;
		}

		public bool Remove(T value)
		{
			if (!_hashset.Remove(value)) return false;
			_callback?.OnDataChanged();
			return true;
		}

		public void Clear()
		{
			_hashset.Clear();
			_callback?.OnDataChanged();
		}
	}
}
