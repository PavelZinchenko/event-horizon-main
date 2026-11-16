//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Collections.Generic;
using Session.Utils;

namespace Session.v3
{
	public readonly partial struct ShipPresetInfo
	{
		private readonly int _id;
		private readonly string _name;
		private readonly ObservableList<Model.ShipComponentInfo> _components;
		private readonly Model.SatelliteInfo _satellite1;
		private readonly Model.SatelliteInfo _satellite2;

		public ShipPresetInfo(IDataChangedCallback parent)
		{
			_id = default(int);
			_name = string.Empty;
			_components = new ObservableList<Model.ShipComponentInfo>(parent);
			_satellite1 = new Model.SatelliteInfo(parent);
			_satellite2 = new Model.SatelliteInfo(parent);
		}

		public ShipPresetInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_id = reader.ReadInt(EncodingType.EliasGamma);
			_name = reader.ReadString(EncodingType.EliasGamma);
			int componentsItemCount;
			componentsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_components = new ObservableList<Model.ShipComponentInfo>(componentsItemCount, parent);
			for (int i = 0; i < componentsItemCount; ++i)
			{
				Model.ShipComponentInfo item;
				item = new Model.ShipComponentInfo(reader, parent);
				_components.Add(item);
			}
			_satellite1 = new Model.SatelliteInfo(reader, parent);
			_satellite2 = new Model.SatelliteInfo(reader, parent);
		}

		public int Id => _id;
		public string Name => _name;
		public ObservableList<Model.ShipComponentInfo> Components => _components;
		public Model.SatelliteInfo Satellite1 => _satellite1;
		public Model.SatelliteInfo Satellite2 => _satellite2;
	}
}
