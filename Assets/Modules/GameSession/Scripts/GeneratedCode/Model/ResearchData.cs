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
	public class ResearchData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private ObservableSet<int> _technologies;
		private ObservableMap<int, int> _researchPoints;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public ResearchData(IDataChangedCallback parent)
		{
			_parent = parent;
			_technologies = new ObservableSet<int>(this);
			_researchPoints = new ObservableMap<int, int>(this);
		}

		public ResearchData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int technologiesItemCount;
			technologiesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_technologies = new ObservableSet<int>(this);
			for (int i = 0; i < technologiesItemCount; ++i)
			{
				int item;
				item = reader.ReadInt(EncodingType.EliasGamma);
				_technologies.Add(item);
			}
			int researchPointsItemCount;
			researchPointsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_researchPoints = new ObservableMap<int, int>(this);
			for (int i = 0; i < researchPointsItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_researchPoints.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableSet<int> Technologies => _technologies;
		public ObservableMap<int, int> ResearchPoints => _researchPoints;

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_technologies.Count, EncodingType.EliasGamma);
				foreach (var item in _technologies.Items)
				{
					writer.WriteInt(item, EncodingType.EliasGamma);
				}
				writer.WriteInt(_researchPoints.Count, EncodingType.EliasGamma);
				foreach (var item in _researchPoints.Items)
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
