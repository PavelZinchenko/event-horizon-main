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
		private readonly IDataChangedCallback _parent;
		private ObservableMap<int, PurchasesMap> _purchases;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public ShopData(IDataChangedCallback parent)
		{
			_parent = parent;
			_purchases = new ObservableMap<int, PurchasesMap>(this);
		}

		public ShopData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int purchasesItemCount;
			purchasesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_purchases = new ObservableMap<int, PurchasesMap>(this);
			for (int i = 0; i < purchasesItemCount; ++i)
			{
				int key;
				PurchasesMap value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = new PurchasesMap(reader, this);
				_purchases.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, PurchasesMap> Purchases => _purchases;

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
