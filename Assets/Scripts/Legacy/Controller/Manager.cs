using System.Linq;
using UnityEngine;
using Combat.Ai;
using Combat.Component.Unit.Classification;
using Combat.Factory;
using Combat.Scene;
using Combat.Unit;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;
using GameStateMachine.States;
using Gui.Combat;
using Services.Messenger;
using Zenject;

namespace Controller
{
	public class Manager : MonoBehaviour
	{
	    [Inject] private readonly ExitSignal.Trigger _exitTrigger;
	    [Inject] private readonly IScene _scene;
	    [Inject] private readonly IDatabase _database;
        [Inject] private readonly ShipFactory _shipFactory;
        [Inject] private readonly ControllerFactory _controllerFactory;
        [Inject] private readonly ShipControlsPanel _shipControlsPanel;
        [Inject] private readonly IKeyboard _keyboard;
        [Inject] private readonly IMouse _mouse;

        public GameObject Background;
		public GameObject SettingsPanel;
		public ViewModel.PanelController StopButton;

	    [Inject]
	    private void Initialize(IMessenger messenger)
	    {
            messenger.AddListener(EventType.EscapeKeyPressed, OnEscapeKeyPressed);

            SettingsPanel.SetActive(true);
            Background.SetActive(false);
        }

        public void Simulate()
		{
			var playerShip = _scene.PlayerShip;
			if (!playerShip.IsActive())
				playerShip = _shipFactory.CreatePlayerShip(new CommonShip(_database.GetShipBuild(new ItemId<ShipBuild>(226)), 
                    _database).CreateBuilder().Build(_database.ShipSettings), Vector2.zero, 0f);

            if (!_scene.EnemyShip.IsActive())
            {
                var enemyBuild = _database.GetShipBuild(new ItemId<ShipBuild>(218)); // TODO: move to database settings
                _shipFactory.CreateShip(new EnemyShip(enemyBuild, _database).CreateBuilder().Build(_database.ShipSettings),
                    _controllerFactory.CreateDefaultAiController(10, enemyBuild.CustomAI), UnitSide.Enemy, new Vector2(5, 5), 0);
            }

            _shipControlsPanel.Load(playerShip);

			Background.SetActive(true);
			SettingsPanel.SetActive(false);
			StopButton.Open();
		}

		public void Stop()
		{
			foreach (var ship in _scene.Ships.Items)
				ship.Vanish();

            _shipControlsPanel.Load(null);

            Background.SetActive(false);
            SettingsPanel.SetActive(true);
			StopButton.Close();
		}

		public void Exit()
		{
            _exitTrigger.Fire();
        }

		private void OnEscapeKeyPressed()
		{
            if (_scene.Ships.Items.Any(item => item.IsActive()))
		        Stop();
		}
	}
}
