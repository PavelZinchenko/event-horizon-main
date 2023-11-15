using System.Collections.Generic;
using System.Linq;
using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Constructor.Satellites
{
    public static class SatelliteExtensions
    {
        public static ISatellite CreateCopy(this ISatellite satellite)
        {
            if (satellite == null)
                return null;

            return new CommonSatellite(satellite.Information, satellite.Components);
        }

        public static bool IsSuitable(this Satellite satellite, SizeClass shipClass, float shipModelScale)
        {
            if (satellite.SizeClass != SizeClass.Undefined)
                return shipClass >= satellite.SizeClass;

            return shipModelScale >= satellite.ModelScale * 2;
        }

        public static IEnumerable<SatelliteBuild> SuitableFor(this IEnumerable<SatelliteBuild> satellites, Ship ship)
        {
            return satellites.Where(item => item.Satellite.IsSuitable(ship.SizeClass, ship.ModelScale));
        }

        public static IEnumerable<SatelliteBuild> LimitClass(this IEnumerable<SatelliteBuild> satellites, DifficultyClass shipClass)
        {
            return satellites.Where(item => item.DifficultyClass <= shipClass);
        }
    }
}
