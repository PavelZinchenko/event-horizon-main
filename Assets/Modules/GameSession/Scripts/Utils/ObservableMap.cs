using System.Collections.Generic;

namespace Session.Utils
{
	public readonly struct ObservableMap<TKey, TValue>
	{
		private readonly IDataChangedCallback _callback;
		private readonly Dictionary<TKey, TValue> _dictionary;

		public int Count => _dictionary.Count;

		public TValue this[TKey key]
		{
			get => _dictionary[key];
			set => SetValue(key, value);
		}

		public void SetValue(TKey key, TValue value)
		{ 
			_dictionary[key] = value;
			_callback?.OnDataChanged();
		}

		public void Assign(ObservableMap<TKey, TValue> other)
		{
			_dictionary.Clear();
			foreach (var item in other._dictionary)
				_dictionary.Add(item.Key, item.Value);
			_callback?.OnDataChanged();
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> Items => _dictionary;
		public IEnumerable<TKey> Keys => _dictionary.Keys;
		public IEnumerable<TValue> Values => _dictionary.Values;

		public ObservableMap(IDataChangedCallback callback)
			: this(callback, new())
		{
		}

		public ObservableMap(ObservableMap<TKey, TValue> original, IDataChangedCallback callback)
			: this(callback, original._dictionary)
		{
		}

		public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
		public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

		public bool TryAdd(TKey key, TValue value)
		{
			if (!_dictionary.TryAdd(key, value)) return false;
			_callback?.OnDataChanged();
			return true;
		}

		public void Add(TKey key, TValue value)
		{
			_dictionary.Add(key, value);
			_callback?.OnDataChanged();
		}

		public bool Remove(TKey key)
		{
			if (!_dictionary.Remove(key)) return false;
			_callback?.OnDataChanged();
			return true;
		}

		public void Clear()
		{
			_dictionary.Clear();
			_callback?.OnDataChanged();
		}

		private ObservableMap(IDataChangedCallback callback, Dictionary<TKey, TValue> dictionary)
		{
			_callback = callback;
			_dictionary = dictionary;
		}
	}
}
