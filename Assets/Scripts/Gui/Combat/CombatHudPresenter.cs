using System;
using Combat.Integration;
using Zenject;

namespace Gui.Combat
{
    // Opens the stats panels and enemy radars from the combat contracts.
    // Exploration drives the same panels through their legacy entries.
    class CombatHudPresenter : ITickable, IDisposable
    {
        // Field names select the panel binding; see CombatSceneInstaller.
        [Inject] private readonly ShipStatsPanel _playerStatsPanel;
        [Inject] private readonly ShipStatsPanel _enemyStatsPanel;
        [Inject] private readonly RadarPanel _radarPanel;

        // Subscribes in the constructor so that no deployment is missed before the first tick.
        [Inject]
        public CombatHudPresenter(IBattleView battle, IShipViews ships, IRosterView roster)
        {
            _battle = battle;
            _ships = ships;
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
            if (change.Reason != RosterChangeReason.Deployed || change.Slot.Side != CombatSide.Enemy)
                return;

            if (_ships.TryGetShip(change.Entity, out var ship))
                _radarPanel.Add(ship);
        }

        private readonly IBattleView _battle;
        private readonly IShipViews _ships;
        private readonly IRosterView _roster;
    }
}
