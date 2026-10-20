using System;
using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Domain;
using Combat.Manager;
using Combat.Scene;
using Combat.Unit;
using GameStateMachine.States;
using Zenject;

namespace Combat.Integration.Legacy
{
    // Projects the legacy simulator through the application contracts.
    // ShipInfo, FleetModel, CombatManager and CombatTimer remain the authority:
    // commands are delegated to them, and the projection only observes their state.
    public sealed class LegacyCombatAdapter : IRosterView, IBattleView, IShipViews, IShipControlSource, ICombatCommands, ITickable, IDisposable
    {
        [Inject]
        public LegacyCombatAdapter(
            ICombatModel combatModel,
            IScene scene,
            CombatManager manager,
            CombatTimer timer,
            ShipCreatedSignal shipCreatedSignal,
            ShipDestroyedSignal shipDestroyedSignal,
            ExitSignal exitSignal)
        {
            _combatModel = combatModel;
            _scene = scene;
            _manager = manager;
            _timer = timer;
            _shipCreatedSignal = shipCreatedSignal;
            _shipDestroyedSignal = shipDestroyedSignal;
            _exitSignal = exitSignal;

            var nextId = 1;
            foreach (var info in combatModel.PlayerFleet.Ships)
                _playerSlots.Add(new Slot(new RosterId(nextId++), CombatSide.Player, info, this));
            foreach (var info in combatModel.EnemyFleet.Ships)
                _enemySlots.Add(new Slot(new RosterId(nextId++), CombatSide.Enemy, info, this));

            _shipCreatedSignal.Event += OnShipCreated;
            _shipDestroyedSignal.Event += OnShipDestroyed;
            _exitSignal.Event += OnExit;
        }

        #region IRosterView

        public IReadOnlyList<IRosterSlotView> PlayerSlots => _playerSlots;
        public IReadOnlyList<IRosterSlotView> EnemySlots => _enemySlots;

        public event Action<RosterSlotChange> SlotChanged;

        public bool TryGetSlot(RosterId id, out IRosterSlotView slot)
        {
            var result = FindSlot(id);
            slot = result;
            return result != null;
        }

        public int Count(CombatSide side, ShipStatus status)
        {
            var slots = side == CombatSide.Player ? _playerSlots : _enemySlots;
            var count = 0;
            for (var i = 0; i < slots.Count; ++i)
                if (slots[i].Status == status)
                    count++;

            return count;
        }

        #endregion

        #region IBattleView

        public EntityId ControlledShip => GetEntityId(_scene.PlayerShip);
        public EntityId TargetShip => GetEntityId(_scene.EnemyShip);

        public CombatTimerState Timer => _timer.State;

        public bool CanChangeShip => _manager.CanChangeShip();
        public bool CanCallReinforcement => _manager.CanCallNextEnemy();
        public bool KillAllAllowedByRules => _manager.CanKillAllEnemies;

        #endregion

        #region IShipViews

        public bool TryGetShip(EntityId id, out IShipView ship)
        {
            var found = _viewsById.TryGetValue(id, out var view);
            ship = view;
            return found;
        }

        #endregion

        #region IShipControlSource

        public bool TryGetControls(EntityId id, out IShipControlDescription description, out IShipCommandSink commands)
        {
            if (!_controlsById.TryGetValue(id, out var controls))
            {
                var ship = FindShip(id);
                if (ship == null)
                {
                    description = null;
                    commands = null;
                    return false;
                }

                controls = new LegacyShipControls(ship);
                _controlsById.Add(id, controls);
            }

            description = controls;
            commands = controls;
            return true;
        }

        #endregion

        #region ICombatCommands

        public void Deploy(RosterId id)
        {
            if (_frozen)
                return;

            var slot = FindSlot(id);
            if (slot == null || slot.Side != CombatSide.Player || slot.Status != ShipStatus.Ready)
                return;
            if (Count(CombatSide.Player, ShipStatus.Active) > 0)
                return;

            _manager.CreateShip(slot.Info);
        }

        public void Retreat(EntityId ship)
        {
            if (_frozen || !ship.IsValid || ship != ControlledShip)
                return;
            if (!_manager.CanChangeShip())
                return;

            _manager.ChangeShip();
        }

        public void CallReinforcement()
        {
            if (_frozen)
                return;

            _manager.CallNextEnemy();
        }

        public void Surrender()
        {
            if (_frozen)
                return;

            // Same steps as CombatManager.Surrender, with the roster batch published before the exit signal.
            _combatModel.PlayerFleet.DestroyAllShips();
            SyncForcedDestruction(_playerSlots, includeDeployed: true);
            _manager.Exit();
        }

        public void KillAllEnemies()
        {
            if (_frozen)
                return;
#if !UNITY_EDITOR
            if (!_manager.CanKillAllEnemies)
                return;
#endif
            _manager.KillAllEnemies();
            SyncForcedDestruction(_enemySlots, includeDeployed: true);
        }

        #endregion

        public void Tick()
        {
            // Legacy code can still destroy reserve slots without an entity removal (for example kill-all
            // through the old menu). Publish those transitions; slots with a live entity wait for its removal.
            if (_frozen)
                return;

            SyncForcedDestruction(_playerSlots, includeDeployed: false);
            SyncForcedDestruction(_enemySlots, includeDeployed: false);
        }

        public void Dispose()
        {
            _frozen = true;
            _shipCreatedSignal.Event -= OnShipCreated;
            _shipDestroyedSignal.Event -= OnShipDestroyed;
            _exitSignal.Event -= OnExit;
            _views.Clear();
            _viewsById.Clear();
            _controlsById.Clear();
        }

