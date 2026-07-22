using System.Collections.Generic;

namespace Utilites.Collections
{
    public interface IQuantityProvier<T>
    {
        int GetQuantity(T item);
    }

    public interface IReadOnlyInventory<T> : IQuantityProvier<T>
    {
        IReadOnlyCollection<T> Items { get; }
    }

    public interface IInventory<T> : IReadOnlyInventory<T>
    {
        void Add(T item, int quantity = 1);
        int Remove(T item, int quantity = 1);
        void Clear();

        public static readonly IInventory<T> Empty = new NullInventory<T>();
    }

    public interface ILockableInventory<T> : IInventory<T>
    {
        bool Lock(T item);
        void Unlock(T item);
    }

    internal class NullInventory<T> : ILockableInventory<T>
    {
        public IReadOnlyCollection<T> Items => System.Array.Empty<T>();
        public void Add(T item, int quantity = 1) { }
        public int Remove(T item, int quantity = 1) => 0;
        public void Clear() { }
        public int GetQuantity(T item) => 0;
        public bool Lock(T item) => false;
        public void Unlock(T item) { }
    }
}
