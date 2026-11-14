using System;
using System.Collections.Generic;
using System.Threading;
using Combat.Component.Unit.Classification;
using Combat.Domain;
using CombatSimulation.Ai;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;
using GameDatabase.Model;
using OnlineDuel;
using CoreEntityId = CombatSimulation.Core.EntityId;

namespace Combat.Integration.Core
{
    // The running online duel that the battle scene binds (phase 7, step 6; Q2). Created on the main thread when the
    // handshake is ready: it creates the session and starts its thread at once, so the network keeps running while the
    // scene loads; the snapshots wait in the thread's queue (owner decision 1, up to 30 s). It owns the session thread and
    // the transport; Dispose stops the thread, then closes the transport (Relay leave, network decision 10).
    // The local player's ships are CombatSide.Player, the peer's CombatSide.Enemy. The session runs pilots only for the
    // local entities: player pilots for the local ships, trees for their drones.
    public sealed class CoreDuelBattle : IDisposable
    {
        public const int MaxQueuedSnapshots = 1500;

        public CoreDuelBattle(MatchHandshake handshake, CoreDuelSetup duelSetup, RelayTransport transport, Func<double> clock)
        {
            if (handshake.State != HandshakeState.Ready) throw new InvalidOperationException("The handshake is not ready.");
            Setup = handshake.Setup;
            LocalSide = handshake.Plan.LocalSide;
            Mapper = duelSetup.Mapper;
            Now = clock;
            _transport = transport;

            var slots = new List<Slot>();
            var player = new List<IShipInfo>();
            var enemy = new List<IShipInfo>();
            foreach (var item in duelSetup.Slots)
            {
                var local = item.Side == LocalSide;
                var info = new ShipInfo(item.Ship, item.Specification, local ? UnitSide.Player : UnitSide.Enemy);
                (local ? player : enemy).Add(info);
                slots.Add(new Slot(item.RosterId, local ? CombatSide.Player : CombatSide.Enemy, info, item.Mapping));
            }

            Slots = slots;
            Model = new DuelCombatModel(duelSetup.CreateRules(), player, enemy);

            _trees = CorePilotProvider.CreateTrees(Setup.MatchSeed, new Torus(Setup.Width, Setup.Height), Mapper, OnBroadcast);
            // No enemy pilots: the session runs none for remote ships.
            Pilots = new CorePilotProvider(duelSetup.Database.CombatSettings, _trees, Mapper.Mappings, LocalSide, -1);
            var session = handshake.CreateSession(Pilots, clock());
            Thread = new SessionThread(session, transport, clock, MaxQueuedSnapshots, beforeStep: BeforeStep);
            Thread.Start();
        }

        public CombatSimulation.Core.BattleSetup Setup { get; }
        public int LocalSide { get; }
        public CoreShipMapper Mapper { get; }
        public CorePilotProvider Pilots { get; }
        public SessionThread Thread { get; }
        public DuelCombatModel Model { get; }
        public IReadOnlyList<Slot> Slots { get; }
        public Func<double> Now { get; }

        // Legacy PlayerInputSignal: the autopilot tree holds back at the next tick.
        public void MarkPlayerInput() => Interlocked.Exchange(ref _playerInput, 1);

        // ShowMessage texts of the ticks up to the given one, in order.
        public void TakeBroadcasts(long tick, Action<CoreEntityId, string, ColorData> show)
        {
            lock (_broadcasts)
            {
                var count = 0;
                while (count < _broadcasts.Count && _broadcasts[count].Tick <= tick)
                {
                    var item = _broadcasts[count++];
                    show(item.Ship, item.Text, item.Color);
                }
                _broadcasts.RemoveRange(0, count);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Thread.Dispose();
            _transport.Dispose();
        }

        // Session thread: before the tick's pilots think, as the single-player adapter does.
        private void BeforeStep(long tick)
        {
            _stepTick = tick;
            if (Interlocked.Exchange(ref _playerInput, 0) == 1)
                _trees.OnPlayerInput(tick);
        }

        // Session thread, inside the step.
        private void OnBroadcast(CoreEntityId ship, string text, ColorData color)
        {
            lock (_broadcasts)
                _broadcasts.Add(new Broadcast(_stepTick, ship, text, color));
        }

        public sealed class Slot
        {
            public Slot(CombatSimulation.Core.RosterId rosterId, CombatSide side, IShipInfo info, CoreShipMapping mapping)
            {
                RosterId = rosterId;
                Side = side;
                Info = info;
                Mapping = mapping;
            }

            public CombatSimulation.Core.RosterId RosterId { get; }
            public CombatSide Side { get; }
            public IShipInfo Info { get; }
            public CoreShipMapping Mapping { get; }
        }

        private readonly struct Broadcast
        {
            public Broadcast(long tick, CoreEntityId ship, string text, ColorData color)
            {
                Tick = tick;
                Ship = ship;
                Text = text;
                Color = color;
            }

            public readonly long Tick;
            public readonly CoreEntityId Ship;
            public readonly string Text;
            public readonly ColorData Color;
        }

        private readonly RelayTransport _transport;
        private readonly BehaviorTreeFactory _trees;
        private readonly List<Broadcast> _broadcasts = new List<Broadcast>();
        private int _playerInput;
        private long _stepTick;
        private bool _disposed;
    }
}
