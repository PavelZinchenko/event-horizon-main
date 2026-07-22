using System.Collections.Generic;
using System.Linq;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;
using DatabaseComponent = GameDatabase.DataModel.Component;
using UnityEngine;

namespace Constructor
{
    /// <summary>
    /// Content that is intentionally developer/quest-only.  Keeping the
    /// exclusion in one place prevents a special ThreeBody item from leaking
    /// into a random shop or exploration reward through a new code path.
    /// </summary>
    public static class ThreeBodyContentRules
    {
        public const int CreativeWorkshopComponentId = 936;
        public const int ObserverCoreComponentId = 937;
        public const int ObserverShipId = 167;
        public const int ObserverShipBuildId = 418;

        private const string CreativeWorkshopBuildPreference = "ThreeBody.CreativeWorkshop.BuildId";

        public static bool IsRestrictedComponent(DatabaseComponent component)
        {
            if (component == null) return false;
            switch (component.Id.Value)
            {
                case 295: // 撕裂星辰 (空幻之梦装备)
                case 296: // 零元素装甲
                case 297: // 量子借贷发生器
                case 298: // 曲率引擎
                case 299: // 电子压缩器
                case 311: // 维度跃升装置
                case CreativeWorkshopComponentId:
                case ObserverCoreComponentId:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRestrictedShip(Ship ship)
        {
            if (ship == null) return false;
            switch (ship.Id.Value)
            {
                case 160:    // 空幻之梦
                case 166:    // 水滴
                case ObserverShipId: // 观众
                case 114514: // 三体模组旗舰
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsRestrictedSatellite(Satellite satellite)
        {
            return satellite != null && (satellite.Id.Value == 950 || // 实验性装备搭载平台
                                         satellite.Id.Value == 951);  // 武器测试平台
        }

        public static IReadOnlyList<ShipBuild> GetCreativeWorkshopBuilds(IDatabase database)
        {
            if (database == null)
                return System.Array.Empty<ShipBuild>();

            // The code is deliberately based on the build list, rather than
            // only ships, so alternate enemy/default layouts can be selected
            // as workshop drones as well.
            return database.ShipBuildList
                .Where(item => item != null && item != ShipBuild.DefaultValue && item.Ship != null)
                .OrderBy(item => item.Id.Value)
                .ToArray();
        }

        public static bool TryGetCreativeWorkshopDrone(IDatabase database, int persistedBarrelId, int behaviour, out ShipBuild shipBuild)
        {
            shipBuild = ShipBuild.DefaultValue;
            var code = ((byte)persistedBarrelId << 8) | (byte)behaviour;
            if (code == 0)
                return false;

            var builds = GetCreativeWorkshopBuilds(database);
            var index = code - 1;
            if (index < 0 || index >= builds.Count)
                return false;

            shipBuild = builds[index];
            return shipBuild != null && shipBuild != ShipBuild.DefaultValue;
        }

        public static bool TryEncodeCreativeWorkshopDrone(IDatabase database, ShipBuild shipBuild, out int persistedBarrelId, out int behaviour)
        {
            persistedBarrelId = 0;
            behaviour = 0;
            if (shipBuild == null || shipBuild == ShipBuild.DefaultValue)
                return false;

            var builds = GetCreativeWorkshopBuilds(database);
            var index = -1;
            for (var i = 0; i < builds.Count; ++i)
            {
                if (builds[i].Id == shipBuild.Id)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0 || index >= ushort.MaxValue - 1)
                return false;

            var code = index + 1;
            persistedBarrelId = (sbyte)(code >> 8);
            behaviour = (sbyte)(code & 0xff);
            return true;
        }

        public static bool TryGetSelectedCreativeWorkshopDrone(IDatabase database, out ShipBuild shipBuild)
        {
            shipBuild = ShipBuild.DefaultValue;
            var selectedId = PlayerPrefs.GetInt(CreativeWorkshopBuildPreference, 0);
            if (selectedId <= 0)
                return false;

            shipBuild = database?.GetShipBuild(new ItemId<ShipBuild>(selectedId));
            return shipBuild != null && shipBuild != ShipBuild.DefaultValue;
        }

        public static bool TryGetCreativeWorkshopSelectionSettings(IDatabase database, out int persistedBarrelId, out int behaviour)
        {
            persistedBarrelId = int.MinValue;
            behaviour = 0;
            return TryGetSelectedCreativeWorkshopDrone(database, out var build) &&
                   TryEncodeCreativeWorkshopDrone(database, build, out persistedBarrelId, out behaviour);
        }

        public static void SetSelectedCreativeWorkshopDrone(ShipBuild shipBuild)
        {
            if (shipBuild == null || shipBuild == ShipBuild.DefaultValue)
                return;

            PlayerPrefs.SetInt(CreativeWorkshopBuildPreference, shipBuild.Id.Value);
            PlayerPrefs.Save();
        }
    }
}
