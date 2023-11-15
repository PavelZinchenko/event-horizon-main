using System.Collections.Generic;
using System.Linq;
using Constructor;
using Constructor.Ships;
using UnityEngine;

namespace GameServices.Multiplayer
{
    public static class RatingCalculator
    {
        public static int GetRatingChange(int rating, int enemyRating, bool victory)
        {
            return victory ? GetRatingChangeOnVictory(rating, enemyRating) : GetRatingChangeOnDefeat(rating, enemyRating);
        }

        public static int GetRatingChangeOnVictory(int rating, int enemyRating)
        {
            return Mathf.Max(100, Mathf.Min(rating, enemyRating)/10);
        }

        public static int GetRatingChangeOnDefeat(int rating, int enemyRating)
        {
            return -Mathf.Max(100, rating/10);
        }

        public static int GetTokensForBattle(int rating) { return 1 + rating / 1000; }

        public static int CalculateFleetPower(IEnumerable<IShip> fleet)
        {
            var rating = 0;
            foreach (var ship in fleet)
            {
                rating += ship.Model.Layout.CellCount*10;
                rating += ship.Components.Sum(item => CalculateComponentPower(item.Info));
                if (ship.FirstSatellite != null)
                {
                    rating += ship.FirstSatellite.Information.Layout.CellCount;
                    rating += ship.FirstSatellite.Components.Sum(item => CalculateComponentPower(item.Info));
                }
                if (ship.SecondSatellite != null)
                {
                    rating += ship.SecondSatellite.Information.Layout.CellCount;
                    rating += ship.SecondSatellite.Components.Sum(item => CalculateComponentPower(item.Info));
                }
            }

            return rating;
        }

        private static int CalculateComponentPower(ComponentInfo component)
        {
            return component.Data.Layout.CellCount * component.Data.Level/5;
        }

    }
}
