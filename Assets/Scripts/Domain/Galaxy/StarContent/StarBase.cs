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
			builder.PlayerFleet = playerFleet;
			builder.EnemyFleet = defenderFleet;
            builder.Rules = _database.GalaxySettings.StarbaseCombatRules ?? _database.CombatSettings.DefaultCombatRules;
			builder.AddSpecialReward(_lootGenerator.GetStarBaseSpecialReward(region));
            builder.StarLevel = UnityEngine.Mathf.RoundToInt(region.MilitaryPower * region.BaseDefensePower);

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
            _session.Quests.SetFactionRelations(starId, -100);
            _startBattleTrigger.Fire(model, result => OnCombatCompleted(starId, result));
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
			public ICombatModel CreateCombatModel() => _starbase.CreateCombatModel(_starId);

			private readonly StarBase _starbase;
			private readonly int _starId;
		}
	}
}
