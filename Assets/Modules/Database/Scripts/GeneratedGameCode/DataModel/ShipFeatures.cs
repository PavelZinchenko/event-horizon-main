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
	public partial class ShipFeatures
	{
		partial void OnDataDeserialized(ShipFeaturesSerializable serializable, Database.Loader loader);

		public static ShipFeatures Create(ShipFeaturesSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new ShipFeatures(serializable, loader);
		}

		private ShipFeatures(ShipFeaturesSerializable serializable, Database.Loader loader)
		{
			EnergyResistance = UnityEngine.Mathf.Clamp(serializable.EnergyResistance, -100f, 100f);
			KineticResistance = UnityEngine.Mathf.Clamp(serializable.KineticResistance, -100f, 100f);
			HeatResistance = UnityEngine.Mathf.Clamp(serializable.HeatResistance, -100f, 100f);
			ShipWeightBonus = UnityEngine.Mathf.Clamp(serializable.ShipWeightBonus, -1f, 10f);
			EquipmentWeightBonus = UnityEngine.Mathf.Clamp(serializable.EquipmentWeightBonus, -1f, 10f);
			VelocityBonus = UnityEngine.Mathf.Clamp(serializable.VelocityBonus, -1f, 10f);
			TurnRateBonus = UnityEngine.Mathf.Clamp(serializable.TurnRateBonus, -1f, 10f);
			ArmorBonus = UnityEngine.Mathf.Clamp(serializable.ArmorBonus, -1f, 10f);
			ShieldBonus = UnityEngine.Mathf.Clamp(serializable.ShieldBonus, -1f, 10f);
			EnergyBonus = UnityEngine.Mathf.Clamp(serializable.EnergyBonus, -1f, 10f);
			Regeneration = serializable.Regeneration;
			BuiltinDevices = new ImmutableCollection<Device>(serializable.BuiltinDevices?.Select(item => loader.GetDevice(new ItemId<Device>(item), true)));

			OnDataDeserialized(serializable, loader);
		}

		public float EnergyResistance { get; private set; }
		public float KineticResistance { get; private set; }
		public float HeatResistance { get; private set; }
		public float ShipWeightBonus { get; private set; }
		public float EquipmentWeightBonus { get; private set; }
		public float VelocityBonus { get; private set; }
		public float TurnRateBonus { get; private set; }
		public float ArmorBonus { get; private set; }
		public float ShieldBonus { get; private set; }
		public float EnergyBonus { get; private set; }
		public bool Regeneration { get; private set; }
		public ImmutableCollection<Device> BuiltinDevices { get; private set; }

		public static ShipFeatures DefaultValue { get; private set; }= new(new(), null);
	}
}
