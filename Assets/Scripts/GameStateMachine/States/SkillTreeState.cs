using System.Collections.Generic;
using GameServices.SceneManager;
using Zenject;

namespace GameStateMachine.States
{
    class SkillTreeState : BaseState
    {
        [Inject]
        public SkillTreeState(
            IStateMachine stateMachine,
            GameStateFactory gameStateFactory,
            ExitSignal exitSignal)
            : base(stateMachine, gameStateFactory)
        {
            _exitSignal = exitSignal;
            _exitSignal.Event += OnExit;
        }

        public override StateType Type => StateType.SkillTree;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.SkillTree; } }

		private void OnExit()
        {
			LoadState(StateFactory.CreateStarMapState());
        }

        private readonly ExitSignal _exitSignal;

        public class Factory : Factory<SkillTreeState> { }
    }
}
