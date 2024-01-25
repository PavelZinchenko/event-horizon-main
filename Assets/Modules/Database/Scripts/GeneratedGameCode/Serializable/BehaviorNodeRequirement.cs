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
	public class BehaviorNodeRequirementSerializable
	{
		public BehaviorRequirementType Type;
		public DeviceClass DeviceClass;
		public AiDifficultyLevel DifficultyLevel;
		public SizeClass SizeClass;
		public float Value = 1f;
		public BehaviorNodeRequirementSerializable[] Requirements;
	}
}
