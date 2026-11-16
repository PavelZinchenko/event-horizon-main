using Constructor.Model;
using Services.Localization;

namespace Constructor.Ships.Modification
{
    public class SatelliteSizeMod : IShipModification
    {
        private const int _numberOfSatellites = 2;
        private const float _satelliteSizeIncrease = 0.4f;

        public static bool IsSuitable(IShipModel ship)
        {
            for (int i = 0; i < ship.Satellites.Count; ++i)
            {
                var satellite = ship.Satellites[i];
                if (!satellite.SizeClassModified && satellite.MaxSizeClass < GameDatabase.Enums.SizeClass.Titan)
                    return true;
            }

            return false;
        }

        public ModificationType Type => ModificationType.SatelliteSize;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Ship_SatelliteSizeMod");
        }

        public void Apply(ref ShipBaseStats stats)
        {
            var modified = 0;

            for (int i = 0; i < stats.Satellites.Count; ++i)
            {
                var satellite = stats.Satellites[i];
                if (satellite.SizeClassModified) continue;

                var size = satellite.MaxSizeClass;
                if (size >= GameDatabase.Enums.SizeClass.Titan) continue;

                if (size < GameDatabase.Enums.SizeClass.Destroyer)
                    size = GameDatabase.Enums.SizeClass.Cruiser;
                else
                    size++;

                satellite.SetSizeClass(size);

                if (++modified >= _numberOfSatellites)
                    break;
            }

            // Main only: the legacy model-scale fit check reads this limit.
            stats.MaxSatelliteModelSize += _satelliteSizeIncrease;
        }

        public bool ChangesBarrels => false;
        public bool ChangesSatellites => true;
        public int Seed => 0;
    }
}
