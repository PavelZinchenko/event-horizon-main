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
			HeatDefenseValue = UnityEngine.Mathf.Clamp(serializable.HeatDefenseValue, 0f, 1f);
			KineticDefenseValue = UnityEngine.Mathf.Clamp(serializable.KineticDefenseValue, 0f, 1f);
			EnergyDefenseValue = UnityEngine.Mathf.Clamp(serializable.EnergyDefenseValue, 0f, 1f);
			RegenerationValue = UnityEngine.Mathf.Clamp(serializable.RegenerationValue, 0f, 1f);
			RegenerationArmor = UnityEngine.Mathf.Clamp(serializable.RegenerationArmor, 0f, 1f);
			WeightReduction = UnityEngine.Mathf.Clamp(serializable.WeightReduction, 0f, 1f);
			AttackReduction = UnityEngine.Mathf.Clamp(serializable.AttackReduction, 0f, 1f);

			OnDataDeserialized(serializable, loader);
		}

		public bool RemoveWeaponSlotMod { get; private set; }
		public float HeatDefenseValue { get; private set; }
		public float KineticDefenseValue { get; private set; }
		public float EnergyDefenseValue { get; private set; }
		public float RegenerationValue { get; private set; }
		public float RegenerationArmor { get; private set; }
		public float WeightReduction { get; private set; }
		public float AttackReduction { get; private set; }

		public static ShipModSettings DefaultValue { get; private set; }
	}
}
