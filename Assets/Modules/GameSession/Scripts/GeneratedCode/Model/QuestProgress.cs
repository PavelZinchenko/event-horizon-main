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
	public readonly partial struct QuestProgress
	{
		private readonly int _seed;
		private readonly int _activeNode;
		private readonly long _startTime;

		public QuestProgress(IDataChangedCallback parent)
		{
			_seed = default(int);
			_activeNode = default(int);
			_startTime = default(long);
		}

		public QuestProgress(SessionDataReader reader, IDataChangedCallback parent)
		{
			_seed = reader.ReadInt(EncodingType.EliasGamma);
			_activeNode = reader.ReadInt(EncodingType.EliasGamma);
			_startTime = reader.ReadLong(EncodingType.EliasGamma);
		}

		public int Seed => _seed;
		public int ActiveNode => _activeNode;
		public long StartTime => _startTime;

		public void Serialize(SessionDataWriter writer)
		{
				writer.WriteInt(_seed, EncodingType.EliasGamma);
				writer.WriteInt(_activeNode, EncodingType.EliasGamma);
				writer.WriteLong(_startTime, EncodingType.EliasGamma);
		}
	}
}
