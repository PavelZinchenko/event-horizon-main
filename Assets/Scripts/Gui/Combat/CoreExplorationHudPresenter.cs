using System;
using Combat.Integration;
using Combat.Integration.Core;
using Zenject;

namespace Gui.Combat
{
    // The exploration HUD on the shared combat core (exploration plan, E2), by the legacy ExplorationSceneManager rules:
    // the player's stats always; the enemy's stats for the nearest active enemy ship, otherwise closed; the controls for
    // the player's ship; a radar for each enemy ship view and a beacon for each objective. CombatHudPresenter stays the
    // roster one: its rule differs, and it reads the roster.
    class CoreExplorationHudPresenter : ITickable, IInitializable, IDisposable
    {
        // Field names select the panel binding, as in CoreCombatSceneInstaller.
        [Inject] private readonly ShipStatsPanel _playerStatsPanel;
        [Inject] private readonly ShipStatsPanel _enemyStatsPanel;
        [Inject] private readonly RadarPanel _radarPanel;
        [Inject] private readonly ShipControlsPanel _shipControlsPanel;

        // Subscribes in the constructor so that no ship is missed before the first tick. A ship that vanishes and returns
        // gets a new view and a new radar; the old radar closes itself when its view goes inactive.
        [Inject]
        public CoreExplorationHudPresenter(CoreExplorationAdapter adapter)
        {
            _adapter = adapter;
            _adapter.ShipAdded += OnShipAdded;
        }

        // After the panels' injection.
        public void Initialize()
        {
            foreach (var beacon in _adapter.Beacons)
                _radarPanel.AddBeacon(beacon);
        }

        public void Tick()
        {
            if (_adapter.TryGetShip(_adapter.ControlledShip, out var player))
                _playerStatsPanel.Open(player);

            if (_adapter.TryGetShip(_adapter.TargetShip, out var enemy) && enemy.IsActive)
                _enemyStatsPanel.Open(enemy);
            else
                _enemyStatsPanel.Close();
        }

        public void Dispose()
        {
            _adapter.ShipAdded -= OnShipAdded;
        }

        private void OnShipAdded(EntityId id, CombatSide side)
        {
            if (!_adapter.TryGetShip(id, out var ship))
                return;

            if (side == CombatSide.Enemy)
                _radarPanel.Add(ship);
            else if (_adapter.TryGetControls(id, out var description, out var commands))
                _shipControlsPanel.Load(ship, description, commands);
        }

        private readonly CoreExplorationAdapter _adapter;
    }
}
