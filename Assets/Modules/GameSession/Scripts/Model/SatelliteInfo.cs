using System.Linq;
using System.Collections.Generic;
using GameDatabase;
using GameDatabase.Model;
using GameDatabase.DataModel;
using Constructor.Satellites;
using Session.Utils;

namespace Session.Model
{
	public readonly partial struct SatelliteInfo
	{
		public SatelliteInfo(ISatellite satellite, IDataChangedCallback callback)
		{
			if (satellite == null)
			{
				_id = 0;
				_components = new ObservableList<ShipComponentInfo>(callback);
			}
			else
			{
				_id = satellite.Information.Id.Value;
				_components = new ObservableList<ShipComponentInfo>(satellite.Components.Select(item => new ShipComponentInfo(item)), callback);
			}
		}

		public SatelliteInfo(int id, IEnumerable<ShipComponentInfo> components, IDataChangedCallback callback)
		{
			_id = id;
			_components = new ObservableList<ShipComponentInfo>(components, callback);
		}

		public ISatellite ToSatellite(IDatabase database)
		{
			if (_id == 0) return null;
			var satellite = database.GetSatellite(new ItemId<Satellite>(_id));
			if (satellite == null) return null;
            var components = _components.ToIntegratedComponents(database);
            return new CommonSatellite(satellite, components);
		}
	}
}
