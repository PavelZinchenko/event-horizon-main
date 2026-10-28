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
	public class RuinsSettingsSerializable : SerializableItem
	{
		public int[] ExcludedSatellites;
		public string MaxSatelliteCells = "MAX(6, FLOOR(level/2))";
	}
}
