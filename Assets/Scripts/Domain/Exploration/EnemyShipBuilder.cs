using System.Linq;
using Constructor.Satellites;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using UnityEngine;

namespace Game.Exploration
{
    public class EnemyShipBuilder : IEnemyShipBuilder
    {
        public EnemyShipBuilder(ItemId<ShipBuild> id, IDatabase database, int level, int seed, bool randomizeColor = false, bool allowSatellites = true)
        {
            _database = database;
            _shipId = id.Value;
            _level = level;
            _seed = seed;
            _allowSatellites = allowSatellites;
            _randomizeColor = randomizeColor;
        }

        public Combat.Component.Ship.Ship Build(Combat.Factory.ShipFactory shipFactory, Combat.Factory.SpaceObjectFactory objectFactory, Vector2 position, float rotation)
        {
            var spec = CreateSpecification();
            var ship = shipFactory.CreateEnemyShip(spec, position, rotation, Maths.Distance.AiLevel(_level));
            return ship;
        }

        public Constructor.IShipSpecification CreateSpecification()
        {
            var model = CreateShip();
            return model.CreateBuilder().Build(_database.ShipSettings);
        }

        private IShip CreateShip()
        {
            var random = new System.Random(_seed);

            var build = _database.GetShipBuild(new ItemId<ShipBuild>(_shipId));
            var ship = new EnemyShip(build, _database);

            var shipLevel = _database.GalaxySettings.EnemyLevel(_level);
            shipLevel -= random.Next(shipLevel/3);
            ship.Experience = Maths.Experience.FromLevel(shipLevel);

            var satelliteClass = Maths.Distance.MaxShipClass(_level);
            if (_allowSatellites && ship.Model.ShipType == ShipType.Common && satelliteClass != DifficultyClass.Default)
            {
                for (int i = 0; i < ship.Model.Satellites.Count; ++i)
                {
                    if (ship.Satellites[i] != null || random.Next(3) == 0) continue;

                    var satellites = _database.SatelliteBuildList.LimitClass(satelliteClass).SuitableFor(ship, i, _database.ShipSettings);
                    if (satellites.Any())
                        ship.Satellites[i] = new CommonSatellite(satellites.RandomElement(random));
                }
            }

            if (_randomizeColor)
            {
                ship.ColorScheme.Type = ShipColorScheme.SchemeType.Hsv;
                ship.ColorScheme.Hue = random.NextFloat();
            }

            return ship;
        }

        private readonly bool _randomizeColor;
        private readonly int _seed;
        private readonly int _shipId;
        private readonly int _level;
        private readonly bool _allowSatellites;
        private readonly IDatabase _database;
    }
}
