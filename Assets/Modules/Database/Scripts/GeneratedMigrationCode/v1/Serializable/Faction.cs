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
	public class FactionSerializable : SerializableItem
	{
		public FactionSerializable()
		{
			ItemType = ItemType.Faction;
		}

		public string Name;
		public string Color;
		public bool NoTerritories;
		public int HomeStarDistance;
		public int HomeStarDistanceMax;
		public bool NoWanderingShips;
		public int WanderingShipsDistance;
		public int WanderingShipsDistanceMax;
		public bool HideFromMerchants;
		public bool HideResearchTree;
		public bool NoMissions;
		public bool Hidden;
		public bool Hostile;
	}
}
