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
	public class SpecialEventSettingsSerializable : SerializableItem
	{
		public SpecialEventSettingsSerializable()
		{
			ItemType = ItemType.SpecialEventSettings;
			FileName = "SpecialEventSettings.json";
		}

		public bool EnableXmasEvent = true;
		public int XmasDaysBefore = 24;
		public int XmasDaysAfter = 15;
		public int XmasQuest;
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
