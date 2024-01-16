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
	public class EventData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableMap<int, long> _completedTime;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

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

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
