using System.Collections.Generic;
using System.Collections;

namespace Session.Utils
{
	public readonly struct ObservableSet<T> : IReadOnlyCollection<T>
    {
		private readonly IDataChangedCallback _callback;
		private readonly HashSet<T> _hashset;

		public int Count => _hashset.Count;

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

        public bool Equals(ObservableSet<T> other, IEqualityComparer<T> equalityComparer)
        {
            if (_hashset == other._hashset) return true;
            if (_hashset.Count != other._hashset.Count) return false;
            return _hashset.SetEquals(other._hashset);
        }

        public void Clear()
		{
			_hashset.Clear();
			_callback?.OnDataChanged();
		}

        public IEnumerator<T> GetEnumerator() => _hashset.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _hashset.GetEnumerator();
    }
}
