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
	public class DatabaseSettingsSerializable : SerializableItem
	{
		public DatabaseSettingsSerializable()
		{
			ItemType = ItemType.DatabaseSettings;
			FileName = "DatabaseSettings.json";
		}

		public int DatabaseVersion;
		public int DatabaseVersionMinor;
		[DefaultValue("")]
		public string ModName;
		[DefaultValue("")]
		public string ModId;
		public int ModVersion;
		public bool UnloadOriginalDatabase;
	}
}
