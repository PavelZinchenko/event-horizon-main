using System;
using System.Collections.Generic;
using System.Linq;
using Constructor.Satellites;
using Constructor.Ships;
using Economy.ItemType;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Maths;

namespace Constructor.Extensions
{
    public static class ShipExtensions
    {
        public static bool IsSuitableSatelliteSize(this IShip ship, Satellite satellite, int mountPoint, ShipSettings settings)
        {
			if (ship.Model.ShipType == ShipType.Starbase) return false;
            var mountPoints = ship.Model.Satellites;
            if (mountPoint < 0 || mountPoint >= mountPoints.Count) return false;
            return satellite.IsSuitable(mountPoints[mountPoint], ship.Model.MaxSatelliteModelSize, settings);
        }

        /// <summary>True when the satellite fits at least one mount point.</summary>
        public static bool IsSuitableSatelliteSize(this IShip ship, Satellite satellite, ShipSettings settings)
        {
            var mountPoints = ship.Model.Satellites;
            for (int i = 0; i < mountPoints.Count; ++i)
                if (ship.IsSuitableSatelliteSize(satellite, i, settings))
                    return true;

            return false;
        }

        public static int Price(this IShip ship)
        {
            return ship.Model.Layout.CellCount*ship.Model.Layout.CellCount;
        }

        public static int Scraps(this IShip ship)
        {
            return ship.Model.Layout.CellCount / 5;
        }

        public static IShip RandomizeColor(this IShip ship, System.Random random)
        {
            ship.ColorScheme.Type = ShipColorScheme.SchemeType.Default;
            ship.ColorScheme.Hue = random.Percentage(50) ? random.NextFloat() * 0.2f : 1.0f - random.NextFloat() * 0.2f;
            ship.ColorScheme.Saturation = random.NextFloat() * 0.1f;
            return ship;
        }

        public static IShip CreateCopy(this IShip ship)
        {
            var copy = new CommonShip(ship.Model, ship.Components)
            {
                Name = ship.Name
            };

            copy.Satellites.CopyFrom(ship.Satellites);
            return copy;
        }

        public static void SetLevel(this IShip ship, int level)
        {
            ship.Experience = Maths.Experience.FromLevel(level);
        }

        public static ItemQuality Quality(this IShipModel shipModel)
        {
            var modsCount = shipModel.Modifications.Count;
            if (modsCount >= 3)
                return ItemQuality.Perfect;
            if (modsCount >= 2)
                return ItemQuality.High;
            if (modsCount >= 1)
                return ItemQuality.Medium;

            return ItemQuality.Common;
        }

        public static IShip Unlocked(this IShip ship)
        {
            foreach (var item in ship.Components)
                item.Locked = false;

            return ship;
        }

        public static IShip OfLevel(this IShip ship, int level)
        {
            ship.Experience = Experience.FromLevel(level);
            return ship;
        }

        public static bool CanBeSold(this IShip ship)
        {
            if (ship.Model.ShipRarity == ShipRarity.Unique) 
                return false;

            return ship.Model.ShipType == ShipType.Common || ship.Model.ShipType == ShipType.Flagship;
        }

        public static IEnumerable<IShip> Create(this IEnumerable<ShipBuild> ships, int requiredLevel, Random random, IDatabase database)
        {
            foreach (var build in ships)
            {
                if (build == null || build.Ship == null)
                {
                    GameDiagnostics.Trace.LogError($"Invalid build: {build?.Id} {build?.Ship?.Id}");
                    continue;
                }

                yield return build.Create(requiredLevel, random, database);
            }
        }

        public static IShip Create(this ShipBuild data, int distance, Random random, IDatabase database)
        {
            var delta = Utilites.MathEx.Min(10, distance / 5);
            var shipLevel = database.GalaxySettings.EnemyLevel(distance + random.Range(-delta, delta));
            var ship = new EnemyShip(data, database) { Experience = Maths.Experience.FromLevel(shipLevel) };

            if (data.Ship.ShipType != ShipType.Common)
                return ship;

            var companionClass = Maths.Distance.CompanionClass(distance);

            var builds = database.SatelliteBuildList.LimitClass(companionClass);

            for (int i = 0; i < ship.Model.Satellites.Count; ++i)
            {
                var companions = builds.SuitableFor(ship, i, database.ShipSettings);
                if (!companions.Any())
                    continue;

                if (ship.Satellites[i] == null && random.Next(3) != 0)
                    ship.Satellites[i] = new CommonSatellite(companions.RandomElement(random));
            }

            return ship;
        }
    }
}
