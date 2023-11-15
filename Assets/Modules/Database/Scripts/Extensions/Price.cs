using GameDatabase.Enums;
using GameDatabase.DataModel;

namespace GameDatabase.Extensions
{
    // TODO: move formulas to database
    public static class PriceExtensions
    {
        public static int PriceInStars(this Satellite satellite)
        {
            return satellite.Layout.CellCount;
        }

        public static int Price(this Satellite satellite)
        {
            return satellite.Layout.CellCount * satellite.Layout.CellCount * 20;
        }

        public static int CraftingPrice(this Satellite satellite)
        {
            return satellite.Price() * 10;
        }

        public static int CraftingStars(this Satellite satellite)
        {
            return UnityEngine.Mathf.Max(0, satellite.Layout.CellCount / 7 - 1);
        }

        public static int CraftingPrice(this Component component)
        {
            return component.Price();
        }

        public static int CraftingStars(this Component component)
        {
            return component.Level / 75;
        }

        public static int CraftingPrice(this Ship ship)
        {
            var price = ship.Layout.CellCount * ship.Layout.CellCount * 5;

            if (ship.SizeClass == SizeClass.Titan)
                return price * 3;
            else if (ship.ShipRarity == ShipRarity.Rare)
                return 3 * price / 2;
            else
                return price;
        }

        public static int CraftingStars(this Ship ship)
        {
            if (ship.SizeClass == SizeClass.Titan)
                return ship.Layout.CellCount / 10;
            else if (ship.ShipRarity == ShipRarity.Rare)
                return 1 + (ship.Layout.CellCount - 30) / 10;
            else
                return ship.Layout.CellCount / 70;
        }

        public static int Price(this Component component, ModificationQuality? quality = null)
        {
            var price = 50 + component.Level * 20;
            if (quality.HasValue)
                price = price * QualityToPrice(quality.Value) / 100;
            if (component.Weapon != null)
                price *= 2;

            return 1 + price;
        }

        public static int PremiumPriceInCredits(this Component component, ModificationQuality? quality = null)
        {
            var price = 100 + component.Level * 100;;
            if (quality.HasValue)
                price = price * QualityToPrice(quality.Value) / 100; 
            if (component.Weapon != null)
                price = 3 * price / 2;

            return 1 + price;
        }

        public static int PremiumPriceInStas(this Component component, ModificationQuality? quality = null)
        {
            var price = component.Level;
            if (quality.HasValue)
                price = price * QualityToPrice(quality.Value) / 100;

            if (component.Weapon != null)
                price = 3 * price / 2;

            return 1 + price/10;
        }

        private static int QualityToPrice(ModificationQuality quality) => quality.ToValue(50, 75, 90, 150, 200, 300);
    }
}
