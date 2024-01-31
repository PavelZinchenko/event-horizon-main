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
	public class RegionData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableMap<int, int> _defeatedFleetCount;
		private ObservableMap<int, int> _factions;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public RegionData(IDataChangedCallback parent)
		{
			_parent = parent;
			_defeatedFleetCount = new ObservableMap<int, int>(this);
			_factions = new ObservableMap<int, int>(this);
		}

		public RegionData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int defeatedFleetCountItemCount;
			defeatedFleetCountItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_defeatedFleetCount = new ObservableMap<int, int>(this);
			for (int i = 0; i < defeatedFleetCountItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_defeatedFleetCount.Add(key,value);
			}
			int factionsItemCount;
			factionsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_factions = new ObservableMap<int, int>(this);
			for (int i = 0; i < factionsItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_factions.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, int> DefeatedFleetCount => _defeatedFleetCount;
		public ObservableMap<int, int> Factions => _factions;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_defeatedFleetCount.Count, EncodingType.EliasGamma);
			foreach (var item in _defeatedFleetCount)
			{
				writer.WriteInt(item.Key, EncodingType.EliasGamma);
				writer.WriteInt(item.Value, EncodingType.EliasGamma);
			}
			writer.WriteInt(_factions.Count, EncodingType.EliasGamma);
			foreach (var item in _factions)
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
