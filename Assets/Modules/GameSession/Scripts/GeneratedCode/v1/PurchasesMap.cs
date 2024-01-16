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
	public readonly partial struct PurchasesMap
	{
		private readonly ObservableMap<string, v1.PurchaseInfo> _purchases;

		public PurchasesMap(IDataChangedCallback parent)
		{
			_purchases = new ObservableMap<string, v1.PurchaseInfo>(parent);
		}

		public PurchasesMap(SessionDataReader reader, IDataChangedCallback parent)
		{
			int purchasesItemCount;
			purchasesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_purchases = new ObservableMap<string, v1.PurchaseInfo>(parent);
			for (int i = 0; i < purchasesItemCount; ++i)
			{
				string key;
				v1.PurchaseInfo value;
				key = reader.ReadString(EncodingType.EliasGamma);
				value = new v1.PurchaseInfo(reader, parent);
				_purchases.Add(key,value);
			}
		}

		public ObservableMap<string, v1.PurchaseInfo> Purchases => _purchases;
	}
}
