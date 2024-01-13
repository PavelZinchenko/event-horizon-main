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
	public class QuestData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private ObservableMap<int, int> _factionRelations;
		private ObservableMap<int, int> _characterRelations;
		private ObservableMap<int, QuestStatistics> _statistics;
		private ObservableMap<int, StarQuestMap> _progress;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public QuestData(IDataChangedCallback parent)
		{
			_parent = parent;
			_factionRelations = new ObservableMap<int, int>(this);
			_characterRelations = new ObservableMap<int, int>(this);
			_statistics = new ObservableMap<int, QuestStatistics>(this);
			_progress = new ObservableMap<int, StarQuestMap>(this);
		}

		public QuestData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int factionRelationsItemCount;
			factionRelationsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_factionRelations = new ObservableMap<int, int>(this);
			for (int i = 0; i < factionRelationsItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_factionRelations.Add(key,value);
			}
			int characterRelationsItemCount;
			characterRelationsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_characterRelations = new ObservableMap<int, int>(this);
			for (int i = 0; i < characterRelationsItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_characterRelations.Add(key,value);
			}
			int statisticsItemCount;
			statisticsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_statistics = new ObservableMap<int, QuestStatistics>(this);
			for (int i = 0; i < statisticsItemCount; ++i)
			{
				int key;
				QuestStatistics value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = new QuestStatistics(reader, this);
				_statistics.Add(key,value);
			}
			int progressItemCount;
			progressItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_progress = new ObservableMap<int, StarQuestMap>(this);
			for (int i = 0; i < progressItemCount; ++i)
			{
				int key;
				StarQuestMap value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = new StarQuestMap(reader, this);
				_progress.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, int> FactionRelations => _factionRelations;
		public ObservableMap<int, int> CharacterRelations => _characterRelations;
		public ObservableMap<int, QuestStatistics> Statistics => _statistics;
		public ObservableMap<int, StarQuestMap> Progress => _progress;

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_factionRelations.Count, EncodingType.EliasGamma);
				foreach (var item in _factionRelations.Items)
				{
					writer.WriteInt(item.Key, EncodingType.EliasGamma);
					writer.WriteInt(item.Value, EncodingType.EliasGamma);
				}
				writer.WriteInt(_characterRelations.Count, EncodingType.EliasGamma);
				foreach (var item in _characterRelations.Items)
				{
					writer.WriteInt(item.Key, EncodingType.EliasGamma);
					writer.WriteInt(item.Value, EncodingType.EliasGamma);
				}
				writer.WriteInt(_statistics.Count, EncodingType.EliasGamma);
				foreach (var item in _statistics.Items)
				{
					writer.WriteInt(item.Key, EncodingType.EliasGamma);
					item.Value.Serialize(writer);
				}
				writer.WriteInt(_progress.Count, EncodingType.EliasGamma);
				foreach (var item in _progress.Items)
				{
					writer.WriteInt(item.Key, EncodingType.EliasGamma);
					item.Value.Serialize(writer);
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
