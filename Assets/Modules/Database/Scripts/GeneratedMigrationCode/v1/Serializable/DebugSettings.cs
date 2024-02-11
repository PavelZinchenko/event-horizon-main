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
	public class DebugSettingsSerializable : SerializableItem
	{
		public DebugSettingsSerializable()
		{
			ItemType = ItemType.DebugSettings;
			FileName = "DebugSettings.json";
		}

		public DebugCodeSerializable[] Codes;
		public bool EnableDebugConsole;
	}
}
