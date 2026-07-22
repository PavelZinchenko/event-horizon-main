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
	public class StatUpgradeTemplateSerializable : SerializableItem
	{
		public StatUpgradeTemplateSerializable()
		{
			ItemType = ItemType.StatUpgradeTemplate;
			FileName = "StatUpgradeTemplate.json";
		}

		[DefaultValue(20)]
		public int MaxLevel = 20;
		[DefaultValue("")]
		public string Stars;
		[DefaultValue("")]
		public string Credits;
		[DefaultValue("")]
		public string Resources;
	}
}
