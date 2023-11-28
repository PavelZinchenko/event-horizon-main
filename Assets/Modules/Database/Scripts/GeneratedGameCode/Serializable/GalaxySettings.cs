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
	public class GalaxySettingsSerializable : SerializableItem
	{
		public int AbandonedStarbaseFaction;
		public int[] StartingShipBuilds;
		public int StartingInvenory;
		public int SupporterPackShip;
		public int DefaultStarbaseBuild;
		public int MaxEnemyShipsLevel = 300;
		public string EnemyLevel;
	}
}
