using System.Collections.Generic;
using GameServices.SceneManager;
using Constructor.Ships;
using Domain.Player;
using Session;
using CommonComponents.Signals;
using Services.Audio;
using Zenject;

namespace GameStateMachine.States
{
    public class MainMenuState : BaseState
    {
		[Inject]
		public MainMenuState(
			IStateMachine stateMachine,
			GameStateFactory stateFactory,
			ISessionData session,
			IMusicPlayer musicPlayer,
			StartGameSignal startGameSignal,
			StartQuickBattleSignal startQuickBattleSignal,
			ConfigureControlsSignal configureControlsSignal,
			OpenShipEditorSignal openShipEditorSignal,
			OpenEhopediaSignal openEhopediaSignal,
            DailyReward dailyReward,
            DailyRewardAwailableSignal dailyRewardAwailableSignal,
			OpenGameSettingsSignal openGameSettingsSignal,
			ReloadUiSignal reloadUiSignal,
            ExitSignal exitSignal)
            : base(stateMachine, stateFactory)
        {
            _dailyReward = dailyReward;
            _dailyRewardAwailableSignal = dailyRewardAwailableSignal;
            _dailyRewardAwailableSignal.Event += CheckDailyReward;

            _session = session;
            _musicPlayer = musicPlayer;

			_reloadUiSignal = reloadUiSignal;
			_reloadUiSignal.Event += OnReloadUi;
            _startGameSignal = startGameSignal;
            _startGameSignal.Event += OnStartGame;
            _startQuickBattleSignal = startQuickBattleSignal;
            _startQuickBattleSignal.Event += OnStartQuickBattle;
            _exitSignal = exitSignal;
            _exitSignal.Event += OnExit;
            _configureControlsSignal = configureControlsSignal;
            _configureControlsSignal.Event += OnConfigureControls;
            _openEhopediaSignal = openEhopediaSignal;
            _openEhopediaSignal.Event += OnOpenEhopedia;
			_openShipEditorSignal = openShipEditorSignal;
			_openShipEditorSignal.Event += OnOpenShipEditor;
			_openGameSettingsSignal = openGameSettingsSignal;
			_openGameSettingsSignal.Event += OnOpenGameSettings;
        }

        public override StateType Type { get { return StateType.MainMenu; } }

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.MainMenu; } }

		protected override void OnLoad()
        {
            _musicPlayer.Play(AudioTrackType.Menu);

#if UNITY_STANDALONE
#else
            CheckDailyReward();
#endif
        }

        private void CheckDailyReward()
        {
            if (_dailyReward.IsRewardExists())
                LoadStateAdditive(StateFactory.CreateDaylyRewardState());
        }

        private void OnStartGame()
        {
            if (_session.Game.GameStartTime == 0)
                LoadState(StateFactory.CreateNewGameState());
            else
                LoadState(StateFactory.CreateStarMapState());
        }

        private void OnStartQuickBattle(bool easyMode, string testShipId)
        {
            LoadState(StateFactory.CreateQuickCombatState(new () { EasyMode = easyMode, TestShipId = testShipId }));
        }

        private void OnConfigureControls()
        {
			var state = StateFactory.CreateTestingState();
			if (Condition == GameStateCondition.Active)
				LoadState(state);
			else
				_loadOnResume = state;
        }

		private void OnOpenShipEditor(IShip ship)
		{
			var context = new ShipEditorState.Context { Ship = ship, DatabaseMode = true, NextState = this };
			LoadState(StateFactory.CreateShipEditorState(context));
		}

		private void OnOpenGameSettings()
		{
			LoadStateAdditive(StateFactory.CreateGameSettingsState());
		}

		public void OnOpenEhopedia()
        {
			LoadStateAdditive(StateFactory.CreateEchopediaState());
        }

		protected override void OnResume()
		{
			if (_loadOnResume != null)
			{
				LoadState(_loadOnResume);
				_loadOnResume = null;
			}
		}

		private void OnReloadUi()
		{
			Reload();
		}

        private void OnExit()
        {
			Unload();
        }

		private IGameState _loadOnResume;
		private readonly IMusicPlayer _musicPlayer;
		private readonly ReloadUiSignal _reloadUiSignal;
		private readonly StartGameSignal _startGameSignal;
        private readonly StartQuickBattleSignal _startQuickBattleSignal;
        private readonly ConfigureControlsSignal _configureControlsSignal;
		private readonly OpenShipEditorSignal _openShipEditorSignal;
		private readonly OpenEhopediaSignal _openEhopediaSignal;
        private readonly ExitSignal _exitSignal;
        private readonly ISessionData _session;
        private readonly DailyReward _dailyReward;
        private readonly DailyRewardAwailableSignal _dailyRewardAwailableSignal;
		private readonly OpenGameSettingsSignal _openGameSettingsSignal;

		public class Factory : Factory<MainMenuState> { }
    }

    public class StartGameSignal : SmartWeakSignal<StartGameSignal> {}
    public class StartQuickBattleSignal : SmartWeakSignal<StartQuickBattleSignal, bool, string> {}
    public class ConfigureControlsSignal : SmartWeakSignal<ConfigureControlsSignal> {}
	public class ReloadUiSignal : SmartWeakSignal<ReloadUiSignal> {}
	public class OpenGameSettingsSignal : SmartWeakSignal<OpenGameSettingsSignal> {}
}
