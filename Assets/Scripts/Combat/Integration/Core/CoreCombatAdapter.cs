using System;
using System.Collections.Generic;
using Combat.Domain;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;
using GameDatabase;
using GameStateMachine.States;
using Gui.Combat;
using Services.Messenger;
using UnityEngine;
using Zenject;
using CoreEntityId = CombatSimulation.Core.EntityId;
using CoreRosterId = CombatSimulation.Core.RosterId;

namespace Combat.Integration.Core
{
    // Projects the shared combat core through the application contracts (migration phase 3c).
    // The BattleRunner is the authority: commands go to it, and the projection only copies its snapshots.
    // Steps the battle at the core's fixed rate and handles each tick's events right after its step, so no event is lost.
    public sealed class CoreCombatAdapter : IRosterView, IBattleView, IShipViews, IShipControlSource, ICombatCommands, ITickable, IDisposable
    {
        [Inject]
        public CoreCombatAdapter(
            ICombatModel combatModel,
            IDatabase database,
            IMessenger messenger,
            LazyInject<CombatMenu> combatMenu,
            ExitSignal.Trigger exitTrigger)
        {
            _combatModel = combatModel;
            _combatMenu = combatMenu;
            _exitTrigger = exitTrigger;

            var mapper = new CoreShipMapper(database);
            var roster = new List<RosterSlotSetup>();
            var nextId = 1;
            foreach (var info in combatModel.PlayerFleet.Ships)
                AddSlot(new RosterId(nextId++), CombatSide.Player, info, mapper, roster);
            foreach (var info in combatModel.EnemyFleet.Ships)
                AddSlot(new RosterId(nextId++), CombatSide.Enemy, info, mapper, roster);

            var size = combatModel.Rules.BattleMapSize;
            var timeLimitTicks = Math.Max(0, combatModel.Rules.TimeLimit) * TicksPerSecond;

            // The seed is drawn here, outside the core, which never uses a random source of its own.
            var seed = (ulong)(uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var setup = new BattleSetup(new BattleId(1), seed, roster, size, size, timeLimitTicks);
            _runner = new BattleRunner(setup, new ChasePilotProvider(new Torus(setup.Width, setup.Height)));

            messenger.AddListener(EventType.EscapeKeyPressed, OnEscapeKeyPressed);
        }

        #region IRosterView

        public IReadOnlyList<IRosterSlotView> PlayerSlots => _playerSlots;
        public IReadOnlyList<IRosterSlotView> EnemySlots => _enemySlots;

        public event Action<RosterSlotChange> SlotChanged;

        public bool TryGetSlot(RosterId id, out IRosterSlotView slot)
        {
            var found = _slotsById.TryGetValue(id, out var result);
            slot = result;
            return found;
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

        public EntityId ControlledShip => FindActiveEntity(_playerSlots);
        public EntityId TargetShip => FindActiveEntity(_enemySlots);

        // Timer and ship change come with the battle flow (step 6); reinforcements with phase 4.
        public CombatTimerState Timer => default;
        public bool CanChangeShip => false;
        public bool CanCallReinforcement => false;
        public bool KillAllAllowedByRules => _combatModel.Rules.KillThemAllButton;

        #endregion

        #region IShipViews

        public bool TryGetShip(EntityId id, out IShipView ship)
        {
            var found = _views.TryGetValue(id, out var view);
            ship = view;
            return found;
        }

        // The view of the ship the camera follows, or null.
        public CoreShipView ControlledShipView => _views.TryGetValue(ControlledShip, out var view) ? view : null;

        #endregion

        #region IShipControlSource

        // Player input comes with step 5.
        public bool TryGetControls(EntityId ship, out IShipControlDescription description, out IShipCommandSink commands)
        {
            description = null;
            commands = null;
            return false;
        }

        #endregion

        #region ICombatCommands

        public void Deploy(RosterId id)
        {
            if (!_slotsById.TryGetValue(id, out var slot) || slot.Side != CombatSide.Player)
                return;

            TryDeploy(slot);
        }

        public void Retreat(EntityId ship)
        {
            if (!IsRunning || ship != ControlledShip || !_ids.TryGetCore(ship, out var core))
                return;

            _runner.Retreat(core, _runner.Tick);
        }

        public void CallReinforcement() { }

        public void Surrender()
        {
            if (!IsRunning)
                return;
            if (!_runner.Surrender(PlayerSide, _runner.Tick))
                return;

            // The menu can pause the game, and the player expects to leave at once. Surrender decides the battle
            // within its own tick, so one step outside the frame accumulator reaches the result.
            Step();
        }

        public void KillAllEnemies()
        {
            if (!IsRunning)
                return;
#if !UNITY_EDITOR
            if (!KillAllAllowedByRules)
                return;
#endif
            _runner.KillAll(PlayerSide, _runner.Tick);
        }

        #endregion

        public void Tick()
        {
            if (!IsRunning)
                return;

            _accumulator += Time.deltaTime;
            var steps = 0;
            while (_accumulator >= Battle.TickDuration && IsRunning)
            {
                if (steps == MaxStepsPerFrame)
                {
                    // A long frame drops the rest instead of catching up later.
                    _accumulator = 0f;
                    break;
                }

                _accumulator -= Battle.TickDuration;
                Step();
                steps++;
            }
        }

        public void Dispose()
        {
            _runner = null;
            _views.Clear();
            _viewsByCore.Clear();
            _ids.Clear();
        }

        // False after the result (the core then throws on every step and command) and after disposal.
        private bool IsRunning => _runner != null && !_runner.IsCompleted;

        private void AddSlot(RosterId id, CombatSide side, IShipInfo info, CoreShipMapper mapper, List<RosterSlotSetup> roster)
        {
            var definition = mapper.Map(info);
            var slot = new Slot(id, side, info, definition.Armor, this);
            (side == CombatSide.Player ? _playerSlots : _enemySlots).Add(slot);
            _slotsById.Add(id, slot);
            _slotsByCore.Add(ToCore(id), slot);
            roster.Add(new RosterSlotSetup(ToCore(id), ToCoreSide(side), definition));
        }

        // One tick: deploy by the commander rules, step, then handle the tick's events before the next step.
        private void Step()
        {
            if (!IsRunning)
                return;

            RunCommander(CombatSide.Player);
            RunCommander(CombatSide.Enemy);

            _runner.Step();
            var snapshot = _runner.Snapshot;
            HandleEvents(snapshot);
            UpdateProjection(snapshot);

            if (snapshot.IsCompleted)
                OnCompleted();
        }

        private void HandleEvents(BattleSnapshot snapshot)
        {
            var events = snapshot.Events;
            for (var i = 0; i < events.Length; ++i)
            {
                var item = events[i];
                switch (item.Kind)
                {
                    case BattleEventKind.Spawn:
                        if (!item.IsBullet && !item.IsDrone)
                            OnShipSpawned(snapshot, item);
                        break;
                    case BattleEventKind.Removal:
                        if (!item.IsBullet && !item.IsDrone)
                            OnShipRemoved(item);
                        break;
                    case BattleEventKind.RosterTransition:
                        OnRosterTransition(snapshot, item);
                        break;
                }
            }
        }

        private void OnShipSpawned(BattleSnapshot snapshot, in BattleEvent item)
        {
            if (!_slotsByCore.TryGetValue(item.RosterId, out var slot) || !snapshot.TryGetShip(item.EntityId, out var state))
                return;

            var id = _ids.Assign(item.EntityId);
            var view = new CoreShipView(id, slot.Side, slot.Info.Specification, state);
            _views.Add(id, view);
            _viewsByCore.Add(item.EntityId, view);
        }

        // The roster transition of the same removal comes later in the batch, so the ID stays mapped until the tick ends.
        private void OnShipRemoved(in BattleEvent item)
        {
            if (_viewsByCore.TryGetValue(item.EntityId, out var view))
                view.Deactivate();

            _removed.Add(item.EntityId);
        }

        private void OnRosterTransition(BattleSnapshot snapshot, in BattleEvent item)
        {
            if (!_slotsByCore.TryGetValue(item.RosterId, out var slot))
                return;

            var previous = slot.Status;
            slot.Status = ToStatus(item.Status);

            EntityId entity = EntityId.None;
            if (!item.EntityId.IsNull)
                _ids.TryGet(item.EntityId, out entity);

            if (item.TransitionReason == RosterTransitionReason.Deployed)
            {
                slot.DeploymentOrder = ++_deploymentCounter;
                slot.ActiveEntity = entity;
                slot.PendingDeployTick = NoPendingDeploy;
            }
            else
            {
                slot.ActiveEntity = EntityId.None;
                if (previous == ShipStatus.Active)
                    _nextDeployTick[(int)slot.Side] = snapshot.Tick + 1 + DeployDelayTicks;
            }

            if (slot.Status != previous)
                SlotChanged?.Invoke(new RosterSlotChange(slot, previous, ToReason(item.TransitionReason), entity));
        }

        // Copies what the views and slots need from the snapshot; spans are valid only until the next step.
        private void UpdateProjection(BattleSnapshot snapshot)
        {
            foreach (var ship in snapshot.Ships)
                if (_viewsByCore.TryGetValue(ship.Id, out var view))
                    view.Update(ship);

            foreach (var state in snapshot.Roster)
            {
                if (!_slotsByCore.TryGetValue(state.RosterId, out var slot))
                    continue;

                slot.ArmorDeficit = state.ArmorDeficit;

                // A deployment that did not happen by its tick was rejected.
                if (slot.PendingDeployTick != NoPendingDeploy && snapshot.Tick >= slot.PendingDeployTick)
                    slot.PendingDeployTick = NoPendingDeploy;
            }

            foreach (var core in _removed)
            {
                if (_viewsByCore.TryGetValue(core, out var view))
                {
                    _viewsByCore.Remove(core);
                    _views.Remove(view.Id);
                }

                _ids.Release(core);
            }

            _removed.Clear();
        }

        // Temporary for both sides (legacy quick combat rule for the enemy): deploy the first ready ship at once at the
        // start, and 3 s after the side's last ship left. Step 6 gives the player side the ship selection panel.
        private void RunCommander(CombatSide side)
        {
            if (_runner.Tick < _nextDeployTick[(int)side])
                return;

            var slots = side == CombatSide.Player ? _playerSlots : _enemySlots;
            for (var i = 0; i < slots.Count; ++i)
            {
                if (slots[i].Status != ShipStatus.Ready)
                    continue;

                TryDeploy(slots[i]);
                return;
            }
        }

        private void TryDeploy(Slot slot)
        {
            if (!IsRunning || slot.Status != ShipStatus.Ready)
                return;

            var slots = slot.Side == CombatSide.Player ? _playerSlots : _enemySlots;
            for (var i = 0; i < slots.Count; ++i)
                if (slots[i].Status == ShipStatus.Active || slots[i].PendingDeployTick != NoPendingDeploy)
                    return;

            var tick = _runner.Tick;
            if (_runner.Deploy(ToCore(slot.Id), tick))
                slot.PendingDeployTick = tick;
        }

        // Temporary: leaves at once. Step 7 adds the freeze and the 3 s wait.
        private void OnCompleted()
        {
            if (_exited)
                return;

            _exited = true;
            _exitTrigger.Fire();
        }

        private void OnEscapeKeyPressed()
        {
            // Lazy: the menu injects this adapter.
            var menu = _combatMenu.Value;
            if (menu)
                menu.Open();
        }

        private static EntityId FindActiveEntity(List<Slot> slots)
        {
            for (var i = 0; i < slots.Count; ++i)
                if (slots[i].Status == ShipStatus.Active)
                    return slots[i].ActiveEntity;

            return EntityId.None;
        }

        private static CoreRosterId ToCore(RosterId id) => new CoreRosterId((uint)id.Value);
        private static int ToCoreSide(CombatSide side) => side == CombatSide.Player ? PlayerSide : EnemySide;

        private static ShipStatus ToStatus(RosterStatus status)
        {
            switch (status)
            {
                case RosterStatus.Active: return ShipStatus.Active;
                case RosterStatus.Destroyed: return ShipStatus.Destroyed;
                default: return ShipStatus.Ready;
            }
        }

        private static RosterChangeReason ToReason(RosterTransitionReason reason)
        {
            switch (reason)
            {
                case RosterTransitionReason.Deployed: return RosterChangeReason.Deployed;
                case RosterTransitionReason.Retreated: return RosterChangeReason.Retreated;
                case RosterTransitionReason.Killed: return RosterChangeReason.Killed;
                default: return RosterChangeReason.ForcedDestruction;
            }
        }

        private sealed class Slot : IRosterSlotView
        {
            public Slot(RosterId id, CombatSide side, IShipInfo info, float maxArmor, CoreCombatAdapter owner)
            {
                Id = id;
                Side = side;
                Info = info;
                _maxArmor = maxArmor;
                _owner = owner;
            }

            public RosterId Id { get; }
            public CombatSide Side { get; }
            public IShipInfo Info { get; }

            public Constructor.Ships.IShip ShipData => Info.ShipData;
            public ShipStatus Status { get; set; }
            public int DeploymentOrder { get; set; }
            public EntityId ActiveEntity { get; set; }

            public float Condition
            {
                get
                {
                    if (Status == ShipStatus.Destroyed)
                        return 0f;
                    if (Status == ShipStatus.Active && _owner._views.TryGetValue(ActiveEntity, out var view))
                        return view.Armor.Fraction;

                    return Mathf.Clamp01(1f - ArmorDeficit / _maxArmor);
                }
            }

            public float ArmorDeficit { get; set; }

            // The tick of a submitted deployment; NoPendingDeploy when none is waiting.
            public long PendingDeployTick { get; set; } = NoPendingDeploy;

            private readonly float _maxArmor;
            private readonly CoreCombatAdapter _owner;
        }

        private const int PlayerSide = 0;
        private const int EnemySide = 1;
        private const int TicksPerSecond = 50;
        private const int MaxStepsPerFrame = 5;
        private const long DeployDelayTicks = 3 * TicksPerSecond;
        private const long NoPendingDeploy = -1;

        private BattleRunner _runner;
        private float _accumulator;
        private int _deploymentCounter;
        private bool _exited;
        private readonly long[] _nextDeployTick = new long[2];

        private readonly List<Slot> _playerSlots = new List<Slot>();
        private readonly List<Slot> _enemySlots = new List<Slot>();
        private readonly Dictionary<RosterId, Slot> _slotsById = new Dictionary<RosterId, Slot>();
        private readonly Dictionary<CoreRosterId, Slot> _slotsByCore = new Dictionary<CoreRosterId, Slot>();
        private readonly Dictionary<EntityId, CoreShipView> _views = new Dictionary<EntityId, CoreShipView>();
        private readonly Dictionary<CoreEntityId, CoreShipView> _viewsByCore = new Dictionary<CoreEntityId, CoreShipView>();
        private readonly List<CoreEntityId> _removed = new List<CoreEntityId>();
        private readonly EntityIdMap _ids = new EntityIdMap();

        private readonly ICombatModel _combatModel;
        private readonly LazyInject<CombatMenu> _combatMenu;
        private readonly ExitSignal.Trigger _exitTrigger;
    }
}
