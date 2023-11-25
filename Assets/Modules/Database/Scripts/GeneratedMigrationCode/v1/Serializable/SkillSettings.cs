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
	public class SkillSettingsSerializable : SerializableItem
	{
		public SkillSettingsSerializable()
		{
			ItemType = ItemType.SkillSettings;
			FileName = $"{ItemType}.json";
		}

		public int[] BeatAllEnemiesFactionList;
		public bool DisableExceedTheLimits;
		public int FuelTankCapacity = 50;
		public int MapFlightRange = 6;
		public int MapFlightSpeed = 40;
		public int AttackBonus = 10;
		public int DefenseBonus = 10;
		public int ExperienceBonus = 10;
		public int ExplorationLootBonus = 10;
		public int HeatDefenseBonus = 10;
		public int KineticDefenseBonus = 10;
		public int EnergyDefenseBonus = 10;
		public int MerchantPriceReduction = 5;
		public int CraftPriceReduction = 5;
		public int CraftLevelReduction = 5;
		public int ShieldStrengthBonus = 10;
		public int ShieldRechargeBonus = 10;
		public int IncreasedLevelLimit = 200;
	}
}
