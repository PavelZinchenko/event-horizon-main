using System.Collections.Generic;
using GameModel.Quests;
using GameServices.SceneManager;
using Gui.Combat;
using Services.Gui;
using Zenject;

namespace GameStateMachine.States
{
    public class CombatRewardState : BaseState
    {
        [Inject]
        public CombatRewardState(
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            IGuiManager guiManager,
            IReward reward)
            : base(stateMachine, stateFactory)
        {
            _guiManager = guiManager;
            _reward = reward;
        }

        public override StateType Type => StateType.CombatReward;

        protected override void OnLoad()
        {
            _guiManager.OpenWindow(WindowNames.CombatRewardWindow, new WindowArgs(_reward), OnWindowClosed);
        }

        private void OnWindowClosed(WindowExitCode exitCode)
        {
			LoadState(StateFactory.CreateStarMapState());
		}

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.Combat; } }

        private readonly IReward _reward;
        private readonly IGuiManager _guiManager;

        public class Factory : Factory<IReward, CombatRewardState> { }
    }
}
