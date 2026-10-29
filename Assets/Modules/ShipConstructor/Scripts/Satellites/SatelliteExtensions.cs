using System.Collections.Generic;
using System.Linq;
using Constructor;
using Constructor.Ships;
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

        public static bool IsSuitable(this Satellite satellite, SizeClass shipClass, float shipModelScale, ShipSettings settings)
        {
            var modelScaleOk = shipModelScale >= satellite.ModelScale * settings.SatelliteModelScaleMultiplier;

            // A satellite without a size class is only limited by its model scale.
            if (satellite.SizeClass == SizeClass.Undefined)
                return modelScaleOk;

            // The size class always has to fit. The model scale only has to fit as well
            // while the setting requires it, so a relaxed mode lets the size class decide
            // on its own.
            return shipClass >= satellite.SizeClass &&
                   (modelScaleOk || !settings.SatelliteRequireModelScaleWithSizeClass);
        }

        /// <summary>
        /// Energy a satellite build adds to a ship, mirroring ShipStatsCalculator.
        /// ShipStatsCalculator multiplies the recharge half by the ship EnergyMultiplier and
        /// leaves the consumption half alone, so a build that is neutral on its own turns
        /// negative on a ship with a negative energy bonus.
        /// </summary>
        public static float GetEnergyRegenContribution(this SatelliteBuild build, int shipSize, float energyMultiplier)
        {
            float recharge = 0;
            float consumption = 0;
            foreach (var item in build.Components)
            {
                // Use the same stats the ship builder uses, so component modifications and
                // the passive costs of devices and drone bays are included.
                var stats = new ComponentInfo(item.Component, item.Modification, item.Quality)
                    .CreateComponent(shipSize).GetStats();

                recharge += stats.EnergyRecharge;
                consumption += stats.EnergyConsumption;
            }

            return recharge * energyMultiplier - consumption;
        }

        /// <summary>
        /// True when the build does not drag the ship's energy regen down.
        /// Reports the first rejection of each build so a modder sees which build is
        /// ignored and why, without one message per enemy ship spawn.
        /// </summary>
        public static bool HasNonNegativeEnergyRegen(this SatelliteBuild build, IShip ship)
        {
            var contribution = build.GetEnergyRegenContribution(
                ship.Model.Layout.CellCount,
                ship.Model.Stats.EnergyMultiplier.Value);

            if (contribution >= 0)
                return true;

            if (_reportedEnergyRejects.Add(build.Id.Value))
                GameDiagnostics.Trace.LogWarning($"Satellite build {build.Id} ignored: energy regen {contribution:F3} on {ship.Name}");

            return false;
        }

        public static IEnumerable<SatelliteBuild> SuitableFor(this IEnumerable<SatelliteBuild> satellites, IShip ship, ShipSettings settings)
        {
            return satellites.Where(item => !item.NotAvailableInGame &&
                                            item.Satellite.IsSuitable(ship.Model.MaxSatelliteSize, ship.Model.MaxSatelliteModelSize, settings) &&
                                            item.HasNonNegativeEnergyRegen(ship))
                             .ToList();
        }

        public static IEnumerable<SatelliteBuild> LimitClass(this IEnumerable<SatelliteBuild> satellites, DifficultyClass shipClass)
        {
            return satellites.Where(item => item.DifficultyClass <= shipClass);
        }

        public static void ReportNegativeEnergyBuilds(this IEnumerable<SatelliteBuild> builds, IEnumerable<IShip> hulls, ShipSettings settings)
        {
            foreach (var build in builds)
            {
                foreach (var hull in hulls)
                {
                    if (!build.Satellite.IsSuitable(hull.Model.MaxSatelliteSize, hull.Model.MaxSatelliteModelSize, settings))
                        continue;

                    var contribution = build.GetEnergyRegenContribution(
                        hull.Model.Layout.CellCount,
                        1f + hull.Model.OriginalShip.Features.EnergyBonus);

                    if (contribution < 0)
                    {
                        GameDiagnostics.Trace.LogWarning($"invalid satellite build: {build.Id} energy regen {contribution:F3} on {hull.Name}");
                        break;
                    }
                }
            }

            _reportedEnergyRejects.Clear();
        }

        private static readonly HashSet<int> _reportedEnergyRejects = new HashSet<int>();
    }
}
