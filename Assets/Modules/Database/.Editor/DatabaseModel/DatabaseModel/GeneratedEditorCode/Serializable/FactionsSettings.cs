//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using EditorDatabase.Enums;
using EditorDatabase.Model;

namespace EditorDatabase.Serializable
{
	[Serializable]
	public class FactionsSettingsSerializable : SerializableItem
	{
		[DefaultValue("MIN(1000, 300 + 5*distance)")]
		public string StarbaseInitialDefense = "MIN(1000, 300 + 5*distance)";
		[DefaultValue(50)]
		public int StarbaseMinDefense = 50;
		[DefaultValue(10)]
		public int DefenseLossPerEnemyDefeated = 10;
	}
}
