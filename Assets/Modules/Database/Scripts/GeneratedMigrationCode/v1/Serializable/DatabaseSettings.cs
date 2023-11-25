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
	public class DatabaseSettingsSerializable : SerializableItem
	{
		public DatabaseSettingsSerializable()
		{
			ItemType = ItemType.DatabaseSettings;
			FileName = $"{ItemType}.json";
		}

		public int DatabaseVersion;
		public int DatabaseVersionMinor;
		public string ModName;
		public string ModId;
		public int ModVersion;
		public bool UnloadOriginalDatabase;
	}
}
