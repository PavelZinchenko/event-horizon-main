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
	public class FactionsSettingsSerializable : SerializableItem
	{
		public FactionsSettingsSerializable()
		{
			ItemType = ItemType.FactionsSettings;
			FileName = "FactionsSettings.json";
		}

		public string StarbaseInitialDefense = "MIN(1000, 300 + 5*distance)";
		public int StarbaseMinDefense = 50;
		public int DefenseLossPerEnemyDefeated = 10;
	}
}
