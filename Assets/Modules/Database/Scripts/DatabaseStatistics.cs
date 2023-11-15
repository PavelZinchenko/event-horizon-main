using System.Collections.Generic;
using GameDatabase.DataModel;
using System;

namespace GameDatabase
{
    public class DatabaseStatistics : IDisposable
    {
        public DatabaseStatistics(IDatabase database)
        {
            _database = database;
            _database.DatabaseLoaded += OnDatabaseLoaded;

            UpdateStatistics();
        }

        public void Dispose()
        {
            _database.DatabaseLoaded -= OnDatabaseLoaded;
        }

        public static int SmallestShipSize(Faction faction)
        {
            int size;
            return _smallestFactionShips.TryGetValue(faction, out size) ? size : 0;
        }

        private void OnDatabaseLoaded()
        {
            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            _smallestFactionShips.Clear();
            foreach (var ship in _database.ShipList)
            {
                var faction = ship.Faction;
                var shipSize = ship.Layout.CellCount;

                int size;
                if (_smallestFactionShips.TryGetValue(faction, out size) && size < shipSize)
                    continue;

                _smallestFactionShips[faction] = shipSize;
            }
        }

        private readonly IDatabase _database;

        private static readonly Dictionary<Faction, int> _smallestFactionShips = new Dictionary<Faction, int>();
    }
}
