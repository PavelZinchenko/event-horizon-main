using Combat.Domain;
using Combat.Integration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.Combat
{
    public class ShipCounter : MonoBehaviour
    {
        public enum Type
        {
            Player,
            Enemy,
        }

        [SerializeField] private Text _countText;
        [SerializeField] private Type _type;

        [Inject]
        private void Initialize(IRosterView roster)
        {
            _roster = roster;
            _roster.SlotChanged += OnSlotChanged;
            UpdateCount();
        }

        private void OnDestroy()
        {
            if (_roster != null)
                _roster.SlotChanged -= OnSlotChanged;
        }

        private void OnSlotChanged(RosterSlotChange change)
        {
            if (change.Slot.Side == Side)
                UpdateCount();
        }

        private void UpdateCount()
        {
            _countText.text = _roster.Count(Side, ShipStatus.Ready).ToString();
        }

        private CombatSide Side => _type == Type.Player ? CombatSide.Player : CombatSide.Enemy;

        private IRosterView _roster;
    }
}
