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
	public class BulletPrefabSerializable : SerializableItem
	{
		public BulletPrefabSerializable()
		{
			ItemType = ItemType.BulletPrefab;
			FileName = $"{ItemType}.json";
		}

		public BulletShape Shape;
		public string Image;
		public float Size;
		public float Margins;
		public float Deformation;
		public string MainColor;
		public ColorMode MainColorMode;
		public string SecondColor;
		public ColorMode SecondColorMode;
	}
}
