using System.Collections.Generic;
using System.Linq;
using DatabaseMigration.v1.Enums;
using DatabaseMigration.v1.Serializable;

namespace DatabaseMigration.v1
{
    public partial class DatabaseUpgrader
    {
		partial void Migrate_7_8()
		{
            GameDiagnostics.Trace.LogWarning("Database migration: v1.7 -> v1.8");

            var marketExclusions = new HashSet<int>();
            var ruinsExclusions = new HashSet<int>();

            foreach (var satellite in Content.SatelliteList)
                CollectExclusions(satellite, marketExclusions, ruinsExclusions);

            if (Content.DatabaseSettings == null || !Content.DatabaseSettings.UnloadOriginalDatabase)
                CollectInheritedExclusions(marketExclusions, ruinsExclusions);

            UpsertSettings(ItemType.BlackMarketSettings, marketExclusions);
            UpsertSettings(ItemType.RuinsSettings, ruinsExclusions);

            foreach (var ship in Content.ShipList)
                AddDefaultMountPoints(ship);

            foreach (var build in Content.ShipBuildList)
                ConvertSatelliteBuilds(build);
		}

        // A 1.7 mod cannot ship the new settings records, so the migration always
        // writes them: the mod's own exclusions plus, for a partial mod, the
        // exclusions the built-in satellites had under the old rules.
        private static void CollectExclusions(SatelliteSerializable satellite, HashSet<int> market, HashSet<int> ruins)
        {
            if (satellite.Disabled) return;

            if (satellite.SizeClass == SizeClass.Titan)
            {
                market.Add(satellite.Id);
                ruins.Add(satellite.Id);
            }
            else if (satellite.NotAvailableInBlackMarket)
            {
                market.Add(satellite.Id);
            }
        }

        private void CollectInheritedExclusions(HashSet<int> market, HashSet<int> ruins)
        {
            if (DatabaseMigration.MigrationResources.Serializer == null)
                throw new System.InvalidOperationException("MigrationResources.Serializer is not set, cannot read the built-in catalog");

            var overridden = new HashSet<int>(Content.SatelliteList.Select(item => item.Id));

            foreach (var satellite in BuiltinContent.SatelliteList)
            {
                if (overridden.Contains(satellite.Id)) continue;
                CollectExclusions(satellite, market, ruins);
            }
        }

        private Storage.DatabaseContent BuiltinContent => _builtinContent ??= new Storage.DatabaseContent(
            DatabaseMigration.MigrationResources.Serializer,
            DatabaseMigration.MigrationResources.GetStorage("Database"));

        private Storage.DatabaseContent _builtinContent;

        // Satellite mount points replace the fixed left and right satellite slots. Every
        // ship except a starbase gets the two legacy slots. Main's legacy combat uses only
        // the index, and the new simulator uses the position and the arc. Position
        // (-0.75, +-1.5) * (ship scale + satellite scale) / 2 is close to the legacy position
        // (-0.5 * ship scale, +-(0.5 * ship scale + 1.5 * satellite scale)). The arc is
        // centered on the slot direction (about +-117 degrees), so a limit of 127 degrees
        // keeps the legacy forward edge at -10 degrees and widens the rear edge.
        private static void AddDefaultMountPoints(ShipSerializable ship)
        {
            if (ship.Disabled || ship.ShipType == ShipType.Starbase) return;
            if (ship.Satellites != null && ship.Satellites.Length > 0) return;

            // A hull without a size class (drones) keeps the satellites it could fit before.
            var maxSizeClass = ship.SizeClass == SizeClass.Undefined ? SizeClass.Frigate : ship.SizeClass;

            ship.Satellites = new[]
            {
                CreateMountPoint(1, maxSizeClass),
                CreateMountPoint(-1, maxSizeClass),
            };
        }

        private static SatelliteMountPointSerializable CreateMountPoint(float side, SizeClass maxSizeClass)
        {
            return new SatelliteMountPointSerializable
            {
                Position = new GameDatabase.Model.Vector2Data(DefaultMountPointX, side * DefaultMountPointY),
                Direction = 0,
                RotationLimit = DefaultRotationLimit,
                MinSizeClass = SizeClass.Undefined,
                MaxSizeClass = maxSizeClass,
            };
        }

        // The left build becomes index 0 and the right build index 1. A reference list
        // cannot hold an empty slot or a missing build (the loader throws), so empty slots
        // and builds that are missing or disabled are dropped. A lone right build moves to
        // index 0.
        private void ConvertSatelliteBuilds(ShipBuildSerializable build)
        {
            if (build.Satellites != null && build.Satellites.Length > 0) return;

            var satellites = new List<int>();
            foreach (var id in new[] { build.LeftSatelliteBuild, build.RightSatelliteBuild })
            {
                if (id <= 0) continue;
                if (SatelliteBuildExists(id))
                    satellites.Add(id);
                else
                    GameDiagnostics.Trace.LogWarning($"Database migration: ship build {build.Id} refers to missing satellite build {id}, removed");
            }

            build.Satellites = satellites.ToArray();
            build.LeftSatelliteBuild = 0;
            build.RightSatelliteBuild = 0;
        }

        private bool SatelliteBuildExists(int id)
        {
            var own = Content.SatelliteBuildList.FirstOrDefault(item => item.Id == id);
            if (own != null) return !own.Disabled;
            if (Content.DatabaseSettings != null && Content.DatabaseSettings.UnloadOriginalDatabase) return false;
            return BuiltinContent.SatelliteBuildList.Any(item => item.Id == id && !item.Disabled);
        }

        private const float DefaultMountPointX = -0.75f;
        private const float DefaultMountPointY = 1.5f;
        private const float DefaultRotationLimit = 127f;

        private void UpsertSettings(ItemType type, IEnumerable<int> satellites)
        {
            var ids = string.Join(",", satellites.OrderBy(item => item));
            var json = "{\"ItemType\":" + (int)type + ",\"ExcludedSatellites\":[" + ids + "]}";
            Content.LoadJson(type + ".json", json);
        }
    }
}
