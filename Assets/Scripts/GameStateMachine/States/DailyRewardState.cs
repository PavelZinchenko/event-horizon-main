using System.Linq;
using System.Collections.Generic;
using GameServices.SceneManager;
using Domain.Player;
using Economy.Products;
using Gui.MainMenu;
using Services.Gui;
using Zenject;

namespace GameStateMachine.States
{
    public class DailyRewardState : BaseState
    {
        [Inject]
        public DailyRewardState(
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            DailyReward dailyReward,
            ExitSignal exitSignal,
            IGuiManager guiManager)
            : base(stateMachine, stateFactory)
        {
            _exitSignal = exitSignal;
            _exitSignal.Event += OnExit;
            _guiManager = guiManager;
            _dailyReward = dailyReward;
        }

        public override StateType Type { get { return StateType.DailyReward; } }

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.MainMenu; } }

        protected override void OnLoad()
        {
            var rewards = _dailyReward.CollectReward();
            foreach (var item in rewards)
                item.Consume();

            var args = new WindowArgs(rewards.Cast<object>().ToArray());
            _guiManager.OpenWindow(WindowNames.DailyRewardWindow, args, OnRewardSelecred);
        }

        private void OnRewardSelecred(WindowExitCode exitCode)
        {
			Unload();
        }

        private void OnExit()
        {
			Unload();
        }

        private readonly IGuiManager _guiManager;
        private readonly ExitSignal _exitSignal;
        private readonly DailyReward _dailyReward;

        public class Factory : Factory<DailyRewardState> { }
    }
}
