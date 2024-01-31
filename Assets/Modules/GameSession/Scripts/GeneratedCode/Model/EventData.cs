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
	public class EventData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableMap<int, int> _completedTime;

		public const int VersionMinor = 0;
		public const int VersionMajor = 2;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public EventData(IDataChangedCallback parent)
		{
			_parent = parent;
			_completedTime = new ObservableMap<int, int>(this);
		}

		public EventData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int completedTimeItemCount;
			completedTimeItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_completedTime = new ObservableMap<int, int>(this);
			for (int i = 0; i < completedTimeItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_completedTime.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, int> CompletedTime => _completedTime;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_completedTime.Count, EncodingType.EliasGamma);
			foreach (var item in _completedTime)
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
