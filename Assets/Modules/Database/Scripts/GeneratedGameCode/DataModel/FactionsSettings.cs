


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
	public partial class FactionsSettings : IDefaultVariablesResolver
	{
		partial void OnDataDeserialized(FactionsSettingsSerializable serializable, Database.Loader loader);

		public static FactionsSettings Create(FactionsSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new FactionsSettings(serializable, loader);
		}

		private FactionsSettings(FactionsSettingsSerializable serializable, Database.Loader loader)
		{
			var variableResolver = new VariableResolver(this);
			_starbaseInitialDefense = new Expressions.IntToInt(serializable.StarbaseInitialDefense, 1, 100000, variableResolver) { ParamName1 = "distance" };
			StarbaseInitialDefense = _starbaseInitialDefense.Evaluate;
			StarbaseMinDefense = UnityEngine.Mathf.Clamp(serializable.StarbaseMinDefense, 1, 2147483647);
			DefenseLossPerEnemyDefeated = UnityEngine.Mathf.Clamp(serializable.DefenseLossPerEnemyDefeated, 0, 2147483647);

			OnDataDeserialized(serializable, loader);
		}

		private readonly Expressions.IntToInt _starbaseInitialDefense;
		public delegate int StarbaseInitialDefenseDelegate(int distance);
		public StarbaseInitialDefenseDelegate StarbaseInitialDefense { get; private set; }
		public int StarbaseMinDefense { get; private set; }
		public int DefenseLossPerEnemyDefeated { get; private set; }

		public static FactionsSettings DefaultValue { get; private set; }

		private class VariableResolver : IVariableResolver
		{
			private FactionsSettings _context;

			public VariableResolver(FactionsSettings context)
			{
				_context = context;
			}

			public IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "StarbaseInitialDefense") return _context._starbaseInitialDefense;
				return ((IVariableResolver)_context).ResolveFunction(name);
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				if (name == "StarbaseMinDefense") return GetStarbaseMinDefense;
				if (name == "DefenseLossPerEnemyDefeated") return GetDefenseLossPerEnemyDefeated;
				return ((IVariableResolver)_context).ResolveVariable(name);
			}

			private Variant GetStarbaseMinDefense() => _context.StarbaseMinDefense;
			private Variant GetDefenseLossPerEnemyDefeated() => _context.DefenseLossPerEnemyDefeated;
		}
	}
}
