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
		private readonly IDataChangedCallback _parent;
		private ObservableMap<int, long> _completedTime;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public EventData(IDataChangedCallback parent)
		{
			_parent = parent;
			_completedTime = new ObservableMap<int, long>(this);
		}

		public EventData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int completedTimeItemCount;
			completedTimeItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_completedTime = new ObservableMap<int, long>(this);
			for (int i = 0; i < completedTimeItemCount; ++i)
			{
				int key;
				long value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadLong(EncodingType.EliasGamma);
				_completedTime.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, long> CompletedTime => _completedTime;

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_completedTime.Count, EncodingType.EliasGamma);
				foreach (var item in _completedTime.Items)
				{
					writer.WriteInt(item.Key, EncodingType.EliasGamma);
					writer.WriteLong(item.Value, EncodingType.EliasGamma);
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
