using System.Collections.Generic;
using Combat.Component.Unit.Classification;
using Combat.Unit;
using Constructor;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.DataModel;
using GameServices.Player;

namespace Combat.Domain
{
    public class FleetModel : IFleetModel
    {
        public FleetModel(IEnumerable<IShip> ships, UnitSide unitSide, IDatabase database, int level, PlayerSkills playerSkills = null)
        {
            var settings = database.ShipSettings;
            AiLevel = level;

            foreach (var ship in ships)
            {
                var shipSpec = Build(ship, settings, playerSkills, false);

                // The new simulator gets every satellite mount point (satellite plan, phase 7, step 4).
                var coreSpec = ship.Model.Satellites.Count > ShipBuilder.LegacySatelliteSlots
                    ? Build(ship, settings, playerSkills, true)
                    : shipSpec;

                var shipInfo = new ShipInfo(ship, shipSpec, coreSpec, unitSide);
                _ships.Add(shipInfo);
            }
        }

        private static IShipSpecification Build(IShip ship, ShipSettings settings, PlayerSkills playerSkills, bool allMountPoints)
        {
            var builder = ship.CreateBuilder();
            builder.AllMountPoints = allMountPoints;
            if (playerSkills != null)
                builder.ApplyPlayerSkills(playerSkills);

            return builder.Build(settings);
        }

        public int AiLevel { get; private set; }

        public IList<IShipInfo> Ships { get { return _ships.AsReadOnly(); } }

        public long GetExpForAllShips()
        {
            long experience = 0;

            foreach (var item in _ships)
            {
                var damage = item.DamageFraction;
                if (damage == 0f)
                    continue;

                experience += (long)(damage * Maths.Experience.TotalExpForShip(item.ShipData));
            }

            return experience;
        }

        private readonly List<IShipInfo> _ships = new List<IShipInfo>();
    }
}
