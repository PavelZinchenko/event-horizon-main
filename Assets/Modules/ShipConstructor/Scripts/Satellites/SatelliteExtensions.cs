using System.Collections.Generic;
using System.Linq;
using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Constructor.Satellites
{
    public static class SatelliteDefaults
    {
        /// <summary>
        /// Fallback for call sites that have no database, so the values match the schema defaults.
        /// </summary>
        public static readonly ShipSettings Settings = ShipSettings.Create(
            new GameDatabase.Serializable.ShipSettingsSerializable(), null);
    }

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
            return IsSuitable(satellite, shipClass, shipModelScale, SatelliteDefaults.Settings);
        }

        public static bool IsSuitable(this Satellite satellite, SizeClass shipClass, float shipModelScale, ShipSettings settings)
        {
            bool sizeClassOk = satellite.SizeClass == SizeClass.Undefined || shipClass >= satellite.SizeClass;
            bool modelScaleOk = shipModelScale >= satellite.ModelScale * settings.SatelliteModelScaleMultiplier;

            // A satellite with a SizeClass must satisfy both checks by default, so a small
            // capital cannot carry a large satellite. Turn the setting off to let either
            // check decide.
            return settings.SatelliteRequireModelScaleWithSizeClass
                ? sizeClassOk && modelScaleOk
                : sizeClassOk || modelScaleOk;
        }

        public static bool HasPositiveEnergyRegen(this SatelliteBuild build)
        {
            float total = 0;
            foreach (var item in build.Components)
                total += item.Component.Stats.EnergyRechargeRate;

            return total > 0;
        }

        public static IEnumerable<SatelliteBuild> SuitableFor(this IEnumerable<SatelliteBuild> satellites, Ship ship, ShipSettings settings)
        {
            return satellites.Where(item => !item.NotAvailableInGame &&
                                            item.HasPositiveEnergyRegen() &&
                                            item.Satellite.IsSuitable(ship.SizeClass, ship.ModelScale, settings));
        }

        public static IEnumerable<SatelliteBuild> LimitClass(this IEnumerable<SatelliteBuild> satellites, DifficultyClass shipClass)
        {
            return satellites.Where(item => item.DifficultyClass <= shipClass);
        }
    }
}
