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
	public readonly partial struct PurchasesMap
	{
		private readonly ObservableMap<string, PurchaseInfo> _purchases;

		public PurchasesMap(IDataChangedCallback parent)
		{
			_purchases = new ObservableMap<string, PurchaseInfo>(parent);
		}

		public PurchasesMap(SessionDataReader reader, IDataChangedCallback parent)
		{
			int purchasesItemCount;
			purchasesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_purchases = new ObservableMap<string, PurchaseInfo>(parent);
			for (int i = 0; i < purchasesItemCount; ++i)
			{
				string key;
				PurchaseInfo value;
				key = reader.ReadString(EncodingType.EliasGamma);
				value = new PurchaseInfo(reader, parent);
				_purchases.Add(key,value);
			}
		}

		public ObservableMap<string, PurchaseInfo> Purchases => _purchases;

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_purchases.Count, EncodingType.EliasGamma);
				foreach (var item in _purchases.Items)
				{
					writer.WriteString(item.Key, EncodingType.EliasGamma);
					item.Value.Serialize(writer);
				}
		}
	}
}
