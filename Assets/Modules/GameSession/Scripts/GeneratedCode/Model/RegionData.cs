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
	public class RegionData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableMap<int, uint> _militaryPower;
		private ObservableBitset _capturedBases;
		private ObservableMap<int, int> _factions;

		public const int VersionMinor = 0;
		public const int VersionMajor = 2;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public RegionData(IDataChangedCallback parent)
		{
			_parent = parent;
			_militaryPower = new ObservableMap<int, uint>(this);
			_capturedBases = new ObservableBitset(this);
			_factions = new ObservableMap<int, int>(this);
		}

		public RegionData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int militaryPowerItemCount;
			militaryPowerItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_militaryPower = new ObservableMap<int, uint>(this);
			for (int i = 0; i < militaryPowerItemCount; ++i)
			{
				int key;
				uint value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadUint(EncodingType.EliasGamma);
				_militaryPower.Add(key,value);
			}
			_capturedBases = new ObservableBitset(reader, EncodingType.EliasGamma, this);
			int factionsItemCount;
			factionsItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_factions = new ObservableMap<int, int>(this);
			for (int i = 0; i < factionsItemCount; ++i)
			{
				int key;
				int value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = reader.ReadInt(EncodingType.EliasGamma);
				_factions.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, uint> MilitaryPower => _militaryPower;
		public ObservableBitset CapturedBases => _capturedBases;
		public ObservableMap<int, int> Factions => _factions;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_militaryPower.Count, EncodingType.EliasGamma);
			foreach (var item in _militaryPower)
			{
				writer.WriteInt(item.Key, EncodingType.EliasGamma);
				writer.WriteUint(item.Value, EncodingType.EliasGamma);
			}
			_capturedBases.Serialize(writer, EncodingType.EliasGamma);
			writer.WriteInt(_factions.Count, EncodingType.EliasGamma);
			foreach (var item in _factions)
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
