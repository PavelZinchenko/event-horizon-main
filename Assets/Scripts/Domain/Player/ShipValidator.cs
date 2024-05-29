using System.Collections.Generic;
using Constructor;
using Constructor.Model;
using Constructor.Ships;
using GameDatabase.DataModel;
using ShipEditor.Model;

namespace Domain.Shipyard
{
    public interface IShipPartsStorage
    {
        public void AddComponent(ComponentInfo component);
        public void AddSatellite(Satellite satellite);
        public bool TryGetComponentReplacement(ComponentInfo original, out ComponentInfo replacement);
    }

    public static class ShipValidator
    {
        public static bool HasForbiddenComponents(IShip ship)
        {
            if (HasForbiddenComponents(ship.Components)) return true;
            if (HasForbiddenComponents(ship.FirstSatellite?.Components)) return true;
            if (HasForbiddenComponents(ship.SecondSatellite?.Components)) return true;
            return false;
        }

        public static bool IsLayoutValid(IShip ship)
        {
            var componentTracker = new ComponentTracker(ship);
            if (HasInvalidComponents(CreateShipLayout(ship, componentTracker), ship.Components, componentTracker)) return false;
            if (IsSatelliteInvalid(ship, ship.FirstSatellite, componentTracker)) return false;
            if (IsSatelliteInvalid(ship, ship.SecondSatellite, componentTracker)) return false;
            return true;
        }

        public static void RemoveInvalidParts(IShip ship, IShipPartsStorage storage = null)
        {
            var componentTracker = new ComponentTracker(ship);
            RemoveInvalidComponents(CreateShipLayout(ship, componentTracker), ship.Components, componentTracker, storage);
            ValidateComponentsConfiguration(ship.Components, storage);
            ship.FirstSatellite = ValidateSatellite(ship, ship.FirstSatellite, componentTracker, storage);
            ship.SecondSatellite = ValidateSatellite(ship, ship.SecondSatellite, componentTracker, storage);
        }

        private static void ReturnSatelliteToStorage(Constructor.Satellites.ISatellite satellite, IShipPartsStorage storage)
        {
            if (storage == null || satellite == null) return;
            storage.AddSatellite(satellite.Information);
            foreach (var item in satellite.Components)
                storage.AddComponent(item.Info);
        }

        private static bool IsSatelliteInvalid(IShip ship, Constructor.Satellites.ISatellite satellite, ComponentTracker componentTracker)
        {
            if (satellite == null) return false;
            if (!ship.IsSuitableSatelliteSize(satellite.Information))
            {
                GameDiagnostics.Trace.LogError($"Incompatible satellite found: {satellite.Information.Name}");
                return true;
            }

            var layout = new ShipLayoutModel(ShipElementType.SatelliteL, new ShipLayoutAdapter(satellite.Information.Layout),
                satellite.Information.Barrels, componentTracker);

            return HasInvalidComponents(layout, satellite.Components, componentTracker);
        }

        private static Constructor.Satellites.ISatellite ValidateSatellite(IShip ship, Constructor.Satellites.ISatellite satellite,
            ComponentTracker componentTracker, IShipPartsStorage storage)
        {
            if (satellite == null) return null;
            if (!ship.IsSuitableSatelliteSize(satellite.Information))
            {
                ReturnSatelliteToStorage(satellite, storage);
                return null;
            }

            var layout = new ShipLayoutModel(ShipElementType.SatelliteL, new ShipLayoutAdapter(satellite.Information.Layout),
                satellite.Information.Barrels, componentTracker);

            RemoveInvalidComponents(layout, satellite.Components, componentTracker, storage);
            ValidateComponentsConfiguration(satellite.Components, storage);
            return satellite;
        }

        private static ShipLayoutModel CreateShipLayout(IShip ship, ComponentTracker componentTracker)
        {
            return new ShipLayoutModel(ShipElementType.Ship, ship.Model.Layout, ship.Model.Barrels, componentTracker);
        }

