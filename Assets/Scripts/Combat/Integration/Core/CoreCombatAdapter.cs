using System;
using System.Collections.Generic;
using Combat.Domain;
using CombatSimulation.Ai;
using CombatSimulation.Core;
using CombatSimulation.Modes;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;
using CombatView;
using GameDatabase;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameStateMachine.States;
using Gui.Combat;
using Services.Audio;
using Services.Localization;
using Services.Messenger;
using Services.Resources;
using Services.Settings;
using UnityEngine;
using Zenject;
using CoreUnitId = CombatSimulation.Core.UnitId;
using PlayerInputSignal = Combat.Ai.PlayerInputSignal;

namespace Combat.Integration.Core
{
    // Projects the shared combat core's roster battle through the application contracts (migration phase 3c), on the
    // mode-neutral CoreBattleHost (exploration plan, E1): the roster slots, the commanders, the timer and the results.
    // Online duel (phase 7, step 6): with a CoreDuelBattle bound, commands go to the session thread's queue. The local
    // player's ships are CombatSide.Player; the peer deploys, retreats and surrenders its own. No enemy commander,
    // kill-all, reinforcement or asteroids; ship selection has a 30 s limit, and a player has 3 retreats (owner, 2026-11-14).
    public sealed class CoreCombatAdapter : CoreBattleHost, IRosterView, IBattleView, ICombatCommands
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
            ISoundPlayer soundPlayer,
            IGameSettings gameSettings,
            UnityEngine.SceneManagement.Scene scene,
            [InjectOptional] CoreDuelBattle duel)
            : base(database, resourceLocator, exitTrigger, localization, playerInputSignal, soundPlayer, scene)
        {
            _combatModel = combatModel;
            _combatMenu = combatMenu;
            _shipSelectionPanel = shipSelectionPanel;

            var rules = combatModel.Rules;
            var roster = new List<UnitSetup>();
            CoreShipMapper mapper;
            if (duel != null)
            {
                mapper = duel.Mapper;
                foreach (var slot in duel.Slots)
                    AddSlot(new RosterId((int)slot.RosterId.Value), slot.Side, slot.Info, slot.Mapping.Definition);
            }
            else
            {
                mapper = new CoreShipMapper(database, resourceLocator);
                var nextId = 1;
                foreach (var info in combatModel.PlayerFleet.Ships)
                    roster.Add(AddSlot(new RosterId(nextId++), CombatSide.Player, info, mapper.Map(info).Definition));
                foreach (var info in combatModel.EnemyFleet.Ships)
                    roster.Add(AddSlot(new RosterId(nextId++), CombatSide.Enemy, info, mapper.Map(info).Definition));
            }
            LogInertDevices(mapper);

            var size = rules.BattleMapSize;

            // Step 8: the core runs the legacy timer rules (TM1). The player side allows one ship; the enemy side
            // allows MaxEnemyShips, or more when the rules start with more (TM2).
            var timeLimitTicks = (long)rules.TimeLimit * TicksPerSecond;
            _maxActive[(int)CombatSide.Player] = 1;
            // The duel setup allows one active ship per side, whatever the rules say.
            _maxActive[(int)CombatSide.Enemy] = duel != null ? 1 : Math.Max(rules.MaxEnemyShips, rules.InitialEnemyShips);
            var maxActiveBySide = new int[BattleSetup.SideCount];
            maxActiveBySide[PlayerSide] = _maxActive[(int)CombatSide.Player];
            maxActiveBySide[EnemySide] = _maxActive[(int)CombatSide.Enemy];

            // The seed is drawn here, outside the core, which never uses a random source of its own. A duel uses the
            // match seed, so both players see the same planet.
            var seed = duel?.Setup.MatchSeed ?? (ulong)(uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            var torus = new Torus(size, size);
            // Phase 5, step 14d: the legacy asteroids, drawn from the seed. Every owner here is local; the core rejects
            // asteroids in a battle with a remote owner (owner question 2 (a)), so a duel has none.
            var asteroids = rules.DisableAsteroids || duel != null ? null : CoreAsteroids.Layout(seed, torus,
                Maths.Experience.LevelToPowerMultiplier(database.GalaxySettings.EnemyLevel(rules.StarLevel)));
            CorePilotProvider pilots;
            BattleRunner runner = null;
            BehaviorTreeFactory trees = null;
            if (duel != null)
            {
                pilots = duel.Pilots;
            }
            else
            {
                // Step 9: RosterMode ends an OnlyOneShip battle after the first player death (SS2).
                var battleRules = new RosterRules(timeLimitTicks, timerMode: ToTimerMode(rules.TimeOutMode),
                    playerSide: PlayerSide, maxActiveShipsBySide: maxActiveBySide,
                    onlyOneShip: rules.ShipSelection == PlayerShipSelectionMode.OnlyOneShip);
                // Phase 2b (D12): a local battle keeps stepping after its result, through the exit delay, as legacy
                // CombatManager keeps the scene running until Exit. The online duel stops at its result.
                var setup = new BattleSetup(new BattleId(1), seed, roster, size, size, battleRules, asteroids: asteroids,
                    continueAfterResult: true);
                trees = CorePilotProvider.CreateTrees(seed, torus, mapper, OnBroadcast);
                pilots = new CorePilotProvider(database.CombatSettings, trees, mapper.Mappings, PlayerSide, combatModel.EnemyFleet.AiLevel);
                runner = new BattleRunner(setup, pilots);
            }

            // Phase 5b, owner question 6: legacy combat sets only the damage text; shadows and static wrecks stay off.
            // Step 4: the power saving mode draws no engine trails (legacy TrailRendererPool).
            var viewSettings = new CombatViewSettings(damageText: gameSettings.ShowDamage, shadows: false, staticWrecks: false,
                trails: gameSettings.QualityMode >= 0);
            Initialize(mapper, pilots, runner, trees, duel, seed, torus, asteroids, !rules.DisablePlanet, viewSettings);

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

        // Legacy CombatTimer.State: shown while both sides have an active ship. Seconds left rounded up, at least 0;
        // 0 in CallNextEnemy while no next enemy can come. CallNextEnemyOrDraw without a ready enemy counts down
        // 3 x the limit, with the alarm in its last 15 s; DrainPlayerHp sounds the alarm from the limit on.
        public CombatTimerState Timer
        {
            get
            {
                // A duel shows the ship selection limit while the local player has no ship.
                if (IsRunning && _selectionDeadline != NoDeadline && Count(CombatSide.Player, ShipStatus.Active) == 0)
                {
                    var selection = CeilSeconds(_selectionDeadline - CommandTick);
                    return new CombatTimerState(true, Math.Max(0, selection), selection <= SelectionAlarmSeconds);
                }

                if (!IsRunning || Count(CombatSide.Player, ShipStatus.Active) == 0 || Count(CombatSide.Enemy, ShipStatus.Active) == 0)
                    return new CombatTimerState(false, 0, false);

                var rules = _combatModel.Rules;
                if (rules.TimeLimit <= 0)
                    return new CombatTimerState(true, 0, false);

                var left = CeilSeconds((long)rules.TimeLimit * TicksPerSecond - TimerElapsedTicks);
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

        // Legacy CombatManager.CanChangeShip. A duel allows DuelMaxRetreats (owner, 2026-11-14).
        public bool CanChangeShip =>
            IsRunning && _combatModel.Rules.ShipSelection == PlayerShipSelectionMode.Default && Count(CombatSide.Player, ShipStatus.Ready) > 0 &&
            (Duel == null || _retreats < DuelMaxRetreats);

        // Legacy CombatManager.CanCallNextEnemy; the menu shows the button by this rule only (TM5).
        public bool CanCallReinforcement => IsRunning && CanCallNextEnemy;
        public bool KillAllAllowedByRules => Duel == null && _combatModel.Rules.KillThemAllButton;

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
            if (!CanChangeShip || ship != ControlledShip || ship == _retreatRequested || !TryGetCore(ship, out var core))
                return;

            if (Duel != null)
            {
                Duel.Thread.Post(SessionCommand.Retreat(core));
                _retreatRequested = ship;
            }
            else if (Runner.Retreat(core, Runner.Tick))
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
            // The duel's result comes back in a later snapshot, once the peer has the surrender.
            if (Duel != null)
            {
                Duel.Thread.Post(SessionCommand.Surrender(LocalSide));
                _surrendered = true;
                return;
            }
            if (!Runner.Surrender(PlayerSide, Runner.Tick))
                return;

            // Legacy CombatManager.Surrender exits at once, without the 3 s after the result.
            _surrendered = true;
            // The menu pauses the game, so the frame accumulator does not advance. Surrender decides the battle
            // within its own tick, so one step outside the accumulator reaches the result.
            Step();
        }

        public void KillAllEnemies()
        {
            if (!IsRunning || Duel != null)
                return;
#if !UNITY_EDITOR
            if (!KillAllAllowedByRules)
                return;
#endif
            Runner.KillAll(PlayerSide, Runner.Tick);
        }

        #endregion

        #region CoreBattleHost

        protected override EntityId PlayerShip => ControlledShip;
        protected override EntityId CameraTarget => TargetShip;

        // Legacy Scene.UpdateEnemies: the player stays in the middle when more than one enemy is active.
        protected override bool CameraCentersPlayer => Count(CombatSide.Enemy, ShipStatus.Active) > 1;

        // The commanders deploy by the roster rules.
        protected override void BeforeStep()
        {
            RunEnemyCommander();
            RunPlayerCommander();
        }

        // The peer deploys its own ships.
        protected override void OnDuelFrame() => RunPlayerCommander();

        protected override void BeginSnapshot(BattleSnapshot snapshot)
        {
            _timerElapsedTicks = snapshot.TimerElapsedTicks;
            ApplyResults(snapshot);
        }

        protected override bool TryDescribeShip(CoreUnitId unit, out CombatSide side,
            out Constructor.IShipSpecification specification)
        {
            if (!_slotsByCore.TryGetValue(unit, out var slot))
            {
                side = default;
                specification = null;
                return false;
            }

            side = slot.Side;
            specification = slot.Info.CoreSpecification;
            return true;
        }

        protected override void OnShipRemoved(EntityId ship)
        {
            if (ship == _retreatRequested)
                _retreatRequested = EntityId.None;
        }

        protected override void OnUnitTransition(BattleSnapshot snapshot, in BattleEvent item)
        {
            if (!_slotsByCore.TryGetValue(item.UnitId, out var slot))
                return;

            var previous = slot.Status;
            slot.Status = ToStatus(item.Status);

            EntityId entity = EntityId.None;
            if (!item.EntityId.IsNull)
                TryGetEntity(item.EntityId, out entity);

            if (item.TransitionReason == UnitTransitionReason.Deployed)
            {
                if (slot.Side == CombatSide.Player)
                {
                    ZoomCamera();
                    _selectionDeadline = NoDeadline;
                }

                slot.DeploymentOrder = ++_deploymentCounter;
                slot.ActiveEntity = entity;
                slot.PendingDeployTick = NoPendingDeploy;
            }
            else
            {
                if (Duel != null && slot.Side == CombatSide.Player && item.TransitionReason == UnitTransitionReason.Retreated)
                    _retreats++;
                slot.ActiveEntity = EntityId.None;
                if (previous == ShipStatus.Active)
                    _nextDeployTick[(int)slot.Side] = snapshot.Tick + 1 + DeployDelayTicks;
            }

            if (slot.Status != previous)
                SlotChanged?.Invoke(new RosterSlotChange(slot, previous, ToReason(item.TransitionReason), entity));
        }

        // Copies what the slots need from the snapshot.
        protected override void EndSnapshot(BattleSnapshot snapshot)
        {
            foreach (var state in snapshot.Units)
            {
                if (!_slotsByCore.TryGetValue(state.UnitId, out var slot))
                    continue;

                slot.ArmorDeficit = state.ArmorDeficit;

                // A deployment that did not happen by its tick was rejected.
                if (slot.PendingDeployTick != NoPendingDeploy && snapshot.Tick >= slot.PendingDeployTick)
                    slot.PendingDeployTick = NoPendingDeploy;
            }
        }

        // Legacy CombatManager exits 3 s after the last death; a local battle keeps stepping through them (phase 2b). A duel
        // freezes at the result, and the last frame and the death fades stay on screen. A surrender exits at once, as
        // legacy. A ShipLost result already waited the legacy 3 s in the core, so it exits at once too (SS3).
        protected override void OnCompleted(BattleResult result)
        {
            if (IsExitScheduled)
                return;

            CheckOutcome(result);
            // Phase 7, step 6 check: both players log the same result; the sides are core sides.
            if (Duel != null)
            {
                Duel.Result = result;
                Debug.Log($"CoreCombatAdapter: duel result {result}, local side {LocalSide}, tick {LastTick}");
            }
            ScheduleExit(_surrendered || result.Reason == BattleEndReason.ShipLost ? 0f : ExitDelaySeconds);
        }

        #endregion

        private long TimerElapsedTicks => Runner?.Snapshot.TimerElapsedTicks ?? _timerElapsedTicks;

        private UnitSetup AddSlot(RosterId id, CombatSide side, IShipInfo info, ShipDefinition definition)
        {
            var slot = new Slot(id, side, info, definition.Armor, this);
            (side == CombatSide.Player ? _playerSlots : _enemySlots).Add(slot);
            _slotsById.Add(id, slot);
            _slotsByCore.Add(ToCore(id), slot);
            return new UnitSetup(ToCore(id), ToCoreSide(side), definition);
        }

        // Step 12 (RR1, RR2): the campaign model reads the roster from ShipInfo. Written before the events, so an
        // experience checkpoint in SlotChanged reads the tick's enemy damage (B2). ShipInfo keeps it after the unload.
        private void ApplyResults(BattleSnapshot snapshot)
        {
            foreach (var state in snapshot.Units)
                if (_slotsByCore.TryGetValue(state.UnitId, out var slot))
                    slot.Info.ApplyCoreState(ToStatus(state.Status), state.DamageFraction, state.DeploymentOrder);
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

            if (Runner.Tick < _nextDeployTick[(int)CombatSide.Enemy])
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
                // The peer deploys its own ships.
                if (Duel != null)
                    return false;
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
                return limit > 0 && TimerElapsedTicks >= limit &&
                    Count(CombatSide.Player, ShipStatus.Active) > 0 && Count(CombatSide.Enemy, ShipStatus.Active) > 0;
            }
        }

        // Legacy CombatManager.UpdateLocalGame: the player side waits for an active enemy. Then, at once at the start
        // and 3 s after the player ship left, the selection panel opens, or ByOrder deploys the first ready ship.
        // A closed panel opens again 3 s later, as the legacy cooldown restarts.
        // Duel (owner, 2026-11-14): the player does not wait for the peer's ship, and the first ready ship deploys when no
        // ship is selected SelectionLimitTicks after the panel first opened.
        private void RunPlayerCommander()
        {
            if (Count(CombatSide.Player, ShipStatus.Active) > 0)
                return;
            if (Duel == null && Count(CombatSide.Enemy, ShipStatus.Active) == 0)
                return;
            if (IsDeployPending(_enemySlots) || IsDeployPending(_playerSlots))
                return;
            // OnlyOneShip: after the first death the core rejects every player deployment and ends the battle 3 s later.
            if (_combatModel.Rules.ShipSelection == PlayerShipSelectionMode.OnlyOneShip && Count(CombatSide.Player, ShipStatus.Destroyed) > 0)
                return;

            var slot = FirstReady(_playerSlots);
            if (slot == null)
                return;

            if (_selectionDeadline != NoDeadline && CommandTick >= _selectionDeadline)
            {
                TryDeploy(slot);
                return;
            }

            if (CommandTick < _nextDeployTick[(int)CombatSide.Player])
                return;

            if (!_combatModel.Rules.ShipSelection.CanChooseShip())
            {
                TryDeploy(slot);
                return;
            }

            _nextDeployTick[(int)CombatSide.Player] = CommandTick + DeployDelayTicks;
            if (Duel != null && _selectionDeadline == NoDeadline)
                _selectionDeadline = CommandTick + SelectionLimitTicks;
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

            // The session thread submits a duel deployment at its own tick, which this thread does not know: the request
            // counts as pending for DuelPendingTicks.
            if (Duel != null)
            {
                Duel.Thread.Post(SessionCommand.Deploy(ToCore(slot.Id)));
                slot.PendingDeployTick = CommandTick + DuelPendingTicks;
                return;
            }

            var tick = Runner.Tick;
            if (Runner.Deploy(ToCore(slot.Id), tick))
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

        internal static TimerMode ToTimerMode(TimeOutMode mode)
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

        // Migration plan, "Completion order" (RR3): the core result and the campaign victory, which reads the roster,
        // must agree. A mismatch is only logged; neither is rewritten, and the battle exits as usual.
        private void CheckOutcome(BattleResult result)
        {
            var coreVictory = result.Outcome == BattleOutcome.Victory && result.WinningSide == LocalSide;
            var victory = _combatModel.IsVictory();
            if (coreVictory == victory)
                return;

            var report = new System.Text.StringBuilder();
            foreach (var slot in _slotsById.Values)
                report.AppendLine($"  {slot.Side} slot {slot.Id.Value}: {slot.Info.Status}");

            Debug.LogError($"CoreCombatAdapter: the core result ({result}) does not match the campaign victory ({victory}).\n{report}");
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

        private static CoreUnitId ToCore(RosterId id) => new CoreUnitId((uint)id.Value);
        private static int ToCoreSide(CombatSide side) => side == CombatSide.Player ? PlayerSide : EnemySide;

        private static ShipStatus ToStatus(UnitStatus status)
        {
            switch (status)
            {
                case UnitStatus.Active: return ShipStatus.Active;
                case UnitStatus.Destroyed: return ShipStatus.Destroyed;
                default: return ShipStatus.Ready;
            }
        }

        private static RosterChangeReason ToReason(UnitTransitionReason reason)
        {
            switch (reason)
            {
                case UnitTransitionReason.Deployed: return RosterChangeReason.Deployed;
                case UnitTransitionReason.Retreated: return RosterChangeReason.Retreated;
                case UnitTransitionReason.Killed: return RosterChangeReason.Killed;
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
                    if (Status == ShipStatus.Active && _owner.TryGetShip(ActiveEntity, out var view))
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

        private const long DeployDelayTicks = 3 * TicksPerSecond;
        private const long NoPendingDeploy = -1;
        private const long NoDeadline = -1;
        // Duel: the ship selection limit (owner, 2026-11-14) with its alarm, and how long a posted deployment counts as pending.
        private const long SelectionLimitTicks = 30 * TicksPerSecond;
        private const int SelectionAlarmSeconds = 10;
        private const long DuelPendingTicks = TicksPerSecond;
        private const int DuelMaxRetreats = 3;
        // Legacy CombatTimer: the alarm of the draw countdown.
        private const int DrawAlarmSeconds = 15;

        // The last handled snapshot's timer, in both modes.
        private long _timerElapsedTicks;
        // Duel: the tick at which the first ready ship deploys while no ship is selected.
        private long _selectionDeadline = NoDeadline;
        // Duel: the local player's completed retreats.
        private int _retreats;
        private int _deploymentCounter;
        private EntityId _retreatRequested = EntityId.None;
        private bool _surrendered;
        private readonly long[] _nextDeployTick = new long[2];
        // Active ship limits by CombatSide, as given to the core.
        private readonly int[] _maxActive = new int[2];
        private bool _enemiesStarted;

        private readonly List<Slot> _playerSlots = new List<Slot>();
        private readonly List<Slot> _enemySlots = new List<Slot>();
        private readonly Dictionary<RosterId, Slot> _slotsById = new Dictionary<RosterId, Slot>();
        private readonly Dictionary<CoreUnitId, Slot> _slotsByCore = new Dictionary<CoreUnitId, Slot>();

        private readonly ICombatModel _combatModel;
        private readonly LazyInject<CombatMenu> _combatMenu;
        private readonly LazyInject<ShipSelectionPanel> _shipSelectionPanel;
    }
}
