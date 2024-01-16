//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using GameDatabase.Model;
using DatabaseMigration.v1.Enums;

namespace DatabaseMigration.v1.Serializable
{
	[Serializable]
	public struct RequirementSerializable
	{
		public RequirementType Type;
		public int ItemId;
		public int MinValue;
		public int MaxValue;
		public bool BoolValue;
		public int Character;
		public int Faction;
		public LootContentSerializable Loot;
		public RequirementSerializable[] Requirements;
	}
}
