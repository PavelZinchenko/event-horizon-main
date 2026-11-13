using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CombatSimulation.Network;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;

namespace OnlineDuel
{
    public enum LinkState { Connecting, Connected, Closed }

    // ITransport over Unity Transport: one remote peer, plain datagrams on the null pipeline. Network N4: the
    // reliability stays in CombatSimulation.Network, so no Transport pipeline stage is used.
    // Single thread. Call order per loop: Update, then the PeerConnection, then Flush.
    public sealed class RelayTransport : ITransport, IDisposable
    {
        // Datagrams that Update received and TryReceive has not taken yet. A full queue drops new datagrams;
        // PeerConnection treats them as lost.
        public const int MaxQueuedDatagrams = 256;

        // Development mode without Relay. The host listens on the port; port 0 picks a free port (see LocalPort).
        public static RelayTransport ListenDirect(ushort port)
        {
            var transport = new RelayTransport(new NetworkSettings(Allocator.Temp), true);
            transport.Listen(NetworkEndpoint.AnyIpv4.WithPort(port));
            return transport;
        }

        // Development mode without Relay. The guest connects to the host's IPv4 or IPv6 address.
        public static RelayTransport ConnectDirect(string address, ushort port)
        {
            if (!NetworkEndpoint.TryParse(address, port, out var endpoint))
                throw new ArgumentException($"Not an IP address: '{address}'", nameof(address));
            var transport = new RelayTransport(new NetworkSettings(Allocator.Temp), false);
            transport.Connect(endpoint);
            return transport;
        }

        private RelayTransport(NetworkSettings settings, bool isHost)
        {
            _isHost = isHost;
            _driver = NetworkDriver.Create(settings);
        }

        public bool IsHost => _isHost;
        public LinkState State { get; private set; }
        // Valid when State is Closed.
        public DisconnectReason CloseReason { get; private set; }
        public ushort LocalPort => _driver.IsCreated ? _driver.GetLocalEndpoint().Port : (ushort)0;
        public long DroppedDatagrams { get; private set; }
        public long RejectedConnections { get; private set; }

        // Receives, sends what EndSend queued and keeps every event: the next driver update clears the driver's queue.
        public void Update()
        {
            if (!_driver.IsCreated) return;
            _driver.ScheduleUpdate().Complete();
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
            if (_driver.IsCreated) _driver.ScheduleFlushSend().Complete();
        }

        public bool TrySend(ReadOnlySpan<byte> datagram)
        {
            if (State != LinkState.Connected || datagram.IsEmpty) return false;
            var status = _driver.BeginSend(NetworkPipeline.Null, _connection, out var writer, datagram.Length);
            // A datagram above the Transport payload limit is a caller error, not backpressure.
            if (status == (int)StatusCode.NetworkPacketOverflow) throw new ArgumentOutOfRangeException(nameof(datagram));
            if (status != (int)StatusCode.Success) return false;
            // WriteBytes only reads the span; it has no ReadOnlySpan overload.
            writer.WriteBytes(MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(datagram), datagram.Length));
            return _driver.EndSend(writer) >= 0;
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
            _driver.Dispose();
            throw new InvalidOperationException(message);
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

        private sealed class Packet
        {
            internal byte[] Bytes = Array.Empty<byte>();
            internal int Length;
        }

        private readonly bool _isHost;
        private NetworkDriver _driver;
        private NetworkConnection _connection;
        private readonly Queue<Packet> _received = new Queue<Packet>();
        private readonly Stack<Packet> _pool = new Stack<Packet>();
    }
}
