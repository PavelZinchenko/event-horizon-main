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
	public class VisualEffectElementSerializable
	{
		public VisualEffectType Type;
		public string Image;
		public ColorMode ColorMode;
		public string Color;
		public int Quantity = 1;
		public float Size = 1f;
		public float GrowthRate;
		public float TurnRate;
		public float StartTime;
		public float Lifetime = 1f;
		public float ParticleSize = 1f;
		public UnityEngine.Vector2 Offset;
		public float Rotation;
		public bool Loop;
		public bool Inverse;
		public bool UseRealTime;
	}
}
