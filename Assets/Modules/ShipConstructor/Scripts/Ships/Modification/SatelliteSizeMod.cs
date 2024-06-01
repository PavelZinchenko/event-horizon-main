using System.Linq;
using Constructor.Model;
using GameDatabase.DataModel;
using Services.Localization;

namespace Constructor.Ships.Modification
{
    public class SatelliteSizeMod : IShipModification
    {
        public static bool IsSuitable(Ship ship)
        {
            return ship.SizeClass < GameDatabase.Enums.SizeClass.Titan;
        }

        public ModificationType Type => ModificationType.SatelliteSize;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Ship_SatelliteSizeMod");
        }

        public void Apply(ref ShipBaseStats stats)
        {
            if (stats.MaxSatelliteSize <= GameDatabase.Enums.SizeClass.Destroyer)
                stats.MaxSatelliteSize = GameDatabase.Enums.SizeClass.Cruiser;
            else
                stats.MaxSatelliteSize++;

            stats.MaxSatelliteModelSize += _satelliteSizeIncrease;
        }

        public bool ChangesBarrels => false;
        public int Seed => 0;

        private const float _satelliteSizeIncrease = 0.4f;
    }
}
