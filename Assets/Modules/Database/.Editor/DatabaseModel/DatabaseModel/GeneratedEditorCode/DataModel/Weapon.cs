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
	public partial class Weapon
	{
		partial void OnDataDeserialized(WeaponSerializable serializable, Database database);
		partial void OnDataSerialized(ref WeaponSerializable serializable);

		public static Weapon Create(WeaponSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new Weapon(serializable, database);
		}

		private Weapon(WeaponSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<Weapon>(serializable.Id, serializable.FileName);
				WeaponClass = serializable.WeaponClass;
				FireRate = new NumericValue<float>(serializable.FireRate, 0f, 100f);
				Spread = new NumericValue<float>(serializable.Spread, 0f, 360f);
				Magazine = new NumericValue<int>(serializable.Magazine, 0, 999999999);
				ActivationType = serializable.ActivationType;
				ShotSound = serializable.ShotSound;
				ChargeSound = serializable.ChargeSound;
				ShotEffectPrefab = serializable.ShotEffectPrefab;
				VisualEffect = database.GetVisualEffectId(serializable.VisualEffect);
				EffectSize = new NumericValue<float>(serializable.EffectSize, 0f, 100f);
				ControlButtonIcon = serializable.ControlButtonIcon;
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(WeaponSerializable serializable)
		{
			serializable.WeaponClass = WeaponClass;
			serializable.FireRate = FireRate.Value;
			serializable.Spread = Spread.Value;
			serializable.Magazine = Magazine.Value;
			serializable.ActivationType = ActivationType;
			serializable.ShotSound = ShotSound;
			serializable.ChargeSound = ChargeSound;
			serializable.ShotEffectPrefab = ShotEffectPrefab;
			serializable.VisualEffect = VisualEffect.Value;
			serializable.EffectSize = EffectSize.Value;
			serializable.ControlButtonIcon = ControlButtonIcon;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<Weapon> Id;

		public WeaponClass WeaponClass;
		public NumericValue<float> FireRate = new NumericValue<float>(0, 0f, 100f);
		public NumericValue<float> Spread = new NumericValue<float>(0, 0f, 360f);
		public NumericValue<int> Magazine = new NumericValue<int>(0, 0, 999999999);
		public ActivationType ActivationType;
		public string ShotSound;
		public string ChargeSound;
		public string ShotEffectPrefab;
		public ItemId<VisualEffect> VisualEffect = ItemId<VisualEffect>.Empty;
		public NumericValue<float> EffectSize = new NumericValue<float>(0, 0f, 100f);
		public string ControlButtonIcon;

		public static Weapon DefaultValue { get; private set; }
	}
}
