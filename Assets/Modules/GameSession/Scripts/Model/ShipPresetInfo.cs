using System.Linq;
using GameDatabase;
using GameDatabase.Model;
using GameDatabase.DataModel;
using Constructor.Ships;
using Session.Utils;

namespace Session.Model
{
	public readonly partial struct ShipPresetInfo
	{
		public ShipPresetInfo(IShipPreset preset, IDataChangedCallback callback)
		{
			_id = preset.Ship.Id.Value;
			_name = preset.Name;
			_components = new ObservableList<ShipComponentInfo>(preset.Components.Select(item => new ShipComponentInfo(item)), callback);
			_satellite1 = new SatelliteInfo(preset.FirstSatellite, callback);
			_satellite2 = new SatelliteInfo(preset.SecondSatellite, callback);
		}

		public IShipPreset ToShipPreset(IDatabase database)
		{
			var shipData = database.GetShip(new ItemId<Ship>(_id));
			if (shipData == null)
				return null;

			var components = _components.ToIntegratedComponents(database);
			var preset = new ShipPreset(shipData);
            preset.Components.Assign(components);
			preset.FirstSatellite = _satellite1.ToSatellite(database);
			preset.SecondSatellite = _satellite2.ToSatellite(database);
			preset.Name = _name;
			return preset;
		}
	}
}
