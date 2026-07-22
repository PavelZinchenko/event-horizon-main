//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using EditorDatabase.Model;
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

		[DefaultValue("")]
		public string Name;
		[DefaultValue("")]
		public string Description;
		[DefaultValue("")]
		public string Icon;
		[DefaultValue("")]
		public string Color;
		public int Price;
	}
}
