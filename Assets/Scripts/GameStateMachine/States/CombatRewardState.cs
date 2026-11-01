using System.Collections.Generic;
using GameModel.Quests;
using GameServices.SceneManager;
using Gui.Combat;
using Services.Gui;
using GameDatabase;
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
            IDatabase database,
            IReward reward)
            : base(stateMachine, stateFactory)
        {
            _guiManager = guiManager;
            _reward = reward;

            // The window opens over the battle scene that CombatState chose with the same switch.
            _useCombatCore = database.CombatSettings.UseNewCombatSimulator;
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

		public override IEnumerable<GameScene> RequiredScenes { get { yield return _useCombatCore ? GameScene.CoreCombat : GameScene.Combat; } }

        private readonly bool _useCombatCore;
        private readonly IReward _reward;
        private readonly IGuiManager _guiManager;

        public class Factory : Factory<IReward, CombatRewardState> { }
    }
}
