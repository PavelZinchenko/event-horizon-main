


//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class BulletBody 
	{
		partial void OnDataDeserialized(BulletBodySerializable serializable, Database.Loader loader);

		public static BulletBody Create(BulletBodySerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new BulletBody(serializable, loader);
		}

		private BulletBody(BulletBodySerializable serializable, Database.Loader loader)
		{
			Size = UnityEngine.Mathf.Clamp(serializable.Size, 0f, 1000f);
			Length = UnityEngine.Mathf.Clamp(serializable.Length, 0f, 1000f);
			Velocity = UnityEngine.Mathf.Clamp(serializable.Velocity, 0f, 1000f);
			ParentVelocityEffect = UnityEngine.Mathf.Clamp(serializable.ParentVelocityEffect, -1000f, 1000f);
			AttachedToParent = serializable.AttachedToParent;
			Range = UnityEngine.Mathf.Clamp(serializable.Range, 0f, 1E+09f);
			Lifetime = UnityEngine.Mathf.Clamp(serializable.Lifetime, 0f, 1E+09f);
			Weight = UnityEngine.Mathf.Clamp(serializable.Weight, 0f, 1E+09f);
			HitPoints = UnityEngine.Mathf.Clamp(serializable.HitPoints, 0, 999999999);
			Color = new ColorData(serializable.Color);
			BulletPrefab = loader?.GetBulletPrefab(new ItemId<BulletPrefab>(serializable.BulletPrefab)) ?? BulletPrefab.DefaultValue;
			EnergyCost = UnityEngine.Mathf.Clamp(serializable.EnergyCost, 0f, 1E+09f);
			CanBeDisarmed = serializable.CanBeDisarmed;
			FriendlyFire = serializable.FriendlyFire;
			DetonateWhenDestroyed = serializable.DetonateWhenDestroyed;
			AiBulletBehavior = serializable.AiBulletBehavior;

			OnDataDeserialized(serializable, loader);
		}

		public float Size { get; private set; }
		public float Length { get; private set; }
		public float Velocity { get; private set; }
		public float ParentVelocityEffect { get; private set; }
		public bool AttachedToParent { get; private set; }
		public float Range { get; private set; }
		public float Lifetime { get; private set; }
		public float Weight { get; private set; }
		public int HitPoints { get; private set; }
		public ColorData Color { get; private set; }
		public BulletPrefab BulletPrefab { get; private set; }
		public float EnergyCost { get; private set; }
		public bool CanBeDisarmed { get; private set; }
		public bool FriendlyFire { get; private set; }
		public bool DetonateWhenDestroyed { get; private set; }
		public AiBulletBehavior AiBulletBehavior { get; private set; }

		public static BulletBody DefaultValue { get; private set; }= new(new(), null);
	}
}
