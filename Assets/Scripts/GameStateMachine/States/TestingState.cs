using System.Collections.Generic;
using GameServices.SceneManager;
using Zenject;

namespace GameStateMachine.States
{
    public class TestingState : BaseState
    {
        [Inject]
        public TestingState(
            IStateMachine stateMachine,
            GameStateFactory gameStateFactory,
            ExitSignal exitSignal)
            : base(stateMachine, gameStateFactory)
        {
            _exitSignal = exitSignal;
            _exitSignal.Event += OnExit;
        }

		public override StateType Type => StateType.Testing;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.ConfigureControls; } }

        private void OnExit()
        {
			LoadState(StateFactory.CreateMainMenuState());
        }

        private readonly ExitSignal _exitSignal;

        public class Factory : Factory<TestingState> { }
    }
}
