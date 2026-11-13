using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Combat.Integration.Core;
using CombatSimulation.Core;
using CombatSimulation.Network;
using CombatSimulation.Runtime;
using Constructor.Extensions;
using GameDatabase;
using GameDatabase.Enums;
using GameServices.Multiplayer;
using Services.Resources;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Phase 7, step 4: the online duel handshake on the built-in database and real ships, in one process.
// 1. The setup hash of every non-drone build: the reflection walk must reach every definition type.
// 2. Two players with 12 ships each reach Ready with one setup hash.
// 3. Another game version is rejected on both players.
// Reports the time of the database hash and of the setup hash.
public static class CoreDuelHandshakeCheck
{
    [MenuItem("Tools/Combat/Check Online Duel Handshake")]
    public static void Run()
    {
        var report = new StringBuilder("Online duel handshake check\n");
        var failures = 0;
        var database = UnityDatabaseFactory.Create();
        database.LoadDefault();
        var mapper = new CoreShipMapper(database, Resources.Load<ResourceLocator>("ResourceLocator"));
        var duelSetup = new CoreDuelSetup(database, mapper);

        var clock = Stopwatch.StartNew();
        var databaseHash = database.ContentHash;
        report.AppendLine($"Database hash {Convert.ToBase64String(databaseHash)}: {clock.ElapsedMilliseconds} ms");

        var builds = database.ShipBuildList.Where(item => item.Ship.ShipType != ShipType.Drone).OrderBy(item => item.Id.Value).ToList();
        var ships = builds.Create(0, new System.Random(0), database).ToList();
        clock.Restart();
        for (var i = 0; i < ships.Count; ++i)
        {
            var fleet = FleetWebSerializer.SerializeFleetData(new[] { ships[i] });
            try
            {
                SetupHash.Compute(duelSetup.Build(Plan(fleet, fleet)));
            }
            catch (Exception e)
            {
                ++failures;
                report.AppendLine($"FAIL build {builds[i].Id.Value}: {e.GetType().Name}: {e.Message}");
            }
        }
        report.AppendLine($"Setup hash of {ships.Count} builds: {clock.ElapsedMilliseconds} ms");

        var random = new System.Random(1);
        var hostFleet = FleetWebSerializer.SerializeFleetData(ships.OrderBy(item => random.Next()).Take(12));
        var guestFleet = FleetWebSerializer.SerializeFleetData(ships.OrderBy(item => random.Next()).Take(12));
        report.AppendLine($"Fleet data: {hostFleet.Length} and {guestFleet.Length} bytes");

        var equal = Handshake(duelSetup.Build, databaseHash, hostFleet, guestFleet, "check", "check");
        if (equal.Host.State == HandshakeState.Ready && equal.Guest.State == HandshakeState.Ready &&
            equal.Host.SetupHashValue == equal.Guest.SetupHashValue)
            report.AppendLine($"Equal players: Ready, setup hash {equal.Host.SetupHashValue:X16}, " +
                $"{equal.Host.Setup.Roster.Count} slots, setup hash time {equal.HashMilliseconds} ms");
        else
        {
            ++failures;
            report.AppendLine($"FAIL equal players: host {Describe(equal.Host)}, guest {Describe(equal.Guest)}");
        }

        var versions = Handshake(duelSetup.Build, databaseHash, hostFleet, guestFleet, "check", "other");
        if (versions.Host.Rejection == HandshakeRejection.GameVersion && versions.Guest.Rejection == HandshakeRejection.GameVersion)
            report.AppendLine("Other game version: rejected on both players");
        else
        {
            ++failures;
            report.AppendLine($"FAIL other game version: host {Describe(versions.Host)}, guest {Describe(versions.Guest)}");
        }

        report.Insert(0, failures == 0 ? "PASS. " : $"{failures} FAILURES. ");
        if (failures == 0) Debug.Log(report); else Debug.LogError(report);
    }

    private static MatchPlan Plan(byte[] hostFleet, byte[] guestFleet) =>
        new MatchPlan(true, new BattleId(1), 42, hostFleet, guestFleet, 2, 3);

    private static string Describe(MatchHandshake handshake) =>
        $"{handshake.State} {handshake.Rejection}" + (handshake.SetupError != null ? $" ({handshake.SetupError.Message})" : string.Empty);

    private static Pair Handshake(Func<MatchPlan, BattleSetup> build, byte[] databaseHash, byte[] hostFleet, byte[] guestFleet,
        string hostVersion, string guestVersion)
    {
        var hostLink = new Loopback();
        var guestLink = new Loopback();
        hostLink.Other = guestLink;
        guestLink.Other = hostLink;
        var hashTime = new Stopwatch();
        BattleSetup Timed(MatchPlan plan)
        {
            var setup = build(plan);
            hashTime.Start();
            SetupHash.Compute(setup);
            hashTime.Stop();
            return setup;
        }
        var pair = new Pair
        {
            Host = MatchHandshake.Host(hostLink, new MatchIdentity(hostVersion, databaseHash, hostFleet), Timed, new BattleId(1), 42),
            Guest = MatchHandshake.Guest(guestLink, new MatchIdentity(guestVersion, databaseHash, guestFleet), Timed),
        };
        for (var now = 0.0; now < 30; now += 0.02)
        {
            pair.Host.Update(now);
            pair.Guest.Update(now);
            if (Settled(pair.Host) && Settled(pair.Guest)) break;
        }
        pair.HashMilliseconds = hashTime.ElapsedMilliseconds / 2;
        return pair;
    }

    private static bool Settled(MatchHandshake handshake) =>
        handshake.State == HandshakeState.Ready || handshake.State == HandshakeState.Rejected;

    private sealed class Pair
    {
        internal MatchHandshake Host, Guest;
        internal long HashMilliseconds;
    }

    // Delivers every datagram at once, in order.
    private sealed class Loopback : ITransport
    {
        internal Loopback Other;
        private readonly Queue<byte[]> _received = new Queue<byte[]>();

        public bool TrySend(ReadOnlySpan<byte> datagram)
        {
            Other._received.Enqueue(datagram.ToArray());
            return true;
        }

        public bool TryReceive(Span<byte> buffer, out int length)
        {
            length = 0;
            if (_received.Count == 0) return false;
            var datagram = _received.Dequeue();
            length = datagram.Length;
            datagram.AsSpan(0, Math.Min(length, buffer.Length)).CopyTo(buffer);
            return true;
        }
    }
}
