using System.Collections.Generic;
using System.Linq;
using Constructor.Ships;
using Model.Military;

namespace GameServices.Multiplayer
{
    public sealed class NetworkFleet : IFleet
    {
        public NetworkFleet(IEnumerable<IShip> ships)
        {
            _ships = ships?.Where(item => item != null).ToArray() ?? new IShip[0];
        }

        public IEnumerable<IShip> Ships => _ships;
        public int AiLevel => 100;
        public float Power => _ships.Sum(Maths.Threat.GetShipPower);

        private readonly IShip[] _ships;
    }
}
