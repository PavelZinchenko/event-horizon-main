using System.Collections.Generic;

namespace Session.Utils
{
	public readonly struct ObservableInventory<T>
	{
		private readonly IDataChangedCallback _callback;
		private readonly Dictionary<T, ObscuredInt> _items;

		public int Count => _items.Count;
		public int GetQuantity(T item) => _items.TryGetValue(item, out var value) ? (int)value : 0;

		public int this[T item]
		{
			get => GetQuantity(item);
			set => SetValue(item, value);
		}

		public void Add(T item, int amount = 1)
		{
			if (amount <= 0) return;
			if (!_items.TryGetValue(item, out var value))
				value = 0;

			_items[item] = value + amount;
			_callback?.OnDataChanged();
		}

		public int Remove(T item, int amount = 1)
		{
			if (amount <= 0) return 0;
			if (!_items.TryGetValue(item, out var value)) return 0;

			var quantity = value - amount;
			if (quantity <= 0)
				_items.Remove(item);
			else
				_items[item] = quantity;

			return quantity >= 0 ? amount : amount + quantity;
		}

		public void SetValue(T item, int quantity)
		{ 
			_items[item] = quantity;
			_callback?.OnDataChanged();
		}

		public void Assign(ObservableInventory<T> other)
		{
			_items.Clear();
			foreach (var item in other._items)
				_items.Add(item.Key, item.Value);
			_callback?.OnDataChanged();
		}

		public IEnumerable<KeyValuePair<T, ObscuredInt>> Items => _items;
		public IEnumerable<T> Keys => _items.Keys;
		public IEnumerable<ObscuredInt> Values => _items.Values;

		public ObservableInventory(IDataChangedCallback callback)
			: this(callback, new())
		{
		}

		public ObservableInventory(ObservableInventory<T> original, IDataChangedCallback callback)
			: this(callback, original._items)
		{
		}

		public void Clear()
		{
			_items.Clear();
			_callback?.OnDataChanged();
		}

		private ObservableInventory(IDataChangedCallback callback, Dictionary<T, ObscuredInt> dictionary)
		{
			_callback = callback;
			_items = dictionary;
		}
	}
}
