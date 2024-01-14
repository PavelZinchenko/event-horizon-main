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
	public class CommonObjectData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableMap<int, int> _intValues;
		private ObservableMap<int, long> _longValues;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public CommonObjectData(IDataChangedCallback parent)
		{
			_parent = parent;
			_intValues = new ObservableMap<int, int>(this);
			_longValues = new ObservableMap<int, long>(this);
		}

		public CommonObjectData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int intValuesItemCount;
			intValuesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_intValues = new ObservableMap<int, int>(this);
			for (int i = 0; i < intValuesItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_intValues.Add(key,value);
			}
			int longValuesItemCount;
			longValuesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_longValues = new ObservableMap<int, long>(this);
			for (int i = 0; i < longValuesItemCount; ++i)
			{
				int key;
				long value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadLong(EncodingType.EliasGamma);
				_longValues.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, int> IntValues => _intValues;
		public ObservableMap<int, long> LongValues => _longValues;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_intValues.Count, EncodingType.EliasGamma);
			foreach (var item in _intValues.Items)
			{
				writer.WriteInt(item.Key, EncodingType.EliasGamma);
				writer.WriteInt(item.Value, EncodingType.EliasGamma);
			}
			writer.WriteInt(_longValues.Count, EncodingType.EliasGamma);
			foreach (var item in _longValues.Items)
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
