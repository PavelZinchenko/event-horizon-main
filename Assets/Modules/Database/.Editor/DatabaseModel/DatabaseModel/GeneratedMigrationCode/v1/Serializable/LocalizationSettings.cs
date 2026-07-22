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
	public class LocalizationSettingsSerializable : SerializableItem
	{
		public LocalizationSettingsSerializable()
		{
			ItemType = ItemType.LocalizationSettings;
			FileName = "LocalizationSettings.json";
		}

		[DefaultValue("$WeaponDamage")]
		public string CorrosiveDamageText = "$WeaponDamage";
		[DefaultValue("$WeaponDPS")]
		public string CorrosiveDpsText = "$WeaponDPS";
	}
}
