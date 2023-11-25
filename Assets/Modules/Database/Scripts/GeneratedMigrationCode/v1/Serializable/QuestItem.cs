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
	public class QuestItemSerializable : SerializableItem
	{
		public QuestItemSerializable()
		{
			ItemType = ItemType.QuestItem;
			FileName = "QuestItem.json";
		}

		public string Name;
		public string Description;
		public string Icon;
		public string Color;
		public int Price;
	}
}
