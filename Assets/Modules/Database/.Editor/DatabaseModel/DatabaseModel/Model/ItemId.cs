using System;
using EditorDatabase.Serializable;

namespace EditorDatabase.Model
{
    public interface IItemId
    {
        int Value { get; }
        string Name { get; }
        Type ItemType { get; }
        bool IsNull { get; }
    }

    public class ItemId<T> : IItemId
    {
        public ItemId() { }

        public ItemId(int id, string name)
        {
            Value = id;
            Name = name;
        }

        public ItemId(SerializableItem item)
        {
            if (item == null)
            {
                Value = 0;
                Name = string.Empty;
                return;
            }

            Value = item.Id;
            Name = item.FileName;
        }

        public int Value { get; }
        public string Name { get; }
        public Type ItemType => typeof(T);
        public bool IsNull => Value <= 0;

        public override int GetHashCode() { return Value; }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case ItemId<T> id:
                    return Value == id.Value;
                case int i:
                    return Value == i;
                default:
                    return false;
            }
        }

        public override string ToString() { return string.IsNullOrEmpty(Name) ? "[EMPTY]" : Name; }

        public static readonly ItemId<T> Empty = new ItemId<T>(0, string.Empty);
    }
}
