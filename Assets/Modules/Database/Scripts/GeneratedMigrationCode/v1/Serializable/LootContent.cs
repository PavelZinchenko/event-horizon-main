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
	public class LootContentSerializable
	{
		public LootItemType Type;
		public int ItemId;
		public int MinAmount;
		public int MaxAmount;
		public float ValueRatio;
		public FactionFilterSerializable Factions;
		public LootItemSerializable[] Items;
	}
}
