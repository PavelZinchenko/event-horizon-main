using System;
using System.Collections.Generic;

namespace Combat.Scene
{
    public class UnitList<T> : IUnitList<T>
        where T : IDisposable
    {
        private readonly object _itemsLock = new();
        private readonly Queue<T> _addedItems = new();
        private readonly List<T> _deletedItems = new();
        private readonly List<T> _items = new();

        public event Action<T> UnitAdded;
        public event Action<T> UnitRemoved;

        public IReadOnlyList<T> Items => _items;
        public object LockObject => _itemsLock;

        public void Add(T item)
        {
            _addedItems.Enqueue(item);
        }

        public void UpdateCollection()
        {
            while (_addedItems.Count > 0)
            {
                var item = _addedItems.Dequeue();

                lock (_itemsLock)
                    _items.Add(item);

                if (UnitAdded != null)
                    UnitAdded.Invoke(item);
            }

            if (_deletedItems.Count > 0)
            {
                for (int i = 0; i < _deletedItems.Count; ++i)
                    _deletedItems[i].Dispose();

                _deletedItems.Clear();
            }
        }

        public void UpdateItems(Func<T, bool> updateFunc)
        {
            var deletedItemCount = _deletedItems.Count;

            lock (_itemsLock)
            {
                var count = _items.Count;
                var index = 0;

                while (index < count)
                {
                    var item = _items[index];

                    if (updateFunc(item))
                    {
                        index++;
                        continue;
                    }

                    _deletedItems.Add(item);
                    if (index < count - 1)
                        _items[index] = _items[count - 1];

                    count--;
                }

                if (count < _items.Count)
                    _items.RemoveRange(count, _items.Count - count);
            }

            if (UnitRemoved != null)
                for (var i = deletedItemCount; i < _deletedItems.Count; ++i)
                    UnitRemoved.Invoke(_deletedItems[i]);
        }

        public void Clear()
        {
            foreach (var item in _addedItems)
                item.Dispose();
            _addedItems.Clear();

            foreach (var item in _deletedItems)
                item.Dispose();
            _deletedItems.Clear();

            lock (_itemsLock)
            {
                foreach (var item in _items)
                    item.Dispose();
                _items.Clear();
            }
        }
    }
}
