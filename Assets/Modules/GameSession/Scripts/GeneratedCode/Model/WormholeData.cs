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
	public class WormholeData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private ObservableMap<int, int> _routes;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public WormholeData(IDataChangedCallback parent)
		{
			_parent = parent;
			_routes = new ObservableMap<int, int>(this);
		}

		public WormholeData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int routesItemCount;
			routesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_routes = new ObservableMap<int, int>(this);
			for (int i = 0; i < routesItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_routes.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, int> Routes => _routes;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_routes.Count, EncodingType.EliasGamma);
			foreach (var item in _routes.Items)
			{
				writer.WriteInt(item.Key, EncodingType.EliasGamma);
				writer.WriteInt(item.Value, EncodingType.EliasGamma);
			}
			DataChanged = false;
		}

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
