#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;
using Constructor.Extensions;
using GameDatabase;
using GameDatabase.Enums;
using GameServices.Multiplayer;
using OnlineDuel;
using Services.Resources;
using UnityEditor;
using UnityEngine;
using CoreRosterId = CombatSimulation.Core.RosterId;
using Debug = UnityEngine.Debug;

namespace Combat.Integration.Core
{
    // Phase 7, step 5 check: two editor instances (Multiplayer Play Mode) run a duel with AI pilots on both sides: the
    // handshake on the main thread, then the session on its thread, over a direct or Relay link. Each instance deploys
    // its next ship when it has none active and shows the loop health. Both instances read one system clock
    // (QueryPerformanceCounter), so a step's deviation from its ideal time (epoch + tick / 50) compares across them: the
    // difference of the two means is the tick offset. Not Stopwatch: Unity's runtime counts it from the process start,
    // so two editor instances read different values (step 6 finding). Built-in database; each player gets six random common ships.
    // Turn it on with Tools/Online Duel/Session Probe; every instance that enters play mode then shows it.
    // The duel combat scene runs from the game's lobby (main menu, Online duel), not from here (step 7).
    public sealed class CoreDuelSessionProbe : MonoBehaviour
    {
        private const string PrefKey = "OnlineDuel.SessionProbe";
        private const string MenuPath = "Tools/Online Duel/Session Probe";
        private const int FleetSize = 6;
        private const int AiLevel = 100;
        private const int RedeployTicks = 50;

        [MenuItem(MenuPath)]
        private static void Toggle() => EditorPrefs.SetBool(PrefKey, !EditorPrefs.GetBool(PrefKey));

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PrefKey));
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Spawn()
        {
            if (!EditorPrefs.GetBool(PrefKey)) return;
            var probe = new GameObject(nameof(CoreDuelSessionProbe));
            DontDestroyOnLoad(probe);
            probe.AddComponent<CoreDuelSessionProbe>();
        }

        // Seconds of the system-wide monotonic clock: every process on the PC reads the same timeline. Other editors fall
        // back to Stopwatch; their deviation means then do not compare across instances.
#if UNITY_EDITOR_WIN
        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long count);

        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long frequency);

        private static readonly double CounterFrequency = QueryPerformanceFrequency(out var frequency) ? frequency : 1;

        private static double Now() => QueryPerformanceCounter(out var count) ? count / CounterFrequency : 0;
#else
        private static double Now() => Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
