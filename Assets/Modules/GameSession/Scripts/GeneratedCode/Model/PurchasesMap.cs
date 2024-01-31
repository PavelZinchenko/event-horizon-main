//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Collections.Generic;
using Session.Utils;

namespace Session.Model
{
	public readonly partial struct PurchasesMap
	{
		private readonly ObservableMap<string, Model.PurchaseInfo> _purchases;

		public PurchasesMap(IDataChangedCallback parent)
		{
			_purchases = new ObservableMap<string, Model.PurchaseInfo>(parent);
		}

		public PurchasesMap(SessionDataReader reader, IDataChangedCallback parent)
		{
			int purchasesItemCount;
			purchasesItemCount = reader.ReadInt(EncodingType.EliasGamma);
			_purchases = new ObservableMap<string, Model.PurchaseInfo>(parent);
			for (int i = 0; i < purchasesItemCount; ++i)
			{
				string key;
				Model.PurchaseInfo value;
				key = reader.ReadString(EncodingType.EliasGamma);
				value = new Model.PurchaseInfo(reader, parent);
				_purchases.Add(key,value);
			}
		}

		public ObservableMap<string, Model.PurchaseInfo> Purchases => _purchases;

        public void Serialize(SessionDataWriter writer)
        {
			writer.WriteInt(_purchases.Count, EncodingType.EliasGamma);
			foreach (var item in _purchases)
			{
				writer.WriteString(item.Key, EncodingType.EliasGamma);
				item.Value.Serialize(writer);
			}
        }

        public bool Equals(PurchasesMap other)
        {
			if (!Purchases.Equals(other.Purchases, new Model.PurchaseInfo.EqualityComparer())) return false;
            return true;
        }

        public struct EqualityComparer : IEqualityComparer<PurchasesMap>
        {
            public bool Equals(PurchasesMap first, PurchasesMap second) => first.Equals(second);
            public int GetHashCode(PurchasesMap obj) => obj.GetHashCode();
        }
	}
}
