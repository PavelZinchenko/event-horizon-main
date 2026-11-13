#if UNITY_EDITOR
using System;
using CombatSimulation.Network;
using UnityEditor;
using UnityEngine;

namespace OnlineDuel
{
    // Development check for two editor instances (Multiplayer Play Mode): a direct link that runs a PeerConnection.
    // Turn it on with Tools/Online Duel/Transport Probe; every instance that enters play mode then shows it.
    public sealed class TransportProbe : MonoBehaviour, IPeerReceiver
    {
        private const string PrefKey = "OnlineDuel.TransportProbe";
        private const string MenuPath = "Tools/Online Duel/Transport Probe";
        private const double ReliableInterval = 0.5;
        private const int MaxReliableSize = 4000; // Several datagrams.

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
            var probe = new GameObject(nameof(TransportProbe));
            DontDestroyOnLoad(probe);
            probe.AddComponent<TransportProbe>();
        }

        public void OnInput(ReadOnlySpan<byte> payload) => ++_inputs;
        public void OnUnreliable(ReadOnlySpan<byte> payload) => ++_unreliable;

        public void OnReliable(ReadOnlySpan<byte> payload)
        {
            var valid = payload.Length >= 4 && ReadInt(payload) == _nextExpected;
            for (var i = 4; valid && i < payload.Length; ++i) valid = payload[i] == (byte)(_nextExpected + i);
            if (!valid) ++_reliableErrors;
            ++_nextExpected;
        }

        private void Update()
        {
            if (_transport == null) return;
            var now = Time.realtimeSinceStartupAsDouble;
            _transport.Update();
            if (_peer == null && _transport.State == LinkState.Connected)
            {
                _peer = new PeerConnection(_transport, this, _transport.IsHost);
                _nextReliable = now;
            }
            if (_peer == null || _peer.IsFaulted || _transport.State != LinkState.Connected) return;

            _input[0] = (byte)Time.frameCount;
            _peer.TrySendInput(_input);
            _peer.TrySendUnreliable(_input);
            if (now >= _nextReliable)
            {
                _nextReliable += ReliableInterval;
                var length = 4 + _random.Next(MaxReliableSize);
                WriteInt(_reliable, _sentReliable);
                for (var i = 4; i < length; ++i) _reliable[i] = (byte)(_sentReliable + i);
                _peer.QueueReliable(_reliable.AsSpan(0, length));
                ++_sentReliable;
            }
            _peer.Update(now);
            _transport.Flush();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 340, 400), GUI.skin.box);
            GUILayout.Label("Transport probe (direct)");
            if (_transport == null)
            {
                _address = GUILayout.TextField(_address);
                _port = GUILayout.TextField(_port);
                if (GUILayout.Button("Host") && ushort.TryParse(_port, out var hostPort)) Open(() => RelayTransport.ListenDirect(hostPort));
                if (GUILayout.Button("Join") && ushort.TryParse(_port, out var joinPort)) Open(() => RelayTransport.ConnectDirect(_address, joinPort));
                if (_error != null) GUILayout.Label(_error);
            }
            else
            {
                GUILayout.Label($"{(_transport.IsHost ? "Host" : "Guest")}, port {_transport.LocalPort}: {_transport.State}" +
                    (_transport.State == LinkState.Closed ? $" ({_transport.CloseReason})" : string.Empty));
                if (_peer != null)
                {
                    GUILayout.Label($"Round trip: {_peer.Clock.RoundTripSeconds * 1000:F1} ms");
                    GUILayout.Label($"Reliable: sent {_sentReliable}, received {_nextExpected}, errors {_reliableErrors}, pending {_peer.PendingReliablePackets}");
                    GUILayout.Label($"Inputs {_inputs}, unreliable {_unreliable}");
                    GUILayout.Label($"Invalid {_peer.InvalidPackets}, dropped {_transport.DroppedDatagrams}, faulted {_peer.IsFaulted}");
                }
                if (GUILayout.Button("Close")) Close();
            }
            GUILayout.EndArea();
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

        private void Close()
        {
            _transport?.Dispose();
            _transport = null;
            _peer = null;
            _sentReliable = _nextExpected = _reliableErrors = 0;
            _inputs = _unreliable = 0;
        }

        private static void WriteInt(Span<byte> bytes, int value)
        {
            bytes[0] = (byte)value; bytes[1] = (byte)(value >> 8); bytes[2] = (byte)(value >> 16); bytes[3] = (byte)(value >> 24);
        }

        private static int ReadInt(ReadOnlySpan<byte> bytes) => bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24;

        private string _address = "127.0.0.1";
        private string _port = "7777";
        private string _error;
        private RelayTransport _transport;
        private PeerConnection _peer;
        private double _nextReliable;
        private int _sentReliable, _nextExpected, _reliableErrors, _inputs, _unreliable;
        private readonly byte[] _input = new byte[8];
        private readonly byte[] _reliable = new byte[4 + MaxReliableSize];
        private readonly System.Random _random = new System.Random();
    }
}
#endif
