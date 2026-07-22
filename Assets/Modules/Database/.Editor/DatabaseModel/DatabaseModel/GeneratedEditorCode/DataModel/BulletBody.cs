//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using EditorDatabase.Enums;
using EditorDatabase.Serializable;
using EditorDatabase.Model;

namespace EditorDatabase.DataModel
{
	public partial class BulletBody
	{
		partial void OnDataDeserialized(BulletBodySerializable serializable, Database database);
		partial void OnDataSerialized(ref BulletBodySerializable serializable);

		public static BulletBody Create(BulletBodySerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new BulletBody(serializable, database);
		}

		public BulletBody() {}

		private BulletBody(BulletBodySerializable serializable, Database database)
		{
			Size = new NumericValue<float>(serializable.Size, 0f, 1000f);
			Length = new NumericValue<float>(serializable.Length, 0f, 1000f);
			Velocity = new NumericValue<float>(serializable.Velocity, 0f, 1000f);
			ParentVelocityEffect = new NumericValue<float>(serializable.ParentVelocityEffect, -1000f, 1000f);
			AttachedToParent = serializable.AttachedToParent;
			Range = new NumericValue<float>(serializable.Range, 0f, 1E+09f);
			Lifetime = new NumericValue<float>(serializable.Lifetime, 0f, 1E+09f);
			Weight = new NumericValue<float>(serializable.Weight, 0f, 1E+09f);
			HitPoints = new NumericValue<int>(serializable.HitPoints, 0, 999999999);
			Color = Helpers.ColorFromString(serializable.Color);
			BulletPrefab = database.GetBulletPrefabId(serializable.BulletPrefab);
			EnergyCost = new NumericValue<float>(serializable.EnergyCost, 0f, 1E+09f);
			CanBeDisarmed = serializable.CanBeDisarmed;
			FriendlyFire = serializable.FriendlyFire;
			DetonateWhenDestroyed = serializable.DetonateWhenDestroyed;
			AiBulletBehavior = serializable.AiBulletBehavior;
			OnDataDeserialized(serializable, database);
		}

		public BulletBodySerializable Serialize()
		{
			var serializable = new BulletBodySerializable();
			serializable.Size = Size.Value;
			serializable.Length = Length.Value;
			serializable.Velocity = Velocity.Value;
			serializable.ParentVelocityEffect = ParentVelocityEffect.Value;
			serializable.AttachedToParent = AttachedToParent;
			serializable.Range = Range.Value;
			serializable.Lifetime = Lifetime.Value;
			serializable.Weight = Weight.Value;
			serializable.HitPoints = HitPoints.Value;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.BulletPrefab = BulletPrefab.Value;
			serializable.EnergyCost = EnergyCost.Value;
			serializable.CanBeDisarmed = CanBeDisarmed;
			serializable.FriendlyFire = FriendlyFire;
			serializable.DetonateWhenDestroyed = DetonateWhenDestroyed;
			serializable.AiBulletBehavior = AiBulletBehavior;
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public NumericValue<float> Size = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> Length = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> Velocity = new NumericValue<float>(0, 0f, 1000f);
		[TooltipText("How hard is the ammunition affected by the parent velocity during spawn.")]
		public NumericValue<float> ParentVelocityEffect = new NumericValue<float>(0, -1000f, 1000f);
		[TooltipText("Specifies whenever ammunition is attached to the parent ship or ammo. Moving ammo will move in parent's coordinate space")]
		public bool AttachedToParent;
		public NumericValue<float> Range = new NumericValue<float>(0, 0f, 1E+09f);
		public NumericValue<float> Lifetime = new NumericValue<float>(0, 0f, 1E+09f);
		public NumericValue<float> Weight = new NumericValue<float>(0, 0f, 1E+09f);
		public NumericValue<int> HitPoints = new NumericValue<int>(0, 0, 999999999);
		public System.Drawing.Color Color;
		public ItemId<BulletPrefab> BulletPrefab = ItemId<BulletPrefab>.Empty;
		public NumericValue<float> EnergyCost = new NumericValue<float>(0, 0f, 1E+09f);
		public bool CanBeDisarmed;
		public bool FriendlyFire;
		public bool DetonateWhenDestroyed;
		[TooltipText("Hints for AI and auto-aim0 on usage of this weapon")]
		public AiBulletBehavior AiBulletBehavior;

		public static BulletBody DefaultValue { get; private set; }
	}
}
