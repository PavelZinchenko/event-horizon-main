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
	public class FactionSerializable : SerializableItem
	{
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
