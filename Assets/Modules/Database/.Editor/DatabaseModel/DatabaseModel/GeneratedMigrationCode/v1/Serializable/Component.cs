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
	public class ComponentSerializable : SerializableItem
	{
		public ComponentSerializable()
		{
			ItemType = ItemType.Component;
			FileName = "Component.json";
		}

		[DefaultValue("")]
		public string Name;
		[DefaultValue("")]
		public string Description;
		public ComponentCategory DisplayCategory;
		public Availability Availability;
		public int ComponentStatsId;
		public int Faction;
		public int Level;
		[DefaultValue("")]
		public string Icon;
		[DefaultValue("")]
		public string Color;
		[DefaultValue("")]
		public string Layout;
		[DefaultValue("")]
		public string CellType;
		public int DeviceId;
		public int WeaponId;
		public int AmmunitionId;
		public string WeaponSlotType;
		public int DroneBayId;
		public int DroneId;
		public ComponentRestrictionsSerializable Restrictions;
		public int[] PossibleModifications;
	}
}
