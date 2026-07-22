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
	public class BulletPrefabSerializable : SerializableItem
	{
		public BulletPrefabSerializable()
		{
			ItemType = ItemType.BulletPrefab;
			FileName = "BulletPrefab.json";
		}

		public BulletShape Shape;
		[DefaultValue("")]
		public string Image;
		public float Size;
		public float Margins;
		public float Deformation;
		[DefaultValue("")]
		public string MainColor;
		public ColorMode MainColorMode;
		[DefaultValue("")]
		public string SecondColor;
		public ColorMode SecondColorMode;
	}
}