        // A ship that is no longer active has no live entity, even before the scene removes it.
        // Removal handlers read _views directly.
        private EntityId GetEntityId(IShip ship)
        {
            if (ship == null || !ship.IsActive())
                return EntityId.None;
            if (_views.TryGetValue(ship, out var view))
                return view.Id;
            if (_frozen)
                return EntityId.None;

            view = new LegacyShipView(ship, new EntityId(_nextEntityId++));
            _views.Add(ship, view);
            _viewsById.Add(view.Id, view);
            return view.Id;
        }

        private EntityId FindEntityId(IShip ship)
        {
            return _views.TryGetValue(ship, out var view) ? view.Id : EntityId.None;
        }

        private IShip FindShip(EntityId id)
        {
            if (!id.IsValid)
                return null;

            foreach (var item in _views)
                if (item.Value.Id == id)
                    return item.Key;

            return null;
        }

        private void OnShipCreated(IShip ship)
        {
            if (_frozen || ship.Type.Class != UnitClass.Ship)
                return;

            var slot = FindSlot(ship);
            if (slot == null)
                return;

            slot.DeploymentOrder = ++_deploymentCounter;
            slot.Deployed = ship;
            Publish(slot, RosterChangeReason.Deployed, GetEntityId(ship));
        }

        private void OnShipDestroyed(IShip ship)
        {
            if (_frozen)
                return;

            var entity = FindEntityId(ship);
            _views.Remove(ship);
            _viewsById.Remove(entity);
            _controlsById.Remove(entity);

            if (ship.Type.Class != UnitClass.Ship)
                return;

            var slot = FindDeployedSlot(ship);
            if (slot == null)
                return;

            slot.Deployed = null;

            RosterChangeReason reason;
            if (slot.Status == ShipStatus.Ready)
                reason = RosterChangeReason.Retreated;
            else if (ship.State == UnitState.Destroyed)
                reason = RosterChangeReason.Killed;
            else
                reason = RosterChangeReason.ForcedDestruction;

            Publish(slot, reason, entity);
        }

        private void OnExit()
        {
            // The battle result is final; scene teardown must not produce roster events.
            _frozen = true;
        }

        // A command-driven batch includes active slots: their entity is removed later (kill-all) or never
        // (surrender clears the scene). The later removal finds the status already published and raises nothing.
        private void SyncForcedDestruction(List<Slot> slots, bool includeDeployed)
        {
            foreach (var slot in slots)
            {
                if (slot.Status != ShipStatus.Destroyed)
                    continue;
                if (slot.Deployed != null && !includeDeployed)
                    continue;

                var entity = slot.Deployed != null ? FindEntityId(slot.Deployed) : EntityId.None;

                Publish(slot, RosterChangeReason.ForcedDestruction, entity);
            }
        }

        private void Publish(Slot slot, RosterChangeReason reason, EntityId entity)
        {
            var status = slot.Status;
            var previous = slot.PublishedStatus;
            if (status == previous)
                return;

            slot.PublishedStatus = status;
            SlotChanged?.Invoke(new RosterSlotChange(slot, previous, reason, entity));
        }

        private Slot FindSlot(RosterId id)
        {
            foreach (var slot in _playerSlots)
                if (slot.Id == id) return slot;
            foreach (var slot in _enemySlots)
                if (slot.Id == id) return slot;
            return null;
        }

        private Slot FindSlot(IShip ship)
        {
            foreach (var slot in _playerSlots)
                if (slot.Info.ShipUnit == ship) return slot;
            foreach (var slot in _enemySlots)
                if (slot.Info.ShipUnit == ship) return slot;
            return null;
        }

        // Surrender and kill-all replace ShipInfo.ShipUnit, so removal is matched by the deployed reference.
        private Slot FindDeployedSlot(IShip ship)
        {
            foreach (var slot in _playerSlots)
                if (slot.Deployed == ship) return slot;
            foreach (var slot in _enemySlots)
                if (slot.Deployed == ship) return slot;
            return null;
        }

        private sealed class Slot : IRosterSlotView
        {
            public Slot(RosterId id, CombatSide side, IShipInfo info, LegacyCombatAdapter owner)
            {
                Id = id;
                Side = side;
                Info = info;
                _owner = owner;
                PublishedStatus = info.Status;
            }

            public RosterId Id { get; }
            public CombatSide Side { get; }
            public IShipInfo Info { get; }

            public Constructor.Ships.IShip ShipData => Info.ShipData;
            public ShipStatus Status => Info.Status;
            public float Condition => Info.Condition;
            public int DeploymentOrder { get; set; }
            public EntityId ActiveEntity => Status == ShipStatus.Active ? _owner.GetEntityId(Info.ShipUnit) : EntityId.None;

            // Change detection only; ShipInfo stays the authority for status.
            public ShipStatus PublishedStatus { get; set; }
            public IShip Deployed { get; set; }

            private readonly LegacyCombatAdapter _owner;
        }

        private readonly List<Slot> _playerSlots = new List<Slot>();
        private readonly List<Slot> _enemySlots = new List<Slot>();
        private readonly Dictionary<IShip, LegacyShipView> _views = new Dictionary<IShip, LegacyShipView>();
        private readonly Dictionary<EntityId, LegacyShipView> _viewsById = new Dictionary<EntityId, LegacyShipView>();
        private readonly Dictionary<EntityId, LegacyShipControls> _controlsById = new Dictionary<EntityId, LegacyShipControls>();
        private int _nextEntityId = 1;
        private int _deploymentCounter;
        private bool _frozen;

        private readonly ICombatModel _combatModel;
        private readonly IScene _scene;
        private readonly CombatManager _manager;
        private readonly CombatTimer _timer;
        private readonly ShipCreatedSignal _shipCreatedSignal;
        private readonly ShipDestroyedSignal _shipDestroyedSignal;
        private readonly ExitSignal _exitSignal;
    }
}
