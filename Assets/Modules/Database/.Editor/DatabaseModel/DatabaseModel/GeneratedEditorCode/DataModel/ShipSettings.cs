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
	public partial class ShipSettings
	{
		partial void OnDataDeserialized(ShipSettingsSerializable serializable, Database database);
		partial void OnDataSerialized(ref ShipSettingsSerializable serializable);

		public static ShipSettings Create(ShipSettingsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ShipSettings(serializable, database);
		}

		private ShipSettings(ShipSettingsSerializable serializable, Database database)
		{
			DefaultWeightPerCell = new NumericValue<float>(serializable.DefaultWeightPerCell, 1f, 1000000f);
			MinimumWeightPerCell = new NumericValue<float>(serializable.MinimumWeightPerCell, 1f, 1000000f);
			BaseArmorPoints = new NumericValue<float>(serializable.BaseArmorPoints, 0f, 1000000f);
			ArmorPointsPerCell = new NumericValue<float>(serializable.ArmorPointsPerCell, 0f, 1000000f);
			ArmorRepairCooldown = new NumericValue<float>(serializable.ArmorRepairCooldown, 0f, 60f);
			BaseEnergyPoints = new NumericValue<float>(serializable.BaseEnergyPoints, 0f, 1000000f);
			BaseEnergyRechargeRate = new NumericValue<float>(serializable.BaseEnergyRechargeRate, 0f, 1000000f);
			EnergyRechargeCooldown = new NumericValue<float>(serializable.EnergyRechargeCooldown, 0f, 60f);
			BaseShieldRechargeRate = new NumericValue<float>(serializable.BaseShieldRechargeRate, 0f, 1000000f);
			ShieldRechargeCooldown = new NumericValue<float>(serializable.ShieldRechargeCooldown, 0f, 60f);
			BaseDroneReconstructionSpeed = new NumericValue<float>(serializable.BaseDroneReconstructionSpeed, 0f, 100f);
			ShieldCorrosiveResistance = new NumericValue<float>(serializable.ShieldCorrosiveResistance, 0f, 1f);
			MaxVelocity = new NumericValue<float>(serializable.MaxVelocity, 5f, 100f);
			MaxAngularVelocity = new NumericValue<float>(serializable.MaxAngularVelocity, 5f, 100f);
			MaxAcceleration = new NumericValue<float>(serializable.MaxAcceleration, 5f, 1000f);
			MaxAngularAcceleration = new NumericValue<float>(serializable.MaxAngularAcceleration, 5f, 1000f);
			DisableCellsExpansions = serializable.DisableCellsExpansions;
			ShipExplosionEffect = database.GetVisualEffectId(serializable.ShipExplosionEffect);
			ShipExplosionSound = serializable.ShipExplosionSound;
			DroneExplosionEffect = database.GetVisualEffectId(serializable.DroneExplosionEffect);
			DroneExplosionSound = serializable.DroneExplosionSound;
			OnDataDeserialized(serializable, database);
		}

		public void Save(ShipSettingsSerializable serializable)
		{
			serializable.DefaultWeightPerCell = DefaultWeightPerCell.Value;
			serializable.MinimumWeightPerCell = MinimumWeightPerCell.Value;
			serializable.BaseArmorPoints = BaseArmorPoints.Value;
			serializable.ArmorPointsPerCell = ArmorPointsPerCell.Value;
			serializable.ArmorRepairCooldown = ArmorRepairCooldown.Value;
			serializable.BaseEnergyPoints = BaseEnergyPoints.Value;
			serializable.BaseEnergyRechargeRate = BaseEnergyRechargeRate.Value;
			serializable.EnergyRechargeCooldown = EnergyRechargeCooldown.Value;
			serializable.BaseShieldRechargeRate = BaseShieldRechargeRate.Value;
			serializable.ShieldRechargeCooldown = ShieldRechargeCooldown.Value;
			serializable.BaseDroneReconstructionSpeed = BaseDroneReconstructionSpeed.Value;
			serializable.ShieldCorrosiveResistance = ShieldCorrosiveResistance.Value;
			serializable.MaxVelocity = MaxVelocity.Value;
			serializable.MaxAngularVelocity = MaxAngularVelocity.Value;
			serializable.MaxAcceleration = MaxAcceleration.Value;
			serializable.MaxAngularAcceleration = MaxAngularAcceleration.Value;
			serializable.DisableCellsExpansions = DisableCellsExpansions;
			serializable.ShipExplosionEffect = ShipExplosionEffect.Value;
			serializable.ShipExplosionSound = ShipExplosionSound;
			serializable.DroneExplosionEffect = DroneExplosionEffect.Value;
			serializable.DroneExplosionSound = DroneExplosionSound;
			OnDataSerialized(ref serializable);
		}

		public NumericValue<float> DefaultWeightPerCell = new NumericValue<float>(0, 1f, 1000000f);
		public NumericValue<float> MinimumWeightPerCell = new NumericValue<float>(0, 1f, 1000000f);
		public NumericValue<float> BaseArmorPoints = new NumericValue<float>(0, 0f, 1000000f);
		public NumericValue<float> ArmorPointsPerCell = new NumericValue<float>(0, 0f, 1000000f);
		public NumericValue<float> ArmorRepairCooldown = new NumericValue<float>(0, 0f, 60f);
		public NumericValue<float> BaseEnergyPoints = new NumericValue<float>(0, 0f, 1000000f);
		public NumericValue<float> BaseEnergyRechargeRate = new NumericValue<float>(0, 0f, 1000000f);
		public NumericValue<float> EnergyRechargeCooldown = new NumericValue<float>(0, 0f, 60f);
		public NumericValue<float> BaseShieldRechargeRate = new NumericValue<float>(0, 0f, 1000000f);
		public NumericValue<float> ShieldRechargeCooldown = new NumericValue<float>(0, 0f, 60f);
		public NumericValue<float> BaseDroneReconstructionSpeed = new NumericValue<float>(0, 0f, 100f);
		public NumericValue<float> ShieldCorrosiveResistance = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> MaxVelocity = new NumericValue<float>(0, 5f, 100f);
		public NumericValue<float> MaxAngularVelocity = new NumericValue<float>(0, 5f, 100f);
		public NumericValue<float> MaxAcceleration = new NumericValue<float>(0, 5f, 1000f);
		public NumericValue<float> MaxAngularAcceleration = new NumericValue<float>(0, 5f, 1000f);
		public bool DisableCellsExpansions;
		public ItemId<VisualEffect> ShipExplosionEffect = ItemId<VisualEffect>.Empty;
		public string ShipExplosionSound;
		public ItemId<VisualEffect> DroneExplosionEffect = ItemId<VisualEffect>.Empty;
		public string DroneExplosionSound;

		public static ShipSettings DefaultValue { get; private set; }
	}
}
