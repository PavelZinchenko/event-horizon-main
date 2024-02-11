using System.Collections.Generic;
using Combat.Component.Unit.Classification;
using Combat.Unit;
using Constructor.Ships;
using GameDatabase;
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
                var shipSpec = playerSkills != null ?
                    ship.CreateBuilder().ApplyPlayerSkills(playerSkills).Build(settings) :
                    ship.CreateBuilder().Build(settings);

                var shipInfo = new ShipInfo(ship, shipSpec, unitSide);
                _ships.Add(shipInfo);
            }
        }

        public int AiLevel { get; private set; }

        public IList<IShipInfo> Ships { get { return _ships.AsReadOnly(); } }

        public long GetExpForAllShips()
        {
            long experience = 0;

            foreach (var item in _ships)
            {
                var model = item.ShipUnit;
                if (model == null)
                    continue;

                var damage = model.State != UnitState.Destroyed ? 1.0f - model.Stats.Armor.Percentage : 1.0f;
                experience += (long)(damage * Maths.Experience.TotalExpForShip(item.ShipData));
            }

            return experience;
        }

        private readonly List<IShipInfo> _ships = new List<IShipInfo>();
    }
}
