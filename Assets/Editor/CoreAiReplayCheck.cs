using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Combat.Integration.Core;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatSimulation.Runtime;
using Constructor;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using Services.Resources;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

// The M2 same-seed check (phase 4, step 7f): "same battle seed gives the same log twice". Every armed built-in build
// fights the next armed build, both on their behaviour trees through the scene's pilot provider at the hard AI level.
// Each fight runs twice with the same seed, the two runs interleaved, so shared static state would show. The log holds
// every event, a digest of the poses, velocities and resources of each tick, and the ShowMessage texts.
// Failures: exceptions and logs that differ. The report gives the first differing line of each pair.
public static class CoreAiReplayCheck
{
    [MenuItem("Tools/Combat/Check AI Replay")]
    public static void Run()
    {
        var database = UnityDatabaseFactory.Create();
        database.LoadDefault();
        var resources = Resources.Load<ResourceLocator>("ResourceLocator");

        var mapper = new CoreShipMapper(database, resources);
        var armed = new List<ShipBuild>();
        foreach (var build in database.ShipBuildList.Where(item => item.Ship.ShipType != ShipType.Drone).OrderBy(item => item.Id.Value))
        {
            var mapping = mapper.Map(new ShipBuilder(build).Build(database.ShipSettings), isDrone: false);
            if (mapping.Definition.Devices.Any(device => device is CannonDefinition || device is BeamDefinition))
                armed.Add(build);
        }

        var results = new List<Result>();
        var clock = Stopwatch.StartNew();
        try
        {
            for (var i = 0; i < armed.Count; ++i)
            {
                var first = armed[i];
                var second = armed[(i + 1) % armed.Count];
                if (EditorUtility.DisplayCancelableProgressBar("Check AI Replay", $"build {first.Id.Value} against {second.Id.Value}", (float)i / armed.Count))
                    break;

                var result = new Result(first.Id.Value, second.Id.Value);
                try
                {
                    // A new mapper per fight, as each scene battle has its own.
                    var fightMapper = new CoreShipMapper(database, resources);
                    var a = fightMapper.Map(new ShipBuilder(first).Build(database.ShipSettings), isDrone: false);
                    var b = fightMapper.Map(new ShipBuilder(second).Build(database.ShipSettings), isDrone: false);
                    Compare(database, fightMapper, a.Definition, b.Definition, (ulong)(result.First * 7919 + result.Second), result);
                }
                catch (Exception e)
                {
                    result.Exception = $"{e.GetType().Name}: {e.Message}";
                }

                results.Add(result);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log(Report(results, clock.Elapsed));
    }

    private static void Compare(IDatabase database, CoreShipMapper mapper, ShipDefinition first, ShipDefinition second, ulong seed,
        Result result)
    {
        var one = new Fight(database, mapper, first, second, seed);
        var two = new Fight(database, mapper, first, second, seed);
        while (!(one.IsCompleted && two.IsCompleted) && one.Tick < ReplayTicks)
        {
            if (!one.IsCompleted) one.Step();
            if (!two.IsCompleted) two.Step();
        }

        result.Ticks = one.Tick;
        result.Completed = one.IsCompleted;
        result.Lines = one.Log.Count;
        result.Texts = one.Texts;

        var count = Math.Min(one.Log.Count, two.Log.Count);
        for (var i = 0; i < count; ++i)
        {
            if (one.Log[i] == two.Log[i]) continue;
            result.Difference = $"line {i}: \"{one.Log[i]}\" against \"{two.Log[i]}\"";
            return;
        }

        if (one.Log.Count != two.Log.Count)
            result.Difference = $"{one.Log.Count} lines against {two.Log.Count}";
    }

    private static string Report(List<Result> results, TimeSpan elapsed)
    {
        var report = new StringBuilder();
        report.AppendLine($"CoreAiReplayCheck: {results.Count} fights, each run twice, in {elapsed.TotalSeconds:F0} s; " +
                          $"{results.Count(item => item.Completed)} reached a result, {results.Sum(item => item.Texts)} texts shown.");

        Section(report, "Exceptions", results.Where(item => item.Exception != null), item => item.Exception);
        Section(report, "Logs differ", results.Where(item => item.Exception == null && item.Difference != null), item => item.Difference);
        var lines = results.Where(item => item.Exception == null).Sum(item => (long)item.Lines);
        report.AppendLine($"Compared lines: {lines}");
        return report.ToString();
    }

    private static void Section(StringBuilder report, string title, IEnumerable<Result> items, Func<Result, string> detail)
    {
        var list = items.ToList();
        report.AppendLine($"{title}: {list.Count}");
        foreach (var item in list)
            report.AppendLine($"  builds {item.First} and {item.Second}: {detail(item)}");
    }

    private sealed class Fight
    {
        public Fight(IDatabase database, CoreShipMapper mapper, ShipDefinition first, ShipDefinition second, ulong seed)
        {
            var roster = new[]
            {
                new RosterSlotSetup(new RosterId(1), 0, first),
                new RosterSlotSetup(new RosterId(2), 1, second),
            };
            var setup = new BattleSetup(new BattleId(1), seed, roster, ArenaSize, ArenaSize);
            var trees = CorePilotProvider.CreateTrees(seed, new Torus(setup.Width, setup.Height), mapper, OnBroadcast);
            _runner = new BattleRunner(setup, new CorePilotProvider(database.CombatSettings, trees, mapper.Mappings,
                playerSide: -1, enemyAiLevel: HardAiLevel));
            _runner.Deploy(new RosterId(1), _runner.Tick);
            _runner.Deploy(new RosterId(2), _runner.Tick);
        }

        public readonly List<string> Log = new List<string>();
        public int Texts { get; private set; }
        public long Tick => _runner.Tick;
        public bool IsCompleted => _runner.IsCompleted;

        public void Step()
        {
            _runner.Step();
            var snapshot = _runner.Snapshot;
            Log.Add($"{snapshot.Tick} {Digest(snapshot):X16}");
            foreach (var item in snapshot.Events)
                Log.Add(item.ToString());
        }

        // Inside the step, while the pilots think: the texts go before the tick's digest line.
        private void OnBroadcast(EntityId ship, string text, ColorData color)
        {
            Log.Add($"text {ship} {text}");
            ++Texts;
        }

        private readonly BattleRunner _runner;
    }

    // Catches a divergence that produces no event (core ScriptedBattle.Digest): poses, velocities and resources as raw bits.
    private static ulong Digest(BattleSnapshot snapshot)
    {
        var hash = 14695981039346656037UL;
        foreach (var ship in snapshot.Ships)
        {
            Mix(ref hash, ship.Id.Value);
            Mix(ref hash, ship.Position);
            Mix(ref hash, ship.LinearVelocity);
            Mix(ref hash, ship.Angle);
            Mix(ref hash, ship.AngularVelocity);
            Mix(ref hash, ship.Armor.Value);
            Mix(ref hash, ship.Shield.Value);
            Mix(ref hash, ship.Energy.Value);
            Mix(ref hash, ship.Systems);
        }

        foreach (var bullet in snapshot.Bullets)
        {
            Mix(ref hash, bullet.Id.Value);
            Mix(ref hash, bullet.Position);
            Mix(ref hash, bullet.LinearVelocity);
            Mix(ref hash, bullet.Angle);
            Mix(ref hash, bullet.BeamEndPoint);
        }

        return hash;
    }

    private static void Mix(ref ulong hash, ulong value) => hash = (hash ^ value) * 1099511628211UL;
    private static void Mix(ref ulong hash, float value) => Mix(ref hash, (ulong)(uint)BitConverter.SingleToInt32Bits(value));

    private static void Mix(ref ulong hash, Vec2 value)
    {
        Mix(ref hash, value.X);
        Mix(ref hash, value.Y);
    }

    private sealed class Result
    {
        public Result(int first, int second)
        {
            First = first;
            Second = second;
        }

        public int First { get; }
        public int Second { get; }
        public long Ticks { get; set; }
        public bool Completed { get; set; }
        public int Lines { get; set; }
        public int Texts { get; set; }
        public string Difference { get; set; }
        public string Exception { get; set; }
    }

    private const float ArenaSize = 200f;
    private const int HardAiLevel = 100;

    // 60 s at 50 ticks per second.
    private const long ReplayTicks = 3000;
}
