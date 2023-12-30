using System.Collections.Generic;
using GameServices.SceneManager;
using Session;
using GameServices.Player;
using Services.Gui;
using Services.Messenger;
using Zenject;

namespace GameStateMachine.States
{
	class RetreatState : BaseState
	{
		[Inject]
		public RetreatState(
			IStateMachine stateMachine,
            GameStateFactory gameStateFactory,
			MotherShip motherShip,
			Galaxy.StarMap starMap,
            IGuiManager guiManager,
            ISessionData session,
			IMessengerContext messenger)
			: base(stateMachine, gameStateFactory)
		{
			_source = session.StarMap.PlayerPosition;
			_starMap = starMap;
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
				UnityEngine.Debug.Log("RetreatState: Finished");

				_session.StarMap.PlayerPosition = _destination;
				Unload();
			}
		}

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.StarMap; } }

		protected override void OnLoad()
		{
            UnityEngine.Debug.Log("RetreatState: Started - " + _destination);
            var starId = _starMap.GetNearestVisited(_source, true);
			_destination = starId < 0 ? _session.StarMap.LastPlayerPosition : starId;
			_motherShip.ViewMode = ViewMode.StarMap;
			_lifeTime = _motherShip.CalculateFlightTime(_source, _destination);

            _guiManager.AutoWindowsAllowed = false;
            _guiManager.CloseAllWindows();
        }

        protected override void OnUnload()
        {
            _guiManager.AutoWindowsAllowed = true;
        }

        private float _lifeTime;
		private float _progress;
		private int _destination;

		private readonly int _source;
		private readonly ISessionData _session;
		private readonly IMessengerContext _messenger;
		private readonly Galaxy.StarMap _starMap;
		private readonly MotherShip _motherShip;
        private readonly IGuiManager _guiManager;

        public class Factory : Factory<RetreatState> {}
	}
}
