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
	public class BulletTriggerSerializable
	{
		public BulletTriggerCondition Condition;
		public BulletEffectType EffectType;
		public int VisualEffect;
		public string AudioClip;
		public int Ammunition;
		public string Color;
		public ColorMode ColorMode;
		public int Quantity;
		public float Size;
		public float Lifetime;
		public float Cooldown;
		public float RandomFactor;
		public float PowerMultiplier;
		public int MaxNestingLevel;
		public string Rotation = "IF(Quantity == 1, 0, RANDOM(0, 360))";
		public string OffsetX = "IF(Quantity == 1, 0, Size / 2)";
		public string OffsetY = "0";
	}
}
