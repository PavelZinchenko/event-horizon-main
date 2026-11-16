using System.Collections.Generic;
using Session.Utils;

namespace Session.v1
{
	public readonly partial struct ShipInfo
	{
		public ShipInfo(
			int id, 
			string name, 
			long colorScheme, 
			long experience, 
			IEnumerable<Model.ShipComponentInfo> components,
			IEnumerable<byte> layoutModifications,
			IEnumerable<long> modifications,
			Model.SatelliteInfo satellite1,
			Model.SatelliteInfo satellite2,
			IDataChangedCallback callback)
		{
			_id = id;
			_name = name;
			_colorScheme = colorScheme;
			_experience = experience;
			_components = new ObservableList<Model.ShipComponentInfo>(components, callback);
			_layoutModifications = new ObservableList<byte>(layoutModifications, callback);
			_modifications = new ObservableList<long>(modifications, callback);
			_satellite1 = satellite1;
			_satellite2 = satellite2;
		}
	}
}
