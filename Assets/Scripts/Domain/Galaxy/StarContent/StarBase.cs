using Combat.Domain;
using GameDatabase;
using GameServices.Economy;
using GameServices.Player;
using GameStateMachine.States;
using Model.Factories;
using Domain.Quests;
using GameModel;
using Session;
using Zenject;
using ViewModel;
using Combat.Component.Unit.Classification;
using System.Collections.Generic;

namespace Galaxy.StarContent
{
    public class StarBase
    {
        [Inject] private readonly PlayerFleet _playerFleet;
        [Inject] private readonly StarData _starData;
        [Inject] private readonly StarContentChangedSignal.Trigger _starContentChangedTrigger;
        [Inject] private readonly StartBattleSignal.Trigger _startBattleTrigger;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly CombatModelBuilder.Factory _combatModelBuilderFactory;
        [Inject] private readonly LootGenerator _lootGenerator;
        [Inject] private readonly ISessionData _session;
		[Inject] private readonly IQuestManager _questManager;

		public ICombatModel CreateCombatModel(int starId)
		{
			var region = _starData.GetRegion(starId);
			if (region.IsCaptured)
				return null;

            var playerFleet = new Model.Military.PlayerFleet(_database, _playerFleet);
			var defenderFleet = Fleet.Capital(region, _database);

			var builder = _combatModelBuilderFactory.Create();
            CombatRelations.SetRelation(0, region.Faction.Id.Value, false);
			builder.PlayerFleet = playerFleet;
			if (FactionPanelViewModel.IncludeStarshipEarthAllies)
            {
				builder.AllyFleet = Fleet.StarshipEarthAllies(region.HomeStarLevel, starId ^ 0x5345, _database);
            }
			builder.EnemyFleet = defenderFleet;
            // A fallback/dedicated station build may carry a database faction
            // different from the region's faction.  All defenders in this
            // station assault still belong to the same defending force.
            builder.EnemyFactionIdOverride = region.Faction.Id.Value;
            builder.Rules = _database.GalaxySettings.StarbaseCombatRules ?? _database.CombatSettings.DefaultCombatRules;
			builder.AddSpecialReward(_lootGenerator.GetStarBaseSpecialReward(region));
            builder.StarLevel = region.HomeStarLevel;

			return builder.Build();
		}

		public bool IsExists(int starId)
		{
			int x, y;
			StarLayout.IdToPosition(starId, out x, out y);
			if (!RegionMap.IsHomeStar(x, y))
				return false;

			return _starData.GetRegion(starId).Id != Region.UnoccupiedRegionId;
		}

        public void Attack(int starId)
        {
			if (!IsExists(starId))
				throw new System.InvalidOperationException();

			var quest = _database.GalaxySettings.CaptureStarbaseQuest;
			if (quest != null)
			{
				_questManager.StartQuest(quest);
				return;
			}

            var model = CreateCombatModel(starId);
            var region = _starData.GetRegion(starId);
            _session.Quests.SetFactionRelations(region.HomeStar, -50);
            CombatRelations.SetRelation(0, region.Faction.Id.Value, false);
            _startBattleTrigger.Fire(model, result => OnCombatCompleted(starId, result));
        }

        public bool PeacefulTransfer(int starId)
        {
            if (!IsExists(starId))
                return false;

            var region = _starData.GetRegion(starId);
            if (region.IsCaptured || _session.Quests.GetFactionRelations(region.HomeStar) <= 25)
                return false;

            region.IsCaptured = true;
            CombatRelations.SetRelation(0, region.Faction.Id.Value, true);
            _starContentChangedTrigger.Fire(starId);
            return true;
        }

        public void Defend(int starId)
        {
            if (!IsExists(starId))
                throw new System.InvalidOperationException();

            var region = _starData.GetRegion(starId);
            if (!region.IsCaptured)
                return;

            var builder = _combatModelBuilderFactory.Create();
            var stationLevel = UnityEngine.Mathf.Max(1, region.BaseDefendersLevel);
            _lastDefenseFactionByStar.TryGetValue(starId, out var previousFactionId);
            var seed = unchecked(starId ^ (++_defenseAttemptSerial * 7919) ^ System.Environment.TickCount);
            builder.PlayerFleet = new Model.Military.PlayerFleet(_database, _playerFleet);
            builder.AllyFleet = Fleet.StarbaseDefenseAllies(region, seed ^ 0x444546, _database);
            builder.EnemyFleet = Fleet.StarbaseDefenseEnemies(region, seed ^ 0x454E4D, previousFactionId,
                _database, out var attackerFactionId);
            if (attackerFactionId >= 0)
                _lastDefenseFactionByStar[starId] = attackerFactionId;
            builder.DefenseStarbaseBuild = Fleet.StarbaseForFaction(region, _database);
            builder.DefenseStarbaseLevel = stationLevel;
            builder.Rules = _database.GalaxySettings.StarbaseCombatRules ?? _database.CombatSettings.DefaultCombatRules;
            builder.StarLevel = stationLevel;
            CombatRelations.SetRelation(0, region.Faction.Id.Value, true);
            var model = builder.Build();
            _startBattleTrigger.Fire(model, result => OnDefenseCompleted(starId, result));
        }

        private void OnDefenseCompleted(int starId, ICombatModel result)
        {
            if (result == null || result.EnemyFleet.IsAnyShipAlive())
                return;

            var region = _starData.GetRegion(starId);
            var currentPower = region.BaseDefensePower;
            region.BaseDefensePower = UnityEngine.Mathf.Max(currentPower + 1,
                UnityEngine.Mathf.CeilToInt(currentPower * 1.5f));
            region.RaiseCapturedServiceLevel();
            _starContentChangedTrigger.Fire(starId);
        }

        private void OnCombatCompleted(int starId, ICombatModel result)
        {
            if (!result.IsVictory())
                return;

            _starData.GetRegion(starId).IsCaptured = true;
            _starContentChangedTrigger.Fire(starId);
        }
	
		public struct Facade
		{
			public Facade(StarBase starbase, int starId)
			{
				_starbase = starbase;
				_starId = starId;
			}

			public bool IsExists => _starbase.IsExists(_starId);
			public void Attack() => _starbase.Attack(_starId);
			public bool PeacefulTransfer() => _starbase.PeacefulTransfer(_starId);
			public void Defend() => _starbase.Defend(_starId);
			public ICombatModel CreateCombatModel() => _starbase.CreateCombatModel(_starId);

			private readonly StarBase _starbase;
		private readonly int _starId;
		}

        private readonly Dictionary<int, int> _lastDefenseFactionByStar = new Dictionary<int, int>();
        private int _defenseAttemptSerial;
	}
}
