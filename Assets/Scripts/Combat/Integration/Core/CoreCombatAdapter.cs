using System;
using System.Collections.Generic;
using Combat.Domain;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;
using CombatView;
using GameDatabase;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using GameStateMachine.States;
using Gui.Combat;
using Services.Messenger;
using Services.Resources;
using UnityEngine;
using Zenject;
using CoreEntityId = CombatSimulation.Core.EntityId;
using CoreRosterId = CombatSimulation.Core.RosterId;
using LegacyViewRect = Combat.Scene.ViewRect;

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
            IResourceLocator resourceLocator,
            LazyInject<CombatMenu> combatMenu,
            LazyInject<ShipSelectionPanel> shipSelectionPanel,
            ExitSignal.Trigger exitTrigger,
            UnityEngine.SceneManagement.Scene scene)
        {
            _combatModel = combatModel;
            _resourceLocator = resourceLocator;
            _combatMenu = combatMenu;
            _shipSelectionPanel = shipSelectionPanel;
            _exitTrigger = exitTrigger;

            var mapper = new CoreShipMapper(database);
            var roster = new List<RosterSlotSetup>();
            var nextId = 1;
            foreach (var info in combatModel.PlayerFleet.Ships)
                AddSlot(new RosterId(nextId++), CombatSide.Player, info, mapper, roster);
            foreach (var info in combatModel.EnemyFleet.Ships)
                AddSlot(new RosterId(nextId++), CombatSide.Enemy, info, mapper, roster);

            var size = combatModel.Rules.BattleMapSize;

            // No time limit: the core ends a battle in a draw at timeout, but quick combat (CallNextEnemy) never
            // ends on time. The TimeOutMode values and reinforcements come in phase 4 (owner, 2026-10-21).
            const long timeLimitTicks = 0;

            // The seed is drawn here, outside the core, which never uses a random source of its own.
            var seed = (ulong)(uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var setup = new BattleSetup(new BattleId(1), seed, roster, size, size, timeLimitTicks);
            var torus = new Torus(setup.Width, setup.Height);
            _pilots = new CorePilotProvider(torus, PlayerSide);
            _runner = new BattleRunner(setup, _pilots);

            var bulletSprite = resourceLocator.GetSprite(new SpriteId(BulletSpriteName, SpriteId.Type.Ammunition));
            _renderer = new BattleRenderer(torus, PlayerSide, bulletSprite, scene);
            _anchor = new Vector2(0.5f * setup.Width, 0.5f * setup.Height);

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

        // Shown while both sides have an active ship, so the pause button is there. No digits: legacy hides them
        // when it cannot call the next enemy, and reinforcements come in phase 4.
        public CombatTimerState Timer => new CombatTimerState(
            IsRunning && Count(CombatSide.Player, ShipStatus.Active) > 0 && Count(CombatSide.Enemy, ShipStatus.Active) > 0, 0, false);

        // Legacy CombatManager.CanChangeShip.
        public bool CanChangeShip =>
            IsRunning && _combatModel.Rules.ShipSelection == PlayerShipSelectionMode.Default && Count(CombatSide.Player, ShipStatus.Ready) > 0;

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

        // Camera framing by the legacy rule: the player ship and the target with margins, zoomed in on each new
        // player ship. Drawn positions are continuous across the wrap, so the camera never jumps at the map edge.
        public Rect CameraRect => _framing.Rect;
        public Vector2 ViewPoint => _framing.Center;

        #endregion

        #region IShipControlSource

        // Only player ships have controls. A redeployed ship has a new pilot and a new description.
        public bool TryGetControls(EntityId ship, out IShipControlDescription description, out IShipCommandSink commands)
        {
            description = null;
            commands = null;
            if (!IsRunning || !_ids.TryGetCore(ship, out var core) || !_viewsByCore.TryGetValue(core, out var view))
                return false;
            if (!_pilots.TryGetPlayerPilot(core, out var pilot))
                return false;

            description = view.Controls;
            commands = pilot;
            return true;
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
            // One request per ship: the snapshot shows the retreat only after the next step.
            if (!CanChangeShip || ship != ControlledShip || ship == _retreatRequested || !_ids.TryGetCore(ship, out var core))
                return;

            if (_runner.Retreat(core, _runner.Tick))
                _retreatRequested = ship;
        }

        public void CallReinforcement() { }

        public void Surrender()
        {
            if (!IsRunning)
                return;
            if (!_runner.Surrender(PlayerSide, _runner.Tick))
                return;

            // The menu pauses the game, so the frame accumulator does not advance. Surrender decides the battle
            // within its own tick, so one step outside the accumulator reaches the result.
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
            if (_runner == null)
                return;

            RunSteps();
            Render();
            UpdateExit();
        }

        // Scene unload. Clears the battle without events: the roster outcome is already final or abandoned.
        public void Dispose()
        {
            _runner = null;
            _renderer.Dispose();
            _views.Clear();
            _viewsByCore.Clear();
            _ids.Clear();
            _pilots.Clear();
        }

        private void RunSteps()
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

        // Also after the result, so the last frame stays on screen; only the stepping stops.
        private void Render()
        {
            var controlled = default(CoreEntityId);
            var player = ActiveView(ControlledShip);
            if (player != null)
                _ids.TryGetCore(player.Id, out controlled);

            var alpha = IsRunning ? Mathf.Clamp01(_accumulator / Battle.TickDuration) : 1f;
            _renderer.Render(alpha, controlled, ref _anchor, Time.deltaTime);

            foreach (var pair in _viewsByCore)
                if (_renderer.TryGetShipPose(pair.Key, out var position, out var degrees))
                    pair.Value.SetVisualPose(position, degrees);

            // Legacy Scene.UpdateEnemies: the player stays in the middle when more than one enemy is active.
            var target = ActiveView(TargetShip);
            _framing.Update(player?.VisualPosition, target?.VisualPosition, Count(CombatSide.Enemy, ShipStatus.Active) > 1);
        }

        private CoreShipView ActiveView(EntityId id) => _views.TryGetValue(id, out var view) && view.IsActive ? view : null;

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

            RunEnemyCommander();
            RunPlayerCommander();

            _runner.Step();
            var snapshot = _runner.Snapshot;
            HandleEvents(snapshot);
            _renderer.Capture(snapshot);
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
                        if (item.IsBullet)
                            _renderer.AddBullet(item);
                        else if (item.IsDrone)
                            _renderer.AddDrone(item.EntityId, item.Definition);
                        else
                            OnShipSpawned(snapshot, item);
                        break;
                    case BattleEventKind.Removal:
                        if (item.IsBullet)
                            _renderer.RemoveBullet(item.EntityId);
                        else if (item.IsDrone)
                            _renderer.RemoveShip(item.EntityId, item.RemovalReason);
                        else
                            OnShipRemoved(item);
                        break;
                    case BattleEventKind.RosterTransition:
                        OnRosterTransition(snapshot, item);
                        break;
                    case BattleEventKind.Rename:
                        _renderer.RenameBullet(item.PreviousId, item.EntityId);
                        break;
                }
            }
        }

        private void OnShipSpawned(BattleSnapshot snapshot, in BattleEvent item)
        {
            if (!_slotsByCore.TryGetValue(item.RosterId, out var slot) || !snapshot.TryGetShip(item.EntityId, out var state))
                return;

            var id = _ids.Assign(item.EntityId);
            var specification = slot.Info.Specification;
            var view = new CoreShipView(id, slot.Side, specification, state, new CoreShipControls(state.Definition));
            _views.Add(id, view);
            _viewsByCore.Add(item.EntityId, view);

            var model = specification.Stats.ShipModel;
            _renderer.AddShip(item.EntityId, _resourceLocator.GetSprite(model.ModelImage), model.ModelScale);
        }

        // The roster transition of the same removal comes later in the batch, so the ID stays mapped until the tick ends.
        private void OnShipRemoved(in BattleEvent item)
        {
            if (_viewsByCore.TryGetValue(item.EntityId, out var view))
            {
                view.Deactivate();
                if (view.Id == _retreatRequested)
                    _retreatRequested = EntityId.None;
            }

            _pilots.Release(item.EntityId);
            _renderer.RemoveShip(item.EntityId, item.RemovalReason);
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
                if (slot.Side == CombatSide.Player)
                    _framing.Zoom();

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

        // Legacy quick combat rule: deploy the first ready enemy at once at the start, and 3 s after the last one left.
        private void RunEnemyCommander()
        {
            if (_runner.Tick < _nextDeployTick[(int)CombatSide.Enemy])
                return;

            var slot = FirstReady(_enemySlots);
            if (slot != null)
                TryDeploy(slot);
        }

        // Legacy CombatManager.UpdateLocalGame: the player side waits for an active enemy. Then, at once at the start
        // and 3 s after the player ship left, the selection panel opens, or ByOrder deploys the first ready ship.
        // A closed panel opens again 3 s later, as the legacy cooldown restarts.
        private void RunPlayerCommander()
        {
            if (_runner.Tick < _nextDeployTick[(int)CombatSide.Player])
                return;
            if (Count(CombatSide.Enemy, ShipStatus.Active) == 0 || Count(CombatSide.Player, ShipStatus.Active) > 0)
                return;
            if (IsDeployPending(_enemySlots) || IsDeployPending(_playerSlots))
                return;

            var slot = FirstReady(_playerSlots);
            if (slot == null)
                return;

            if (!_combatModel.Rules.ShipSelection.CanChooseShip())
            {
                TryDeploy(slot);
                return;
            }

            _nextDeployTick[(int)CombatSide.Player] = _runner.Tick + DeployDelayTicks;
            var panel = _shipSelectionPanel.Value;
            if (panel)
                panel.Open();
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

        private static Slot FirstReady(List<Slot> slots)
        {
            for (var i = 0; i < slots.Count; ++i)
                if (slots[i].Status == ShipStatus.Ready)
                    return slots[i];

            return null;
        }

        private static bool IsDeployPending(List<Slot> slots)
        {
            for (var i = 0; i < slots.Count; ++i)
                if (slots[i].PendingDeployTick != NoPendingDeploy)
                    return true;

            return false;
        }

        // The battle freezes at the result: stepping stops, and the last frame and the death fades stay on screen.
        // Surrender ends the same way.
        private void OnCompleted()
        {
            if (_exitDelay < 0f)
                _exitDelay = ExitDelaySeconds;
        }

        // Real time, not game time: surrender comes from the menu, which pauses the game and stays open.
        private void UpdateExit()
        {
            if (_exitDelay < 0f || _exited)
                return;

            _exitDelay -= Time.unscaledDeltaTime;
            if (_exitDelay > 0f)
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
        private const float ExitDelaySeconds = 3f;
        private const string BulletSpriteName = "ball";

        private BattleRunner _runner;
        private readonly CorePilotProvider _pilots;
        // The drawn controlled ship, or its last position: every other entity is drawn at its image nearest to it.
        private Vector2 _anchor;
        private float _accumulator;
        private int _deploymentCounter;
        private EntityId _retreatRequested = EntityId.None;
        // Seconds left until the exit after the result; negative while the battle runs.
        private float _exitDelay = -1f;
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
        private readonly BattleRenderer _renderer;
        private readonly LegacyViewRect _framing = new LegacyViewRect();

        private readonly ICombatModel _combatModel;
        private readonly IResourceLocator _resourceLocator;
        private readonly LazyInject<CombatMenu> _combatMenu;
        private readonly LazyInject<ShipSelectionPanel> _shipSelectionPanel;
        private readonly ExitSignal.Trigger _exitTrigger;
    }
}
