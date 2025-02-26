using GameDatabase.Model;
using UnityEngine;

namespace Economy.ItemType
{
    public enum ItemQuality
    {
        Low,
        Common,
        Medium,
        High,
        Perfect,
    }

    public interface IItemType
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        SpriteId Icon { get; }
        Color Color { get; }
        Price Price { get; }
        ItemQuality Quality { get; }

        void Consume(int amount);
        void Withdraw(int amount);
        int MaxItemsToConsume { get; }
        int MaxItemsToWithdraw { get; }
    }

    public class UnknownItem : IItemType
    {
        public string Id => string.Empty;
        public string Name => "???";
        public string Description => string.Empty;
        public SpriteId Icon => SpriteId.Empty;
        public Color Color => Color.white;
        public Price Price => new();
        public ItemQuality Quality => ItemQuality.Common;
        public int MaxItemsToConsume => int.MaxValue;
        public int MaxItemsToWithdraw => int.MaxValue;

        public void Consume(int amount) {}
        public void Withdraw(int amount) {}
    }
}
