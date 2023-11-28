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
	public class SkillSettingsSerializable : SerializableItem
	{
		public int[] BeatAllEnemiesFactionList;
		public bool DisableExceedTheLimits;
		public string FuelTankCapacity;
		public string AttackBonus;
		public string DefenseBonus;
		public string ShieldStrengthBonus;
		public string ShieldRechargeBonus;
		public string ExperienceBonus;
		public string FlightSpeed;
		public string FlightRange;
		public string ExplorationLootBonus;
		public string HeatResistance;
		public string KineticResistance;
		public string EnergyResistance;
		public string MerchantPriceFactor;
		public string CraftingPriceFactor;
		public string CraftingLevelReduction;
		public int IncreasedLevelLimit = 200;
		public int BaseFuelCapacity = 100;
		public float BaseFlightRange = 1.5f;
		public float BaseFlightSpeed = 1f;
	}
}
