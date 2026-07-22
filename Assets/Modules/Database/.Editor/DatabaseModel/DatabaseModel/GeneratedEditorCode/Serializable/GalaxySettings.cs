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
	public class GalaxySettingsSerializable : SerializableItem
	{
		public int AbandonedStarbaseFaction;
		public int[] StartingShipBuilds;
		public int StartingInventory;
		public int SupporterPackShip;
		public int DefaultStarbaseBuild;
		[DefaultValue(300)]
		public int MaxEnemyShipsLevel = 300;
		[DefaultValue("MIN(3*distance/5 - 5, MaxEnemyShipsLevel)")]
		public string EnemyLevel = "MIN(3*distance/5 - 5, MaxEnemyShipsLevel)";
		[DefaultValue("IF(size == Destroyer, 5, size == Cruiser, 15, size == Battleship, 50, size == Titan, 100, 0)")]
		public string ShipMinSpawnDistance = "IF(size == Destroyer, 5, size == Cruiser, 15, size == Battleship, 50, size == Titan, 100, 0)";
		public int CaptureStarbaseQuest;
		public int StartingInvenory;
		public int SurvivalCombatRules;
		public int StarbaseCombatRules;
		public int FlagshipCombatRules;
		public int ArenaCombatRules;
		public int ChallengeCombatRules;
		public int QuickCombatRules;
	}
}
