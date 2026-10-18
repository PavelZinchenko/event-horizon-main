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

        /// <summary>
        /// Energy a satellite build adds to a ship, mirroring ShipStatsCalculator.
        /// ShipStatsCalculator multiplies the recharge half by the ship EnergyMultiplier and
        /// leaves the consumption half alone, so a build that is neutral on its own turns
        /// negative on a ship with a negative energy bonus.
        /// </summary>
        /// <returns>True when the build does not drag the ship's energy regen down.</returns>
        public static bool HasNonNegativeEnergyRegen(this SatelliteBuild build, Ship ship)
        {
            var energyMultiplier = 1f + ship.Features.EnergyBonus;

            float recharge = 0;
            float consumption = 0;
            foreach (var item in build.Components)
            {
                var component = item.Component;
                var stats = component.Stats;

                // PerOneCell components contribute once per cell, same as ShipEquipmentStats.
                var multiplier = stats.Type == ComponentStatsType.PerOneCell ? component.Layout.CellCount : 1f;

                if (stats.EnergyRechargeRate > 0)
                    recharge += stats.EnergyRechargeRate * multiplier;
                else
                    consumption -= stats.EnergyRechargeRate * multiplier;
            }

            return recharge * energyMultiplier - consumption >= 0;
        }

        public static IEnumerable<SatelliteBuild> SuitableFor(this IEnumerable<SatelliteBuild> satellites, Ship ship, ShipSettings settings)
        {
            return satellites.Where(item => !item.NotAvailableInGame &&
                                            item.HasNonNegativeEnergyRegen(ship) &&
                                            item.Satellite.IsSuitable(ship.SizeClass, ship.ModelScale, settings));
        }

        public static IEnumerable<SatelliteBuild> LimitClass(this IEnumerable<SatelliteBuild> satellites, DifficultyClass shipClass)
        {
            return satellites.Where(item => item.DifficultyClass <= shipClass);
        }
    }
}
