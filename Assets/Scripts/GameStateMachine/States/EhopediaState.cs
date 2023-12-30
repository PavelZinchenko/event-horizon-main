using System.Collections.Generic;
using GameServices.SceneManager;
using Zenject;

namespace GameStateMachine.States
{
    public class EhopediaState : BaseState
    {
        [Inject]
        public EhopediaState(
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            ExitSignal exitSignal)
            : base(stateMachine, stateFactory)
        {
            _exitSignal = exitSignal;
            _exitSignal.Event += OnExit;
        }

        public override StateType Type => StateType.Ehopedia;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.Ehopedia; } }

		private void OnExit()
        {
			Unload();
        }

        private readonly ExitSignal _exitSignal;

        public class Factory : Factory<EhopediaState> { }
    }
}
