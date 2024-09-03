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
	public class ShipSettingsSerializable : SerializableItem
	{
		public ShipSettingsSerializable()
		{
			ItemType = ItemType.ShipSettings;
			FileName = "ShipSettings.json";
		}

		public float DefaultWeightPerCell;
		public float MinimumWeightPerCell;
		public float BaseArmorPoints;
		public float ArmorPointsPerCell;
		public float ArmorRepairCooldown;
		public float BaseEnergyPoints;
		public float BaseEnergyRechargeRate;
		public float EnergyRechargeCooldown;
		public float BaseShieldRechargeRate;
		public float ShieldRechargeCooldown;
		public float BaseDroneReconstructionSpeed;
		public float ShieldCorrosiveResistance = 0.9f;
		public float MaxVelocity = 30f;
		public float MaxAngularVelocity = 30f;
		public float MaxAcceleration = 300f;
		public float MaxAngularAcceleration = 300f;
		public bool DisableCellsExpansions;
	}
}
