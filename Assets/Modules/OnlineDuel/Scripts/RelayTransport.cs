using System;
using System.Collections.Generic;
using CombatSimulation.Network;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using Unity.Networking.Transport.Relay;

namespace OnlineDuel
{
    public enum LinkState { Connecting, Connected, Closed }

    // ITransport over Unity Transport: one remote peer, plain datagrams on the null pipeline. Network N4: the
    // reliability stays in CombatSimulation.Network, so no Transport pipeline stage is used.
    // One thread at a time. Call order per loop: Update, then the PeerConnection, then Flush.
    // Phase 7, step 5: the session thread may own it. Unity allows Allocator.Temp only on the main thread and in jobs,
    // and BeginSend allocates with it, so TrySend queues the datagram and a job sends the queue in Update and Flush.
    // The driver is created from persistent settings for the same reason.
    public sealed class RelayTransport : IPumpedTransport, IDisposable
    {
        // Datagrams that Update received and TryReceive has not taken yet. A full queue drops new datagrams;
        // PeerConnection treats them as lost.
        public const int MaxQueuedDatagrams = 256;
        public const int LinkTimeoutMilliseconds = 10000;
        private const int ConnectAttemptMilliseconds = 1000;

        // Development mode without Relay. The host listens on the port; port 0 picks a free port (see LocalPort).
        public static RelayTransport ListenDirect(ushort port)
        {
            var transport = new RelayTransport(Settings(), true);
            transport.Listen(NetworkEndpoint.AnyIpv4.WithPort(port));
            return transport;
        }

        // Development mode without Relay. The guest connects to the host's IPv4 or IPv6 address.
        public static RelayTransport ConnectDirect(string address, ushort port)
        {
            if (!NetworkEndpoint.TryParse(address, port, out var endpoint))
                throw new ArgumentException($"Not an IP address: '{address}'", nameof(address));
            var transport = new RelayTransport(Settings(), false);
            transport.Connect(endpoint);
            return transport;
        }

        // The host binds to the Relay server of its allocation and waits there for the guest.
        public static RelayTransport ListenRelay(RelayServerData relay)
        {
            var transport = new RelayTransport(RelaySettings(ref relay), true, true);
            transport.Listen(NetworkEndpoint.AnyIpv4);
            return transport;
        }

        // The guest connects to the host through the Relay server of the joined allocation.
        public static RelayTransport ConnectRelay(RelayServerData relay)
        {
            var transport = new RelayTransport(RelaySettings(ref relay), false, true);
            transport.Connect(relay.Endpoint);
            return transport;
        }

        private static NetworkSettings RelaySettings(ref RelayServerData relay)
        {
            var settings = Settings();
            settings.WithRelayParameters(ref relay);
            return settings;
        }

        // Phase 7, step 8 (Q4): a guest gives up connecting after about LinkTimeoutMilliseconds, and Transport closes a link
        // without any packet for as long. Transport defaults are 60 attempts and 30 s.
        private static NetworkSettings Settings()
        {
            var settings = new NetworkSettings(Allocator.Persistent);
            settings.WithNetworkConfigParameters(
                connectTimeoutMS: ConnectAttemptMilliseconds,
                maxConnectAttempts: LinkTimeoutMilliseconds / ConnectAttemptMilliseconds,
                disconnectTimeoutMS: LinkTimeoutMilliseconds);
            return settings;
        }

        private RelayTransport(NetworkSettings settings, bool isHost, bool usesRelay = false)
        {
            _isHost = isHost;
            _usesRelay = usesRelay;
            try { _driver = NetworkDriver.Create(settings); }
            finally { settings.Dispose(); }
            _outgoing = new NativeList<byte>(MaxQueuedDatagrams * 64, Allocator.Persistent);
            _outgoingLengths = new NativeList<int>(MaxQueuedDatagrams, Allocator.Persistent);
            _sendFailures = new NativeArray<int>(1, Allocator.Persistent);
        }

