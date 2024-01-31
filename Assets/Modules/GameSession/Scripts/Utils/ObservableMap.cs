using System.Collections;
using System.Collections.Generic;

namespace Session.Utils
{
	public readonly struct ObservableMap<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		private readonly IDataChangedCallback _callback;
		private readonly Dictionary<TKey, TValue> _dictionary;

		public int Count => _dictionary.Count;

		public TValue this[TKey key] => _dictionary[key];

		public void SetValue(TKey key, TValue value)
		{ 
			_dictionary[key] = value;
			_callback?.OnDataChanged();
		}

        public void SetValue(TKey key, TValue value, IEqualityComparer<TValue> valueComparer)
        {
            if (_dictionary.TryGetValue(key, out TValue oldValue) && valueComparer.Equals(value, oldValue)) return;

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
        
        public bool Equals(ObservableMap<TKey, TValue> other, IEqualityComparer<TValue> valueComparer)
        {
            if (_dictionary == other._dictionary) return true;
            if (_dictionary.Count != other._dictionary.Count) return false;
            
            foreach (var item in _dictionary)
            {
                if (!other._dictionary.TryGetValue(item.Key, out TValue value)) return false;
                if (!valueComparer.Equals(item.Value, value)) return false;
            }

            return true;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        private ObservableMap(IDataChangedCallback callback, Dictionary<TKey, TValue> dictionary)
		{
			_callback = callback;
			_dictionary = dictionary;
		}
	}
}