#endif

        private void Awake()
        {
            _database = UnityDatabaseFactory.Create();
            _database.LoadDefault();
            _mapper = new CoreShipMapper(_database, Resources.Load<ResourceLocator>("ResourceLocator"));
            _duelSetup = new CoreDuelSetup(_database, _mapper);
        }

        private void Update()
        {
            if (_transport == null) return;
            if (_thread != null)
            {
                Drain();
                if (!_errorLogged && !_thread.Status.IsRunning && _thread.Error != null)
                {
                    _errorLogged = true;
                    Debug.LogError($"CoreDuelSessionProbe: the session thread stopped at tick {_thread.Status.Tick}: {_thread.Error}");
                }
                return;
            }

            var now = Now();
            _transport.Update();
            if (_handshake == null && _transport.State == LinkState.Connected) _handshake = CreateHandshake();
            _handshake?.Update(now);
            _transport.Flush();
            if (_handshake?.State != HandshakeState.Ready) return;
            StartSession(Now());
        }

        private MatchHandshake CreateHandshake()
        {
            IDatabase database = _database;
            _duelSetup = new CoreDuelSetup(database, _mapper);
            var builds = database.ShipBuildList.Where(item => item.Ship.ShipType == ShipType.Common)
                .OrderBy(item => item.Id.Value).ToList();
            var random = new System.Random(_transport.IsHost ? 1 : 2);
            var fleet = builds.OrderBy(_ => random.Next()).Take(FleetSize).Create(0, random, database);
            var identity = new MatchIdentity(Application.version, database.ContentHash, FleetWebSerializer.SerializeFleetData(fleet));
            if (!_transport.IsHost) return MatchHandshake.Guest(_transport, identity, _duelSetup.Build);
            var seed = new System.Random();
            return MatchHandshake.Host(_transport, identity, _duelSetup.Build, new BattleId((ulong)seed.Next(1, int.MaxValue)),
                (ulong)seed.Next());
        }

        // From here the session thread owns the transport.
        private void StartSession(double now)
        {
            var setup = _handshake.Setup;
            var trees = CorePilotProvider.CreateTrees(setup.MatchSeed, new Torus(setup.Width, setup.Height), _mapper);
            var pilots = new CorePilotProvider(_database.CombatSettings, trees, _mapper.Mappings, -1, AiLevel);
            var session = _handshake.CreateSession(pilots, now);
            _epoch = _handshake.Epoch;
            _localSide = _handshake.Plan.LocalSide;
            _thread = new SessionThread(session, _transport, Now, afterStep: AfterStep);
            _thread.Start();
        }

        // Session thread. The step of tick N is ideal at epoch + N / 50 on the reference clock.
        private void AfterStep(NetworkSession session, double now)
        {
            var deviation = (now - (_epoch + session.Snapshot.Tick / TicksPerSecond)) * 1000;
            lock (_lock)
            {
                ++_steps;
                _deviationSum += deviation;
                _deviationMin = Math.Min(_deviationMin, deviation);
                _deviationMax = Math.Max(_deviationMax, deviation);
                if (Math.Abs(deviation) > 1000 / TicksPerSecond) ++_offTicks;
            }
        }

        private void Drain()
        {
            var active = false;
            var ready = default(CoreRosterId);
            var hasReady = false;
            while (_thread.TryTake(out var snapshot))
            {
                _tick = snapshot.Tick;
                if (snapshot.IsCompleted) _result = snapshot.Result;
                active = false;
                hasReady = false;
                foreach (var slot in snapshot.Roster)
                {
                    if (slot.Side != _localSide) continue;
                    if (slot.Status == RosterStatus.Active) active = true;
                    if (slot.Status == RosterStatus.Ready && !hasReady) { ready = slot.RosterId; hasReady = true; }
                }
                _thread.Release(snapshot);
                _seen = true;
            }

            if (!_seen || _result.IsCompleted || active || !hasReady) return;
            if (_deploying.Equals(ready) && _tick < _deployTick + RedeployTicks) return;
            _thread.Post(SessionCommand.Deploy(ready));
            _deploying = ready;
            _deployTick = _tick;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 380, 560), GUI.skin.box);
            GUILayout.Label("Session probe");
            if (_pending != null)
            {
                GUILayout.Label("Waiting for Unity Services...");
                if (GUILayout.Button("Cancel")) Close();
            }
            else if (_transport == null)
            {
                GUILayout.Label("Direct: address, port");
                _address = GUILayout.TextField(_address);
                _port = GUILayout.TextField(_port);
                if (GUILayout.Button("Host") && ushort.TryParse(_port, out var hostPort)) Open(() => RelayTransport.ListenDirect(hostPort));
                if (GUILayout.Button("Join") && ushort.TryParse(_port, out var joinPort)) Open(() => RelayTransport.ConnectDirect(_address, joinPort));
                GUILayout.Label("Relay: join code");
                _joinCode = GUILayout.TextField(_joinCode);
                if (GUILayout.Button("Host (Relay)")) HostRelay();
                if (GUILayout.Button("Join (Relay)")) JoinRelay();
                if (_error != null) GUILayout.Label(_error);
            }
            else
            {
                if (_relayCode != null) GUILayout.Label($"Join code: {_relayCode}");
                GUILayout.Label(_transport.IsHost ? "Host (side 0, clock reference)" : "Guest (side 1)");
                if (_thread == null) ShowHandshake();
                else ShowSession();
                if (GUILayout.Button("Close")) Close();
            }
            GUILayout.EndArea();
        }

        private void ShowHandshake()
        {
            GUILayout.Label($"Link: {_transport.State}");
            if (_handshake == null) return;
            GUILayout.Label($"Handshake: {_handshake.State}" + (_handshake.State == HandshakeState.Rejected
                ? $" ({_handshake.Rejection}{(_handshake.RejectedByPeer ? ", by the peer" : string.Empty)})" : string.Empty));
            if (_handshake.SetupError != null) GUILayout.Label(_handshake.SetupError.Message);
        }

        private void ShowSession()
        {
            var status = _thread.Status;
            GUILayout.Label($"Tick {_tick}" + (_result.IsCompleted
                ? $", {_result.Outcome} (side {_result.WinningSide}, {_result.Reason})" : string.Empty));
            GUILayout.Label(status.IsRunning ? "Session thread running"
                : status.LinkLoss != SessionLinkLoss.None ? $"Link lost: {status.LinkLoss}"
                : $"Session thread stopped: {_thread.Error?.Message}");
            GUILayout.Label($"Queued copies {status.QueuedSnapshots}, catch-up ticks {status.CatchUpTicks}");
            GUILayout.Label($"Max loop gap {status.MaxGap:F1} ms, max late step {status.MaxLate:F1} ms");
            GUILayout.Label($"Round trip {status.RoundTrip:F1} ms, clock error {status.ClockErrorTicks:F2} ticks");
            GUILayout.Label($"Pending reliable {status.PendingReliablePackets}, rejected commands {status.RejectedCommands}");
            // Both labels always: the control count must not change between the layout and repaint events.
            lock (_lock)
            {
                GUILayout.Label(_steps == 0 ? "Step deviation: -"
                    : $"Step deviation: mean {_deviationSum / _steps:F2} ms, min {_deviationMin:F1}, max {_deviationMax:F1}");
                GUILayout.Label($"Steps {_steps}, more than one tick off {_offTicks}");
            }
        }

        private void OnDestroy() => Close();

        private void Open(Func<RelayTransport> create)
        {
            try
            {
                _transport = create();
                _error = null;
            }
            catch (Exception e)
            {
                _error = e.Message;
            }
        }

        private async void HostRelay()
        {
            var pending = Begin();
            try
            {
                var (transport, joinCode) = await DuelRelay.HostAsync(pending.Token);
                if (End(pending, transport)) _relayCode = joinCode;
            }
            catch (Exception e)
            {
                Fail(pending, e);
            }
        }

        private async void JoinRelay()
        {
            var pending = Begin();
            try
            {
                End(pending, await DuelRelay.JoinAsync(_joinCode, pending.Token));
            }
            catch (Exception e)
            {
                Fail(pending, e);
            }
        }

        private CancellationTokenSource Begin()
        {
            _error = null;
            return _pending = new CancellationTokenSource();
        }

        // False when Close or OnDestroy cancelled the request while it ran; the transport is then disposed.
        private bool End(CancellationTokenSource pending, RelayTransport transport)
        {
            if (pending.IsCancellationRequested)
            {
                transport.Dispose();
                return false;
            }
            _pending = null;
            _transport = transport;
            return true;
        }

        private void Fail(CancellationTokenSource pending, Exception e)
        {
            if (pending.IsCancellationRequested) return;
            _pending = null;
            _error = e.Message;
            Debug.LogException(e);
        }

        // The thread stops first, so the transport is disposed on the main thread.
        private void Close()
        {
            _pending?.Cancel();
            _pending = null;
            _relayCode = null;
            _thread?.Dispose();
            _thread = null;
            _transport?.Dispose();
            _transport = null;
            _handshake = null;
            _result = default;
            _tick = -1;
            _seen = false;
            _errorLogged = false;
            _deploying = default;
            lock (_lock)
            {
                _steps = _offTicks = 0;
                _deviationSum = 0;
                _deviationMin = double.MaxValue;
                _deviationMax = double.MinValue;
            }
        }

        private const double TicksPerSecond = 50;
        private GameDatabase.Database _database;
        private CoreShipMapper _mapper;
        private CoreDuelSetup _duelSetup;
        private string _address = "127.0.0.1";
        private string _port = "7777";
        private string _error;
        private string _joinCode = string.Empty;
        private string _relayCode;
        private CancellationTokenSource _pending;
        private RelayTransport _transport;
        private MatchHandshake _handshake;
        private SessionThread _thread;
        private double _epoch;
        private int _localSide;
        private long _tick = -1, _deployTick;
        private bool _seen;
        private bool _errorLogged;
        private CoreRosterId _deploying;
        private BattleResult _result;
        // Written by the session thread.
        private readonly object _lock = new object();
        private long _steps, _offTicks;
        private double _deviationSum, _deviationMin = double.MaxValue, _deviationMax = double.MinValue;
    }
}
#endif
