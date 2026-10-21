using System;
using System.Collections.Generic;
using Combat.Domain;
using GameStateMachine.States;
using Gui.Combat;
using Services.Messenger;
using Zenject;

namespace Combat.Integration.Core
{
    // Projects the shared combat core through the application contracts (migration phase 3c).
    // Skeleton: no battle yet; the menu can only leave the scene.
    public sealed class CoreCombatAdapter : IRosterView, IBattleView, IShipViews, IShipControlSource, ICombatCommands
    {
        [Inject]
        public CoreCombatAdapter(ICombatModel combatModel, IMessenger messenger, LazyInject<CombatMenu> combatMenu, ExitSignal.Trigger exitTrigger)
        {
            _combatModel = combatModel;
            _combatMenu = combatMenu;
            _exitTrigger = exitTrigger;

            messenger.AddListener(EventType.EscapeKeyPressed, OnEscapeKeyPressed);
        }

        #region IRosterView

        public IReadOnlyList<IRosterSlotView> PlayerSlots => Array.Empty<IRosterSlotView>();
        public IReadOnlyList<IRosterSlotView> EnemySlots => Array.Empty<IRosterSlotView>();

        public event Action<RosterSlotChange> SlotChanged;

        public bool TryGetSlot(RosterId id, out IRosterSlotView slot)
        {
            slot = null;
            return false;
        }

        public int Count(CombatSide side, ShipStatus status) => 0;

        #endregion

        #region IBattleView

        public EntityId ControlledShip => EntityId.None;
        public EntityId TargetShip => EntityId.None;
        public CombatTimerState Timer => default;
        public bool CanChangeShip => false;
        public bool CanCallReinforcement => false;
        public bool KillAllAllowedByRules => _combatModel.Rules.KillThemAllButton;

        #endregion

        #region IShipViews

        public bool TryGetShip(EntityId id, out IShipView ship)
        {
            ship = null;
            return false;
        }

        #endregion

        #region IShipControlSource

        public bool TryGetControls(EntityId ship, out IShipControlDescription description, out IShipCommandSink commands)
        {
            description = null;
            commands = null;
            return false;
        }

        #endregion

        #region ICombatCommands

        public void Deploy(RosterId slot) { }
        public void Retreat(EntityId ship) { }
        public void CallReinforcement() { }

        public void Surrender()
        {
            if (_exited) return;
            _exited = true;
            _exitTrigger.Fire();
        }

        public void KillAllEnemies() { }

        #endregion

        private void OnEscapeKeyPressed()
        {
            // Lazy: the menu injects this adapter.
            var menu = _combatMenu.Value;
            if (menu)
                menu.Open();
        }

        private bool _exited;

        private readonly ICombatModel _combatModel;
        private readonly LazyInject<CombatMenu> _combatMenu;
        private readonly ExitSignal.Trigger _exitTrigger;
    }
}
