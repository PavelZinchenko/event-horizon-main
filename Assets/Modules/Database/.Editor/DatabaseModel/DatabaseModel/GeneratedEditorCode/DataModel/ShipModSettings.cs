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
	public partial class ShipModSettings
	{
		partial void OnDataDeserialized(ShipModSettingsSerializable serializable, Database database);
		partial void OnDataSerialized(ref ShipModSettingsSerializable serializable);

		public static ShipModSettings Create(ShipModSettingsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ShipModSettings(serializable, database);
		}

		private ShipModSettings(ShipModSettingsSerializable serializable, Database database)
		{
			RemoveWeaponSlotMod = serializable.RemoveWeaponSlotMod;
			RemoveUnlimitedRespawnMod = serializable.RemoveUnlimitedRespawnMod;
			RemoveEnergyRechargeCdMod = serializable.RemoveEnergyRechargeCdMod;
			RemoveShieldRechargeCdMod = serializable.RemoveShieldRechargeCdMod;
			RemoveBiggerSatellitesMod = serializable.RemoveBiggerSatellitesMod;
			HeatDefenseValue = new NumericValue<float>(serializable.HeatDefenseValue, 0f, 10f);
			KineticDefenseValue = new NumericValue<float>(serializable.KineticDefenseValue, 0f, 10f);
			EnergyDefenseValue = new NumericValue<float>(serializable.EnergyDefenseValue, 0f, 10f);
			RegenerationValue = new NumericValue<float>(serializable.RegenerationValue, 0f, 1f);
			RegenerationArmor = new NumericValue<float>(serializable.RegenerationArmor, 0f, 1f);
			WeightReduction = new NumericValue<float>(serializable.WeightReduction, 0f, 1f);
			AttackReduction = new NumericValue<float>(serializable.AttackReduction, 0f, 1f);
			EnergyReduction = new NumericValue<float>(serializable.EnergyReduction, 0f, 1f);
			ShieldReduction = new NumericValue<float>(serializable.ShieldReduction, 0f, 1f);
			OnDataDeserialized(serializable, database);
		}

		public void Save(ShipModSettingsSerializable serializable)
		{
			serializable.RemoveWeaponSlotMod = RemoveWeaponSlotMod;
			serializable.RemoveUnlimitedRespawnMod = RemoveUnlimitedRespawnMod;
			serializable.RemoveEnergyRechargeCdMod = RemoveEnergyRechargeCdMod;
			serializable.RemoveShieldRechargeCdMod = RemoveShieldRechargeCdMod;
			serializable.RemoveBiggerSatellitesMod = RemoveBiggerSatellitesMod;
			serializable.HeatDefenseValue = HeatDefenseValue.Value;
			serializable.KineticDefenseValue = KineticDefenseValue.Value;
			serializable.EnergyDefenseValue = EnergyDefenseValue.Value;
			serializable.RegenerationValue = RegenerationValue.Value;
			serializable.RegenerationArmor = RegenerationArmor.Value;
			serializable.WeightReduction = WeightReduction.Value;
			serializable.AttackReduction = AttackReduction.Value;
			serializable.EnergyReduction = EnergyReduction.Value;
			serializable.ShieldReduction = ShieldReduction.Value;
			OnDataSerialized(ref serializable);
		}

		public bool RemoveWeaponSlotMod;
		public bool RemoveUnlimitedRespawnMod;
		public bool RemoveEnergyRechargeCdMod;
		public bool RemoveShieldRechargeCdMod;
		public bool RemoveBiggerSatellitesMod;
		public NumericValue<float> HeatDefenseValue = new NumericValue<float>(0, 0f, 10f);
		public NumericValue<float> KineticDefenseValue = new NumericValue<float>(0, 0f, 10f);
		public NumericValue<float> EnergyDefenseValue = new NumericValue<float>(0, 0f, 10f);
		public NumericValue<float> RegenerationValue = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> RegenerationArmor = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> WeightReduction = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> AttackReduction = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> EnergyReduction = new NumericValue<float>(0, 0f, 1f);
		public NumericValue<float> ShieldReduction = new NumericValue<float>(0, 0f, 1f);

		public static ShipModSettings DefaultValue { get; private set; }
	}
}
