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
	public class ShipModSettingsSerializable : SerializableItem
	{
		public bool RemoveWeaponSlotMod;
		public float HeatDefenseValue = 0.5f;
		public float KineticDefenseValue = 0.5f;
		public float EnergyDefenseValue = 0.5f;
		public float RegenerationValue = 0.01f;
		public float RegenerationArmor = 0.85f;
		public float WeightReduction = 0.8f;
		public float AttackReduction = 0.2f;
	}
}
