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
	public readonly partial struct StarQuestMap
	{
		private readonly ObservableMap<int, Model.QuestProgress> _starQuestsMap;

		public StarQuestMap(IDataChangedCallback parent)
		{
			_starQuestsMap = new ObservableMap<int, Model.QuestProgress>(parent);
		}

		public StarQuestMap(SessionDataReader reader, IDataChangedCallback parent)
		{
			int starQuestsMapItemCount;
			starQuestsMapItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_starQuestsMap = new ObservableMap<int, Model.QuestProgress>(parent);
			for (int i = 0; i < starQuestsMapItemCount; ++i)
			{
				int key;
				Model.QuestProgress value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = new Model.QuestProgress(reader, parent);
				_starQuestsMap.Add(key,value);
			}
		}

		public ObservableMap<int, Model.QuestProgress> StarQuestsMap => _starQuestsMap;

        public void Serialize(SessionDataWriter writer)
        {
			writer.WriteInt(_starQuestsMap.Count, EncodingType.EliasGamma);
			foreach (var item in _starQuestsMap)
			{
				writer.WriteInt(item.Key, EncodingType.EliasGamma);
				item.Value.Serialize(writer);
			}
        }

        public bool Equals(StarQuestMap other)
        {
			if (!StarQuestsMap.Equals(other.StarQuestsMap, new Model.QuestProgress.EqualityComparer())) return false;
            return true;
        }

        public struct EqualityComparer : IEqualityComparer<StarQuestMap>
        {
            public bool Equals(StarQuestMap first, StarQuestMap second) => first.Equals(second);
            public int GetHashCode(StarQuestMap obj) => obj.GetHashCode();
        }
	}
}
