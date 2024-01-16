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
	public class ShopData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private ObservableMap<int, v1.PurchasesMap> _purchases;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public ShopData(IDataChangedCallback parent)
		{
			_parent = parent;
			_purchases = new ObservableMap<int, v1.PurchasesMap>(this);
		}

		public ShopData(SessionDataReader reader, IDataChangedCallback parent)
		{
			int purchasesItemCount;
			purchasesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_purchases = new ObservableMap<int, v1.PurchasesMap>(this);
			for (int i = 0; i < purchasesItemCount; ++i)
			{
				int key;
				v1.PurchasesMap value;
				key = reader.ReadInt(EncodingType.EliasGamma);
				value = new v1.PurchasesMap(reader, this);
				_purchases.Add(key,value);
			}
			_parent = parent;
			DataChanged = false;
		}

		public ObservableMap<int, v1.PurchasesMap> Purchases => _purchases;

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
