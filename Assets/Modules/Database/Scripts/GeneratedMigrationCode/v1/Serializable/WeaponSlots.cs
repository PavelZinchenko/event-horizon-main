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
	public class WeaponSlotsSerializable : SerializableItem
	{
		public WeaponSlotsSerializable()
		{
			ItemType = ItemType.WeaponSlots;
			FileName = "WeaponSlots.json";
		}

		public WeaponSlotSerializable[] Slots;
		public string DefaultSlotName = "$GroupWeaponAny";
		public string DefaultSlotIcon = "icon_weapon_x";
	}
}
