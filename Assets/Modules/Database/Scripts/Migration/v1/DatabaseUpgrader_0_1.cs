using System.Linq;
using DatabaseMigration.v1.Serializable;

namespace DatabaseMigration.v1
{
    public partial class DatabaseUpgrader
    {
        partial void Migrate_0_1()
        {
            UnityEngine.Debug.LogWarning("Database migration: v1.0 -> v1.1");

            UpdateBarrels();
            UpdateComponentStats();
            UpdateFactions();
            UpdateShips();
            UpdateBuilds();
        }

        private void UpdateBuilds()
        {
            foreach (var build in Content.ShipBuildList)
                if (build.BuildFaction < 0) build.BuildFaction = 0;
        }

        private void UpdateShips()
        {
            foreach (var ship in Content.ShipList)
            {
                if (ship.EngineSize > 0)
                {
                    var engine = new EngineSerializable { Position = ship.EnginePosition, Size = ship.EngineSize };
                    if (ship.Engines == null)
                        ship.Engines = new EngineSerializable[] { engine };
                    else
                    {
                        var length = ship.Engines.Length;
                        System.Array.Resize(ref ship.Engines, length + 1);
                        ship.Engines[length] = engine;
                    }
                }

                switch (ship.ShipCategory)
                {
                    case 1: // Rare
                        ship.ShipRarity = Enums.ShipRarity.Rare;
                        break;
                    case 2: // Flagship
                        ship.ShipType = Enums.ShipType.Flagship;
                        break;
                    case 3: // Special
                        ship.ShipType = Enums.ShipType.Special;
                        break;
                    case 4: // Starbase
                        ship.ShipType = Enums.ShipType.Starbase;
                        ship.SizeClass = Enums.SizeClass.Starbase;
                        break;
                    case 5: // Hidden
                        ship.ShipRarity = Enums.ShipRarity.Hidden;
                        break;
                    case 6: // Drone
                        ship.ShipType = Enums.ShipType.Drone;
                        break;
                }

                if (ship.EnergyResistance != 0 ||
                    ship.HeatResistance != 0 ||
                    ship.KineticResistance != 0 ||
                    ship.BaseWeightModifier != 0 ||
                    ship.Regeneration ||
                    (ship.BuiltinDevices != null && ship.BuiltinDevices.Length > 0))
                {
                    ship.Features = new ShipFeaturesSerializable
                    {
                        HeatResistance = ship.HeatResistance,
                        EnergyResistance = ship.EnergyResistance,
                        KineticResistance = ship.KineticResistance,
                        ShipWeightBonus = ship.BaseWeightModifier,
                        Regeneration = ship.Regeneration,
                        BuiltinDevices = ship.BuiltinDevices
                    };
                }
            }
        }

        private void UpdateFactions()
        {
            foreach (var faction in Content.FactionList)
            {
                if (faction.Hidden)
                {
                    faction.NoTerritories = true;
                    faction.NoWanderingShips = true;
                    faction.HideFromMerchants = true;
                    faction.HideResearchTree = true;
                }

                if (faction.Hostile)
                {
                    faction.NoMissions = true;
                }
            }
        }

        private void UpdateBarrels()
        {
            foreach (var ship in Content.ShipList)
                ship.Barrels = ship.Barrels?.Select(ProcessBarrel).ToArray();
            foreach (var satellite in Content.SatelliteList)
                satellite.Barrels = satellite.Barrels?.Select(ProcessBarrel).ToArray();
        }

        private void UpdateComponentStats()
        {
            foreach (var stats in Content.ComponentStatsList)
                if (stats.AlterWeaponPlatform > 0)
                    stats.AutoAimingArc = PlatformTypeToAngle(stats.AlterWeaponPlatform);

        }

        private static BarrelSerializable ProcessBarrel(BarrelSerializable barrel)
        {
            if (barrel.PlatformType > 0)
                barrel.AutoAimingArc = PlatformTypeToAngle(barrel.PlatformType);

            return barrel;
        }

        private static float PlatformTypeToAngle(int platformType)
        {
            switch (platformType)
            {
                case 1: //AutoTarget
                    return 360;
                case 2:// AutoTargetFrontal
                    return 80;
                case 3:// TargetingUnit
                    return 20;
                default:
                    return 0;
            }
        }
    }
}
