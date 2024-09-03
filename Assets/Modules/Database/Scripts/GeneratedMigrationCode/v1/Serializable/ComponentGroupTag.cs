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
	public class ComponentGroupTagSerializable : SerializableItem
	{
		public ComponentGroupTagSerializable()
		{
			ItemType = ItemType.ComponentGroupTag;
			FileName = "ComponentGroupTag.json";
		}

		public int MaxInstallableComponents;
	}
}