        private static void ValidateComponentsConfiguration(IList<IntegratedComponent> components, IShipPartsStorage storage)
        {
            if (components == null) return;

            int index = 0;
            while (index < components.Count)
            {
                var component = components[index];

                if (!component.Info.IsValidModification)
                {
                    ComponentInfo replacement;
                    if (storage == null)
                    {
                        replacement = new ComponentInfo(component.Info.Data, component.Info.Data.PossibleModifications[0], component.Info.ModificationQuality);
                    }
                    else if (!storage.TryGetComponentReplacement(component.Info, out replacement))
                    {
                        components.QuickRemove(index);
                        continue;
                    }

                    components[index] = new IntegratedComponent(replacement, component.X, component.Y,
                        component.BarrelId, component.KeyBinding, component.Behaviour, component.Locked);

                    GameDiagnostics.Debug.LogError($"Component replaced: {component.Info.Data.Name}");
                }

                component.Locked = component.Info.Data.Availability == GameDatabase.Enums.Availability.Special;
                index++;
            }
        }

        private static bool HasInvalidComponents(ShipLayoutModel layout, IList<IntegratedComponent> components, ComponentTracker componentTracker)
        {
            for (int i = 0; i < components.Count; ++i)
                if (!TryInstallComponent(components[i], layout, componentTracker))
                    return true;

            return false;
        }

        private static void RemoveInvalidComponents(ShipLayoutModel layout, IList<IntegratedComponent> components,
            ComponentTracker componentTracker, IShipPartsStorage storage)
        {
            if (components == null) return;

            int index = 0;
            while (index < components.Count)
            {
                if (!TryInstallComponent(components[index], layout, componentTracker, storage))
                {
                    components.QuickRemove(index);
                    continue;
                }

                index++;
            }
        }

        private static bool TryInstallComponent(IntegratedComponent component, ShipLayoutModel layout, ComponentTracker tracker, IShipPartsStorage storage = null)
        {
            if (!layout.IsSuitableLocation(component.X, component.Y, component.Info.Data) ||
                !tracker.IsCompatible(component.Info.Data))
            {
                GameDiagnostics.Trace.LogError($"Invalid component {component.Info.Data.Name} at [{component.X},{component.Y}]");
                storage?.AddComponent(component.Info);
                return false;
            }

            layout.InstallComponent(component.X, component.Y, component.Info,
                new ComponentSettings(component.KeyBinding, component.Behaviour, component.Locked));

            return true;
        }

        private static bool HasForbiddenComponents(IList<IntegratedComponent> components)
        {
            if (components == null) return false;
            for (int i = 0; i < components.Count; ++i)
            {
                var item = components[i];
                if (item.Info.Data.Availability == GameDatabase.Enums.Availability.None)
                {
                    GameDiagnostics.Trace.LogError($"Found a module that isn't available to players: {item.Info.Data.Name}");
                    return true;
                }

                if (item.Info.Level > 0)
                {
                    GameDiagnostics.Trace.LogError($"Found a module with +{item.Info.Level} upgrade: {item.Info.Data.Name}");
                    return true;
                }

                if (item.Info.Data.Availability == GameDatabase.Enums.Availability.Special && !item.Locked)
                {
                    GameDiagnostics.Trace.LogError($"Found a special module in the wrong place: {item.Info.Data.Name}");
                    return true;
                }
            }

            return false;
        }
    }

    public struct FleetPartsStorage : IShipPartsStorage
    {
        private readonly GameServices.Player.PlayerInventory _inventory;

        public FleetPartsStorage(GameServices.Player.PlayerInventory inventory)
        {
            _inventory = inventory;
        }

        public void AddComponent(ComponentInfo component)
        {
            _inventory.Components.Add(component);
        }

        public void AddSatellite(Satellite satellite)
        {
            _inventory.Satellites.Add(satellite);
        }

        public bool TryGetComponentReplacement(ComponentInfo original, out ComponentInfo replacement)
        {
            throw new System.NotImplementedException();
        }
    }
}
