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
	public class BarrelSerializable
	{
		public UnityEngine.Vector2 Position;
		public float Rotation;
		public float Offset;
		public int PlatformType;
		public float AutoAimingArc;
		public float RotationSpeed;
		public string WeaponClass;
		public string Image;
		public float Size;
	}
}
