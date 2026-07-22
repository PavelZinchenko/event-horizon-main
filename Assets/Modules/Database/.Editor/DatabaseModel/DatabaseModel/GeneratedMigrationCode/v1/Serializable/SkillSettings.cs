//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using EditorDatabase.Model;
using DatabaseMigration.v1.Enums;

namespace DatabaseMigration.v1.Serializable
{
	[Serializable]
	public class SkillSettingsSerializable : SerializableItem
	{
		public SkillSettingsSerializable()
		{
			ItemType = ItemType.SkillSettings;
			FileName = "SkillSettings.json";
		}

		public int[] BeatAllEnemiesFactionList;
		public bool DisableExceedTheLimits;
		[DefaultValue("BaseFuelCapacity + 50*level")]
		public string FuelTankCapacity = "BaseFuelCapacity + 50*level";
		[DefaultValue("0.1*level")]
		public string AttackBonus = "0.1*level";
		[DefaultValue("0.1*level")]
		public string DefenseBonus = "0.1*level";
		[DefaultValue("0.1*level")]
		public string ShieldStrengthBonus = "0.1*level";
		[DefaultValue("0.1*level")]
		public string ShieldRechargeBonus = "0.1*level";
		[DefaultValue("0.1*level")]
		public string ExperienceBonus = "0.1*level";
		[DefaultValue("BaseFlightSpeed + 0.4*level")]
		public string FlightSpeed = "BaseFlightSpeed + 0.4*level";
		[DefaultValue("BaseFlightRange + 0.09*level")]
		public string FlightRange = "BaseFlightRange + 0.09*level";
		[DefaultValue("0.1*level")]
		public string ExplorationLootBonus = "0.1*level";
		[DefaultValue("0.1*level")]
		public string HeatResistance = "0.1*level";
		[DefaultValue("0.1*level")]
		public string KineticResistance = "0.1*level";
		[DefaultValue("0.1*level")]
		public string EnergyResistance = "0.1*level";
		[DefaultValue("1 - 0.05*level")]
		public string MerchantPriceFactor = "1 - 0.05*level";
		[DefaultValue("1 - 0.05*level")]
		public string CraftingPriceFactor = "1 - 0.05*level";
		[DefaultValue("5*level")]
		public string CraftingLevelReduction = "5*level";
		[DefaultValue(100)]
		public int MaxPlayerShipsLevel = 100;
		[DefaultValue(200)]
		public int IncreasedLevelLimit = 200;
		[DefaultValue(100)]
		public int BaseFuelCapacity = 100;
		[DefaultValue(1.5f)]
		public float BaseFlightRange = 1.5f;
		[DefaultValue(1f)]
		public float BaseFlightSpeed = 1f;
	}
}
