using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;

namespace OnlineDuel.Tests
{
    // Phase 7, step 5: can a worker thread run a driver of the main thread? BeginSend on a managed thread fails
    // (it allocates with Allocator.Temp), so this sends from a job, which may allocate Temp memory.
    public class TransportThreadTests
    {
        private const double TimeoutSeconds = 10;
        private NetworkDriver _host, _guest;
        private NetworkConnection _hostSide, _guestSide;

        [SetUp]
        public void SetUp()
        {
            _host = NetworkDriver.Create();
            _guest = NetworkDriver.Create();
            Assert.AreEqual(0, _host.Bind(NetworkEndpoint.LoopbackIpv4.WithPort(0)));
            Assert.AreEqual(0, _host.Listen());
            _guestSide = _guest.Connect(NetworkEndpoint.LoopbackIpv4.WithPort(_host.GetLocalEndpoint().Port));
            var connected = false;
            PumpUntil(() =>
            {
                if (!_hostSide.IsCreated) _hostSide = _host.Accept();
                NetworkEvent.Type type;
                while ((type = _guest.PopEventForConnection(_guestSide, out _)) != NetworkEvent.Type.Empty)
                    if (type == NetworkEvent.Type.Connect) connected = true;
                return connected && _hostSide.IsCreated;
            });
        }

        [TearDown]
        public void TearDown()
        {
            if (_guest.IsCreated) _guest.Dispose();
            if (_host.IsCreated) _host.Dispose();
        }

        // The main thread run is the control: if it fails too, the test is wrong, not the thread.
        [TestCase(false)]
        [TestCase(true)]
        public void SendsFromAJobAndReceives(bool onWorker)
        {
            void Exchange()
            {
                var data = new byte[300];
                for (var i = 0; i < data.Length; ++i) data[i] = (byte)(i * 31 + 7);
                Send(_guest, _guestSide, data);
                Send(_host, _hostSide, data);
                byte[] atHost = null, atGuest = null;
                var events = new System.Text.StringBuilder();
                PumpUntil(() =>
                {
                    atHost ??= Pop(_host, _hostSide, "host", events);
                    atGuest ??= Pop(_guest, _guestSide, "guest", events);
                    return atHost != null && atGuest != null;
                }, () => $"host {(atHost != null ? "has" : "has no")} data, guest {(atGuest != null ? "has" : "has no")} data; events: {events}");
                CollectionAssert.AreEqual(data, atHost);
                CollectionAssert.AreEqual(data, atGuest);
            }

            if (onWorker) RunOnWorker(Exchange);
            else Exchange();
        }

        // The send job runs before the flush job; Complete waits for both.
        private static void Send(NetworkDriver driver, NetworkConnection connection, byte[] data)
        {
            var bytes = new NativeArray<byte>(data, Allocator.Persistent);
            var status = new NativeArray<int>(1, Allocator.Persistent);
            try
            {
                var job = new SendJob { Driver = driver.ToConcurrent(), Connection = connection, Data = bytes, Status = status };
                driver.ScheduleFlushSend(job.Schedule()).Complete();
                Assert.AreEqual(data.Length, status[0], "EndSend returns the bytes sent, a BeginSend error is negative.");
            }
            finally
            {
                bytes.Dispose();
                status.Dispose();
            }
        }

        // PopEvent fails on a managed thread (it allocates Temp memory on a host); PopEventForConnection does not.
        private static byte[] Pop(NetworkDriver driver, NetworkConnection connection, string name, System.Text.StringBuilder events)
        {
            byte[] received = null;
            NetworkEvent.Type type;
            while ((type = driver.PopEventForConnection(connection, out var reader)) != NetworkEvent.Type.Empty)
            {
                events.Append($"{name}: {type} on {connection}; ");
                if (type != NetworkEvent.Type.Data) continue;
                received = new byte[reader.Length];
                reader.ReadBytes(received.AsSpan());
            }
            return received;
        }

        private void PumpUntil(Func<bool> done, Func<string> report = null)
        {
            var clock = Stopwatch.StartNew();
            while (true)
            {
                _host.ScheduleUpdate().Complete();
                _guest.ScheduleUpdate().Complete();
                if (done()) return;
                if (clock.Elapsed.TotalSeconds > TimeoutSeconds) Assert.Fail($"Timed out. {report?.Invoke()}");
                Thread.Sleep(1);
            }
        }

        private static void RunOnWorker(Action action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception e) { error = e; }
            }) { IsBackground = true, Name = "Transport worker test" };
            thread.Start();
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(2 * TimeoutSeconds)), "The worker thread did not finish.");
            if (error != null) Assert.Fail($"Worker thread: {error}");
        }

        private struct SendJob : IJob
        {
            public NetworkDriver.Concurrent Driver;
            public NetworkConnection Connection;
            [ReadOnly] public NativeArray<byte> Data;
            public NativeArray<int> Status;

            public void Execute()
            {
                var result = Driver.BeginSend(NetworkPipeline.Null, Connection, out var writer, Data.Length);
                if (result != 0) { Status[0] = result; return; }
                writer.WriteBytes(Data);
                Status[0] = Driver.EndSend(writer);
            }
        }
    }
}
