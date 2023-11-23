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
	public class GalaxySettingsSerializable : SerializableItem
	{
		public GalaxySettingsSerializable()
		{
			ItemType = ItemType.GalaxySettings;
		}

		public int AbandonedStarbaseFaction;
		public int[] StartingShipBuilds;
		public int DefaultStarbaseBuild;
		public int MaxEnemyShipsLevel = 300;
	}
}
