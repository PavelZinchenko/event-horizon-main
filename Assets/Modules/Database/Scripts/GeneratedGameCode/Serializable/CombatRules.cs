//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace GameDatabase.Serializable
{
	[Serializable]
	public class CombatRulesSerializable : SerializableItem
	{
		public string InitialEnemyShips = "1";
		public string MaxEnemyShips = "12";
		public string TimeLimit = "MAX(40, 100 - level)";
		public TimeOutMode TimeOutMode;
		public RewardCondition LootCondition;
		public RewardCondition ExpCondition;
		public PlayerShipSelectionMode ShipSelection;
		public bool DisableSkillBonuses;
		public bool DisableRandomLoot;
		public bool DisableAsteroids;
		public bool DisablePlanet;
		public bool NextEnemyButton = true;
		public bool KillThemAllButton;
	}
}
