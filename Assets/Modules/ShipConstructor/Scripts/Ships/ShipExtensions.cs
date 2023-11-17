using System;
using System.Collections.Generic;
using System.Linq;
using Constructor.Satellites;
using Economy.ItemType;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using Maths;

namespace Constructor.Ships
{
    public static class ShipExtensions
    {
        public static bool IsSuitableSatelliteSize(this IShip ship, Satellite satellite)
        {
            return satellite.IsSuitable(ship.Model.SizeClass, ship.Model.ModelScale);
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
            return new CommonShip(ship.Model, ship.Components)
            {
                FirstSatellite = ship.FirstSatellite.CreateCopy(),
                SecondSatellite = ship.SecondSatellite.CreateCopy(),
                Name = ship.Name
            };
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

        public static IEnumerable<IShip> Create(this IEnumerable<ShipBuild> ships, int requiredLevel, Random random, IDatabase database)
        {
            return ships.Select(item => item.Create(requiredLevel, random, database));
        }

        public static IShip Create(this ShipBuild data, int distance, Random random, IDatabase database)
        {
            var shipLevel = Maths.Distance.ToShipLevel(distance, database.GalaxySettings.MaxEnemyShipsLevel);
            var delta = Math.Min(10, shipLevel / 5);
            shipLevel += random.Next(delta + 1) - delta / 2;
            var ship = new EnemyShip(data) { Experience = Maths.Experience.FromLevel(shipLevel) };

            if (data.Ship.ShipType != ShipType.Common)
                return ship;

            var companionClass = Maths.Distance.CompanionClass(distance);

            var companions = database.SatelliteBuildList.LimitClass(companionClass).SuitableFor(data.Ship);

            if (companions.Any())
            {
                if (random.Next(3) != 0)
                    ship.FirstSatellite = new CommonSatellite(companions.RandomElement(random));
                if (random.Next(3) != 0)
                    ship.SecondSatellite = new CommonSatellite(companions.RandomElement(random));
            }

            return ship;
        }
    }
}
