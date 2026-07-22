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
	public partial class AmmunitionObsolete
	{
		partial void OnDataDeserialized(AmmunitionObsoleteSerializable serializable, Database database);
		partial void OnDataSerialized(ref AmmunitionObsoleteSerializable serializable);

		public static AmmunitionObsolete Create(AmmunitionObsoleteSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new AmmunitionObsolete(serializable, database);
		}

		private AmmunitionObsolete(AmmunitionObsoleteSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<AmmunitionObsolete>(serializable.Id, serializable.FileName);
				AmmunitionClass = serializable.AmmunitionClass;
				DamageType = serializable.DamageType;
				Impulse = new NumericValue<float>(serializable.Impulse, 0f, 10f);
				Recoil = new NumericValue<float>(serializable.Recoil, 0f, 10f);
				Size = new NumericValue<float>(serializable.Size, 0f, 1000f);
				InitialPosition = serializable.InitialPosition;
				AreaOfEffect = new NumericValue<float>(serializable.AreaOfEffect, 0f, 1000f);
				Damage = new NumericValue<float>(serializable.Damage, 0f, 1E+09f);
				Range = new NumericValue<float>(serializable.Range, 0f, 1000f);
				Velocity = new NumericValue<float>(serializable.Velocity, 0f, 1000f);
				LifeTime = new NumericValue<float>(serializable.LifeTime, 0f, 1E+09f);
				HitPoints = new NumericValue<int>(serializable.HitPoints, 0, 999999999);
				IgnoresShipVelocity = serializable.IgnoresShipVelocity;
				EnergyCost = new NumericValue<float>(serializable.EnergyCost, 0f, 1E+09f);
				CoupledAmmunition = database.GetAmmunitionObsoleteId(serializable.CoupledAmmunitionId);
				Color = Helpers.ColorFromString(serializable.Color);
				FireSound = serializable.FireSound;
				HitSound = serializable.HitSound;
				HitEffectPrefab = serializable.HitEffectPrefab;
				BulletPrefab = serializable.BulletPrefab;
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(AmmunitionObsoleteSerializable serializable)
		{
			serializable.AmmunitionClass = AmmunitionClass;
			serializable.DamageType = DamageType;
			serializable.Impulse = Impulse.Value;
			serializable.Recoil = Recoil.Value;
			serializable.Size = Size.Value;
			serializable.InitialPosition = InitialPosition;
			serializable.AreaOfEffect = AreaOfEffect.Value;
			serializable.Damage = Damage.Value;
			serializable.Range = Range.Value;
			serializable.Velocity = Velocity.Value;
			serializable.LifeTime = LifeTime.Value;
			serializable.HitPoints = HitPoints.Value;
			serializable.IgnoresShipVelocity = IgnoresShipVelocity;
			serializable.EnergyCost = EnergyCost.Value;
			serializable.CoupledAmmunitionId = CoupledAmmunition.Value;
			serializable.Color = Helpers.ColorToString(Color);
			serializable.FireSound = FireSound;
			serializable.HitSound = HitSound;
			serializable.HitEffectPrefab = HitEffectPrefab;
			serializable.BulletPrefab = BulletPrefab;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<AmmunitionObsolete> Id;

		public AmmunitionClassObsolete AmmunitionClass;
		public DamageType DamageType;
		public NumericValue<float> Impulse = new NumericValue<float>(0, 0f, 10f);
		public NumericValue<float> Recoil = new NumericValue<float>(0, 0f, 10f);
		public NumericValue<float> Size = new NumericValue<float>(0, 0f, 1000f);
		public Vector2 InitialPosition;
		public NumericValue<float> AreaOfEffect = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> Damage = new NumericValue<float>(0, 0f, 1E+09f);
		public NumericValue<float> Range = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> Velocity = new NumericValue<float>(0, 0f, 1000f);
		public NumericValue<float> LifeTime = new NumericValue<float>(0, 0f, 1E+09f);
		public NumericValue<int> HitPoints = new NumericValue<int>(0, 0, 999999999);
		public bool IgnoresShipVelocity;
		public NumericValue<float> EnergyCost = new NumericValue<float>(0, 0f, 1E+09f);
		public ItemId<AmmunitionObsolete> CoupledAmmunition = ItemId<AmmunitionObsolete>.Empty;
		public System.Drawing.Color Color;
		public string FireSound;
		public string HitSound;
		public string HitEffectPrefab;
		public string BulletPrefab;

		public static AmmunitionObsolete DefaultValue { get; private set; }
	}
}
