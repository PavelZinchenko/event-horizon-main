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
	public class GameObjectPrefabSerializable : SerializableItem
	{
		public GameObjectPrefabSerializable()
		{
			ItemType = ItemType.GameObjectPrefab;
			FileName = "GameObjectPrefab.json";
		}

		public ObjectPrefabType Type;
		public string Image1;
		public string Image2;
	}
}
