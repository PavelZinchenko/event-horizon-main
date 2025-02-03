﻿using System.Linq;
using System.Collections.Generic;
using Zenject;
using GameServices.SceneManager;
using Combat.Domain;
using Services.Audio;
using GameDatabase;
using GameDatabase.Model;
using GameDatabase.Query;
using GameDatabase.DataModel;
using Database.Legacy;
using Session;
using Model.Military;
using GameServices.Audio;
using System.Text.RegularExpressions;

namespace GameStateMachine.States
{
    public class QuickCombatState : BaseState
    {
        public QuickCombatState(
            Settings settings,
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            IDatabase database,
            ISessionData session,
            CombatModelBuilder.Factory combatModelBuilderFactory,
            IMusicPlayer musicPlayer,
            DatabaseMusicPlaylist playlist,
			ExitSignal exitSignal)
            : base(stateMachine, stateFactory)
        {
			_session = session;
			_settings = settings;
			_database = database;
            _musicPlayer = musicPlayer;
			_combatModelBuilderFactory = combatModelBuilderFactory;
            _playlist = playlist;

            _exitSignal = exitSignal;
            _exitSignal.Event += OnCombatCompleted;
        }

        public override StateType Type => StateType.QuickCombat;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.Combat; } }

		public override void InstallBindings(DiContainer container)
		{
			container.Bind<ICombatModel>().FromInstance(CreateCombatModel());
		}

		protected override void OnLoad()
        {
            var rules = _database.GalaxySettings.QuickCombatRules ?? _database.CombatSettings.DefaultCombatRules;
            _playlist.SetCustomCombatPlaylist(rules.CustomSoundtrack);
            _musicPlayer.Play(AudioTrackType.Combat);
        }

        private void OnCombatCompleted()
        {
			LoadState(StateFactory.CreateMainMenuState());
        }

		private ICombatModel CreateCombatModel()
		{
			IFleet firstFleet, secondFleet;

            var testShips = new Queue<ShipBuild>();
            var matches = Regex.Matches(_settings.TestShipId, @"\d+");
            for (int i = 0; i < matches.Count; ++i)
            {
                if (!int.TryParse(matches[i].Value, out var id)) continue;
                var build = _database.GetShipBuild(new ItemId<ShipBuild>(id));
                if (build == null) continue;
                testShips.Enqueue(build);
            }

			var random = new System.Random();
			var fleet1 = _database.ShipBuildList.RandomUniqueElements(12, random);
			var fleet2 = _database.ShipBuildList.RandomUniqueElements(12, random);

#if UNITY_EDITOR
			if (testShips.Count > 0)
#else
            if (_database.IsEditable && testShips.Count > 0)
#endif
            {
                var noRandomEnemies = _settings.TestShipId.Contains('*');
                var disableEnemyAi = _settings.TestShipId.Contains('#');

                var playerFleet = fleet1;
				var enemyFleet = noRandomEnemies ? Enumerable.Empty<ShipBuild>() : fleet2;

                if (testShips.TryDequeue(out var playerShip))
                    playerFleet = playerFleet.Prepend(playerShip);

                if (testShips.Count == 0)
                    enemyFleet = enemyFleet.Prepend(playerShip);
                else while (testShips.TryDequeue(out var enemyShip))
                    enemyFleet = enemyFleet.Prepend(enemyShip);

                var aiLevel = disableEnemyAi ? -1 : _settings.EasyMode ? 0 : 100; // TODO: get rid of magic numbers

				firstFleet = new TestFleet(_database, playerFleet, 100);
				secondFleet = new TestFleet(_database, enemyFleet, aiLevel);
			}
			else
			{
				var ships = GetUnlockedShips();
				firstFleet = new TestFleet(_database, ships.RandomUniqueElements(12, random).OrderBy(item => random.Next()), _settings.EasyMode ? 0 : 100);
				secondFleet = new TestFleet(_database, ships.RandomUniqueElements(12, random).OrderBy(item => random.Next()), _settings.EasyMode ? 0 : 100);
			}

			var builder = _combatModelBuilderFactory.Create();
			builder.PlayerFleet = firstFleet;
			builder.EnemyFleet = secondFleet;
			builder.Rules = _database.GalaxySettings.QuickCombatRules ?? _database.CombatSettings.DefaultCombatRules;

            return builder.Build();
		}

		private HashSet<ShipBuild> GetUnlockedShips()
		{
			var ships = new HashSet<ShipBuild>();
			{
				ships.Add(_database.GetShipBuild(LegacyShipBuildNames.GetId("f0s1")));
				ships.Add(_database.GetShipBuild(LegacyShipBuildNames.GetId("f1s1")));
				ships.Add(_database.GetShipBuild(LegacyShipBuildNames.GetId("f2s1")));
				ships.Add(_database.GetShipBuild(LegacyShipBuildNames.GetId("f3s1")));
				ships.Add(_database.GetShipBuild(LegacyShipBuildNames.GetId("f4s1")));
				ships.Add(_database.GetShipBuild(LegacyShipBuildNames.GetId("f5s1")));
				ships.Add(_database.GetShipBuild(LegacyShipBuildNames.GetId("f6s1")));
				ships.Add(_database.GetShipBuild(LegacyShipBuildNames.GetId("f7s1")));

				foreach (var id in _session.Statistics.UnlockedShips)
				{
					var build = ShipBuildQuery.PlayerShips(_database).All.FirstOrDefault(item => item.Ship.Id == id);
					if (build != null)
						ships.Add(build);
				}
			}

			return ships;
		}

		private readonly Settings _settings;
		private readonly IDatabase _database;
		private readonly ISessionData _session;
		private readonly ExitSignal _exitSignal;
        private readonly IMusicPlayer _musicPlayer;
		private readonly CombatModelBuilder.Factory _combatModelBuilderFactory;
        private readonly DatabaseMusicPlaylist _playlist;

        public class Factory : Factory<Settings, QuickCombatState> { }

		public struct Settings
		{
			public string TestShipId;
			public bool EasyMode;
		}
    }
}
