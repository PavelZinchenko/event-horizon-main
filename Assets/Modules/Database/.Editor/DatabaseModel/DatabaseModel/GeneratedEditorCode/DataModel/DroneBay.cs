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
	public partial class DroneBay
	{
		partial void OnDataDeserialized(DroneBaySerializable serializable, Database database);
		partial void OnDataSerialized(ref DroneBaySerializable serializable);

		public static DroneBay Create(DroneBaySerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new DroneBay(serializable, database);
		}

		private DroneBay(DroneBaySerializable serializable, Database database)
		{
			try
			{
				Id = new ItemId<DroneBay>(serializable.Id, serializable.FileName);
				EnergyConsumption = new NumericValue<float>(serializable.EnergyConsumption, 0f, 1E+09f);
				PassiveEnergyConsumption = new NumericValue<float>(serializable.PassiveEnergyConsumption, 0f, 1E+09f);
				Range = new NumericValue<float>(serializable.Range, 1f, 1000f);
				DamageMultiplier = new NumericValue<float>(serializable.DamageMultiplier, 0.01f, 1000f);
				DefenseMultiplier = new NumericValue<float>(serializable.DefenseMultiplier, 0.01f, 1000f);
				SpeedMultiplier = new NumericValue<float>(serializable.SpeedMultiplier, 0.01f, 1000f);
				BuildExtraCycles = new NumericValue<int>(serializable.BuildExtraCycles, 0, 100);
				Capacity = new NumericValue<int>(serializable.Capacity, 1, 1000);
				ActivationType = serializable.ActivationType;
				LaunchSound = serializable.LaunchSound;
				LaunchEffectPrefab = serializable.LaunchEffectPrefab;
				ControlButtonIcon = serializable.ControlButtonIcon;
				DefensiveDroneAI = database.GetBehaviorTreeId(serializable.DefensiveDroneAI);
				OffensiveDroneAI = database.GetBehaviorTreeId(serializable.OffensiveDroneAI);
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(this.GetType() + ": deserialization failed. " + serializable.FileName + " (" + serializable.Id + ")", e);
			}
			OnDataDeserialized(serializable, database);
		}

		public void Save(DroneBaySerializable serializable)
		{
			serializable.EnergyConsumption = EnergyConsumption.Value;
			serializable.PassiveEnergyConsumption = PassiveEnergyConsumption.Value;
			serializable.Range = Range.Value;
			serializable.DamageMultiplier = DamageMultiplier.Value;
			serializable.DefenseMultiplier = DefenseMultiplier.Value;
			serializable.SpeedMultiplier = SpeedMultiplier.Value;
			serializable.BuildExtraCycles = BuildExtraCycles.Value;
			serializable.Capacity = Capacity.Value;
			serializable.ActivationType = ActivationType;
			serializable.LaunchSound = LaunchSound;
			serializable.LaunchEffectPrefab = LaunchEffectPrefab;
			serializable.ControlButtonIcon = ControlButtonIcon;
			serializable.DefensiveDroneAI = DefensiveDroneAI.Value;
			serializable.OffensiveDroneAI = OffensiveDroneAI.Value;
			OnDataSerialized(ref serializable);
		}

		public readonly ItemId<DroneBay> Id;

		public NumericValue<float> EnergyConsumption = new NumericValue<float>(0, 0f, 1E+09f);
		public NumericValue<float> PassiveEnergyConsumption = new NumericValue<float>(0, 0f, 1E+09f);
		public NumericValue<float> Range = new NumericValue<float>(0, 1f, 1000f);
		public NumericValue<float> DamageMultiplier = new NumericValue<float>(0, 0.01f, 1000f);
		public NumericValue<float> DefenseMultiplier = new NumericValue<float>(0, 0.01f, 1000f);
		public NumericValue<float> SpeedMultiplier = new NumericValue<float>(0, 0.01f, 1000f);
		public NumericValue<int> BuildExtraCycles = new NumericValue<int>(0, 0, 100);
		public NumericValue<int> Capacity = new NumericValue<int>(0, 1, 1000);
		public ActivationType ActivationType;
		public string LaunchSound;
		public string LaunchEffectPrefab;
		public string ControlButtonIcon;
		public ItemId<BehaviorTreeModel> DefensiveDroneAI = ItemId<BehaviorTreeModel>.Empty;
		public ItemId<BehaviorTreeModel> OffensiveDroneAI = ItemId<BehaviorTreeModel>.Empty;

		public static DroneBay DefaultValue { get; private set; }
	}
}
