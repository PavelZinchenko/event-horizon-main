using System;
using System.Collections.Generic;
using Combat.Domain;
using CombatSimulation.Ai;
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
using Services.Localization;
using Services.Messenger;
using Services.Resources;
using UnityEngine;
using Zenject;
using CoreEntityId = CombatSimulation.Core.EntityId;
using CoreRosterId = CombatSimulation.Core.RosterId;
using LegacyViewRect = Combat.Scene.ViewRect;
using PlayerInputSignal = Combat.Ai.PlayerInputSignal;

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
            ILocalization localization,
            PlayerInputSignal playerInputSignal,
            UnityEngine.SceneManagement.Scene scene)
        {
            _combatModel = combatModel;
            _localization = localization;
            _playerInputSignal = playerInputSignal;
            _resourceLocator = resourceLocator;
            _combatMenu = combatMenu;
            _shipSelectionPanel = shipSelectionPanel;
            _exitTrigger = exitTrigger;

            _mapper = new CoreShipMapper(database, resourceLocator);
            var roster = new List<RosterSlotSetup>();
            var nextId = 1;
            foreach (var info in combatModel.PlayerFleet.Ships)
                AddSlot(new RosterId(nextId++), CombatSide.Player, info, roster);
            foreach (var info in combatModel.EnemyFleet.Ships)
                AddSlot(new RosterId(nextId++), CombatSide.Enemy, info, roster);
            LogInertDevices();

            var rules = combatModel.Rules;
            var size = rules.BattleMapSize;

            // Step 8: the core runs the legacy timer rules (TM1). The player side allows one ship; the enemy side
            // allows MaxEnemyShips, or more when the rules start with more (TM2).
            var timeLimitTicks = (long)rules.TimeLimit * TicksPerSecond;
            _maxActive[(int)CombatSide.Player] = 1;
            _maxActive[(int)CombatSide.Enemy] = Math.Max(rules.MaxEnemyShips, rules.InitialEnemyShips);
            var maxActiveBySide = new int[BattleSetup.SideCount];
            maxActiveBySide[PlayerSide] = _maxActive[(int)CombatSide.Player];
            maxActiveBySide[EnemySide] = _maxActive[(int)CombatSide.Enemy];

            // The seed is drawn here, outside the core, which never uses a random source of its own.
            var seed = (ulong)(uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            // Step 9: the core ends an OnlyOneShip battle after the first player death (SS2).
            var setup = new BattleSetup(new BattleId(1), seed, roster, size, size, timeLimitTicks,
                timerMode: ToTimerMode(rules.TimeOutMode), playerSide: PlayerSide, maxActiveShipsBySide: maxActiveBySide,
                onlyOneShip: rules.ShipSelection == PlayerShipSelectionMode.OnlyOneShip);
            var torus = new Torus(setup.Width, setup.Height);
            _trees = CorePilotProvider.CreateTrees(seed, torus, _mapper, OnBroadcast);
            _pilots = new CorePilotProvider(database.CombatSettings, _trees, _mapper.Mappings, PlayerSide, combatModel.EnemyFleet.AiLevel);
            _runner = new BattleRunner(setup, _pilots);

            var bulletSprite = resourceLocator.GetSprite(new SpriteId(BulletSpriteName, SpriteId.Type.Ammunition));
            // Only the font of the legacy text effect prefab is used; the text view is built in code (step 7f).
            var textFont = Resources.Load<TextMesh>(TextEffectPrefab)?.font;
            var flashSprite = resourceLocator.GetSprite(FlashSpriteName);
            _renderer = new BattleRenderer(torus, bulletSprite, flashSprite, textFont, scene);
            _bulletVisuals = new CoreBulletVisuals(_mapper.BulletSources, resourceLocator, flashSprite);
            _anchor = new Vector2(0.5f * setup.Width, 0.5f * setup.Height);
            // Step 10: the legacy planet, presentation only (D8).
            if (!rules.DisablePlanet)
                _planet = CorePlanet.Create(seed, torus, scene);

            messenger.AddListener(EventType.EscapeKeyPressed, OnEscapeKeyPressed);
            _playerInputSignal.Event += OnPlayerInput;
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

        // Legacy CombatTimer.State: shown while both sides have an active ship. Seconds left rounded up, at least 0;
        // 0 in CallNextEnemy while no next enemy can come. CallNextEnemyOrDraw without a ready enemy counts down
        // 3 x the limit, with the alarm in its last 15 s; DrainPlayerHp sounds the alarm from the limit on.
        public CombatTimerState Timer
        {
            get
            {
                if (!IsRunning || Count(CombatSide.Player, ShipStatus.Active) == 0 || Count(CombatSide.Enemy, ShipStatus.Active) == 0)
                    return new CombatTimerState(false, 0, false);

                var rules = _combatModel.Rules;
                if (rules.TimeLimit <= 0)
                    return new CombatTimerState(true, 0, false);

                var left = CeilSeconds((long)rules.TimeLimit * TicksPerSecond - _runner.Snapshot.TimerElapsedTicks);
                var alarm = false;
                switch (rules.TimeOutMode)
                {
                    case TimeOutMode.CallNextEnemy:
                        if (!CanCallNextEnemy)
                            left = 0;
                        break;
                    case TimeOutMode.CallNextEnemyOrDraw:
                        if (FirstReady(_enemySlots) == null)
                        {
                            left += 2 * rules.TimeLimit;
                            alarm = left > 0 && left < DrawAlarmSeconds;
                        }
                        break;
                    case TimeOutMode.DrainPlayerHp:
                        alarm = left <= 0;
                        break;
                }

                return new CombatTimerState(true, Math.Max(0, left), alarm);
            }
        }

        // Legacy CombatManager.CanChangeShip.
        public bool CanChangeShip =>
            IsRunning && _combatModel.Rules.ShipSelection == PlayerShipSelectionMode.Default && Count(CombatSide.Player, ShipStatus.Ready) > 0;

        // Legacy CombatManager.CanCallNextEnemy; the menu shows the button by this rule only (TM5).
        public bool CanCallReinforcement => IsRunning && CanCallNextEnemy;
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

        public void CallReinforcement()
        {
            if (CanCallReinforcement)
                TryDeploy(NextReady(_enemySlots));
        }

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
            _playerInput = false;
            Render();
            UpdateExit();
        }

        // Scene unload. Clears the battle without events: the roster outcome is already final or abandoned.
        public void Dispose()
        {
            _playerInputSignal.Event -= OnPlayerInput;
            _runner = null;
            _renderer.Dispose();
            _planet?.Dispose();
            _views.Clear();
            _viewsByCore.Clear();
            _ids.Clear();
            _pilots.Clear();
            _broadcasts.Clear();
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
            _planet?.Render(_anchor, Time.deltaTime);

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

        private void AddSlot(RosterId id, CombatSide side, IShipInfo info, List<RosterSlotSetup> roster)
        {
            var definition = _mapper.Map(info).Definition;
            var slot = new Slot(id, side, info, definition.Armor, this);
            (side == CombatSide.Player ? _playerSlots : _enemySlots).Add(slot);
            _slotsById.Add(id, slot);
            _slotsByCore.Add(ToCore(id), slot);
            roster.Add(new RosterSlotSetup(ToCore(id), ToCoreSide(side), definition));
        }

        // M1 check: the console lists the inert devices (D1) once per battle, by mapped ship or drone.
        private void LogInertDevices()
        {
            var report = new System.Text.StringBuilder();
            var count = 0;
            foreach (var mapping in _mapper.Mappings.Values)
            {
                for (var i = 0; i < mapping.Devices.Count; ++i)
                {
                    var device = mapping.Devices[i];
                    if (!device.IsInert) continue;
                    report.AppendLine($"  {mapping.Name}, device {i}: {device.Name}: {device.InertReason}");
                    ++count;
                }
            }

            if (count > 0)
                Debug.Log($"CoreCombatAdapter: {count} inert devices (phase 5 or step 4).\n{report}");
        }

        // One tick: deploy by the commander rules, step, then handle the tick's events before the next step.
        private void Step()
        {
            if (!IsRunning)
                return;

            RunEnemyCommander();
            RunPlayerCommander();

            // The pilots of this step think on the snapshot of the current tick.
            if (_playerInput)
                _trees.OnPlayerInput(_runner.Tick);

            _runner.Step();
            var snapshot = _runner.Snapshot;
            ApplyResults(snapshot);
            HandleEvents(snapshot);
            _renderer.Capture(snapshot);
            ShowBroadcasts(snapshot);
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
                            _renderer.AddBullet(item, _bulletVisuals.Get(item));
                        else if (item.IsDrone)
                            OnDroneSpawned(item);
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
                    case BattleEventKind.Hit:
                        _renderer.AddHit(item);
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
            var view = new CoreShipView(id, slot.Side, specification, state, new CoreShipControls(_mapper.Mappings[state.Definition]));
            _views.Add(id, view);
            _viewsByCore.Add(item.EntityId, view);

            var model = specification.Stats.ShipModel;
            _renderer.AddShip(item.EntityId, _resourceLocator.GetSprite(model.ModelImage), model.ModelScale,
                _mapper.Mappings[state.Definition].TintColor?.ToUnity());
        }

        private void OnDroneSpawned(in BattleEvent item)
        {
            var mapping = _mapper.Mappings[item.Definition];
            _renderer.AddShip(item.EntityId, _resourceLocator.GetSprite(mapping.ModelImage), mapping.ModelScale,
                mapping.TintColor?.ToUnity());
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

        // Step 12 (RR1, RR2): the campaign model reads the roster from ShipInfo. Written before the events, so an
        // experience checkpoint in SlotChanged reads the tick's enemy damage (B2). ShipInfo keeps it after the unload.
        private void ApplyResults(BattleSnapshot snapshot)
        {
            foreach (var state in snapshot.Roster)
                if (_slotsByCore.TryGetValue(state.RosterId, out var slot))
                    slot.Info.ApplyCoreState(ToStatus(state.Status), state.DamageFraction, state.DeploymentOrder);
        }

        // Copies what the views and slots need from the snapshot; spans are valid only until the next step.
        private void UpdateProjection(BattleSnapshot snapshot)
        {
            foreach (var ship in snapshot.Ships)
                if (_viewsByCore.TryGetValue(ship.Id, out var view))
                    view.Update(ship, snapshot.GetDevices(ship));

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

        // Legacy CombatManager: the first InitialEnemyShips ready enemies at once at the start (TM4), and the first
        // ready one 3 s after the last active enemy left. While an enemy is active, legacy CombatTimer calls the next
        // enemy when the time runs out; the enemy deployment resets the core timer.
        private void RunEnemyCommander()
        {
            if (Count(CombatSide.Enemy, ShipStatus.Active) > 0 || IsDeployPending(_enemySlots))
            {
                if (IsTimerExpired && CanCallNextEnemy)
                    TryDeploy(NextReady(_enemySlots));
                return;
            }

            if (_runner.Tick < _nextDeployTick[(int)CombatSide.Enemy])
                return;

            var count = _enemiesStarted ? 1 : Math.Max(1, _combatModel.Rules.InitialEnemyShips);
            _enemiesStarted = true;
            for (var i = 0; i < count; ++i)
            {
                var slot = NextReady(_enemySlots);
                if (slot == null)
                    break;

                TryDeploy(slot);
            }
        }

        // Legacy CombatManager.CheckIfCanCallNextEnemy. A submitted deployment counts as active, so one call
        // deploys one ship.
        private bool CanCallNextEnemy
        {
            get
            {
                var rules = _combatModel.Rules;
                if (rules.TimeOutMode != TimeOutMode.CallNextEnemy && rules.TimeOutMode != TimeOutMode.CallNextEnemyOrDraw)
                    return false;
                if (rules.TimeLimit <= 0)
                    return false;

                return CountActiveOrPending(_enemySlots) < rules.MaxEnemyShips && NextReady(_enemySlots) != null;
            }
        }

        // Legacy CombatTimer acts only while both sides have an active ship.
        private bool IsTimerExpired
        {
            get
            {
                var limit = (long)_combatModel.Rules.TimeLimit * TicksPerSecond;
                return limit > 0 && _runner.Snapshot.TimerElapsedTicks >= limit &&
                    Count(CombatSide.Player, ShipStatus.Active) > 0 && Count(CombatSide.Enemy, ShipStatus.Active) > 0;
            }
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
            // OnlyOneShip: after the first death the core rejects every player deployment and ends the battle 3 s later.
            if (_combatModel.Rules.ShipSelection == PlayerShipSelectionMode.OnlyOneShip && Count(CombatSide.Player, ShipStatus.Destroyed) > 0)
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
            if (!IsRunning || slot == null || slot.Status != ShipStatus.Ready || slot.PendingDeployTick != NoPendingDeploy)
                return;

            var slots = slot.Side == CombatSide.Player ? _playerSlots : _enemySlots;
            if (CountActiveOrPending(slots) >= _maxActive[(int)slot.Side])
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

        // The first ready slot without a submitted deployment.
        private static Slot NextReady(List<Slot> slots)
        {
            for (var i = 0; i < slots.Count; ++i)
                if (slots[i].Status == ShipStatus.Ready && slots[i].PendingDeployTick == NoPendingDeploy)
                    return slots[i];

            return null;
        }

        private static int CountActiveOrPending(List<Slot> slots)
        {
            var count = 0;
            for (var i = 0; i < slots.Count; ++i)
                if (slots[i].Status == ShipStatus.Active || slots[i].PendingDeployTick != NoPendingDeploy)
                    count++;

            return count;
        }

        // Legacy Mathf.CeilToInt of the seconds left.
        private static int CeilSeconds(long ticks) =>
            (int)(ticks > 0 ? (ticks + TicksPerSecond - 1) / TicksPerSecond : -(-ticks / TicksPerSecond));

        private static TimerMode ToTimerMode(TimeOutMode mode)
        {
            switch (mode)
            {
                case TimeOutMode.DrainPlayerHp: return TimerMode.DrainPlayerHp;
                case TimeOutMode.CallNextEnemyOrDraw: return TimerMode.CallNextEnemyOrDraw;
                default: return TimerMode.CallNextEnemy;
            }
        }

        private static bool IsDeployPending(List<Slot> slots)
        {
            for (var i = 0; i < slots.Count; ++i)
                if (slots[i].PendingDeployTick != NoPendingDeploy)
                    return true;

            return false;
        }

        // The battle freezes at the result: stepping stops, and the last frame and the death fades stay on screen.
        // Surrender ends the same way. A ShipLost result already waited the legacy 3 s in the core, so it exits at once (SS3).
        private void OnCompleted()
        {
            if (_exitDelay >= 0f)
                return;

            var result = _runner.Snapshot.Result;
            CheckOutcome(result);
            _exitDelay = result.Reason == BattleEndReason.ShipLost ? 0f : ExitDelaySeconds;
        }

        // Migration plan, "Completion order" (RR3): the core result and the campaign victory, which reads the roster,
        // must agree. A mismatch is only logged; neither is rewritten, and the battle exits as usual.
        private void CheckOutcome(BattleResult result)
        {
            var coreVictory = result.Outcome == BattleOutcome.Victory && result.WinningSide == PlayerSide;
            var victory = _combatModel.IsVictory();
            if (coreVictory == victory)
                return;

            var report = new System.Text.StringBuilder();
            foreach (var slot in _slotsById.Values)
                report.AppendLine($"  {slot.Side} slot {slot.Id.Value}: {slot.Info.Status}");

            Debug.LogError($"CoreCombatAdapter: the core result ({result}) does not match the campaign victory ({victory}).\n{report}");
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

        // Legacy PlayerInputSignal: the keyboard driver and the HUD fire it in every frame with player input (step 7f).
        // The flag holds until the end of the frame's steps.
        private void OnPlayerInput() => _playerInput = true;

        // ShowMessage (step 7e): called inside the step, while the pilots think. Local only (D6).
        private void OnBroadcast(CoreEntityId ship, string text, ColorData color) =>
            _broadcasts.Add(new Broadcast(ship, text, color));

        // Legacy RadioTransmitter: the localized text at the ship, drifting with it. Legacy NodeBuilder localizes at
        // build time; the text is the same.
        private void ShowBroadcasts(BattleSnapshot snapshot)
        {
            foreach (var item in _broadcasts)
                if (snapshot.TryGetShip(item.Ship, out var state))
                    _renderer.AddText(_localization.Localize(item.Text), item.Color.ToUnity(), state.Position, state.LinearVelocity);

            _broadcasts.Clear();
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

        private readonly struct Broadcast
        {
            public Broadcast(CoreEntityId ship, string text, ColorData color)
            {
                Ship = ship;
                Text = text;
                Color = color;
            }

            public readonly CoreEntityId Ship;
            public readonly string Text;
            public readonly ColorData Color;
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
        // Legacy CombatTimer: the alarm of the draw countdown.
        private const int DrawAlarmSeconds = 15;
        private const string BulletSpriteName = "ball";
        // Legacy Flash effect prefab image.
        private const string FlashSpriteName = "Textures/star";
        // Legacy ShowMessage effect (RadioTransmitter).
        private const string TextEffectPrefab = "Combat/Effects/MessageText";

        private BattleRunner _runner;
        private readonly BehaviorTreeFactory _trees;
        private readonly CorePilotProvider _pilots;
        private readonly List<Broadcast> _broadcasts = new List<Broadcast>();
        private bool _playerInput;
        private readonly CoreShipMapper _mapper;
        // The drawn controlled ship, or its last position: every other entity is drawn at its image nearest to it.
        private Vector2 _anchor;
        private float _accumulator;
        private int _deploymentCounter;
        private EntityId _retreatRequested = EntityId.None;
        // Seconds left until the exit after the result; negative while the battle runs.
        private float _exitDelay = -1f;
        private bool _exited;
        private readonly long[] _nextDeployTick = new long[2];
        // Active ship limits by CombatSide, as given to the core.
        private readonly int[] _maxActive = new int[2];
        private bool _enemiesStarted;

        private readonly List<Slot> _playerSlots = new List<Slot>();
        private readonly List<Slot> _enemySlots = new List<Slot>();
        private readonly Dictionary<RosterId, Slot> _slotsById = new Dictionary<RosterId, Slot>();
        private readonly Dictionary<CoreRosterId, Slot> _slotsByCore = new Dictionary<CoreRosterId, Slot>();
        private readonly Dictionary<EntityId, CoreShipView> _views = new Dictionary<EntityId, CoreShipView>();
        private readonly Dictionary<CoreEntityId, CoreShipView> _viewsByCore = new Dictionary<CoreEntityId, CoreShipView>();
        private readonly List<CoreEntityId> _removed = new List<CoreEntityId>();
        private readonly EntityIdMap _ids = new EntityIdMap();
        private readonly BattleRenderer _renderer;
        private readonly CoreBulletVisuals _bulletVisuals;
        private readonly CorePlanet _planet;
        private readonly LegacyViewRect _framing = new LegacyViewRect();

        private readonly ICombatModel _combatModel;
        private readonly IResourceLocator _resourceLocator;
        private readonly LazyInject<CombatMenu> _combatMenu;
        private readonly LazyInject<ShipSelectionPanel> _shipSelectionPanel;
        private readonly ExitSignal.Trigger _exitTrigger;
        private readonly ILocalization _localization;
        private readonly PlayerInputSignal _playerInputSignal;
    }
}
