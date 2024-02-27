//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace GameDatabase.Serializable
{
	[Serializable]
	public class BulletControllerSerializable
	{
		public BulletControllerType Type;
		public float StartingVelocityModifier = 0.1f;
		public bool IgnoreRotation;
		public bool SmartAim;
	}
}
