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
	public class CombatRulesSerializable : SerializableItem
	{
		[DefaultValue("1")]
		public string InitialEnemyShips = "1";
		[DefaultValue("12")]
		public string MaxEnemyShips = "12";
		[DefaultValue(200)]
		public int BattleMapSize = 200;
		[DefaultValue("MAX(40, 100 - level)")]
		public string TimeLimit = "MAX(40, 100 - level)";
		public TimeOutMode TimeOutMode;
		public RewardCondition LootCondition;
		public RewardCondition ExpCondition;
		public PlayerShipSelectionMode ShipSelection;
		public bool DisableSkillBonuses;
		public bool DisableRandomLoot;
		public bool DisableAsteroids;
		public bool DisablePlanet;
		[DefaultValue(true)]
		public bool NextEnemyButton = true;
		public bool KillThemAllButton;
		public SoundTrackSerializable[] CustomSoundtrack;
	}
}
