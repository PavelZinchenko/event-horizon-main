//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using EditorDatabase.Enums;
using EditorDatabase.Model;

namespace EditorDatabase.Serializable
{
	[Serializable]
	public class FleetSerializable : SerializableItem
	{
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
