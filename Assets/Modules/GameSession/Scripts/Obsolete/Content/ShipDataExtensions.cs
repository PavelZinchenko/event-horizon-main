using System;
using System.Collections.Generic;
using System.Linq;
using GameDatabase;
using GameDatabase.Enums;
using GameDatabase.Model;
using GameDatabase.DataModel;
using Constructor;
using Constructor.Ships;
using Constructor.Satellites;
using Constructor.Ships.Modification;

namespace Session.ContentObsolete
{
    public static class ShipDataExtensions
    {
        public static IEnumerable<IntegratedComponent> FromShipComponentsData(this ShipComponentsData data, IDatabase database)
        {
            foreach (var item in data.Components)
            {
                var component = database.GetComponent(new ItemId<Component>(item.Id));
                if (component == null)
                {
                    UnityEngine.Debug.LogException(new ArgumentException("Unknown component - " + item.Id));
                    continue;
                }

                var info = new ComponentInfo(component, database.GetComponentMod(new(item.Modification)), (ModificationQuality)item.Quality, item.UpgradeLevel);
                var x = item.X > -component.Layout.Size ? item.X : 256 + item.X;
                var y = item.Y > -component.Layout.Size ? item.Y : 256 + item.Y;
                yield return new IntegratedComponent(info, x, y, item.BarrelId, item.KeyBinding, item.Behaviour, item.Locked);
            }
        }

        public static ShipComponentsData ToShipComponentsData(this IEnumerable<IntegratedComponent> components)
        {
            var data = new ShipComponentsData
            {
                Components = components.Select<IntegratedComponent, ShipComponentsData.Component>(item => new ShipComponentsData.Component
                {
                    Id = item.Info.Data.Id.Value,
                    Quality = (int)item.Info.ModificationQuality,
                    Modification = item.Info.ModificationType.Id.Value,
                    UpgradeLevel = 0,
                    X = item.X,
                    Y = item.Y,
                    BarrelId = item.BarrelId,
                    KeyBinding = item.KeyBinding,
                    Behaviour = item.Behaviour,
                    Locked = item.Locked,
                })
            };

            return data;
        }

        public static ISatellite FromSatelliteData(IDatabase database, SatelliteData data)
        {
            var satellite = database.GetSatellite(new ItemId<Satellite>(data.Id));
            if (satellite == null)
                return null;

            return new CommonSatellite(satellite, data.Components.FromShipComponentsData(database));
        }

        public static SatelliteData ToSatelliteData(this ISatellite satellite)
        {
            if (satellite == null)
                return new SatelliteData { Id = 0, Components = new ShipComponentsData() };

            var info = new SatelliteData
            {
                Id = satellite.Information.Id.Value,
                Components = satellite.Components.ToShipComponentsData()
            };

            return info;
        }

        #region Obsolete
        public static ISatellite FromSatelliteInfo(IDatabase database, FleetData.SatelliteInfoV2 info)
        {
            if (string.IsNullOrEmpty(info.Id))
                return null;

            int id;
            return new CommonSatellite(database.GetSatellite(int.TryParse(info.Id, out id) ? new ItemId<Satellite>(id) : Database.Legacy.LegacySatelliteNames.GetId(info.Id)),
                info.Components.Select(item => ComponentExtensions.Deserialize(database, item)));
        }

        public static ISatellite FromSatelliteInfoObsolete(IDatabase database, FleetData.SatelliteInfoV2 info)
        {
            if (string.IsNullOrEmpty(info.Id))
                return null;

            int id;
            return new CommonSatellite(database.GetSatellite(int.TryParse(info.Id, out id) ? new ItemId<Satellite>(id) : Database.Legacy.LegacySatelliteNames.GetId(info.Id)),
                info.Components.Select(item => ComponentExtensions.DeserializeObsolete(database, item)));
        }
        #endregion

        public static IShip FromShipData(IDatabase database, ShipData shipData)
        {
            var shipWrapper = database.GetShip(new ItemId<Ship>(shipData.Id));
            if (shipWrapper == null)
                return null;

            var shipModel = new ShipModel(shipWrapper, database);
            var factory = new ModificationFactory(database);
            shipModel.Modifications.Assign(shipData.Modifications.Modifications.Select(item => ShipModificationExtensions.Deserialize(item, factory)));
            shipModel.LayoutModifications.Deserialize(shipData.Modifications.Layout.ToArray());
            var components = shipData.Components.FromShipComponentsData(database);
            var ship = new CommonShip(shipModel, components);

            ship.FirstSatellite = FromSatelliteData(database, shipData.Satellite1);
            ship.SecondSatellite = FromSatelliteData(database, shipData.Satellite2);
            ship.Name = shipData.Name;
            ship.ColorScheme.Value = shipData.ColorScheme;
            ship.Experience = (long)shipData.Experience;
            ship.DataChanged = false;
            return ship;
        }

        public static ShipData ToShipData(this IShip ship)
        {
            return new ShipData
            {
                Id = ship.Id.Value,
                Name = ship.Name,
                ColorScheme = ship.ColorScheme.Value,
                Experience = (long)ship.Experience,
                Components = ship.Components.ToShipComponentsData(),
                Modifications = new ShipModificationsData
                {
                    Layout = ship.Model.LayoutModifications.Serialize(),
                    Modifications = ship.Model.Modifications.Select<IShipModification, long>(ShipModificationExtensions.Serialize)
                },
                Satellite1 = ship.FirstSatellite.ToSatelliteData(),
                Satellite2 = ship.SecondSatellite.ToSatelliteData(),
            };
        }

        #region Obsolete

        public static IShip FromShipInfoObsolete(IDatabase database, FleetData.ShipInfoObsolete shipInfo)
        {
            Ship baseShip;

            int shipId;
            if (int.TryParse(shipInfo.Id, out shipId))
                baseShip = database.GetShip(new ItemId<Ship>(shipId));
            else
                baseShip = database.GetShip(Database.Legacy.LegacyShipNames.GetId(shipInfo.Id));

            if (baseShip == null)
                return null;

            var shipModel = new ShipModel(baseShip, database);

            var factory = new ModificationFactory(database);
            shipModel.Modifications.Assign(shipInfo.Modifications.Select(item => ShipModificationExtensions.Deserialize(item, factory)));

            var components = shipInfo.Components.Select(item => ComponentExtensions.Deserialize(database, item));
            var ship = new CommonShip(shipModel, components);

            ship.FirstSatellite = FromSatelliteInfo(database, shipInfo.FirstSatellite);
            ship.SecondSatellite = FromSatelliteInfo(database, shipInfo.SecondSatellite);
            ship.Name = shipInfo.Name;
            ship.Experience = (long)shipInfo.Experience;
            ship.DataChanged = false;
            return ship;
        }

        #endregion
    }
}
