using System;
using Combat.Integration;
using Zenject;

namespace Gui.Combat
{
    // Opens the stats panels, enemy radars and player controls from the combat contracts.
    // Exploration drives the same panels through their legacy entries.
    class CombatHudPresenter : ITickable, IDisposable
    {
        // Field names select the panel binding; see CombatSceneInstaller.
        [Inject] private readonly ShipStatsPanel _playerStatsPanel;
        [Inject] private readonly ShipStatsPanel _enemyStatsPanel;
        [Inject] private readonly RadarPanel _radarPanel;
        [Inject] private readonly ShipControlsPanel _shipControlsPanel;

        // Subscribes in the constructor so that no deployment is missed before the first tick.
        [Inject]
        public CombatHudPresenter(IBattleView battle, IShipViews ships, IShipControlSource controls, IRosterView roster)
        {
            _battle = battle;
            _ships = ships;
            _controls = controls;
            _roster = roster;
            _roster.SlotChanged += OnSlotChanged;
        }

        public void Tick()
        {
            var player = _battle.ControlledShip;
            var enemy = _battle.TargetShip;
            if (!player.IsValid || !enemy.IsValid)
                return;

            if (_ships.TryGetShip(player, out var playerShip))
                _playerStatsPanel.Open(playerShip);
            if (_ships.TryGetShip(enemy, out var enemyShip))
                _enemyStatsPanel.Open(enemyShip);
        }

        public void Dispose()
        {
            _roster.SlotChanged -= OnSlotChanged;
        }

        private void OnSlotChanged(RosterSlotChange change)
        {
            if (change.Reason != RosterChangeReason.Deployed)
                return;
            if (!_ships.TryGetShip(change.Entity, out var ship))
                return;

            if (change.Slot.Side == CombatSide.Enemy)
                _radarPanel.Add(ship);
            else if (_controls.TryGetControls(change.Entity, out var description, out var commands))
                _shipControlsPanel.Load(ship, description, commands);
        }

        private readonly IBattleView _battle;
        private readonly IShipViews _ships;
        private readonly IShipControlSource _controls;
        private readonly IRosterView _roster;
    }
}
