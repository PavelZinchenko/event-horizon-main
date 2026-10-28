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
	public class RuinsSettingsSerializable : SerializableItem
	{
		public RuinsSettingsSerializable()
		{
			ItemType = ItemType.RuinsSettings;
			FileName = "RuinsSettings.json";
		}

		public int[] ExcludedSatellites;
		public string MaxSatelliteCells = "MAX(6, FLOOR(level/2))";
	}
}
