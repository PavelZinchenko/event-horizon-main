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
	public class BehaviorTreeNodeSerializable
	{
		public BehaviorNodeType Type;
		public BehaviorNodeRequirementSerializable Requirement;
		public BehaviorTreeNodeSerializable[] Nodes;
		public BehaviorTreeNodeSerializable Node;
		public int ItemId;
		public AiWeaponCategory WeaponType;
		public NodeExecutionMode ExecutionMode;
		public bool Result;
		public float MinValue = 0.1f;
		public float MaxValue = 0.9f;
		public float Cooldown;
		public bool InRange;
		public bool NoDrones;
		public bool UseSystems;
		public DeviceClass DeviceClass;
		public string Text;
		public string Color;
	}
}
