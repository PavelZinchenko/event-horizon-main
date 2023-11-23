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
	public class DroneBaySerializable : SerializableItem
	{
		public DroneBaySerializable()
		{
			ItemType = ItemType.DroneBay;
		}

		public float EnergyConsumption;
		public float PassiveEnergyConsumption;
		public float Range;
		public float DamageMultiplier;
		public float DefenseMultiplier;
		public float SpeedMultiplier;
		public int BuildExtraCycles;
		public bool ImprovedAi;
		public int Capacity;
		public ActivationType ActivationType;
		public string LaunchSound;
		public string LaunchEffectPrefab;
		public string ControlButtonIcon;
	}
}
