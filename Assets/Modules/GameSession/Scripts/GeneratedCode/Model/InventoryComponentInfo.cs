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
	public readonly partial struct InventoryComponentInfo
	{
		private readonly uint _id;
		private readonly byte _quality;
		private readonly uint _modification;
		private readonly byte _upgradeLevel;

		public InventoryComponentInfo(IDataChangedCallback parent)
		{
			_id = default(uint);
			_quality = default(byte);
			_modification = default(uint);
			_upgradeLevel = default(byte);
		}

		public InventoryComponentInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_id = reader.ReadUint(EncodingType.EliasGamma);
			_quality = reader.ReadByte(EncodingType.EliasGamma);
			_modification = reader.ReadUint(EncodingType.EliasGamma);
			_upgradeLevel = reader.ReadByte(EncodingType.EliasGamma);
		}

		public uint Id => _id;
		public byte Quality => _quality;
		public uint Modification => _modification;
		public byte UpgradeLevel => _upgradeLevel;

        public void Serialize(SessionDataWriter writer)
        {
			writer.WriteUint(_id, EncodingType.EliasGamma);
			writer.WriteByte(_quality, EncodingType.EliasGamma);
			writer.WriteUint(_modification, EncodingType.EliasGamma);
			writer.WriteByte(_upgradeLevel, EncodingType.EliasGamma);
        }

        public bool Equals(InventoryComponentInfo other)
        {
			if (Id != other.Id) return false;
			if (Quality != other.Quality) return false;
			if (Modification != other.Modification) return false;
			if (UpgradeLevel != other.UpgradeLevel) return false;
            return true;
        }

        public struct EqualityComparer : IEqualityComparer<InventoryComponentInfo>
        {
            public bool Equals(InventoryComponentInfo first, InventoryComponentInfo second) => first.Equals(second);
            public int GetHashCode(InventoryComponentInfo obj) => obj.GetHashCode();
        }
	}
}