        public bool IsHost => _isHost;
        public LinkState State { get; private set; }
        public bool IsClosed => State == LinkState.Closed;
        // Closed because the peer never answered or went silent, not because it left.
        public bool TimedOut => IsClosed &&
            (CloseReason == DisconnectReason.Timeout || CloseReason == DisconnectReason.MaxConnectionAttempts);
        // Valid when State is Closed.
        public DisconnectReason CloseReason { get; private set; }
        public ushort LocalPort => _driver.IsCreated ? _driver.GetLocalEndpoint().Port : (ushort)0;
        public long DroppedDatagrams { get; private set; }
        // Queued datagrams the send job could not send: the driver's send queue was full, or the link closed.
        public long FailedSends { get; private set; }
        public long RejectedConnections { get; private set; }
        // The Relay server dropped the allocation: it was invalid or timed out. A new allocation is needed.
        public bool AllocationInvalid { get; private set; }

        // Receives, sends what EndSend queued and keeps every event: the next driver update clears the driver's queue.
        public void Update()
        {
            if (!_driver.IsCreated) return;
            _driver.ScheduleUpdate(ScheduleSends()).Complete();
            SendsDone();
            if (_usesRelay && State != LinkState.Closed &&
                _driver.GetRelayConnectionStatus() == RelayConnectionStatus.AllocationInvalid)
            {
                AllocationInvalid = true;
                Close(DisconnectReason.Default);
                return;
            }
            AcceptConnections();
            if (State == LinkState.Closed || !_connection.IsCreated) return;

            NetworkEvent.Type type;
            while ((type = _driver.PopEventForConnection(_connection, out var reader)) != NetworkEvent.Type.Empty)
            {
                switch (type)
                {
                    case NetworkEvent.Type.Connect:
                        State = LinkState.Connected;
                        break;
                    case NetworkEvent.Type.Data:
                        Enqueue(ref reader);
                        break;
                    case NetworkEvent.Type.Disconnect:
                        Close(reader.Length > 0 ? (DisconnectReason)reader.ReadByte() : DisconnectReason.Default);
                        return;
                }
            }
        }

        // Sends now what TrySend queued, instead of at the next Update.
        public void Flush()
        {
            if (!_driver.IsCreated) return;
            _driver.ScheduleFlushSend(ScheduleSends()).Complete();
            SendsDone();
        }

        // Queues the datagram for the next Update or Flush.
        public bool TrySend(ReadOnlySpan<byte> datagram)
        {
            if (State != LinkState.Connected || datagram.IsEmpty) return false;
            var capacity = _driver.GetMaxSupportedPayloadSize(_connection, NetworkPipeline.Null);
            if (capacity < 0) return false;
            // A datagram above the Transport payload limit is a caller error, not backpressure.
            if (datagram.Length > capacity) throw new ArgumentOutOfRangeException(nameof(datagram));
            if (_outgoingLengths.Length >= MaxQueuedDatagrams) return false;
            var start = _outgoing.Length;
            _outgoing.ResizeUninitialized(start + datagram.Length);
            datagram.CopyTo(_outgoing.AsArray().GetSubArray(start, datagram.Length).AsSpan());
            _outgoingLengths.Add(datagram.Length);
            return true;
        }

        public bool TryReceive(Span<byte> buffer, out int length)
        {
            length = 0;
            if (_received.Count == 0) return false;
            var packet = _received.Dequeue();
            length = packet.Length;
            packet.Bytes.AsSpan(0, Math.Min(length, buffer.Length)).CopyTo(buffer);
            _pool.Push(packet);
            return true;
        }

        // Leaves the match. With Relay this also leaves the Relay session (network decision 10): the driver stops its
        // keep-alive messages, and the Relay server frees the allocation after its inactivity timeout of about 10 s.
        public void Dispose()
        {
            if (!_driver.IsCreated) return;
            if (State != LinkState.Closed && _connection.IsCreated)
            {
                // The update sends the disconnect message, so the peer closes at once instead of after its timeout.
                _driver.Disconnect(_connection);
                _driver.ScheduleUpdate().Complete();
            }
            if (State != LinkState.Closed) Close(DisconnectReason.Default);
            _driver.Dispose();
            _outgoing.Dispose();
            _outgoingLengths.Dispose();
            _sendFailures.Dispose();
        }

