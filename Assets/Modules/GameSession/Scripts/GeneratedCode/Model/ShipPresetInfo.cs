//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Collections.Generic;
using Session.Utils;

namespace Session.Model
{
	public readonly partial struct ShipPresetInfo
	{
		private readonly int _id;
		private readonly string _name;
		private readonly ObservableList<Model.ShipComponentInfo> _components;
		private readonly ObservableList<Model.SatelliteInfo> _satellites;

		public ShipPresetInfo(IDataChangedCallback parent)
		{
			_id = default(int);
			_name = string.Empty;
			_components = new ObservableList<Model.ShipComponentInfo>(parent);
			_satellites = new ObservableList<Model.SatelliteInfo>(parent);
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
			int satellitesItemCount;
			satellitesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_satellites = new ObservableList<Model.SatelliteInfo>(satellitesItemCount, parent);
			for (int i = 0; i < satellitesItemCount; ++i)
			{
				Model.SatelliteInfo item;
				item = new Model.SatelliteInfo(reader, parent);
				_satellites.Add(item);
			}
		}

		public int Id => _id;
		public string Name => _name;
		public ObservableList<Model.ShipComponentInfo> Components => _components;
		public ObservableList<Model.SatelliteInfo> Satellites => _satellites;

        public void Serialize(SessionDataWriter writer)
        {
			writer.WriteInt(_id, EncodingType.EliasGamma);
			writer.WriteString(_name, EncodingType.EliasGamma);
			writer.WriteInt(_components.Count, EncodingType.EliasGamma);
			for (int i = 0; i < _components.Count; ++i)
			{
				_components[i].Serialize(writer);
			}
			writer.WriteInt(_satellites.Count, EncodingType.EliasGamma);
			for (int i = 0; i < _satellites.Count; ++i)
			{
				_satellites[i].Serialize(writer);
			}
        }

        public bool Equals(ShipPresetInfo other)
        {
			if (Id != other.Id) return false;
			if (Name != other.Name) return false;
			if (!Components.Equals(other.Components, new Model.ShipComponentInfo.EqualityComparer())) return false;
			if (!Satellites.Equals(other.Satellites, new Model.SatelliteInfo.EqualityComparer())) return false;
            return true;
        }

        public struct EqualityComparer : IEqualityComparer<ShipPresetInfo>
        {
            public bool Equals(ShipPresetInfo first, ShipPresetInfo second) => first.Equals(second);
            public int GetHashCode(ShipPresetInfo obj) => obj.GetHashCode();
        }
	}
}
