using System.Linq;
using System.Collections.Generic;
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
			_satellites = new ObservableList<SatelliteInfo>(preset.Satellites.Select(item => new SatelliteInfo(item, callback)), callback);
		}

		public ShipPresetInfo(
			int id,
			string name,
			IEnumerable<ShipComponentInfo> components,
			IEnumerable<SatelliteInfo> satellites,
			IDataChangedCallback callback)
		{
			_id = id;
			_name = name;
			_components = new ObservableList<ShipComponentInfo>(components, callback);
			_satellites = new ObservableList<SatelliteInfo>(satellites, callback);
		}

		public IShipPreset ToShipPreset(IDatabase database)
		{
			var shipData = database.GetShip(new ItemId<Ship>(_id));
			if (shipData == null)
				return null;

			var components = _components.ToIntegratedComponents(database);
			var preset = new ShipPreset(shipData);
            preset.Components.Assign(components);
			for (int i = 0; i < _satellites.Count; ++i)
				preset.Satellites[i] = _satellites[i].ToSatellite(database);

			preset.Name = _name;
			return preset;
		}
	}
}