        private void Listen(NetworkEndpoint endpoint)
        {
            if (_driver.Bind(endpoint) != 0)
                Fail($"Cannot bind port {endpoint.Port}");
            if (_driver.Listen() != 0)
                Fail("Cannot listen");
        }

        private void Connect(NetworkEndpoint endpoint)
        {
            _connection = _driver.Connect(endpoint);
            if (!_connection.IsCreated)
                Fail($"Cannot connect to {endpoint}");
        }

        private void Fail(string message)
        {
            Dispose();
            throw new InvalidOperationException(message);
        }

        private JobHandle ScheduleSends()
        {
            if (_outgoingLengths.Length == 0) return default;
            _sendFailures[0] = 0;
            return new SendJob
            {
                Driver = _driver.ToConcurrent(), Connection = _connection, Bytes = _outgoing, Lengths = _outgoingLengths,
                Failures = _sendFailures,
            }.Schedule();
        }

        private void SendsDone()
        {
            if (_outgoingLengths.Length == 0) return;
            FailedSends += _sendFailures[0];
            _outgoing.Clear();
            _outgoingLengths.Clear();
        }

        // One peer per match: the host keeps the first guest and turns away the rest.
        private void AcceptConnections()
        {
            if (!_isHost) return;
            NetworkConnection connection;
            while ((connection = _driver.Accept()) != default)
            {
                if (State == LinkState.Connecting && !_connection.IsCreated)
                {
                    _connection = connection;
                    State = LinkState.Connected;
                    continue;
                }
                _driver.Disconnect(connection);
                ++RejectedConnections;
            }
        }

        private void Enqueue(ref DataStreamReader reader)
        {
            if (_received.Count >= MaxQueuedDatagrams)
            {
                ++DroppedDatagrams;
                return;
            }
            var packet = _pool.Count == 0 ? new Packet() : _pool.Pop();
            if (packet.Bytes.Length < reader.Length) packet.Bytes = new byte[reader.Length];
            packet.Length = reader.Length;
            reader.ReadBytes(packet.Bytes.AsSpan(0, packet.Length));
            _received.Enqueue(packet);
        }

        private void Close(DisconnectReason reason)
        {
            State = LinkState.Closed;
            CloseReason = reason;
            _connection = default;
        }

        // Jobs may allocate with Allocator.Temp, as BeginSend does. A closed link fails each send.
        private struct SendJob : IJob
        {
            public NetworkDriver.Concurrent Driver;
            public NetworkConnection Connection;
            [ReadOnly] public NativeList<byte> Bytes;
            [ReadOnly] public NativeList<int> Lengths;
            public NativeArray<int> Failures;

            public void Execute()
            {
                var bytes = Bytes.AsArray();
                var offset = 0;
                for (var i = 0; i < Lengths.Length; ++i)
                {
                    var length = Lengths[i];
                    if (Driver.BeginSend(NetworkPipeline.Null, Connection, out var writer, length) != (int)StatusCode.Success)
                        ++Failures[0];
                    else
                    {
                        writer.WriteBytes(bytes.GetSubArray(offset, length));
                        if (Driver.EndSend(writer) < 0) ++Failures[0];
                    }
                    offset += length;
                }
            }
        }

        private sealed class Packet
        {
            internal byte[] Bytes = Array.Empty<byte>();
            internal int Length;
        }

        private readonly bool _isHost;
        private readonly bool _usesRelay;
        private NetworkDriver _driver;
        private NetworkConnection _connection;
        // The datagrams TrySend queued, back to back, and their lengths.
        private NativeList<byte> _outgoing;
        private NativeList<int> _outgoingLengths;
        private NativeArray<int> _sendFailures;
        private readonly Queue<Packet> _received = new Queue<Packet>();
        private readonly Stack<Packet> _pool = new Stack<Packet>();
    }
}
