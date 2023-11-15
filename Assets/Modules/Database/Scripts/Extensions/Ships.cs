using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace GameDatabase.Extensions
{
    public static class ShipExtensions
    {
        public static IEnumerable<ShipBuild> ValidForPlayer(this IEnumerable<ShipBuild> builds)
        {
            foreach (var build in builds)
            {
                if (build.NotAvailableInGame) continue;
                if (build.DifficultyClass != DifficultyClass.Default) continue;
                if (build.Ship.ShipType != ShipType.Common && build.Ship.ShipType != ShipType.Flagship) continue;
                yield return build;
            }
        }

        public static IEnumerable<ShipBuild> ValidForEnemy(this IEnumerable<ShipBuild> builds)
        {
            foreach (var build in builds)
            {
                if (build.NotAvailableInGame) continue;
                if (build.Ship.ShipType != ShipType.Common && build.Ship.ShipType != ShipType.Flagship) continue;
                if (build.Ship.ShipRarity == ShipRarity.Unique) continue;
                yield return build;
            }
        }

        public static IEnumerable<ShipBuild> Flagships(this IEnumerable<ShipBuild> builds)
        {
            foreach (var build in builds)
            {
                if (build.NotAvailableInGame) continue;
                if (build.Ship.ShipType != ShipType.Flagship) continue;
                if (build.Ship.ShipRarity == ShipRarity.Unique) continue;
                yield return build;
            }
        }

        public static IEnumerable<ShipBuild> Starbases(this IEnumerable<ShipBuild> builds)
        {
            foreach (var build in builds)
            {
                if (build.NotAvailableInGame) continue;
                if (build.Ship.ShipType != ShipType.Starbase) continue;
                yield return build;
            }
        }

        public static IEnumerable<ShipBuild> Drones(this IEnumerable<ShipBuild> builds)
        {
            foreach (var build in builds)
            {
                if (build.NotAvailableInGame) continue;
                if (build.Ship.ShipType != ShipType.Drone) continue;
                yield return build;
            }
        }

        public static IEnumerable<ShipBuild> BelongToFaction(this IEnumerable<ShipBuild> builds, Faction faction)
        {
            foreach (var build in builds)
            {
                if (build.BuildFaction != Faction.Undefined)
                {
                    if (build.BuildFaction == faction)
                        yield return build;
                }
                else
                {
                    if (build.Ship.Faction == faction)
                        yield return build;
                }
            }
        }

        public static IEnumerable<ShipBuild> LimitFactionByStarLevel(this IEnumerable<ShipBuild> ships, int distance)
        {
            return ships.Where(item => !item.Ship.Faction.NoWanderingShips && item.Ship.Faction.WanderingShipsDistance <= distance);
        }

        public static IEnumerable<ShipBuild> LimitByFactionOrStarLevel(this IEnumerable<ShipBuild> ships, Faction faction, int distance)
        {
            if (faction == Faction.Undefined)
                return ships.LimitFactionByStarLevel(distance);
            else
                return ships.BelongToFaction(faction);
        }

        public static IEnumerable<ShipBuild> WithSizeClass(this IEnumerable<ShipBuild> builds, SizeClass size)
        {
            return builds.Where(item => item.Ship.SizeClass == size);
        }

        public static IEnumerable<ShipBuild> WithSizeClass(this IEnumerable<ShipBuild> builds, SizeClass sizeMin, SizeClass sizeMax)
        {
            return builds.Where(item => item.Ship.SizeClass >= sizeMin && item.Ship.SizeClass <= sizeMax);
        }

        public static IEnumerable<ShipBuild> CommonShips(this IEnumerable<ShipBuild> builds)
        {
            return builds.Where(item => item.Ship.ShipType == ShipType.Common && item.Ship.ShipRarity == ShipRarity.Normal);
        }

        public static IEnumerable<ShipBuild> CommonAndRareShips(this IEnumerable<ShipBuild> builds)
        {
            return builds.Where(item => item.Ship.ShipType == ShipType.Common && 
                (item.Ship.ShipRarity == ShipRarity.Normal || item.Ship.ShipRarity == ShipRarity.Rare));
        }

        public static IEnumerable<ShipBuild> HiddenShips(this IEnumerable<ShipBuild> builds)
        {
            return builds.Where(item => item.Ship.ShipRarity == ShipRarity.Hidden);
        }

        public static IEnumerable<ShipBuild> WithDifficultyClass(this IEnumerable<ShipBuild> ships, DifficultyClass shipClassMin, DifficultyClass shipClassMax)
        {
            return ships.Where(item => item.DifficultyClass <= shipClassMax && item.DifficultyClass >= shipClassMin);
        }

        public static IEnumerable<ShipBuild> Hardcore(this IEnumerable<ShipBuild> ships)
        {
            return ships.Where(item => item.DifficultyClass >= DifficultyClass.Class1);
        }

        public static IEnumerable<ShipBuild> LimitSizeByStarLevel(this IEnumerable<ShipBuild> ships, int distance)
        {
            var maxSize = Utils.Helpers.StarLevelToShipSize(distance);
            return ships.Where(item =>
                item.Ship.Layout.CellCount <=
                Mathf.Max(maxSize, DatabaseStatistics.SmallestShipSize(item.Ship.Faction)));
        }

        public static IEnumerable<ShipBuild> LimitClassByStarLevel(this IEnumerable<ShipBuild> ships, int distance)
        {
            var maxShipClass = Utils.Helpers.StarLevelToMaxDifficulty(distance);
            var minShipClass = Utils.Helpers.StarLevelToMinDifficulty(distance);
            return ships.Where(item => item.DifficultyClass <= maxShipClass && item.DifficultyClass >= minShipClass);
        }

        public static IEnumerable<ShipBuild> WithBestAvailableClass(this IEnumerable<ShipBuild> ships, DifficultyClass shipClassMax)
        {
            var bestClass = DifficultyClass.Default;
            foreach (var ship in ships)
                if (ship.DifficultyClass > bestClass && ship.DifficultyClass <= shipClassMax)
                    bestClass = ship.DifficultyClass;

            return ships.Where(item => item.DifficultyClass == bestClass);
        }

        public static IEnumerable<ShipBuild> LimitByStarLevel(this IEnumerable<ShipBuild> builds, int distance, Faction faction)
        {
            return builds.
                LimitByFactionOrStarLevel(faction, distance).
                LimitSizeByStarLevel(distance).
                LimitClassByStarLevel(distance);
        }
    }
}
