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
	public class FactionsSettingsSerializable : SerializableItem
	{
		public string StarbaseInitialDefense = "MIN(1000, 300 + 5*distance)";
		public int StarbaseMinDefense = 50;
		public int DefenseLossPerEnemyDefeated = 10;
	}
}
