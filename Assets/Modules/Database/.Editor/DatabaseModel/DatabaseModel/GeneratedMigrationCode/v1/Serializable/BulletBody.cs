//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using EditorDatabase.Model;
using DatabaseMigration.v1.Enums;

namespace DatabaseMigration.v1.Serializable
{
	[Serializable]
	public class BulletBodySerializable
	{
		public float Size;
		public float Length;
		public float Velocity;
		[DefaultValue(1f)]
		public float ParentVelocityEffect = 1f;
		public bool AttachedToParent;
		public float Range;
		public float Lifetime;
		public float Weight;
		public int HitPoints;
		[DefaultValue("")]
		public string Color;
		public int BulletPrefab;
		public float EnergyCost;
		public bool CanBeDisarmed;
		public bool FriendlyFire;
		[DefaultValue(true)]
		public bool DetonateWhenDestroyed = true;
		public AiBulletBehavior AiBulletBehavior;
		public BulletTypeObsolete Type;
	}
}
