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
		public float ImageScale = 1f;
		public float Thickness = 0.1f;
		public float AspectRatio = 1f;
		public float ImageOffset;
		public float Length;
		public float Offset1;
		public float Offset2;
		public float Angle1;
		public float Angle2;
	}
}
