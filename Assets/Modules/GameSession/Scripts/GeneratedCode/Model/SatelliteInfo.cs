//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session.Model
{
	public readonly partial struct SatelliteInfo
	{
		private readonly int _id;
		private readonly ObservableList<Model.ShipComponentInfo> _components;

		public SatelliteInfo(IDataChangedCallback parent)
		{
			_id = default(int);
			_components = new ObservableList<Model.ShipComponentInfo>(parent);
		}

		public SatelliteInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_id = reader.ReadInt(EncodingType.EliasGamma);
			int componentsItemCount;
			componentsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_components = new ObservableList<Model.ShipComponentInfo>(componentsItemCount, parent);
			for (int i = 0; i < componentsItemCount; ++i)
			{
				Model.ShipComponentInfo item;
				item = new Model.ShipComponentInfo(reader, parent);
				_components.Add(item);
			}
		}

		public int Id => _id;
		public ObservableList<Model.ShipComponentInfo> Components => _components;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_id, EncodingType.EliasGamma);
			writer.WriteInt(_components.Count, EncodingType.EliasGamma);
			for (int i = 0; i < _components.Count; ++i)
			{
				_components[i].Serialize(writer);
			}
		}
	}
}
