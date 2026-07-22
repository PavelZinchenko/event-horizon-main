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
	public class BarrelSerializable
	{
		public Vector2 Position;
		public float Rotation;
		public float Offset;
		public int PlatformType;
		public float AutoAimingArc;
		public float RotationSpeed;
		[DefaultValue("")]
		public string WeaponClass;
		[DefaultValue("")]
		public string Image;
		public float Size;
	}
}
