using System.Collections.Generic;

namespace Utilites.Collections
{
    public class SecureInventory<T> : IInventory<T>
    {
        private readonly int _mask = (int)System.DateTime.UtcNow.Ticks;
        private readonly SecureList<int> _quantities = new();
        private readonly Dictionary<T, int> _indices = new();
        private readonly Queue<int> _unusedIndices = new();

        public bool IsDirty { get; set; }
        public IReadOnlyCollection<T> Items => _indices.Keys;

        public int GetQuantity(T item) => _indices.TryGetValue(item, out var index) ? _quantities[index] ^ _mask : 0;

        public void Add(T item, int quantity = 1)
        {
            if (quantity <= 0) return;
            if (_indices.TryGetValue(item, out int index))
            {
                var oldQuantity = _quantities[index] ^ _mask;
                _quantities[index] = (oldQuantity + quantity) ^ _mask;
            }
            else if (_unusedIndices.TryDequeue(out index))
            {
                _indices.Add(item, index);
                _quantities[index] = quantity ^ _mask;
            }
            else
            {
                _indices.Add(item, _indices.Count);
                _quantities.Add(quantity ^ _mask);
            }

            OnDataChanged();
        }

        public int Remove(T item, int quantity = 1)
        {
            if (quantity <= 0) return 0;
            if (!_indices.TryGetValue(item, out var index))
                return 0;

            var oldQuantity = _quantities[index] ^ _mask;
            if (oldQuantity <= quantity)
            {
                _indices.Remove(item);
                _unusedIndices.Enqueue(index);
                quantity = oldQuantity;
            }
            else
            {
                _quantities[index] = (oldQuantity - quantity) ^ _mask;
            }

            OnDataChanged();
            return quantity;
        }

        public void Clear()
        {
            _indices.Clear();
            _quantities.Clear();
            _unusedIndices.Clear();
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            IsDirty = true;
        }
    }
}
