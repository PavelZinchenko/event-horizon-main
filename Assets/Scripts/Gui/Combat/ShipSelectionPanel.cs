using System.Collections.Generic;
using Combat.Domain;
using Combat.Integration;
using Services.Gui;
using UnityEngine;
using Zenject;

namespace Gui.Combat
{
    public class ShipSelectionPanel : MonoBehaviour
    {
        [Inject] private readonly IRosterView _roster;
        [Inject] private readonly IBattleView _battle;
        [Inject] private readonly ICombatCommands _commands;

        [SerializeField] private ShipList _enemyShips;
        [SerializeField] private ShipList _playerShips;

        public void Open()
        {
            if (Window.IsVisible)
                return;

            _enemyShips.Initialize(_roster.EnemySlots, FindActiveIndex(_roster.EnemySlots));
            _playerShips.Initialize(_roster.PlayerSlots, FindActiveIndex(_roster.PlayerSlots));
            GetComponent<IWindow>().Open();
        }

        public void StartButtonClicked()
        {
            var slot = _playerShips.SelectedShip;
            if (slot == null || slot.Status != ShipStatus.Ready)
                return;

            _commands.Deploy(slot.Id);
            GetComponent<IWindow>().Close();
        }

        private void Update()
        {
            if (_battle.ControlledShip.IsValid && Window.IsVisible)
                Window.Close();
        }

        private static int FindActiveIndex(IReadOnlyList<IRosterSlotView> slots)
        {
            for (var i = 0; i < slots.Count; ++i)
                if (slots[i].Status == ShipStatus.Active)
                    return i;

            return -1;
        }

        private IWindow Window { get { return GetComponent<IWindow>(); } }
    }
}
