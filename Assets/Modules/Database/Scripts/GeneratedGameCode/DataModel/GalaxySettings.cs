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
	public partial class GalaxySettings
	{
		partial void OnDataDeserialized(GalaxySettingsSerializable serializable, Database.Loader loader);

		public static GalaxySettings Create(GalaxySettingsSerializable serializable, Database.Loader loader)
		{
			return new GalaxySettings(serializable, loader);
		}

		private GalaxySettings(GalaxySettingsSerializable serializable, Database.Loader loader)
		{
			var variableResolver = new VariableResolver(this);
			AbandonedStarbaseFaction = loader.GetFaction(new ItemId<Faction>(serializable.AbandonedStarbaseFaction));
			StartingShipBuilds = new ImmutableCollection<ShipBuild>(serializable.StartingShipBuilds?.Select(item => loader.GetShipBuild(new ItemId<ShipBuild>(item), true)));
			StartingInventory = loader.GetLoot(new ItemId<LootModel>(serializable.StartingInventory));
			SupporterPackShip = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.SupporterPackShip));
			DefaultStarbaseBuild = loader.GetShipBuild(new ItemId<ShipBuild>(serializable.DefaultStarbaseBuild));
			MaxEnemyShipsLevel = UnityEngine.Mathf.Clamp(serializable.MaxEnemyShipsLevel, 100, 500);
			_enemyLevel = new Expressions.IntToInt(serializable.EnemyLevel, 0, 500, variableResolver) { ParamName1 = "distance" };
			EnemyLevel = _enemyLevel.Evaluate;
			_shipMinSpawnDistance = new Expressions.SizeClassToInt(serializable.ShipMinSpawnDistance, 0, 1000, variableResolver) { ParamName1 = "size" };
			ShipMinSpawnDistance = _shipMinSpawnDistance.Evaluate;

			OnDataDeserialized(serializable, loader);
		}

		public Faction AbandonedStarbaseFaction { get; private set; }
		public ImmutableCollection<ShipBuild> StartingShipBuilds { get; private set; }
		public LootModel StartingInventory { get; private set; }
		public ShipBuild SupporterPackShip { get; private set; }
		public ShipBuild DefaultStarbaseBuild { get; private set; }
		public int MaxEnemyShipsLevel { get; private set; }
		private readonly Expressions.IntToInt _enemyLevel;
		public delegate int EnemyLevelDelegate(int distance);
		public EnemyLevelDelegate EnemyLevel { get; private set; }
		private readonly Expressions.SizeClassToInt _shipMinSpawnDistance;
		public delegate int ShipMinSpawnDistanceDelegate(SizeClass size);
		public ShipMinSpawnDistanceDelegate ShipMinSpawnDistance { get; private set; }

		public static GalaxySettings DefaultValue { get; private set; }

		private class VariableResolver : IVariableResolver
		{
			private GalaxySettings _context;

			public VariableResolver(GalaxySettings context)
			{
				_context = context;
			}

			public IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "EnemyLevel") return _context._enemyLevel;
				if (name == "ShipMinSpawnDistance") return _context._shipMinSpawnDistance;
				return null;
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				if (name == "MaxEnemyShipsLevel") return GetMaxEnemyShipsLevel;
				return null;
			}

			private Variant GetMaxEnemyShipsLevel() => _context.MaxEnemyShipsLevel;
		}
	}
}
