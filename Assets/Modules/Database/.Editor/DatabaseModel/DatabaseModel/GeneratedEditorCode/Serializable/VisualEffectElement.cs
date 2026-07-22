//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using EditorDatabase.Enums;
using EditorDatabase.Model;

namespace EditorDatabase.Serializable
{
	[Serializable]
	public class VisualEffectElementSerializable
	{
		public VisualEffectType Type;
		[DefaultValue("")]
		public string Image;
		public ColorMode ColorMode;
		[DefaultValue("")]
		public string Color;
		[DefaultValue(1)]
		public int Quantity = 1;
		[DefaultValue(1f)]
		public float Size = 1f;
		public float GrowthRate;
		public float TurnRate;
		public float StartTime;
		[DefaultValue(1f)]
		public float Lifetime = 1f;
		[DefaultValue(1f)]
		public float ParticleSize = 1f;
		public Vector2 Offset;
		public float Rotation;
		public bool Loop;
		public bool Inverse;
		public bool UseRealTime;
	}
}
