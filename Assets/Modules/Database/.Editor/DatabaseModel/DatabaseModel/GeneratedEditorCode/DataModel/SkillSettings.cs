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
	public partial class SkillSettings
	{
		partial void OnDataDeserialized(SkillSettingsSerializable serializable, Database database);
		partial void OnDataSerialized(ref SkillSettingsSerializable serializable);

		public static SkillSettings Create(SkillSettingsSerializable serializable, Database database)
		{
			if (serializable == null) return DefaultValue;
			return new SkillSettings(serializable, database);
		}

		private SkillSettings(SkillSettingsSerializable serializable, Database database)
		{
			BeatAllEnemiesFactionList = serializable.BeatAllEnemiesFactionList?.Select(id => new Wrapper<Faction> { Item = database.GetFactionId(id) }).ToArray();
			DisableExceedTheLimits = serializable.DisableExceedTheLimits;
			FuelTankCapacity = serializable.FuelTankCapacity;
			AttackBonus = serializable.AttackBonus;
			DefenseBonus = serializable.DefenseBonus;
			ShieldStrengthBonus = serializable.ShieldStrengthBonus;
			ShieldRechargeBonus = serializable.ShieldRechargeBonus;
			ExperienceBonus = serializable.ExperienceBonus;
			FlightSpeed = serializable.FlightSpeed;
			FlightRange = serializable.FlightRange;
			ExplorationLootBonus = serializable.ExplorationLootBonus;
			HeatResistance = serializable.HeatResistance;
			KineticResistance = serializable.KineticResistance;
			EnergyResistance = serializable.EnergyResistance;
			MerchantPriceFactor = serializable.MerchantPriceFactor;
			CraftingPriceFactor = serializable.CraftingPriceFactor;
			CraftingLevelReduction = serializable.CraftingLevelReduction;
			MaxPlayerShipsLevel = new NumericValue<int>(serializable.MaxPlayerShipsLevel, 0, 500);
			IncreasedLevelLimit = new NumericValue<int>(serializable.IncreasedLevelLimit, 0, 1000);
			BaseFuelCapacity = new NumericValue<int>(serializable.BaseFuelCapacity, 10, 2147483647);
			BaseFlightRange = new NumericValue<float>(serializable.BaseFlightRange, 1.5f, 3.402823E+38f);
			BaseFlightSpeed = new NumericValue<float>(serializable.BaseFlightSpeed, 1f, 3.402823E+38f);
			OnDataDeserialized(serializable, database);
		}

		public void Save(SkillSettingsSerializable serializable)
		{
			if (BeatAllEnemiesFactionList == null || BeatAllEnemiesFactionList.Length == 0)
			    serializable.BeatAllEnemiesFactionList = null;
			else
			    serializable.BeatAllEnemiesFactionList = BeatAllEnemiesFactionList.Select(wrapper => wrapper.Item.Value).ToArray();
			serializable.DisableExceedTheLimits = DisableExceedTheLimits;
			serializable.FuelTankCapacity = FuelTankCapacity;
			serializable.AttackBonus = AttackBonus;
			serializable.DefenseBonus = DefenseBonus;
			serializable.ShieldStrengthBonus = ShieldStrengthBonus;
			serializable.ShieldRechargeBonus = ShieldRechargeBonus;
			serializable.ExperienceBonus = ExperienceBonus;
			serializable.FlightSpeed = FlightSpeed;
			serializable.FlightRange = FlightRange;
			serializable.ExplorationLootBonus = ExplorationLootBonus;
			serializable.HeatResistance = HeatResistance;
			serializable.KineticResistance = KineticResistance;
			serializable.EnergyResistance = EnergyResistance;
			serializable.MerchantPriceFactor = MerchantPriceFactor;
			serializable.CraftingPriceFactor = CraftingPriceFactor;
			serializable.CraftingLevelReduction = CraftingLevelReduction;
			serializable.MaxPlayerShipsLevel = MaxPlayerShipsLevel.Value;
			serializable.IncreasedLevelLimit = IncreasedLevelLimit.Value;
			serializable.BaseFuelCapacity = BaseFuelCapacity.Value;
			serializable.BaseFlightRange = BaseFlightRange.Value;
			serializable.BaseFlightSpeed = BaseFlightSpeed.Value;
			OnDataSerialized(ref serializable);
		}

		public Wrapper<Faction>[] BeatAllEnemiesFactionList;
		public bool DisableExceedTheLimits;
		public string FuelTankCapacity;
		public string AttackBonus;
		public string DefenseBonus;
		public string ShieldStrengthBonus;
		public string ShieldRechargeBonus;
		public string ExperienceBonus;
		public string FlightSpeed;
		public string FlightRange;
		public string ExplorationLootBonus;
		public string HeatResistance;
		public string KineticResistance;
		public string EnergyResistance;
		public string MerchantPriceFactor;
		public string CraftingPriceFactor;
		public string CraftingLevelReduction;
		public NumericValue<int> MaxPlayerShipsLevel = new NumericValue<int>(0, 0, 500);
		public NumericValue<int> IncreasedLevelLimit = new NumericValue<int>(0, 0, 1000);
		public NumericValue<int> BaseFuelCapacity = new NumericValue<int>(0, 10, 2147483647);
		public NumericValue<float> BaseFlightRange = new NumericValue<float>(0, 1.5f, 3.402823E+38f);
		public NumericValue<float> BaseFlightSpeed = new NumericValue<float>(0, 1f, 3.402823E+38f);

		public static SkillSettings DefaultValue { get; private set; }
	}
}
