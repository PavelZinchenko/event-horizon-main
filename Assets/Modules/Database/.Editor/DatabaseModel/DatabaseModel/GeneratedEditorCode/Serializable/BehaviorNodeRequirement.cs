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
	public class BehaviorNodeRequirementSerializable
	{
		public BehaviorRequirementType Type;
		public DeviceClass DeviceClass;
		public AiDifficultyLevel DifficultyLevel;
		public SizeClass SizeClass;
		[DefaultValue(1f)]
		public float Value = 1f;
		public BehaviorNodeRequirementSerializable[] Requirements;
	}
}
