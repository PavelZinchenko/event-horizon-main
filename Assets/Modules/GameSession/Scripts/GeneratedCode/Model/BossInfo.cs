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
	public readonly partial struct BossInfo
	{
		private readonly int _defeatCount;
		private readonly int _lastDefeatTime;

		public BossInfo(IDataChangedCallback parent)
		{
			_defeatCount = default(int);
			_lastDefeatTime = default(int);
		}

		public BossInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_defeatCount = reader.ReadInt(EncodingType.EliasGamma);
			_lastDefeatTime = reader.ReadInt(EncodingType.EliasGamma);
		}

		public int DefeatCount => _defeatCount;
		public int LastDefeatTime => _lastDefeatTime;

        public void Serialize(SessionDataWriter writer)
        {
			writer.WriteInt(_defeatCount, EncodingType.EliasGamma);
			writer.WriteInt(_lastDefeatTime, EncodingType.EliasGamma);
        }

        public bool Equals(BossInfo other)
        {
			if (DefeatCount != other.DefeatCount) return false;
			if (LastDefeatTime != other.LastDefeatTime) return false;
            return true;
        }

        public struct EqualityComparer : IEqualityComparer<BossInfo>
        {
            public bool Equals(BossInfo first, BossInfo second) => first.Equals(second);
            public int GetHashCode(BossInfo obj) => obj.GetHashCode();
        }
	}
}
