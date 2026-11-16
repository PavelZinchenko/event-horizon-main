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
	public class SatelliteMountPointSerializable
	{
		public GameDatabase.Model.Vector2Data Position;
		public float Direction;
		public float RotationLimit = 100f;
		public SizeClass MinSizeClass;
		public SizeClass MaxSizeClass;
	}
}
