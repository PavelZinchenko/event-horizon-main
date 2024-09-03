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
	public class ShipModSettingsSerializable : SerializableItem
	{
		public ShipModSettingsSerializable()
		{
			ItemType = ItemType.ShipModSettings;
			FileName = "ShipModSettings.json";
		}

		public bool RemoveWeaponSlotMod;
		public bool RemoveUnlimitedRespawnMod;
		public bool RemoveEnergyRechargeCdMod;
		public bool RemoveShieldRechargeCdMod;
		public bool RemoveBiggerSatellitesMod;
		public float HeatDefenseValue = 0.5f;
		public float KineticDefenseValue = 0.5f;
		public float EnergyDefenseValue = 0.5f;
		public float RegenerationValue = 0.01f;
		public float RegenerationArmor = 0.85f;
		public float WeightReduction = 0.8f;
		public float AttackReduction = 0.2f;
		public float EnergyReduction = 0.5f;
		public float ShieldReduction = 0.5f;
	}
}
