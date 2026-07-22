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
		[DefaultValue("")]
		public string Name;
		[DefaultValue("")]
		public string Description;
		public int Faction;
		[DefaultValue("")]
		public string IconImage;
		public float IconScale;
		[DefaultValue("")]
		public string ModelImage;
		public float ModelScale;
		[DefaultValue("")]
		public string EngineColor;
		public EngineSerializable[] Engines;
		[DefaultValue("")]
		public string Layout;
		public BarrelSerializable[] Barrels;
		public ShipFeaturesSerializable Features;
		public ShipVisualEffectsSerializable VisualEffects;
		public ToggleState CellsExpansions;
		[DefaultValue(0.02f)]
		public float ColliderTolerance = 0.02f;
		public Vector2 EnginePosition;
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
