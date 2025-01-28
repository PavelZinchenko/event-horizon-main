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
	public class WeaponSlotsSerializable : SerializableItem
	{
		public WeaponSlotSerializable[] Slots;
		public string DefaultSlotName = "$GroupWeaponAny";
		public string DefaultSlotIcon = "icon_weapon_x";
	}
}
