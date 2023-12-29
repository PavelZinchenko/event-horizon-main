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
	public partial class ExplorationSettings
	{
		partial void OnDataDeserialized(ExplorationSettingsSerializable serializable, Database.Loader loader);

		public static ExplorationSettings Create(ExplorationSettingsSerializable serializable, Database.Loader loader)
		{
			return new ExplorationSettings(serializable, loader);
		}

		private ExplorationSettings(ExplorationSettingsSerializable serializable, Database.Loader loader)
		{
			var variableResolver = new VariableResolver(this);
			OutpostShip = loader.GetShip(new ItemId<Ship>(serializable.OutpostShip));
			TurretShip = loader.GetShip(new ItemId<Ship>(serializable.TurretShip));
			InfectedPlanetFaction = loader.GetFaction(new ItemId<Faction>(serializable.InfectedPlanetFaction));
			HiveShipBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.HiveShipBuild));
			_gasCloudDPS = new Expressions.IntToFloat(serializable.GasCloudDPS, 1, 2147483647, variableResolver) { ParamName1 = "level" };
			GasCloudDPS = _gasCloudDPS.Evaluate;

			OnDataDeserialized(serializable, loader);
		}

		public Ship OutpostShip { get; private set; }
		public Ship TurretShip { get; private set; }
		public Faction InfectedPlanetFaction { get; private set; }
		public ShipBuild HiveShipBuild { get; private set; }
		private readonly Expressions.IntToFloat _gasCloudDPS;
		public delegate float GasCloudDPSDelegate(int level);
		public GasCloudDPSDelegate GasCloudDPS { get; private set; }

		public static ExplorationSettings DefaultValue { get; private set; }

		private class VariableResolver : IVariableResolver
		{
			private ExplorationSettings _context;

			public VariableResolver(ExplorationSettings context)
			{
				_context = context;
			}

			public IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "GasCloudDPS") return _context._gasCloudDPS;
				return null;
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				return null;
			}

		}
	}
}
