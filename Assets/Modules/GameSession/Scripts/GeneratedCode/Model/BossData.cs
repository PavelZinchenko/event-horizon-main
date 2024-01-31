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
	public class BossData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableMap<int, Model.BossInfo> _bosses;

		public const int VersionMinor = 0;
		public const int VersionMajor = 2;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public BossData(IDataChangedCallback parent)
		{
			_parent = parent;
			_bosses = new ObservableMap<int, Model.BossInfo>(this);
		}

		public BossData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int bossesItemCount;
			bossesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_bosses = new ObservableMap<int, Model.BossInfo>(this);
			for (int i = 0; i < bossesItemCount; ++i)
			{
				int key;
				Model.BossInfo value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = new Model.BossInfo(reader, this);
				_bosses.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, Model.BossInfo> Bosses => _bosses;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_bosses.Count, EncodingType.EliasGamma);
			foreach (var item in _bosses)
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
