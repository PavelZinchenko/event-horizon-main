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
	public partial class ShipFeatures
	{
		partial void OnDataDeserialized(ShipFeaturesSerializable serializable, Database database);
		partial void OnDataSerialized(ref ShipFeaturesSerializable serializable);

		public static ShipFeatures Create(ShipFeaturesSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new ShipFeatures(serializable, database);
		}

		public ShipFeatures() {}

		private ShipFeatures(ShipFeaturesSerializable serializable, Database database)
		{
			EnergyResistance = new NumericValue<float>(serializable.EnergyResistance, -100f, 100f);
			KineticResistance = new NumericValue<float>(serializable.KineticResistance, -100f, 100f);
			HeatResistance = new NumericValue<float>(serializable.HeatResistance, -100f, 100f);
			ShipWeightBonus = new NumericValue<float>(serializable.ShipWeightBonus, -1f, 10f);
			EquipmentWeightBonus = new NumericValue<float>(serializable.EquipmentWeightBonus, -1f, 10f);
			VelocityBonus = new NumericValue<float>(serializable.VelocityBonus, -1f, 10f);
			TurnRateBonus = new NumericValue<float>(serializable.TurnRateBonus, -1f, 10f);
			ArmorBonus = new NumericValue<float>(serializable.ArmorBonus, -1f, 10f);
			ShieldBonus = new NumericValue<float>(serializable.ShieldBonus, -1f, 10f);
			EnergyBonus = new NumericValue<float>(serializable.EnergyBonus, -1f, 10f);
			DroneBuildSpeedBonus = new NumericValue<float>(serializable.DroneBuildSpeedBonus, -1f, 10f);
			DroneAttackBonus = new NumericValue<float>(serializable.DroneAttackBonus, -1f, 10f);
			DroneDefenseBonus = new NumericValue<float>(serializable.DroneDefenseBonus, -1f, 10f);
			Regeneration = serializable.Regeneration;
			BuiltinDevices = serializable.BuiltinDevices?.Select(id => new Wrapper<Device> { Item = database.GetDeviceId(id) }).ToArray();
			OnDataDeserialized(serializable, database);
		}

		public ShipFeaturesSerializable Serialize()
		{
			var serializable = new ShipFeaturesSerializable();
			serializable.EnergyResistance = EnergyResistance.Value;
			serializable.KineticResistance = KineticResistance.Value;
			serializable.HeatResistance = HeatResistance.Value;
			serializable.ShipWeightBonus = ShipWeightBonus.Value;
			serializable.EquipmentWeightBonus = EquipmentWeightBonus.Value;
			serializable.VelocityBonus = VelocityBonus.Value;
			serializable.TurnRateBonus = TurnRateBonus.Value;
			serializable.ArmorBonus = ArmorBonus.Value;
			serializable.ShieldBonus = ShieldBonus.Value;
			serializable.EnergyBonus = EnergyBonus.Value;
			serializable.DroneBuildSpeedBonus = DroneBuildSpeedBonus.Value;
			serializable.DroneAttackBonus = DroneAttackBonus.Value;
			serializable.DroneDefenseBonus = DroneDefenseBonus.Value;
			serializable.Regeneration = Regeneration;
			if (BuiltinDevices == null || BuiltinDevices.Length == 0)
			    serializable.BuiltinDevices = null;
			else
			    serializable.BuiltinDevices = BuiltinDevices.Select(wrapper => wrapper.Item.Value).ToArray();
			OnDataSerialized(ref serializable);
			return serializable;
		}

		public NumericValue<float> EnergyResistance = new NumericValue<float>(0, -100f, 100f);
		public NumericValue<float> KineticResistance = new NumericValue<float>(0, -100f, 100f);
		public NumericValue<float> HeatResistance = new NumericValue<float>(0, -100f, 100f);
		public NumericValue<float> ShipWeightBonus = new NumericValue<float>(0, -1f, 10f);
		public NumericValue<float> EquipmentWeightBonus = new NumericValue<float>(0, -1f, 10f);
		public NumericValue<float> VelocityBonus = new NumericValue<float>(0, -1f, 10f);
		public NumericValue<float> TurnRateBonus = new NumericValue<float>(0, -1f, 10f);
		public NumericValue<float> ArmorBonus = new NumericValue<float>(0, -1f, 10f);
		public NumericValue<float> ShieldBonus = new NumericValue<float>(0, -1f, 10f);
		public NumericValue<float> EnergyBonus = new NumericValue<float>(0, -1f, 10f);
		public NumericValue<float> DroneBuildSpeedBonus = new NumericValue<float>(0, -1f, 10f);
		public NumericValue<float> DroneAttackBonus = new NumericValue<float>(0, -1f, 10f);
		public NumericValue<float> DroneDefenseBonus = new NumericValue<float>(0, -1f, 10f);
		public bool Regeneration;
		public Wrapper<Device>[] BuiltinDevices;

		public static ShipFeatures DefaultValue { get; private set; }
	}
}
