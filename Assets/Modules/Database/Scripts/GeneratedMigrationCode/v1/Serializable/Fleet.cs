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
	public class FleetSerializable : SerializableItem
	{
		public FleetSerializable()
		{
			ItemType = ItemType.Fleet;
			FileName = "Fleet.json";
		}

		public FactionFilterSerializable Factions;
		public int LevelBonus;
		public bool NoRandomShips;
		public int CombatTimeLimit;
		public RewardCondition LootCondition;
		public RewardCondition ExpCondition;
		public int[] SpecificShips;
		public bool NoShipChanging;
		public bool PlayerHasOneShip;
		public int CombatRules;
	}
}
