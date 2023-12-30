using System.Collections.Generic;
using GameServices.SceneManager;
using Services.Messenger;
using Constructor.Ships;
using CommonComponents.Utils;
using Zenject;

namespace GameStateMachine.States
{
    class ConstructorState : BaseState
    {
        [Inject]
        public ConstructorState(
            IStateMachine stateMachine,
            GameStateFactory stateFactory,
            IMessengerContext messenger,
            IShip ship,
			IGameState nextState,
            ExitSignal exitSignal,
            ShipSelectedSignal shipSelectedSignal)
            : base(stateMachine, stateFactory)
        {
            _ship = ship;
			_nextState = nextState;
            _messenger = messenger;
            _exitSignal = exitSignal;
            _exitSignal.Event += OnExit;
            _shipSelectedSignal = shipSelectedSignal;
            _shipSelectedSignal.Event += OnShipSelected;
        }

		public override StateType Type => StateType.Constructor;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.ShipEditor; } }

		protected override void OnLoad()
        {
            OnShipSelected(_ship);
        }

        private void OnExit()
        {
			LoadState(_nextState);
        }

        private void OnShipSelected(IShip ship)
        {
			if (Condition == GameStateCondition.Active)
			{
				_ship = ship;
                _messenger.Broadcast(EventType.ConstructorShipChanged, _ship);
            }
        }

        private IShip _ship;
        private readonly ExitSignal _exitSignal;
        private readonly ShipSelectedSignal _shipSelectedSignal;
        private readonly IMessengerContext _messenger;
		private readonly IGameState _nextState;

		public class Factory : Factory<IShip, IGameState, ConstructorState> { }
    }

    public class ShipSelectedSignal : SmartWeakSignal<IShip>
    {
        public class Trigger : TriggerBase { }
    }
}
