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
	public class GameObjectPrefabSerializable : SerializableItem
	{
		public GameObjectPrefabSerializable()
		{
			ItemType = ItemType.GameObjectPrefab;
			FileName = "GameObjectPrefab.json";
		}

		public ObjectPrefabType Type;
		[DefaultValue("")]
		public string Image1;
		[DefaultValue("")]
		public string Image2;
		[DefaultValue(1f)]
		public float ImageScale = 1f;
		[DefaultValue(0.1f)]
		public float Thickness = 0.1f;
		[DefaultValue(1f)]
		public float AspectRatio = 1f;
		public float ImageOffset;
		public float Length;
		public float Offset1;
		public float Offset2;
		public float Angle1;
		public float Angle2;
	}
}
