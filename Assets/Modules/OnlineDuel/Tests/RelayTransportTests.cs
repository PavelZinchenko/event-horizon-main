using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CombatSimulation.Network;
using NUnit.Framework;
using Unity.Networking.Transport.Error;

namespace OnlineDuel.Tests
{
    // Direct mode over UDP loopback, with real time.
    public class RelayTransportTests
    {
        private const double TimeoutSeconds = 10;
        private readonly List<RelayTransport> _transports = new List<RelayTransport>();

        [TearDown]
        public void TearDown()
        {
            foreach (var transport in _transports) transport.Dispose();
            _transports.Clear();
        }

        [Test]
        public void DirectConnectionCarriesDatagramsBothWays()
        {
            var (host, guest) = Connect();
            Assert.IsFalse(host.TryReceive(new byte[16], out _));

            var toHost = Payload(PeerConnection.DefaultDatagramSize, 1);
            var toGuest = Payload(17, 2);
            Assert.IsTrue(guest.TrySend(toHost));
            Assert.IsTrue(host.TrySend(toGuest));
            var buffer = new byte[2048];
            var length = 0;
            PumpUntil(() => host.TryReceive(buffer, out length), host, guest);
            CollectionAssert.AreEqual(toHost, buffer.AsSpan(0, length).ToArray());
            PumpUntil(() => guest.TryReceive(buffer, out length), host, guest);
            CollectionAssert.AreEqual(toGuest, buffer.AsSpan(0, length).ToArray());
        }

        [Test]
        public void ShortBufferReportsTheOriginalLength()
        {
            var (host, guest) = Connect();
            var data = Payload(100, 3);
            Assert.IsTrue(guest.TrySend(data));
            var buffer = new byte[10];
            var length = 0;
            PumpUntil(() => host.TryReceive(buffer, out length), host, guest);
            Assert.AreEqual(100, length);
            CollectionAssert.AreEqual(data.AsSpan(0, 10).ToArray(), buffer);
        }

        [Test]
        public void SendFailsBeforeTheConnectionAndAfterClose()
        {
            var host = Track(RelayTransport.ListenDirect(0));
            var guest = Track(RelayTransport.ConnectDirect("127.0.0.1", host.LocalPort));
            Assert.IsFalse(guest.TrySend(new byte[] { 1 }));
            PumpUntil(() => guest.State == LinkState.Connected && host.State == LinkState.Connected, host, guest);
            guest.Dispose();
            Assert.IsFalse(guest.TrySend(new byte[] { 1 }));
        }

        [Test]
        public void ClosingOnePeerClosesTheOther()
        {
            var (host, guest) = Connect();
            guest.Dispose();
            PumpUntil(() => host.State == LinkState.Closed, host);
            Assert.AreEqual(DisconnectReason.ClosedByRemote, host.CloseReason);
            Assert.IsFalse(host.TrySend(new byte[] { 1 }));
        }

        [Test]
        public void HostTurnsAwayASecondGuest()
        {
            var (host, guest) = Connect();
            var third = Track(RelayTransport.ConnectDirect("127.0.0.1", host.LocalPort));
            PumpUntil(() => third.State == LinkState.Closed, host, guest, third);
            Assert.AreEqual(1, host.RejectedConnections);
            Assert.AreEqual(LinkState.Connected, host.State);
            Assert.AreEqual(LinkState.Connected, guest.State);
        }

        [Test]
        public void DatagramAboveTheTransportLimitIsACallerError()
        {
            var (_, guest) = Connect();
            Assert.Throws<ArgumentOutOfRangeException>(() => guest.TrySend(new byte[4096]));
        }

        [TestCase(1)]
        [TestCase(1191)] // With its 2-byte prefix, fills one datagram exactly.
        [TestCase(85000)] // Several reliable windows.
        public void PeerConnectionDeliversCompleteMessagesOverLoopback(int size)
        {
            var (host, guest) = Connect();
            var hostReceiver = new Receiver();
            var guestReceiver = new Receiver();
            var a = new PeerConnection(host, hostReceiver, true);
            var b = new PeerConnection(guest, guestReceiver, false);
            var data = Payload(size, 4);
            a.QueueReliable(data);
            a.QueueReliable(new byte[] { 99 });
            b.QueueReliable(data);
            Assert.IsTrue(a.TrySendInput(new byte[] { 5 }));
            Assert.IsTrue(b.TrySendUnreliable(new byte[] { 6 }));

            var clock = Stopwatch.StartNew();
            PumpUntil(() =>
            {
                foreach (var (transport, peer) in new[] { (host, a), (guest, b) })
                {
                    transport.Update();
                    peer.Update(clock.Elapsed.TotalSeconds);
                    transport.Flush();
                }
                return guestReceiver.Reliable.Count == 2 && hostReceiver.Reliable.Count == 1 &&
                       a.PendingReliablePackets == 0 && b.PendingReliablePackets == 0;
            });
            CollectionAssert.AreEqual(data, guestReceiver.Reliable[0]);
            CollectionAssert.AreEqual(new byte[] { 99 }, guestReceiver.Reliable[1]);
            CollectionAssert.AreEqual(data, hostReceiver.Reliable[0]);
            Assert.AreEqual(1, guestReceiver.Inputs);
            Assert.AreEqual(1, hostReceiver.Unreliable);
            Assert.AreEqual(0, a.InvalidPackets + b.InvalidPackets);
            Assert.AreEqual(0, host.DroppedDatagrams + guest.DroppedDatagrams);
        }

        private (RelayTransport host, RelayTransport guest) Connect()
        {
            var host = Track(RelayTransport.ListenDirect(0));
            Assert.AreNotEqual(0, host.LocalPort);
            var guest = Track(RelayTransport.ConnectDirect("127.0.0.1", host.LocalPort));
            PumpUntil(() => guest.State == LinkState.Connected && host.State == LinkState.Connected, host, guest);
            return (host, guest);
        }

        private RelayTransport Track(RelayTransport transport)
        {
            _transports.Add(transport);
            return transport;
        }

        private static void PumpUntil(Func<bool> done, params RelayTransport[] transports)
        {
            var clock = Stopwatch.StartNew();
            while (true)
            {
                foreach (var transport in transports) transport.Update();
                if (done()) return;
                if (clock.Elapsed.TotalSeconds > TimeoutSeconds) Assert.Fail("Timed out");
                Thread.Sleep(1);
            }
        }

        private static byte[] Payload(int size, int seed)
        {
            var bytes = new byte[size];
            for (var i = 0; i < size; ++i) bytes[i] = (byte)(i * 31 + seed);
            return bytes;
        }

        private sealed class Receiver : IPeerReceiver
        {
            internal readonly List<byte[]> Reliable = new List<byte[]>();
            internal int Inputs, Unreliable;
            public void OnReliable(ReadOnlySpan<byte> payload) => Reliable.Add(payload.ToArray());
            public void OnUnreliable(ReadOnlySpan<byte> payload) => ++Unreliable;
            public void OnInput(ReadOnlySpan<byte> payload) => ++Inputs;
        }
    }
}
