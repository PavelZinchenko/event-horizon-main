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
	public partial class ComponentStats
	{
		partial void OnDataDeserialized(ComponentStatsSerializable serializable, Database database);
		partial void OnDataSerialized(ref ComponentStatsSerializable serializable);

		public static ComponentStats Create(ComponentStatsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ComponentStats(serializable, database);
		}

		private ComponentStats(ComponentStatsSerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<ComponentStats>(serializable.Id, serializable.FileName);
				Type = serializable.Type;
				ArmorPoints = new NumericValue<float>(serializable.ArmorPoints, -1000000f, 1000000f);
				ArmorRepairRate = new NumericValue<float>(serializable.ArmorRepairRate, -1000000f, 1000000f);
				ArmorRepairCooldownModifier = new NumericValue<float>(serializable.ArmorRepairCooldownModifier, -1f, 1f);
				EnergyPoints = new NumericValue<float>(serializable.EnergyPoints, -1000000f, 1000000f);
				EnergyRechargeRate = new NumericValue<float>(serializable.EnergyRechargeRate, -1000000f, 1000000f);
				EnergyRechargeCooldownModifier = new NumericValue<float>(serializable.EnergyRechargeCooldownModifier, -5f, 5f);
				ShieldPoints = new NumericValue<float>(serializable.ShieldPoints, -1000000f, 1000000f);
				ShieldRechargeRate = new NumericValue<float>(serializable.ShieldRechargeRate, -1000000f, 1000000f);
				ShieldRechargeCooldownModifier = new NumericValue<float>(serializable.ShieldRechargeCooldownModifier, -5f, 5f);
				Weight = new NumericValue<float>(serializable.Weight, -1000000f, 1000000f);
				RammingDamage = new NumericValue<float>(serializable.RammingDamage, -1000000f, 1000000f);
				EnergyAbsorption = new NumericValue<float>(serializable.EnergyAbsorption, -1000000f, 1000000f);
				KineticResistance = new NumericValue<float>(serializable.KineticResistance, -1000000f, 1000000f);
				EnergyResistance = new NumericValue<float>(serializable.EnergyResistance, -1000000f, 1000000f);
				ThermalResistance = new NumericValue<float>(serializable.ThermalResistance, -1000000f, 1000000f);
				EnginePower = new NumericValue<float>(serializable.EnginePower, 0f, 2000f);
				TurnRate = new NumericValue<float>(serializable.TurnRate, 0f, 2000f);
				Autopilot = serializable.Autopilot;
				DroneRangeModifier = new NumericValue<float>(serializable.DroneRangeModifier, -50f, 50f);
				DroneDamageModifier = new NumericValue<float>(serializable.DroneDamageModifier, -50f, 50f);
				DroneDefenseModifier = new NumericValue<float>(serializable.DroneDefenseModifier, -50f, 50f);
				DroneSpeedModifier = new NumericValue<float>(serializable.DroneSpeedModifier, -50f, 50f);
				DronesBuiltPerSecond = new NumericValue<float>(serializable.DronesBuiltPerSecond, 0f, 100f);
				DroneBuildTimeModifier = new NumericValue<float>(serializable.DroneBuildTimeModifier, 0f, 100f);
				WeaponFireRateModifier = new NumericValue<float>(serializable.WeaponFireRateModifier, -100f, 100f);
				WeaponDamageModifier = new NumericValue<float>(serializable.WeaponDamageModifier, -100f, 100f);
				WeaponRangeModifier = new NumericValue<float>(serializable.WeaponRangeModifier, -100f, 100f);
				WeaponEnergyCostModifier = new NumericValue<float>(serializable.WeaponEnergyCostModifier, -100f, 100f);
				AutoAimingArc = new NumericValue<float>(serializable.AutoAimingArc, 0f, 360f);
				TurretTurnSpeed = new NumericValue<float>(serializable.TurretTurnSpeed, -1000f, 1000f);
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(ComponentStatsSerializable serializable)
		{
			serializable.Type = Type;
			serializable.ArmorPoints = ArmorPoints.Value;
			serializable.ArmorRepairRate = ArmorRepairRate.Value;
			serializable.ArmorRepairCooldownModifier = ArmorRepairCooldownModifier.Value;
			serializable.EnergyPoints = EnergyPoints.Value;
			serializable.EnergyRechargeRate = EnergyRechargeRate.Value;
			serializable.EnergyRechargeCooldownModifier = EnergyRechargeCooldownModifier.Value;
			serializable.ShieldPoints = ShieldPoints.Value;
			serializable.ShieldRechargeRate = ShieldRechargeRate.Value;
			serializable.ShieldRechargeCooldownModifier = ShieldRechargeCooldownModifier.Value;
			serializable.Weight = Weight.Value;
			serializable.RammingDamage = RammingDamage.Value;
			serializable.EnergyAbsorption = EnergyAbsorption.Value;
			serializable.KineticResistance = KineticResistance.Value;
			serializable.EnergyResistance = EnergyResistance.Value;
			serializable.ThermalResistance = ThermalResistance.Value;
			serializable.EnginePower = EnginePower.Value;
			serializable.TurnRate = TurnRate.Value;
			serializable.Autopilot = Autopilot;
			serializable.DroneRangeModifier = DroneRangeModifier.Value;
			serializable.DroneDamageModifier = DroneDamageModifier.Value;
			serializable.DroneDefenseModifier = DroneDefenseModifier.Value;
			serializable.DroneSpeedModifier = DroneSpeedModifier.Value;
			serializable.DronesBuiltPerSecond = DronesBuiltPerSecond.Value;
			serializable.DroneBuildTimeModifier = DroneBuildTimeModifier.Value;
			serializable.WeaponFireRateModifier = WeaponFireRateModifier.Value;
			serializable.WeaponDamageModifier = WeaponDamageModifier.Value;
			serializable.WeaponRangeModifier = WeaponRangeModifier.Value;
			serializable.WeaponEnergyCostModifier = WeaponEnergyCostModifier.Value;
			serializable.AutoAimingArc = AutoAimingArc.Value;
			serializable.TurretTurnSpeed = TurretTurnSpeed.Value;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<ComponentStats> Id;

		public ComponentStatsType Type;
		public NumericValue<float> ArmorPoints = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> ArmorRepairRate = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> ArmorRepairCooldownModifier = new NumericValue<float>(0, -1f, 1f);
		public NumericValue<float> EnergyPoints = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> EnergyRechargeRate = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> EnergyRechargeCooldownModifier = new NumericValue<float>(0, -5f, 5f);
		public NumericValue<float> ShieldPoints = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> ShieldRechargeRate = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> ShieldRechargeCooldownModifier = new NumericValue<float>(0, -5f, 5f);
		public NumericValue<float> Weight = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> RammingDamage = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> EnergyAbsorption = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> KineticResistance = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> EnergyResistance = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> ThermalResistance = new NumericValue<float>(0, -1000000f, 1000000f);
		public NumericValue<float> EnginePower = new NumericValue<float>(0, 0f, 2000f);
		public NumericValue<float> TurnRate = new NumericValue<float>(0, 0f, 2000f);
		public bool Autopilot;
		public NumericValue<float> DroneRangeModifier = new NumericValue<float>(0, -50f, 50f);
		public NumericValue<float> DroneDamageModifier = new NumericValue<float>(0, -50f, 50f);
		public NumericValue<float> DroneDefenseModifier = new NumericValue<float>(0, -50f, 50f);
		public NumericValue<float> DroneSpeedModifier = new NumericValue<float>(0, -50f, 50f);
		public NumericValue<float> DronesBuiltPerSecond = new NumericValue<float>(0, 0f, 100f);
		public NumericValue<float> DroneBuildTimeModifier = new NumericValue<float>(0, 0f, 100f);
		public NumericValue<float> WeaponFireRateModifier = new NumericValue<float>(0, -100f, 100f);
		public NumericValue<float> WeaponDamageModifier = new NumericValue<float>(0, -100f, 100f);
		public NumericValue<float> WeaponRangeModifier = new NumericValue<float>(0, -100f, 100f);
		public NumericValue<float> WeaponEnergyCostModifier = new NumericValue<float>(0, -100f, 100f);
		public NumericValue<float> AutoAimingArc = new NumericValue<float>(0, 0f, 360f);
		public NumericValue<float> TurretTurnSpeed = new NumericValue<float>(0, -1000f, 1000f);

		public static ComponentStats DefaultValue { get; private set; }
	}
}
