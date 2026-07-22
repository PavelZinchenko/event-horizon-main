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
	public class SpecialEventSettingsSerializable : SerializableItem
	{
		[DefaultValue(true)]
		public bool EnableXmasEvent = true;
		[DefaultValue(24)]
		public int XmasDaysBefore = 24;
		[DefaultValue(15)]
		public int XmasDaysAfter = 15;
		public int XmasQuest;
		public int XmasCombatRules;
		[DefaultValue("1 + credits/500")]
		public string ConvertCreditsToSnowflakes = "1 + credits/500";
		public bool EnableEasterEvent;
		public int EasterDaysBefore;
		public int EasterDaysAfter;
		public int EasterQuest;
		public bool EnableHalloweenEvent;
		public int HalloweenDaysBefore;
		public int HalloweenDaysAfter;
		public int HalloweenQuest;
	}
}
