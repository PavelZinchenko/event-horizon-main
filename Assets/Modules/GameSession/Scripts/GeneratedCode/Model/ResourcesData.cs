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
	public class ResourcesData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObscuredLong _money;
		private ObscuredLong _stars;
		private ObscuredInt _fuel;
		private ObscuredInt _tokens;
		private ObservableInventory<int> _resources;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public ResourcesData(IDataChangedCallback parent)
		{
			_parent = parent;
			_money = default(long);
			_stars = default(long);
			_fuel = default(int);
			_tokens = default(int);
			_resources = new ObservableInventory<int>(this);
		}

		public ResourcesData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_money = reader.ReadLong(EncodingType.EliasGamma);
			_stars = reader.ReadLong(EncodingType.EliasGamma);
			_fuel = reader.ReadInt(EncodingType.EliasGamma);
			_tokens = reader.ReadInt(EncodingType.EliasGamma);
			int resourcesItemCount;
			resourcesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_resources = new ObservableInventory<int>(this);
			for (int i = 0; i < resourcesItemCount; ++i)
			{
				int value;
				int quantity;
				value = reader.ReadInt(EncodingType.EliasGamma);
				quantity = reader.ReadInt(EncodingType.EliasGamma);
				_resources.Add(value,quantity);
			}
			_parent = parent;
			DataChanged = false;
		}

		public long Money
		{
			get => _money;
			set
			{
				if (_money == value) return;
				_money = value;
				OnDataChanged();
			}
		}
		public long Stars
		{
			get => _stars;
			set
			{
				if (_stars == value) return;
				_stars = value;
				OnDataChanged();
			}
		}
		public int Fuel
		{
			get => _fuel;
			set
			{
				if (_fuel == value) return;
				_fuel = value;
				OnDataChanged();
			}
		}
		public int Tokens
		{
			get => _tokens;
			set
			{
				if (_tokens == value) return;
				_tokens = value;
				OnDataChanged();
			}
		}
		public ObservableInventory<int> Resources => _resources;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteLong(_money, EncodingType.EliasGamma);
			writer.WriteLong(_stars, EncodingType.EliasGamma);
			writer.WriteInt(_fuel, EncodingType.EliasGamma);
			writer.WriteInt(_tokens, EncodingType.EliasGamma);
			writer.WriteInt(_resources.Count, EncodingType.EliasGamma);
			foreach (var item in _resources.Items)
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
