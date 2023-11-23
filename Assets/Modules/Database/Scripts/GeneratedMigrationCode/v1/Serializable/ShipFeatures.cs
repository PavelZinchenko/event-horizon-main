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
	public struct ShipFeaturesSerializable
	{
		public float EnergyResistance;
		public float KineticResistance;
		public float HeatResistance;
		public float ShipWeightBonus;
		public float EquipmentWeightBonus;
		public float VelocityBonus;
		public float TurnRateBonus;
		public float ArmorBonus;
		public float ShieldBonus;
		public float EnergyBonus;
		public bool Regeneration;
		public int[] BuiltinDevices;
	}
}
