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
	public partial class SpecialEventSettings : IDefaultVariablesResolver
	{
		partial void OnDataDeserialized(SpecialEventSettingsSerializable serializable, Database.Loader loader);

		public static SpecialEventSettings Create(SpecialEventSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new SpecialEventSettings(serializable, loader);
		}

		private SpecialEventSettings(SpecialEventSettingsSerializable serializable, Database.Loader loader)
		{
			var variableResolver = new VariableResolver(this);
			EnableXmasEvent = serializable.EnableXmasEvent;
			XmasDaysBefore = UnityEngine.Mathf.Clamp(serializable.XmasDaysBefore, 0, 30);
			XmasDaysAfter = UnityEngine.Mathf.Clamp(serializable.XmasDaysAfter, 0, 30);
			XmasQuest = loader?.GetQuest(new ItemId<QuestModel>(serializable.XmasQuest)) ?? QuestModel.DefaultValue;
			XmasCombatRules = loader?.GetCombatRules(new ItemId<CombatRules>(serializable.XmasCombatRules)) ?? CombatRules.DefaultValue;
			_convertCreditsToSnowflakes = new Expressions.IntToInt(serializable.ConvertCreditsToSnowflakes, 1, 2147483647, variableResolver) { ParamName1 = "credits" };
			ConvertCreditsToSnowflakes = _convertCreditsToSnowflakes.Evaluate;
			EnableEasterEvent = serializable.EnableEasterEvent;
			EasterDaysBefore = UnityEngine.Mathf.Clamp(serializable.EasterDaysBefore, 0, 30);
			EasterDaysAfter = UnityEngine.Mathf.Clamp(serializable.EasterDaysAfter, 0, 30);
			EasterQuest = loader?.GetQuest(new ItemId<QuestModel>(serializable.EasterQuest)) ?? QuestModel.DefaultValue;
			EnableHalloweenEvent = serializable.EnableHalloweenEvent;
			HalloweenDaysBefore = UnityEngine.Mathf.Clamp(serializable.HalloweenDaysBefore, 0, 30);
			HalloweenDaysAfter = UnityEngine.Mathf.Clamp(serializable.HalloweenDaysAfter, 0, 30);
			HalloweenQuest = loader?.GetQuest(new ItemId<QuestModel>(serializable.HalloweenQuest)) ?? QuestModel.DefaultValue;

			OnDataDeserialized(serializable, loader);
		}

		public bool EnableXmasEvent { get; private set; }
		public int XmasDaysBefore { get; private set; }
		public int XmasDaysAfter { get; private set; }
		public QuestModel XmasQuest { get; private set; }
		public CombatRules XmasCombatRules { get; private set; }
		private readonly Expressions.IntToInt _convertCreditsToSnowflakes;
		public delegate int ConvertCreditsToSnowflakesDelegate(int credits);
		public ConvertCreditsToSnowflakesDelegate ConvertCreditsToSnowflakes { get; private set; }
		public bool EnableEasterEvent { get; private set; }
		public int EasterDaysBefore { get; private set; }
		public int EasterDaysAfter { get; private set; }
		public QuestModel EasterQuest { get; private set; }
		public bool EnableHalloweenEvent { get; private set; }
		public int HalloweenDaysBefore { get; private set; }
		public int HalloweenDaysAfter { get; private set; }
		public QuestModel HalloweenQuest { get; private set; }

		public static SpecialEventSettings DefaultValue { get; private set; }

		private class VariableResolver : IVariableResolver
		{
			private SpecialEventSettings _context;

			public VariableResolver(SpecialEventSettings context)
			{
				_context = context;
			}

			public IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "ConvertCreditsToSnowflakes") return _context._convertCreditsToSnowflakes;
				return ((IVariableResolver)_context).ResolveFunction(name);
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				if (name == "EnableXmasEvent") return GetEnableXmasEvent;
				if (name == "XmasDaysBefore") return GetXmasDaysBefore;
				if (name == "XmasDaysAfter") return GetXmasDaysAfter;
				if (name == "EnableEasterEvent") return GetEnableEasterEvent;
				if (name == "EasterDaysBefore") return GetEasterDaysBefore;
				if (name == "EasterDaysAfter") return GetEasterDaysAfter;
				if (name == "EnableHalloweenEvent") return GetEnableHalloweenEvent;
				if (name == "HalloweenDaysBefore") return GetHalloweenDaysBefore;
				if (name == "HalloweenDaysAfter") return GetHalloweenDaysAfter;
				return ((IVariableResolver)_context).ResolveVariable(name);
			}

			private Variant GetEnableXmasEvent() => _context.EnableXmasEvent;
			private Variant GetXmasDaysBefore() => _context.XmasDaysBefore;
			private Variant GetXmasDaysAfter() => _context.XmasDaysAfter;
			private Variant GetEnableEasterEvent() => _context.EnableEasterEvent;
			private Variant GetEasterDaysBefore() => _context.EasterDaysBefore;
			private Variant GetEasterDaysAfter() => _context.EasterDaysAfter;
			private Variant GetEnableHalloweenEvent() => _context.EnableHalloweenEvent;
			private Variant GetHalloweenDaysBefore() => _context.HalloweenDaysBefore;
			private Variant GetHalloweenDaysAfter() => _context.HalloweenDaysAfter;
		}
	}
}
