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
	public readonly partial struct HangarSlotInfo
	{
		private readonly int _index;
		private readonly int _shipId;

		public HangarSlotInfo(IDataChangedCallback parent)
		{
			_index = default(int);
			_shipId = default(int);
		}

		public HangarSlotInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_index = reader.ReadInt(EncodingType.EliasGamma);
			_shipId = reader.ReadInt(EncodingType.EliasGamma);
		}

		public int Index => _index;
		public int ShipId => _shipId;

		public void Serialize(SessionDataWriter writer)
		{
			writer.WriteInt(_index, EncodingType.EliasGamma);
			writer.WriteInt(_shipId, EncodingType.EliasGamma);
		}
	}
}
