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
	public readonly partial struct ShipComponentInfo
	{
		private readonly int _id;
		private readonly sbyte _quality;
		private readonly int _modification;
		private readonly sbyte _upgradeLevel;
		private readonly short _x;
		private readonly short _y;
		private readonly sbyte _barrelId;
		private readonly sbyte _keyBinding;
		private readonly sbyte _behaviour;
		private readonly bool _locked;

		public ShipComponentInfo(IDataChangedCallback parent)
		{
			_id = default(int);
			_quality = default(sbyte);
			_modification = default(int);
			_upgradeLevel = default(sbyte);
			_x = default(short);
			_y = default(short);
			_barrelId = default(sbyte);
			_keyBinding = default(sbyte);
			_behaviour = default(sbyte);
			_locked = default(bool);
		}

		public ShipComponentInfo(SessionDataReader reader, IDataChangedCallback parent)
		{
			_id = reader.ReadInt(EncodingType.EliasGamma);
			_quality = reader.ReadSbyte(EncodingType.EliasGamma);
			_modification = reader.ReadInt(EncodingType.EliasGamma);
			_upgradeLevel = reader.ReadSbyte(EncodingType.EliasGamma);
			_x = reader.ReadShort(EncodingType.EliasGamma);
			_y = reader.ReadShort(EncodingType.EliasGamma);
			_barrelId = reader.ReadSbyte(EncodingType.EliasGamma);
			_keyBinding = reader.ReadSbyte(EncodingType.EliasGamma);
			_behaviour = reader.ReadSbyte(EncodingType.EliasGamma);
			_locked = reader.ReadBool(EncodingType.EliasGamma);
		}

		public int Id => _id;
		public sbyte Quality => _quality;
		public int Modification => _modification;
		public sbyte UpgradeLevel => _upgradeLevel;
		public short X => _x;
		public short Y => _y;
		public sbyte BarrelId => _barrelId;
		public sbyte KeyBinding => _keyBinding;
		public sbyte Behaviour => _behaviour;
		public bool Locked => _locked;

        public void Serialize(SessionDataWriter writer)
        {
			writer.WriteInt(_id, EncodingType.EliasGamma);
			writer.WriteSbyte(_quality, EncodingType.EliasGamma);
			writer.WriteInt(_modification, EncodingType.EliasGamma);
			writer.WriteSbyte(_upgradeLevel, EncodingType.EliasGamma);
			writer.WriteShort(_x, EncodingType.EliasGamma);
			writer.WriteShort(_y, EncodingType.EliasGamma);
			writer.WriteSbyte(_barrelId, EncodingType.EliasGamma);
			writer.WriteSbyte(_keyBinding, EncodingType.EliasGamma);
			writer.WriteSbyte(_behaviour, EncodingType.EliasGamma);
			writer.WriteBool(_locked, EncodingType.EliasGamma);
        }

        public bool Equals(ShipComponentInfo other)
        {
			if (Id != other.Id) return false;
			if (Quality != other.Quality) return false;
			if (Modification != other.Modification) return false;
			if (UpgradeLevel != other.UpgradeLevel) return false;
			if (X != other.X) return false;
			if (Y != other.Y) return false;
			if (BarrelId != other.BarrelId) return false;
			if (KeyBinding != other.KeyBinding) return false;
			if (Behaviour != other.Behaviour) return false;
			if (Locked != other.Locked) return false;
            return true;
        }

        public struct EqualityComparer : IEqualityComparer<ShipComponentInfo>
        {
            public bool Equals(ShipComponentInfo first, ShipComponentInfo second) => first.Equals(second);
            public int GetHashCode(ShipComponentInfo obj) => obj.GetHashCode();
        }
	}
}
