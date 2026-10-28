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
	public partial class RuinsSettings : IDefaultVariablesResolver
	{
		partial void OnDataDeserialized(RuinsSettingsSerializable serializable, Database.Loader loader);

		public static RuinsSettings Create(RuinsSettingsSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new RuinsSettings(serializable, loader);
		}

		private RuinsSettings(RuinsSettingsSerializable serializable, Database.Loader loader)
		{
			var variableResolver = new VariableResolver(this);
			ExcludedSatellites = new ImmutableCollection<Satellite>(serializable.ExcludedSatellites?.Select(item => loader.GetSatellite(new ItemId<Satellite>(item), true)));
			_maxSatelliteCells = new Expressions.IntToInt(serializable.MaxSatelliteCells, 0, 1000, variableResolver) { ParamName1 = "level" };
			MaxSatelliteCells = _maxSatelliteCells.Evaluate;

			OnDataDeserialized(serializable, loader);
		}

		public ImmutableCollection<Satellite> ExcludedSatellites { get; private set; }
		private readonly Expressions.IntToInt _maxSatelliteCells;
		public delegate int MaxSatelliteCellsDelegate(int level);
		public MaxSatelliteCellsDelegate MaxSatelliteCells { get; private set; }

		public static RuinsSettings DefaultValue { get; private set; }

		private class VariableResolver : IVariableResolver
		{
			private RuinsSettings _context;

			public VariableResolver(RuinsSettings context)
			{
				_context = context;
			}

			public IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "MaxSatelliteCells") return _context._maxSatelliteCells;
				return ((IVariableResolver)_context).ResolveFunction(name);
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				return ((IVariableResolver)_context).ResolveVariable(name);
			}

		}
	}
}
