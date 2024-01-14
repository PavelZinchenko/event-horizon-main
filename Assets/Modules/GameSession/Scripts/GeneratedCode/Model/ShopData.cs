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
	public class ShopData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableMap<int, Model.PurchasesMap> _purchases;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public ShopData(IDataChangedCallback parent)
		{
			_parent = parent;
			_purchases = new ObservableMap<int, Model.PurchasesMap>(this);
		}

		public ShopData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int purchasesItemCount;
			purchasesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_purchases = new ObservableMap<int, Model.PurchasesMap>(this);
			for (int i = 0; i < purchasesItemCount; ++i)
			{
				int key;
				Model.PurchasesMap value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = new Model.PurchasesMap(reader, this);
				_purchases.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, Model.PurchasesMap> Purchases => _purchases;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_purchases.Count, EncodingType.EliasGamma);
			foreach (var item in _purchases.Items)
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
