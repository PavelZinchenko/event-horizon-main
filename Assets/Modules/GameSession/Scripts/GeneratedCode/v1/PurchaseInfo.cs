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
	public readonly partial struct PurchaseInfo
	{
		private readonly long _time;
		private readonly int _quantity;

		public PurchaseInfo(IDataChangedCallback parent)
		{
			_time = default(long);
			_quantity = default(int);
		}

		public PurchaseInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_time = reader.ReadLong(EncodingType.EliasGamma);
			_quantity = reader.ReadInt(EncodingType.EliasGamma);
		}

		public long Time => _time;
		public int Quantity => _quantity;
	}
}
