using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utilites.Collections
{
    public class SecureList<T> : IList<T>
    {
        private const int _reserveSize = 8;
        private readonly int _mask = (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);
        private readonly List<T> _items = new();
        private readonly List<int> _indices = new();
        private readonly Queue<int> _unusedIndices = new();

        public T this[int index] 
        {
            get => _items[_indices[index] ^ _mask];
            set
            {
                var count = _indices.Count;
                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException();

                int newIndex;
                int oldIndex = _indices[index] ^ _mask;
                _unusedIndices.Enqueue(oldIndex);

                if (_unusedIndices.Count > _reserveSize)
                {
                    newIndex = _unusedIndices.Dequeue();
                    _items[newIndex] = value;
                }
                else
                {
                    newIndex = _items.Count;
                    _items.Add(value);
                }

                _indices[index] = newIndex ^ _mask;
            }
        }

        public int Count => _indices.Count;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if (_unusedIndices.Count > _reserveSize)
            {
                var index = _unusedIndices.Dequeue();
                _indices.Add(index ^ _mask);
                _items[index] = item;
            }
            else
            {
                _indices.Add(_items.Count ^ _mask);
                _items.Add(item);
            }
        }

        public void Clear()
        {
            _items.Clear();
            _indices.Clear();
            _unusedIndices.Clear();
        }

        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _indices.Count; ++i)
                if (comparer.Equals(_items[_indices[i] ^ _mask], item))
                    return true;

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var total = array.Length - arrayIndex;
            if (total < _items.Count) 
                throw new ArgumentException();

            for (int i = 0; i < _indices.Count; ++i)
                array[i + arrayIndex] = _items[_indices[i] ^ _mask];
        }

        public IEnumerator<T> GetEnumerator() => _indices.Select(index => _items[index ^ _mask]).GetEnumerator();

        public int IndexOf(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _indices.Count; ++i)
            {
                var index = _indices[i] ^ _mask;
                if (comparer.Equals(_items[index], item))
                    return index;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            int newIndex;
            if (_unusedIndices.Count > _reserveSize)
            {
                newIndex = _unusedIndices.Dequeue();
                _items[newIndex] = item;
            }
            else
            {
                newIndex = _items.Count;
                _items.Add(item);
            }

            _indices.Insert(index, newIndex ^ _mask);
        }

        public bool Remove(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _indices.Count; ++i)
            {
                var index = _indices[i] ^ _mask;
                if (comparer.Equals(_items[index], item))
                {
                    _unusedIndices.Enqueue(index);
                    _indices.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            _unusedIndices.Enqueue(_indices[index] ^ _mask);
            _indices.RemoveAt(index);
        }

        public override string ToString()
        {
            var result = $"{_indices.Count}/{_items.Count}/{_unusedIndices.Count}";
            for (int i = 0; i < Count; ++i)
            {
                result += $" {this[i]}";
            }

            return result;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
