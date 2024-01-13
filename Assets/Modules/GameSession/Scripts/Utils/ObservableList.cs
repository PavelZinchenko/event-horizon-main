using System.Collections.Generic;

namespace Session.Utils
{
	public readonly struct ObservableList<T>
	{
		private readonly IDataChangedCallback _callback;
		private readonly List<T> _list;

		public int Count => _list.Count;

		public T this[int index]
		{
			get => _list[index];
			set
			{
				_list[index] = value;
				_callback?.OnDataChanged();
			}
		}

		public IEnumerable<T> Items => _list;
		public T[] ToArray() => _list.ToArray();

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
	}
}
