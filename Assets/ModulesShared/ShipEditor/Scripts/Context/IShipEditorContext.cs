using System.Collections.Generic;
using Constructor;
using Constructor.Ships;
using Constructor.Satellites;
using GameDatabase.DataModel;

namespace ShipEditor.Context
{
    public interface IShipEditorContext
	{
		IShip Ship { get; }
		IInventoryProvider Inventory { get; }
        IShipDataProvider ShipDataProvider { get; }
        IShipPresetStorage ShipPresetStorage { get; }
        IComponentUpgradesProvider UpgradesProvider { get; }
        bool CanBeUnlocked(Component component);
        bool IsShipNameEditable { get; }
    }

    public interface IInventoryProvider 
	{
		IEnumerable<IShip> Ships { get; }
		IReadOnlyCollection<ISatellite> SatelliteBuilds { get; }

        IReadOnlyCollection<Satellite> Satellites { get; }
        int GetQuantity(Satellite satellite);
        void AddSatellite(Satellite satellite);
        bool TryRemoveSatellite(Satellite satellite);

        IReadOnlyCollection<ComponentInfo> Components { get; }
        int GetQuantity(ComponentInfo component);
        void AddComponent(ComponentInfo component);
		bool TryRemoveComponent(ComponentInfo component);

		public Economy.Price GetUnlockPrice(ComponentInfo component);
		bool TryPayForUnlock(ComponentInfo component);
	}

    public interface IShipPresetStorage
    {
        IEnumerable<IShipPreset> GetPresets(Ship ship);
        IShipPreset Create(Ship ship);
        void Update(IShipPreset preset);
        void Delete(IShipPreset preset);
    }
}
