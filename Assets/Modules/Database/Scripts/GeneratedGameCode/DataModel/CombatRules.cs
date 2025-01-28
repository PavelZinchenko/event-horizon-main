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
	public partial class CombatRules : IDefaultVariablesResolver
	{
		partial void OnDataDeserialized(CombatRulesSerializable serializable, Database.Loader loader);

		public static CombatRules Create(CombatRulesSerializable serializable, Database.Loader loader)
		{
			return serializable == null ? DefaultValue : new CombatRules(serializable, loader);
		}

		private CombatRules(CombatRulesSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<CombatRules>(serializable.Id);
			loader.AddCombatRules(serializable.Id, this);

			var variableResolver = new VariableResolver(this);
			_initialEnemyShips = new Expressions.IntToInt(serializable.InitialEnemyShips, 1, 2147483647, variableResolver) { ParamName1 = "level" };
			InitialEnemyShips = _initialEnemyShips.Evaluate;
			_maxEnemyShips = new Expressions.IntToInt(serializable.MaxEnemyShips, 1, 2147483647, variableResolver) { ParamName1 = "level" };
			MaxEnemyShips = _maxEnemyShips.Evaluate;
			BattleMapSize = UnityEngine.Mathf.Clamp(serializable.BattleMapSize, 50, 2147483647);
			_timeLimit = new Expressions.IntToInt(serializable.TimeLimit, 0, 2147483647, variableResolver) { ParamName1 = "level" };
			TimeLimit = _timeLimit.Evaluate;
			TimeOutMode = serializable.TimeOutMode;
			LootCondition = serializable.LootCondition;
			ExpCondition = serializable.ExpCondition;
			ShipSelection = serializable.ShipSelection;
			DisableSkillBonuses = serializable.DisableSkillBonuses;
			DisableRandomLoot = serializable.DisableRandomLoot;
			DisableAsteroids = serializable.DisableAsteroids;
			DisablePlanet = serializable.DisablePlanet;
			NextEnemyButton = serializable.NextEnemyButton;
			KillThemAllButton = serializable.KillThemAllButton;
			CustomSoundtrack = new ImmutableCollection<SoundTrack>(serializable.CustomSoundtrack?.Select(item => SoundTrack.Create(item, loader)));

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<CombatRules> Id;

		private readonly Expressions.IntToInt _initialEnemyShips;
		public delegate int InitialEnemyShipsDelegate(int level);
		public InitialEnemyShipsDelegate InitialEnemyShips { get; private set; }
		private readonly Expressions.IntToInt _maxEnemyShips;
		public delegate int MaxEnemyShipsDelegate(int level);
		public MaxEnemyShipsDelegate MaxEnemyShips { get; private set; }
		public int BattleMapSize { get; private set; }
		private readonly Expressions.IntToInt _timeLimit;
		public delegate int TimeLimitDelegate(int level);
		public TimeLimitDelegate TimeLimit { get; private set; }
		public TimeOutMode TimeOutMode { get; private set; }
		public RewardCondition LootCondition { get; private set; }
		public RewardCondition ExpCondition { get; private set; }
		public PlayerShipSelectionMode ShipSelection { get; private set; }
		public bool DisableSkillBonuses { get; private set; }
		public bool DisableRandomLoot { get; private set; }
		public bool DisableAsteroids { get; private set; }
		public bool DisablePlanet { get; private set; }
		public bool NextEnemyButton { get; private set; }
		public bool KillThemAllButton { get; private set; }
		public ImmutableCollection<SoundTrack> CustomSoundtrack { get; private set; }

		public static CombatRules DefaultValue { get; private set; }

		private class VariableResolver : IVariableResolver
		{
			private CombatRules _context;

			public VariableResolver(CombatRules context)
			{
				_context = context;
			}

			public IFunction<Variant> ResolveFunction(string name)
            {
				if (name == "InitialEnemyShips") return _context._initialEnemyShips;
				if (name == "MaxEnemyShips") return _context._maxEnemyShips;
				if (name == "TimeLimit") return _context._timeLimit;
				return ((IVariableResolver)_context).ResolveFunction(name);
			}

			public Expression<Variant> ResolveVariable(string name)
			{
				if (name == "BattleMapSize") return GetBattleMapSize;
				if (name == "DisableSkillBonuses") return GetDisableSkillBonuses;
				if (name == "DisableRandomLoot") return GetDisableRandomLoot;
				if (name == "DisableAsteroids") return GetDisableAsteroids;
				if (name == "DisablePlanet") return GetDisablePlanet;
				if (name == "NextEnemyButton") return GetNextEnemyButton;
				if (name == "KillThemAllButton") return GetKillThemAllButton;
				return ((IVariableResolver)_context).ResolveVariable(name);
			}

			private Variant GetBattleMapSize() => _context.BattleMapSize;
			private Variant GetDisableSkillBonuses() => _context.DisableSkillBonuses;
			private Variant GetDisableRandomLoot() => _context.DisableRandomLoot;
			private Variant GetDisableAsteroids() => _context.DisableAsteroids;
			private Variant GetDisablePlanet() => _context.DisablePlanet;
			private Variant GetNextEnemyButton() => _context.NextEnemyButton;
			private Variant GetKillThemAllButton() => _context.KillThemAllButton;
		}
	}
}
