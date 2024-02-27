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
	public class CombatRulesSerializable : SerializableItem
	{
		public CombatRulesSerializable()
		{
			ItemType = ItemType.CombatRules;
			FileName = "CombatRules.json";
		}

		public string InitialEnemyShips = "1";
		public string MaxEnemyShips = "12";
		public int BattleMapSize = 200;
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
