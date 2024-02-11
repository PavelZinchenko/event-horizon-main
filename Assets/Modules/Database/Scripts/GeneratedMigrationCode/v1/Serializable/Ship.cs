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
	public class ShipSerializable : SerializableItem
	{
		public ShipSerializable()
		{
			ItemType = ItemType.Ship;
			FileName = "Ship.json";
		}

		public ShipType ShipType;
		public ShipRarity ShipRarity;
		public SizeClass SizeClass;
		public string Name;
		public string Description;
		public int Faction;
		public string IconImage;
		public float IconScale;
		public string ModelImage;
		public float ModelScale;
		public string EngineColor;
		public EngineSerializable[] Engines;
		public string Layout;
		public BarrelSerializable[] Barrels;
		public ShipFeaturesSerializable Features;
		public float ColliderTolerance = 0.02f;
		public UnityEngine.Vector2 EnginePosition;
		public float EngineSize;
		public int ShipCategory;
		public float EnergyResistance;
		public float KineticResistance;
		public float HeatResistance;
		public bool Regeneration;
		public int[] BuiltinDevices;
		public float BaseWeightModifier;
	}
}
