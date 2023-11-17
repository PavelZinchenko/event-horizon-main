using System.Collections.Generic;
using System.Linq;
using GameDatabase.DataModel;

namespace GameDatabase.Extensions
{
    public static class FactionExtensions
    {
        public static IEnumerable<Faction> WithTechTree(this IEnumerable<Faction> factions)
        {
            return factions.Where(item => !item.HideResearchTree);
        }

        public static IEnumerable<Faction> CanGiveTechPoints(this IEnumerable<Faction> factions, int distance)
        {
            return factions.Where(item => !item.HideResearchTree && item.HomeStarDistance <= distance);
        }

        public static IEnumerable<Faction> ValidForMerchants(this IEnumerable<Faction> factions)
        {
            return factions.Where(item => !item.HideFromMerchants);
        }

        public static IEnumerable<Faction> WithStarbases(this IEnumerable<Faction> factions, int distance = 0)
        {
            return factions.Where(item => !item.NoTerritories && item.HomeStarRange.Contains(distance));
        }
    }
}
