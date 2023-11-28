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
using CodeWriter.ExpressionParser;

namespace GameDatabase.DataModel
{
	public partial class SkillSettings
	{
		partial void OnDataDeserialized(SkillSettingsSerializable serializable, Database.Loader loader);

		public static SkillSettings Create(SkillSettingsSerializable serializable, Database.Loader loader)
		{
			return new SkillSettings(serializable, loader);
		}

		private SkillSettings(SkillSettingsSerializable serializable, Database.Loader loader)
		{
			var variableResolver = new VariableResolver(this);
			BeatAllEnemiesFactionList = new ImmutableCollection<Faction>(serializable.BeatAllEnemiesFactionList?.Select(item => loader.GetFaction(new ItemId<Faction>(item), true)));
			DisableExceedTheLimits = serializable.DisableExceedTheLimits;
			_fuelTankCapacity = new Expressions.IntToInt(serializable.FuelTankCapacity, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			FuelTankCapacity = _fuelTankCapacity.Evaluate;
			_attackBonus = new Expressions.IntToFloat(serializable.AttackBonus, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			AttackBonus = _attackBonus.Evaluate;
			_defenseBonus = new Expressions.IntToFloat(serializable.DefenseBonus, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			DefenseBonus = _defenseBonus.Evaluate;
			_shieldStrengthBonus = new Expressions.IntToFloat(serializable.ShieldStrengthBonus, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			ShieldStrengthBonus = _shieldStrengthBonus.Evaluate;
			_shieldRechargeBonus = new Expressions.IntToFloat(serializable.ShieldRechargeBonus, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			ShieldRechargeBonus = _shieldRechargeBonus.Evaluate;
			_experienceBonus = new Expressions.IntToFloat(serializable.ExperienceBonus, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			ExperienceBonus = _experienceBonus.Evaluate;
			_flightSpeed = new Expressions.IntToFloat(serializable.FlightSpeed, 1, 2147483647, variableResolver) { ParamName1 = "level" };
			FlightSpeed = _flightSpeed.Evaluate;
			_flightRange = new Expressions.IntToFloat(serializable.FlightRange, -2147483648, 2147483647, variableResolver) { ParamName1 = "level" };
			FlightRange = _flightRange.Evaluate;
			_explorationLootBonus = new Expressions.IntToFloat(serializable.ExplorationLootBonus, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			ExplorationLootBonus = _explorationLootBonus.Evaluate;
			_heatResistance = new Expressions.IntToFloat(serializable.HeatResistance, 0, 1, variableResolver) { ParamName1 = "level" };
			HeatResistance = _heatResistance.Evaluate;
			_kineticResistance = new Expressions.IntToFloat(serializable.KineticResistance, 0, 1, variableResolver) { ParamName1 = "level" };
			KineticResistance = _kineticResistance.Evaluate;
			_energyResistance = new Expressions.IntToFloat(serializable.EnergyResistance, 0, 1, variableResolver) { ParamName1 = "level" };
			EnergyResistance = _energyResistance.Evaluate;
			_merchantPriceFactor = new Expressions.IntToFloat(serializable.MerchantPriceFactor, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			MerchantPriceFactor = _merchantPriceFactor.Evaluate;
			_craftingPriceFactor = new Expressions.IntToFloat(serializable.CraftingPriceFactor, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			CraftingPriceFactor = _craftingPriceFactor.Evaluate;
			_craftingLevelReduction = new Expressions.IntToInt(serializable.CraftingLevelReduction, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			CraftingLevelReduction = _craftingLevelReduction.Evaluate;
			IncreasedLevelLimit = UnityEngine.Mathf.Clamp(serializable.IncreasedLevelLimit, 100, 1000);
			BaseFuelCapacity = UnityEngine.Mathf.Clamp(serializable.BaseFuelCapacity, 10, 2147483647);
			BaseFlightRange = UnityEngine.Mathf.Clamp(serializable.BaseFlightRange, 1.5f, 3.402823E+38f);
			BaseFlightSpeed = UnityEngine.Mathf.Clamp(serializable.BaseFlightSpeed, 1f, 3.402823E+38f);

			OnDataDeserialized(serializable, loader);
		}

		public ImmutableCollection<Faction> BeatAllEnemiesFactionList { get; private set; }
		public bool DisableExceedTheLimits { get; private set; }
		private readonly Expressions.IntToInt _fuelTankCapacity;
		public delegate int FuelTankCapacityDelegate(int level);
		public FuelTankCapacityDelegate FuelTankCapacity { get; private set; }
		private readonly Expressions.IntToFloat _attackBonus;
		public delegate float AttackBonusDelegate(int level);
		public AttackBonusDelegate AttackBonus { get; private set; }
		private readonly Expressions.IntToFloat _defenseBonus;
		public delegate float DefenseBonusDelegate(int level);
		public DefenseBonusDelegate DefenseBonus { get; private set; }
		private readonly Expressions.IntToFloat _shieldStrengthBonus;
		public delegate float ShieldStrengthBonusDelegate(int level);
		public ShieldStrengthBonusDelegate ShieldStrengthBonus { get; private set; }
		private readonly Expressions.IntToFloat _shieldRechargeBonus;
		public delegate float ShieldRechargeBonusDelegate(int level);
		public ShieldRechargeBonusDelegate ShieldRechargeBonus { get; private set; }
		private readonly Expressions.IntToFloat _experienceBonus;
		public delegate float ExperienceBonusDelegate(int level);
		public ExperienceBonusDelegate ExperienceBonus { get; private set; }
		private readonly Expressions.IntToFloat _flightSpeed;
		public delegate float FlightSpeedDelegate(int level);
		public FlightSpeedDelegate FlightSpeed { get; private set; }
		private readonly Expressions.IntToFloat _flightRange;
		public delegate float FlightRangeDelegate(int level);
		public FlightRangeDelegate FlightRange { get; private set; }
		private readonly Expressions.IntToFloat _explorationLootBonus;
		public delegate float ExplorationLootBonusDelegate(int level);
		public ExplorationLootBonusDelegate ExplorationLootBonus { get; private set; }
		private readonly Expressions.IntToFloat _heatResistance;
		public delegate float HeatResistanceDelegate(int level);
		public HeatResistanceDelegate HeatResistance { get; private set; }
		private readonly Expressions.IntToFloat _kineticResistance;
		public delegate float KineticResistanceDelegate(int level);
		public KineticResistanceDelegate KineticResistance { get; private set; }
		private readonly Expressions.IntToFloat _energyResistance;
		public delegate float EnergyResistanceDelegate(int level);
		public EnergyResistanceDelegate EnergyResistance { get; private set; }
		private readonly Expressions.IntToFloat _merchantPriceFactor;
		public delegate float MerchantPriceFactorDelegate(int level);
		public MerchantPriceFactorDelegate MerchantPriceFactor { get; private set; }
		private readonly Expressions.IntToFloat _craftingPriceFactor;
		public delegate float CraftingPriceFactorDelegate(int level);
		public CraftingPriceFactorDelegate CraftingPriceFactor { get; private set; }
		private readonly Expressions.IntToInt _craftingLevelReduction;
		public delegate int CraftingLevelReductionDelegate(int level);
		public CraftingLevelReductionDelegate CraftingLevelReduction { get; private set; }
		public int IncreasedLevelLimit { get; private set; }
		public int BaseFuelCapacity { get; private set; }
		public float BaseFlightRange { get; private set; }
		public float BaseFlightSpeed { get; private set; }

		public static SkillSettings DefaultValue { get; private set; }

		private class VariableResolver : IVariableResolver
		{
			private SkillSettings _context;

			public VariableResolver(SkillSettings context)
			{
				_context = context;
			}

			public IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "FuelTankCapacity") return _context._fuelTankCapacity;
				if (name == "AttackBonus") return _context._attackBonus;
				if (name == "DefenseBonus") return _context._defenseBonus;
				if (name == "ShieldStrengthBonus") return _context._shieldStrengthBonus;
				if (name == "ShieldRechargeBonus") return _context._shieldRechargeBonus;
				if (name == "ExperienceBonus") return _context._experienceBonus;
				if (name == "FlightSpeed") return _context._flightSpeed;
				if (name == "FlightRange") return _context._flightRange;
				if (name == "ExplorationLootBonus") return _context._explorationLootBonus;
				if (name == "HeatResistance") return _context._heatResistance;
				if (name == "KineticResistance") return _context._kineticResistance;
				if (name == "EnergyResistance") return _context._energyResistance;
				if (name == "MerchantPriceFactor") return _context._merchantPriceFactor;
				if (name == "CraftingPriceFactor") return _context._craftingPriceFactor;
				if (name == "CraftingLevelReduction") return _context._craftingLevelReduction;
				return null;
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				if (name == "DisableExceedTheLimits") return GetDisableExceedTheLimits;
				if (name == "IncreasedLevelLimit") return GetIncreasedLevelLimit;
				if (name == "BaseFuelCapacity") return GetBaseFuelCapacity;
				if (name == "BaseFlightRange") return GetBaseFlightRange;
				if (name == "BaseFlightSpeed") return GetBaseFlightSpeed;
				return null;
			}

			private Variant GetDisableExceedTheLimits() => _context.DisableExceedTheLimits;
			private Variant GetIncreasedLevelLimit() => _context.IncreasedLevelLimit;
			private Variant GetBaseFuelCapacity() => _context.BaseFuelCapacity;
			private Variant GetBaseFlightRange() => _context.BaseFlightRange;
			private Variant GetBaseFlightSpeed() => _context.BaseFlightSpeed;
		}
	}
}
