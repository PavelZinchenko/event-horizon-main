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
	public partial class ComponentStats 
	{
		partial void OnDataDeserialized(ComponentStatsSerializable serializable, Database.Loader loader);

		public static ComponentStats Create(ComponentStatsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ComponentStats(serializable, loader);
		}

		private ComponentStats(ComponentStatsSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<ComponentStats>(serializable.Id);
			loader.AddComponentStats(serializable.Id, this);

			Type = serializable.Type;
			ArmorPoints = Utilites.MathEx.Clamp(serializable.ArmorPoints, -1000000f, 1000000f);
			ArmorRepairRate = Utilites.MathEx.Clamp(serializable.ArmorRepairRate, -1000000f, 1000000f);
			ArmorRepairCooldownModifier = Utilites.MathEx.Clamp(serializable.ArmorRepairCooldownModifier, -1f, 1f);
			EnergyPoints = Utilites.MathEx.Clamp(serializable.EnergyPoints, -1000000f, 1000000f);
			EnergyRechargeRate = Utilites.MathEx.Clamp(serializable.EnergyRechargeRate, -1000000f, 1000000f);
			EnergyRechargeCooldownModifier = Utilites.MathEx.Clamp(serializable.EnergyRechargeCooldownModifier, -5f, 5f);
			ShieldPoints = Utilites.MathEx.Clamp(serializable.ShieldPoints, -1000000f, 1000000f);
			ShieldRechargeRate = Utilites.MathEx.Clamp(serializable.ShieldRechargeRate, -1000000f, 1000000f);
			ShieldRechargeCooldownModifier = Utilites.MathEx.Clamp(serializable.ShieldRechargeCooldownModifier, -5f, 5f);
			Weight = Utilites.MathEx.Clamp(serializable.Weight, -1000000f, 1000000f);
			RammingDamage = Utilites.MathEx.Clamp(serializable.RammingDamage, -1000000f, 1000000f);
			EnergyAbsorption = Utilites.MathEx.Clamp(serializable.EnergyAbsorption, -1000000f, 1000000f);
			KineticResistance = Utilites.MathEx.Clamp(serializable.KineticResistance, -1000000f, 1000000f);
			EnergyResistance = Utilites.MathEx.Clamp(serializable.EnergyResistance, -1000000f, 1000000f);
			ThermalResistance = Utilites.MathEx.Clamp(serializable.ThermalResistance, -1000000f, 1000000f);
			EnginePower = Utilites.MathEx.Clamp(serializable.EnginePower, 0f, 2000f);
			TurnRate = Utilites.MathEx.Clamp(serializable.TurnRate, 0f, 2000f);
			Autopilot = serializable.Autopilot;
			DroneRangeModifier = Utilites.MathEx.Clamp(serializable.DroneRangeModifier, -50f, 50f);
			DroneDamageModifier = Utilites.MathEx.Clamp(serializable.DroneDamageModifier, -50f, 50f);
			DroneDefenseModifier = Utilites.MathEx.Clamp(serializable.DroneDefenseModifier, -50f, 50f);
			DroneSpeedModifier = Utilites.MathEx.Clamp(serializable.DroneSpeedModifier, -50f, 50f);
			DronesBuiltPerSecond = Utilites.MathEx.Clamp(serializable.DronesBuiltPerSecond, 0f, 100f);
			DroneBuildTimeModifier = Utilites.MathEx.Clamp(serializable.DroneBuildTimeModifier, 0f, 100f);
			WeaponFireRateModifier = Utilites.MathEx.Clamp(serializable.WeaponFireRateModifier, -100f, 100f);
			WeaponDamageModifier = Utilites.MathEx.Clamp(serializable.WeaponDamageModifier, -100f, 100f);
			WeaponRangeModifier = Utilites.MathEx.Clamp(serializable.WeaponRangeModifier, -100f, 100f);
			WeaponEnergyCostModifier = Utilites.MathEx.Clamp(serializable.WeaponEnergyCostModifier, -100f, 100f);
			AutoAimingArc = Utilites.MathEx.Clamp(serializable.AutoAimingArc, 0f, 360f);
			TurretTurnSpeed = Utilites.MathEx.Clamp(serializable.TurretTurnSpeed, -1000f, 1000f);

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<ComponentStats> Id;

		public ComponentStatsType Type { get; private set; }
		public float ArmorPoints { get; private set; }
		public float ArmorRepairRate { get; private set; }
		public float ArmorRepairCooldownModifier { get; private set; }
		public float EnergyPoints { get; private set; }
		public float EnergyRechargeRate { get; private set; }
		public float EnergyRechargeCooldownModifier { get; private set; }
		public float ShieldPoints { get; private set; }
		public float ShieldRechargeRate { get; private set; }
		public float ShieldRechargeCooldownModifier { get; private set; }
		public float Weight { get; private set; }
		public float RammingDamage { get; private set; }
		public float EnergyAbsorption { get; private set; }
		public float KineticResistance { get; private set; }
		public float EnergyResistance { get; private set; }
		public float ThermalResistance { get; private set; }
		public float EnginePower { get; private set; }
		public float TurnRate { get; private set; }
		public bool Autopilot { get; private set; }
		public float DroneRangeModifier { get; private set; }
		public float DroneDamageModifier { get; private set; }
		public float DroneDefenseModifier { get; private set; }
		public float DroneSpeedModifier { get; private set; }
		public float DronesBuiltPerSecond { get; private set; }
		public float DroneBuildTimeModifier { get; private set; }
		public float WeaponFireRateModifier { get; private set; }
		public float WeaponDamageModifier { get; private set; }
		public float WeaponRangeModifier { get; private set; }
		public float WeaponEnergyCostModifier { get; private set; }
		public float AutoAimingArc { get; private set; }
		public float TurretTurnSpeed { get; private set; }

		public static ComponentStats DefaultValue { get; private set; }
	}
}
