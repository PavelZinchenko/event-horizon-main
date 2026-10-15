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
			EnergyResistance = Utilites.MathEx.Clamp(serializable.EnergyResistance, -100f, 100f);
			KineticResistance = Utilites.MathEx.Clamp(serializable.KineticResistance, -100f, 100f);
			HeatResistance = Utilites.MathEx.Clamp(serializable.HeatResistance, -100f, 100f);
			ShipWeightBonus = Utilites.MathEx.Clamp(serializable.ShipWeightBonus, -1f, 100f);
			EquipmentWeightBonus = Utilites.MathEx.Clamp(serializable.EquipmentWeightBonus, -1f, 100f);
			VelocityBonus = Utilites.MathEx.Clamp(serializable.VelocityBonus, -1f, 100f);
			TurnRateBonus = Utilites.MathEx.Clamp(serializable.TurnRateBonus, -1f, 100f);
			ArmorBonus = Utilites.MathEx.Clamp(serializable.ArmorBonus, -1f, 100f);
			ShieldBonus = Utilites.MathEx.Clamp(serializable.ShieldBonus, -1f, 100f);
			EnergyBonus = Utilites.MathEx.Clamp(serializable.EnergyBonus, -1f, 100f);
			DroneBuildSpeedBonus = Utilites.MathEx.Clamp(serializable.DroneBuildSpeedBonus, -1f, 100f);
			DroneAttackBonus = Utilites.MathEx.Clamp(serializable.DroneAttackBonus, -1f, 100f);
			DroneDefenseBonus = Utilites.MathEx.Clamp(serializable.DroneDefenseBonus, -1f, 100f);
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
		public float DroneBuildSpeedBonus { get; private set; }
		public float DroneAttackBonus { get; private set; }
		public float DroneDefenseBonus { get; private set; }
		public bool Regeneration { get; private set; }
		public ImmutableCollection<Device> BuiltinDevices { get; private set; }

		public static ShipFeatures DefaultValue { get; private set; }= new(new(), null);
	}
}
