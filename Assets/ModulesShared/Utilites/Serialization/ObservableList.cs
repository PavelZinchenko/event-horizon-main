using System.Collections;
using System.Collections.Generic;

namespace Session.Utils
{
	public readonly struct ObservableList<T> : IReadOnlyList<T>
    {
		private readonly IDataChangedCallback _callback;
		private readonly List<T> _list;

		public int Count => _list.Count;

		public T this[int index] => _list[index];

		public void Assign(ObservableList<T> other)
		{
			_list.Clear();
			_list.AddRange(other._list);
			_callback?.OnDataChanged();
		}

		public T[] ToArray() => _list.ToArray();

        public int FindIndex(System.Predicate<T> match) => _list.FindIndex(match);

        public void Sort(System.Comparison<T> comparison) => _list.Sort(comparison);
        public void Sort(IComparer<T> comparer) => _list.Sort(comparer);

        public bool Equals(ObservableList<T> other, IEqualityComparer<T> equalityComparer)
        {
            if (_list == other._list) return true;
            if (_list.Count != other._list.Count) return false;

            for (int i = 0; i < _list.Count; ++i)
                if (!equalityComparer.Equals(_list[i], other._list[i])) 
                    return false;

            return true;
        }

        public void SetValue(int index, T value)
        {
            _list[index] = value;
            _callback?.OnDataChanged();
        }

        public int Assign(IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
        {
            var oldCount = _list.Count;
            int changedItems = 0;
            int index = 0;
            foreach (var item in items)
            {
                if (index >= oldCount)
                {
                    _list.Add(item);
                    changedItems++;
                }
                else if (changedItems > 0 || !equalityComparer.Equals(_list[index], item))
                {
                    _list[index] = item;
                    changedItems++;
                }

                index++;
            }

            if (index < oldCount)
            {
                _list.RemoveRange(index, oldCount - index);
                changedItems += oldCount - index;
            }

            if (changedItems > 0)
                _callback?.OnDataChanged();

            return changedItems;
        }

        public void SetValue(int index, T value, IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer.Equals(_list[index], value)) return;

            _list[index] = value;
            _callback?.OnDataChanged();
        }

        public ObservableList(IDataChangedCallback callback)
		{
			_callback = callback;
			_list = new();
		}

		public ObservableList(int capacity, IDataChangedCallback callback)
		{
			_callback = callback;
			_list = new(capacity);
		}

		public ObservableList(IEnumerable<T> items, IDataChangedCallback callback)
		{
			_callback = callback;
			_list = new(items);
		}

		public void Clear()
		{
			_list.Clear();
			_callback?.OnDataChanged();
		}

		public void Add(T value)
		{
			_list.Add(value);
			_callback?.OnDataChanged();
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
			_callback?.OnDataChanged();
		}

        public void QuickRemove(int index)
        {
            var count = _list.Count;
            if (index < 0 || index >= count)
                return;

            if (index + 1 < count)
                _list[index] = _list[count - 1];

            _list.RemoveAt(count - 1);

            _callback?.OnDataChanged();
        }

        public void RemoveRange(int index, int count)
        {
            if (index >= _list.Count || count <= 0) return;

            _list.TrimExcess();
            _callback?.OnDataChanged();
        }

        public override int GetHashCode()
        {
            const int seed = 17;
            const int modifier = 31;

            int hash = seed;
            for (int i = 0; i < _list.Count; i++)
                if (_list[i] != null)
                    hash = hash * modifier + _list[i].GetHashCode();

            return hash;
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}
