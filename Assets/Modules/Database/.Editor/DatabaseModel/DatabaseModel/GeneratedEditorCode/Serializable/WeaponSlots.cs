//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using EditorDatabase.Enums;
using EditorDatabase.Model;

namespace EditorDatabase.Serializable
{
	[Serializable]
	public class WeaponSlotsSerializable : SerializableItem
	{
		public WeaponSlotSerializable[] Slots;
		[DefaultValue("$GroupWeaponAny")]
		public string DefaultSlotName = "$GroupWeaponAny";
		[DefaultValue("icon_weapon_x")]
		public string DefaultSlotIcon = "icon_weapon_x";
	}
}
