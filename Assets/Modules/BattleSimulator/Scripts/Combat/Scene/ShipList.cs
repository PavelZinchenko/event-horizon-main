using System.Collections.Generic;
using Combat.Component.Ship;

namespace Combat.Scene
{
    public class ShipList : IUnitList<IShip>
    {
        public ShipList()
        {
            _lockObject = new object();
            _ships = new List<IShip>();
        }

        public IReadOnlyList<IShip> Items => _ships;
        public object LockObject => _lockObject;

        public void Add(IShip ship)
        {
            lock (_lockObject)
            {
                _ships.Add(ship);
            }
        }

        public void Remove(IShip ship)
        {
            lock (_lockObject)
            {
                var index = _ships.IndexOf(ship);
                _ships.QuickRemove(index);
            }
        }

        public void Clear()
        {
            lock (LockObject)
            {
                _ships.Clear();
            }
        }

        private readonly object _lockObject;
        private readonly List<IShip> _ships;
    }
}
