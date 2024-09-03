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
	public class StatUpgradeTemplateSerializable : SerializableItem
	{
		public StatUpgradeTemplateSerializable()
		{
			ItemType = ItemType.StatUpgradeTemplate;
			FileName = "StatUpgradeTemplate.json";
		}

		public int MaxLevel = 20;
		public string Stars;
		public string Credits;
		public string Resources;
	}
}
