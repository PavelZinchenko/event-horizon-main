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
	public class BulletControllerSerializable
	{
		public BulletControllerType Type;
		[DefaultValue(1f)]
		public float StartingVelocityModifier = 1f;
		public bool IgnoreRotation;
		public bool SmartAim;
		public float Lifetime;
		[DefaultValue("0")]
		public string X = "0";
		[DefaultValue("0")]
		public string Y = "0";
		[DefaultValue("0")]
		public string Rotation = "0";
		[DefaultValue("1")]
		public string Size = "1";
		[DefaultValue("1")]
		public string Length = "1";
	}
}
