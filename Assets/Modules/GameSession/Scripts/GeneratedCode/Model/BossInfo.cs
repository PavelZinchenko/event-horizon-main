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
	public readonly partial struct BossInfo
	{
		private readonly int _defeatCount;
		private readonly long _lastDefeatTime;

		public BossInfo(IDataChangedCallback parent)
		{
			_defeatCount = default(int);
			_lastDefeatTime = default(long);
		}

		public BossInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_defeatCount = reader.ReadInt(EncodingType.EliasGamma);
			_lastDefeatTime = reader.ReadLong(EncodingType.EliasGamma);
		}

		public int DefeatCount => _defeatCount;
		public long LastDefeatTime => _lastDefeatTime;

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_defeatCount, EncodingType.EliasGamma);
				writer.WriteLong(_lastDefeatTime, EncodingType.EliasGamma);
		}
	}
}
