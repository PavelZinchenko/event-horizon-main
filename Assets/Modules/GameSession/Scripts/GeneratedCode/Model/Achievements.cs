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
	public class Achievements : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableSet<int> _gained;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public Achievements(IDataChangedCallback parent)
		{
			_parent = parent;
			_gained = new ObservableSet<int>(this);
		}

		public Achievements(SessionDataReader reader, IDataChangedCallback parent)
		{
			int gainedItemCount;
			gainedItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_gained = new ObservableSet<int>(this);
			for (int i = 0; i < gainedItemCount; ++i)
			{
				int item;
				item = reader.ReadInt(EncodingType.EliasGamma);
				_gained.Add(item);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableSet<int> Gained => _gained;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_gained.Count, EncodingType.EliasGamma);
			foreach (var item in _gained.Items)
			{
				writer.WriteInt(item, EncodingType.EliasGamma);
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
