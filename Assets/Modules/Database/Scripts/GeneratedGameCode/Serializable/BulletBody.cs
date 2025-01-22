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
	public class BulletBodySerializable
	{
		public float Size;
		public float Length;
		public float Velocity;
		public float ParentVelocityEffect = 1f;
		public bool AttachedToParent;
		public float Range;
		public float Lifetime;
		public float Weight;
		public int HitPoints;
		public string Color;
		public int BulletPrefab;
		public float EnergyCost;
		public bool CanBeDisarmed;
		public bool FriendlyFire;
		public bool DetonateWhenDestroyed = true;
		public AiBulletBehavior AiBulletBehavior;
		public BulletTypeObsolete Type;
	}
}
