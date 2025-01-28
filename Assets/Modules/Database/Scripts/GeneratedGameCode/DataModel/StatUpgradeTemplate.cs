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
	public partial class StatUpgradeTemplate : IDefaultVariablesResolver
	{
		partial void OnDataDeserialized(StatUpgradeTemplateSerializable serializable, Database.Loader loader);

		public static StatUpgradeTemplate Create(StatUpgradeTemplateSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new StatUpgradeTemplate(serializable, loader);
		}

		private StatUpgradeTemplate(StatUpgradeTemplateSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<StatUpgradeTemplate>(serializable.Id);
			loader.AddStatUpgradeTemplate(serializable.Id, this);

			var variableResolver = new VariableResolver(this);
			MaxLevel = UnityEngine.Mathf.Clamp(serializable.MaxLevel, 0, 2147483647);
			_stars = new Expressions.IntToInt(serializable.Stars, -2147483648, 2147483647, variableResolver) { ParamName1 = "level" };
			Stars = _stars.Evaluate;
			_credits = new Expressions.IntToInt(serializable.Credits, -2147483648, 2147483647, variableResolver) { ParamName1 = "level" };
			Credits = _credits.Evaluate;
			_resources = new Expressions.IntToInt(serializable.Resources, -2147483648, 2147483647, variableResolver) { ParamName1 = "level" };
			Resources = _resources.Evaluate;

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<StatUpgradeTemplate> Id;

		public int MaxLevel { get; private set; }
		private readonly Expressions.IntToInt _stars;
		public delegate int StarsDelegate(int level);
		public StarsDelegate Stars { get; private set; }
		private readonly Expressions.IntToInt _credits;
		public delegate int CreditsDelegate(int level);
		public CreditsDelegate Credits { get; private set; }
		private readonly Expressions.IntToInt _resources;
		public delegate int ResourcesDelegate(int level);
		public ResourcesDelegate Resources { get; private set; }

		public static StatUpgradeTemplate DefaultValue { get; private set; }

		private class VariableResolver : IVariableResolver
		{
			private StatUpgradeTemplate _context;

			public VariableResolver(StatUpgradeTemplate context)
			{
				_context = context;
			}

			public IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "Stars") return _context._stars;
				if (name == "Credits") return _context._credits;
				if (name == "Resources") return _context._resources;
				return ((IVariableResolver)_context).ResolveFunction(name);
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				if (name == "MaxLevel") return GetMaxLevel;
				return ((IVariableResolver)_context).ResolveVariable(name);
			}

			private Variant GetMaxLevel() => _context.MaxLevel;
		}
	}
}
