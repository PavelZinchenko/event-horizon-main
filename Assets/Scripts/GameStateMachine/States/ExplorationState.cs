using System.Collections.Generic;
using GameServices.SceneManager;
using Game.Exploration;
using Gui.Combat;
using Services.Audio;
using Services.Gui;
using Zenject;

namespace GameStateMachine.States
{
    public class ExplorationState : BaseState
    {
        [Inject]
        public ExplorationState(
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            IGuiManager guiManager,
            Planet planet,
            IMusicPlayer musicPlayer,
            EscapeKeyPressedSignal escapeKeyPressedSignal,
            ExitSignal exitSignal)
            : base(stateMachine, stateFactory)
        {
            _guiManager = guiManager;
            _planet = planet;
            _musicPlayer = musicPlayer;
            _exitSignal = exitSignal;
            _exitSignal.Event += OnCombatCompleted;
            _escapeKeyPressedSignal = escapeKeyPressedSignal;
            _escapeKeyPressedSignal.Event += OnEscapeKeyPressed;
        }

        public override StateType Type => StateType.Exploration;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.Exploration; } }

		public override void InstallBindings(DiContainer container)
		{
			container.Bind<Planet>().FromInstance(_planet);
		}

		protected override void OnLoad()
        {
            _musicPlayer.Play(AudioTrackType.Exploration);
        }

        private void OnEscapeKeyPressed()
        {
            _guiManager.OpenWindow(WindowNames.CombatMenuWindow);
        }

        private void OnCombatCompleted()
        {
			//if (!IsActive)
			//    return;

			//var reward = _combatModel.GetReward(_lootGenerator, _playerSkills, _motherShip.CurrentStar);
			//reward.Consume(_playerSkills);

			//var action = _onCompleteAction;
			//_onCompleteAction = null;

			//if (action != null)
			//    action.Invoke(_combatModel);

			//_combatCompletedTrigger.Fire(_combatModel);

			//if (reward.Any())
			//    ShowRewardDialog(reward);

			LoadState(StateFactory.CreateStarMapState());
        }

        //private void ShowRewardDialog(IReward reward)
        //{
        //    StateMachine.LoadState(StateFactory.CreateCombatRewardState(reward));
        //}

        private readonly Planet _planet;
        private readonly IMusicPlayer _musicPlayer;
        private readonly IGuiManager _guiManager;
        private readonly EscapeKeyPressedSignal _escapeKeyPressedSignal;
        private readonly ExitSignal _exitSignal;

        public class Factory : Factory<Planet, ExplorationState> { }
    }
}
