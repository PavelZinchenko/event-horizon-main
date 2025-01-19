


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
	public partial class ShipSettings 
	{
		partial void OnDataDeserialized(ShipSettingsSerializable serializable, Database.Loader loader);

		public static ShipSettings Create(ShipSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ShipSettings(serializable, loader);
		}

		private ShipSettings(ShipSettingsSerializable serializable, Database.Loader loader)
		{
			DefaultWeightPerCell = UnityEngine.Mathf.Clamp(serializable.DefaultWeightPerCell, 1f, 1000000f);
			MinimumWeightPerCell = UnityEngine.Mathf.Clamp(serializable.MinimumWeightPerCell, 1f, 1000000f);
			BaseArmorPoints = UnityEngine.Mathf.Clamp(serializable.BaseArmorPoints, 0f, 1000000f);
			ArmorPointsPerCell = UnityEngine.Mathf.Clamp(serializable.ArmorPointsPerCell, 0f, 1000000f);
			ArmorRepairCooldown = UnityEngine.Mathf.Clamp(serializable.ArmorRepairCooldown, 0f, 60f);
			BaseEnergyPoints = UnityEngine.Mathf.Clamp(serializable.BaseEnergyPoints, 0f, 1000000f);
			BaseEnergyRechargeRate = UnityEngine.Mathf.Clamp(serializable.BaseEnergyRechargeRate, 0f, 1000000f);
			EnergyRechargeCooldown = UnityEngine.Mathf.Clamp(serializable.EnergyRechargeCooldown, 0f, 60f);
			BaseShieldRechargeRate = UnityEngine.Mathf.Clamp(serializable.BaseShieldRechargeRate, 0f, 1000000f);
			ShieldRechargeCooldown = UnityEngine.Mathf.Clamp(serializable.ShieldRechargeCooldown, 0f, 60f);
			BaseDroneReconstructionSpeed = UnityEngine.Mathf.Clamp(serializable.BaseDroneReconstructionSpeed, 0f, 100f);
			ShieldCorrosiveResistance = UnityEngine.Mathf.Clamp(serializable.ShieldCorrosiveResistance, 0f, 1f);
			MaxVelocity = UnityEngine.Mathf.Clamp(serializable.MaxVelocity, 5f, 100f);
			MaxAngularVelocity = UnityEngine.Mathf.Clamp(serializable.MaxAngularVelocity, 5f, 100f);
			MaxAcceleration = UnityEngine.Mathf.Clamp(serializable.MaxAcceleration, 5f, 1000f);
			MaxAngularAcceleration = UnityEngine.Mathf.Clamp(serializable.MaxAngularAcceleration, 5f, 1000f);
			DisableCellsExpansions = serializable.DisableCellsExpansions;
			ShipExplosionEffect = loader?.GetVisualEffect(new ItemId<VisualEffect>(serializable.ShipExplosionEffect)) ?? VisualEffect.DefaultValue;
			ShipExplosionSound = new AudioClipId(serializable.ShipExplosionSound);
			DroneExplosionEffect = loader?.GetVisualEffect(new ItemId<VisualEffect>(serializable.DroneExplosionEffect)) ?? VisualEffect.DefaultValue;
			DroneExplosionSound = new AudioClipId(serializable.DroneExplosionSound);

			OnDataDeserialized(serializable, loader);
		}

		public float DefaultWeightPerCell { get; private set; }
		public float MinimumWeightPerCell { get; private set; }
		public float BaseArmorPoints { get; private set; }
		public float ArmorPointsPerCell { get; private set; }
		public float ArmorRepairCooldown { get; private set; }
		public float BaseEnergyPoints { get; private set; }
		public float BaseEnergyRechargeRate { get; private set; }
		public float EnergyRechargeCooldown { get; private set; }
		public float BaseShieldRechargeRate { get; private set; }
		public float ShieldRechargeCooldown { get; private set; }
		public float BaseDroneReconstructionSpeed { get; private set; }
		public float ShieldCorrosiveResistance { get; private set; }
		public float MaxVelocity { get; private set; }
		public float MaxAngularVelocity { get; private set; }
		public float MaxAcceleration { get; private set; }
		public float MaxAngularAcceleration { get; private set; }
		public bool DisableCellsExpansions { get; private set; }
		public VisualEffect ShipExplosionEffect { get; private set; }
		public AudioClipId ShipExplosionSound { get; private set; }
		public VisualEffect DroneExplosionEffect { get; private set; }
		public AudioClipId DroneExplosionSound { get; private set; }

		public static ShipSettings DefaultValue { get; private set; }
	}
}
