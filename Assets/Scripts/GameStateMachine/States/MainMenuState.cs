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
			OpenConstructorSignal openConstructorSignal,
			OpenEhopediaSignal openEhopediaSignal,
            DailyReward dailyReward,
            DailyRewardAwailableSignal dailyRewardAwailableSignal,
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
            _openConstructorSignal = openConstructorSignal;
            _openConstructorSignal.Event += OnOpenConstructor;
            _openEhopediaSignal = openEhopediaSignal;
            _openEhopediaSignal.Event += OnOpenEhopedia;
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
			LoadState(StateFactory.CreateTestingState());
        }

        private void OnOpenConstructor(IShip ship)
        {
            LoadState(StateFactory.CreateConstructorState(ship, this));
        }

		public void OnOpenEhopedia()
        {
			LoadStateAdditive(StateFactory.CreateEchopediaState());
        }

		private void OnReloadUi()
		{
			Reload();
		}

        private void OnExit()
        {
			Unload();
        }

        private readonly IMusicPlayer _musicPlayer;
		private readonly ReloadUiSignal _reloadUiSignal;
		private readonly StartGameSignal _startGameSignal;
        private readonly StartQuickBattleSignal _startQuickBattleSignal;
        private readonly ConfigureControlsSignal _configureControlsSignal;
        private readonly OpenConstructorSignal _openConstructorSignal;
		private readonly OpenEhopediaSignal _openEhopediaSignal;
        private readonly ExitSignal _exitSignal;
        private readonly ISessionData _session;
        private readonly DailyReward _dailyReward;
        private readonly DailyRewardAwailableSignal _dailyRewardAwailableSignal;

		public class Factory : Factory<MainMenuState> { }
    }

    public class StartGameSignal : SmartWeakSignal<StartGameSignal> {}
    public class StartQuickBattleSignal : SmartWeakSignal<StartQuickBattleSignal, bool, string> {}
    public class ConfigureControlsSignal : SmartWeakSignal<ConfigureControlsSignal> {}
	public class ReloadUiSignal : SmartWeakSignal<ReloadUiSignal> {}
}
