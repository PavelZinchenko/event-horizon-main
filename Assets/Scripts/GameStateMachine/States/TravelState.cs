using System.Collections.Generic;
using GameServices.SceneManager;
using Session;
using GameServices.Player;
using Services.Gui;
using Services.Messenger;
using Zenject;

namespace GameStateMachine.States
{
    class TravelState : BaseState
    {
        [Inject]
        public TravelState(
            IStateMachine stateMachine,
            GameStateFactory gameStateFactory,
            int destination,
            PlayerResources playerResources,
            MotherShip motherShip,
            IGuiManager guiManager,
            ISessionData session,
            IMessengerContext messenger)
            : base(stateMachine, gameStateFactory)
        {
            _source = session.StarMap.PlayerPosition;
            _destination = destination;
            _playerResources = playerResources;
            _session = session;
            _messenger = messenger;
            _motherShip = motherShip;
            _guiManager = guiManager;
        }

		public override StateType Type => StateType.Travel;

		public override void Update(float elapsedTime)
        {
            _progress += elapsedTime / _lifeTime;

            if (_progress < 1f)
            {
                _messenger.Broadcast<int, int, float>(EventType.PlayerShipMoved, _source, _destination, _progress);
            }
            else
            {
				GameDiagnostics.Debug.Log("FlightState: Finished");

                _session.StarMap.PlayerPosition = _destination;
                Unload();
            }
        }

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.StarMap; } }

		protected override void OnLoad()
        {
            GameDiagnostics.Debug.Log("FlightState: Started - " + _destination);

			_lifeTime = _motherShip.CalculateFlightTime(_source, _destination);
			var requiredFuel = _motherShip.CalculateRequiredFuel(_source, _destination);

			if (!_playerResources.TryConsumeFuel(requiredFuel) && requiredFuel > 1)
            {
				GameDiagnostics.Debug.Log("FlightState: not enough fuel");
				Unload();
                return;
            }

            _guiManager.AutoWindowsAllowed = false;
            _guiManager.CloseAllWindows(window => window.Class != WindowClass.Balloon);
        }

        protected override void OnUnload()
        {
            _guiManager.AutoWindowsAllowed = true;
        }

        private float _lifeTime;
        private float _progress;

        private readonly int _source;
        private readonly int _destination;
        private readonly PlayerResources _playerResources;
        private readonly ISessionData _session;
        private readonly IMessengerContext _messenger;
        private readonly MotherShip _motherShip;
        private readonly IGuiManager _guiManager;

        public class Factory : Factory<int, TravelState> {}
    }
}
