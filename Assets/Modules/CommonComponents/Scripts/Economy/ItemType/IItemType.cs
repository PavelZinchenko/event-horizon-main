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

    public static class ItemQualityExtensions
    {
        public static Color ToColor(this ItemQuality quality)
        {
            return AppConfiguration.ColorTable.QualityColor((AppConfiguration.ColorTable.Quality)quality);
        }
    }
}
