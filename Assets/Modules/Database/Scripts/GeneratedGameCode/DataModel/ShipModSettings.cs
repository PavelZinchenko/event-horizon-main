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
	public partial class ShipModSettings 
	{
		partial void OnDataDeserialized(ShipModSettingsSerializable serializable, Database.Loader loader);

		public static ShipModSettings Create(ShipModSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ShipModSettings(serializable, loader);
		}

		private ShipModSettings(ShipModSettingsSerializable serializable, Database.Loader loader)
		{
			RemoveWeaponSlotMod = serializable.RemoveWeaponSlotMod;
			RemoveUnlimitedRespawnMod = serializable.RemoveUnlimitedRespawnMod;
			RemoveEnergyRechargeCdMod = serializable.RemoveEnergyRechargeCdMod;
			RemoveShieldRechargeCdMod = serializable.RemoveShieldRechargeCdMod;
			RemoveBiggerSatellitesMod = serializable.RemoveBiggerSatellitesMod;
			HeatDefenseValue = Utilites.MathEx.Clamp(serializable.HeatDefenseValue, 0f, 10f);
			KineticDefenseValue = Utilites.MathEx.Clamp(serializable.KineticDefenseValue, 0f, 10f);
			EnergyDefenseValue = Utilites.MathEx.Clamp(serializable.EnergyDefenseValue, 0f, 10f);
			RegenerationValue = Utilites.MathEx.Clamp(serializable.RegenerationValue, 0f, 1f);
			RegenerationArmor = Utilites.MathEx.Clamp(serializable.RegenerationArmor, 0f, 1f);
			WeightReduction = Utilites.MathEx.Clamp(serializable.WeightReduction, 0f, 1f);
			AttackReduction = Utilites.MathEx.Clamp(serializable.AttackReduction, 0f, 1f);
			EnergyReduction = Utilites.MathEx.Clamp(serializable.EnergyReduction, 0f, 1f);
			ShieldReduction = Utilites.MathEx.Clamp(serializable.ShieldReduction, 0f, 1f);

			OnDataDeserialized(serializable, loader);
		}

		public bool RemoveWeaponSlotMod { get; private set; }
		public bool RemoveUnlimitedRespawnMod { get; private set; }
		public bool RemoveEnergyRechargeCdMod { get; private set; }
		public bool RemoveShieldRechargeCdMod { get; private set; }
		public bool RemoveBiggerSatellitesMod { get; private set; }
		public float HeatDefenseValue { get; private set; }
		public float KineticDefenseValue { get; private set; }
		public float EnergyDefenseValue { get; private set; }
		public float RegenerationValue { get; private set; }
		public float RegenerationArmor { get; private set; }
		public float WeightReduction { get; private set; }
		public float AttackReduction { get; private set; }
		public float EnergyReduction { get; private set; }
		public float ShieldReduction { get; private set; }

		public static ShipModSettings DefaultValue { get; private set; }
	}
}
