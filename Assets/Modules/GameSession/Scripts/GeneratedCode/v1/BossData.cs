//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session.v1
{
	public class BossData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableMap<int, v1.BossInfo> _bosses;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public BossData(IDataChangedCallback parent)
		{
			_parent = parent;
			_bosses = new ObservableMap<int, v1.BossInfo>(this);
		}

		public BossData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int bossesItemCount;
			bossesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_bosses = new ObservableMap<int, v1.BossInfo>(this);
			for (int i = 0; i < bossesItemCount; ++i)
			{
				int key;
				v1.BossInfo value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = new v1.BossInfo(reader, this);
				_bosses.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, v1.BossInfo> Bosses => _bosses;

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
