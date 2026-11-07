using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Combat.Integration.Core;
using Constructor;
using Constructor.Component;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using UnityEngine;
using Component = GameDatabase.DataModel.Component;

// The phase 5 coverage report (step 1, TODO/battle_simulator_phase5.md): one row per built-in component, ship device,
// satellite build and ship. A row is supported, inert (pending) or deferred. A row lists every reason its parts give;
// a weapon gives only its first reason, as in the mapper. A reason is deferred when DeferredListPath names it, so the
// tool and the release notes read one list. The phase completes when no row is inert.
// Rows are records, not builds: a component maps alone, with the modifiers of its own data and no ship bonuses.
// Ship stats the core does not read (finding 5) are reasons of the components and ships that carry them.
// EngineEnergyConsumption has no row: legacy reads it only for the Accelerator energy cost, so the Accelerator covers it.
public static class CoreCoverageReport
{
    public const string DeferredListPath = "Assets/Editor/CoreCoverageDeferred.json";
    public const string ReportPath = "Logs/CombatCoverage.md";

    private const string ArmorRepairReason = "stat armor repair";
    private const string RammingDamageReason = "stat flat ramming damage";
    private const string DroneRangeReason = "stat drone range";

    // Writes the report file and adds its summary to report. Returns false for errors: components that fail to build
    // and deferred entries that no row uses (a renamed reason).
    public static bool Write(IDatabase database, StringBuilder report)
    {
        var deferred = LoadDeferred(report);
        var errors = new List<string>();
        var sections = new[]
        {
            ("Components", Components(database, errors)),
            ("Ship devices (Ship.Features.BuiltinDevices)", BuiltinDevices(database)),
            ("Satellite builds", SatelliteBuilds(database, errors)),
            ("Ships", Ships(database)),
        };
        var rows = sections.SelectMany(item => item.Item2).ToList();

        var reasons = new SortedDictionary<string, ReasonInfo>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            foreach (var reason in row.Reasons)
            {
                if (!reasons.TryGetValue(reason, out var info))
                    reasons.Add(reason, info = new ReasonInfo());
                ++info.Rows;
                info.ShipBuilds.UnionWith(row.ShipBuilds);
            }
        }

        var stale = deferred.Keys.Where(reason => !reasons.ContainsKey(reason)).OrderBy(reason => reason).ToList();
        errors.AddRange(stale.Select(reason => $"deferred reason \"{reason}\" is not used by any row"));

        int supported = 0, inert = 0, deferredRows = 0;
        foreach (var row in rows)
        {
            switch (Status(row, deferred))
            {
                case "supported": ++supported; break;
                case "deferred": ++deferredRows; break;
                default: ++inert; break;
            }
        }

        var file = new StringBuilder();
        file.AppendLine("# Combat core coverage");
        file.AppendLine();
        file.AppendLine($"Written by *Tools/Combat/Check Ship Builds* on {DateTime.Now:yyyy-MM-dd HH:mm}. Deferred list: `{DeferredListPath}`.");
        file.AppendLine($"Completion check: {(inert == 0 ? "pass" : "fail")}. Rows: {rows.Count}; supported {supported}, inert {inert}, deferred {deferredRows}.");
        file.AppendLine("Ship builds exclude drones; a drone build counts in the drone column.");
        foreach (var error in errors)
            file.AppendLine($"- Error: {error}");
        file.AppendLine();

        file.AppendLine("## Reasons");
        file.AppendLine();
        file.AppendLine("| Reason | Status | Rows | Ship builds | Decision |");
        file.AppendLine("| --- | --- | --- | --- | --- |");
        foreach (var item in reasons.OrderByDescending(item => item.Value.ShipBuilds.Count).ThenBy(item => item.Key, StringComparer.Ordinal))
        {
            var decision = deferred.TryGetValue(item.Key, out var entry) ? Cell(entry.decision) : string.Empty;
            file.AppendLine($"| {Cell(item.Key)} | {(entry != null ? "deferred" : "inert")} | {item.Value.Rows} | {item.Value.ShipBuilds.Count} | {decision} |");
        }

        foreach (var (title, items) in sections)
        {
            file.AppendLine();
            file.AppendLine($"## {title}");
            file.AppendLine();
            file.AppendLine("| ID | Name | Status | Reasons | Ship builds | Drone builds | Satellite builds |");
            file.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (var row in items)
                file.AppendLine($"| {row.Id} | {Cell(row.Name)} | {Status(row, deferred)} | {Cell(string.Join(", ", row.Reasons))} | " +
                    $"{row.ShipBuilds.Count} | {row.DroneBuilds.Count} | {row.SatelliteBuilds.Count} |");
        }

        File.WriteAllText(ReportPath, file.ToString());

        report.AppendLine($"Coverage ({ReportPath}): {rows.Count} rows, supported {supported}, inert {inert}, deferred {deferredRows}; " +
            $"completion check {(inert == 0 ? "passes" : "fails")}.");
        foreach (var error in errors)
            report.AppendLine($"  coverage error: {error}");
        return errors.Count == 0;
    }

    private static List<Row> Components(IDatabase database, List<string> errors)
    {
        var rows = new SortedDictionary<int, Row>();
        foreach (var component in database.ComponentList.OrderBy(item => item.Id.Value))
        {
            var row = new Row(component.Id.Value, component.Name);
            rows.Add(row.Id, row);
            try
            {
                AddComponentReasons(new ComponentInfo(component).CreateComponent(1), component, database.ShipSettings, row.Reasons);
            }
            catch (Exception e)
            {
                errors.Add($"component {component.Id.Value}: {e.Message}");
                row.Reasons.Add("build error");
            }
        }

        foreach (var build in database.ShipBuildList)
            foreach (var id in build.Components.Select(item => item.Component.Id.Value).Distinct())
                if (rows.TryGetValue(id, out var row))
                    BuildsOf(row, build).Add(build.Id.Value);

        foreach (var build in database.SatelliteBuildList)
            foreach (var id in build.Components.Select(item => item.Component.Id.Value).Distinct())
                if (rows.TryGetValue(id, out var row))
                    row.SatelliteBuilds.Add(build.Id.Value);

        return rows.Values.ToList();
    }

    // Weapons, obsolete weapons, devices and drone bays as the mapper sees them, then the stats.
    private static void AddComponentReasons(IComponent component, Component data, ShipSettings settings, SortedSet<string> reasons)
    {
        foreach (var weapon in component.Weapons)
        {
            CoreWeaponMapper.Map(new ComponentWeapon(weapon), 0, 1f, out var reason);
            AddReason(reasons, reason);
        }

        if (component.WeaponsObsolete.Any())
            reasons.Add(CoreShipMapper.ObsoleteAmmunitionReason);
        foreach (var device in component.Devices)
            AddReason(reasons, CoreShipMapper.DeviceInertReason(device.DeviceClass));

        // The mapper adds the replicator to every ship with drone bays that builds drones (ShipStatsCalculator.DroneBuildSpeed).
        var stats = component.GetStats();
        if ((component.DroneBays.Any() && settings.BaseDroneReconstructionSpeed > 0) || stats.DroneReconstructionSpeed > 0)
            reasons.Add(CoreShipMapper.DroneReplicatorReason);

        if (stats.ArmorRepairRate != 0f || stats.ArmorRepairCooldownMultiplier.HasValue)
            reasons.Add(ArmorRepairReason);
        if (stats.RammingDamage != 0f)
            reasons.Add(RammingDamageReason);
        if (stats.DroneRangeMultiplier.HasValue)
            reasons.Add(DroneRangeReason);
    }

    private static List<Row> BuiltinDevices(IDatabase database)
    {
        var rows = new SortedDictionary<int, Row>();
        foreach (var build in database.ShipBuildList)
        {
            foreach (var device in build.Ship.Features.BuiltinDevices)
            {
                if (device == null) continue;
                if (!rows.TryGetValue(device.Id.Value, out var row))
                {
                    var name = string.IsNullOrEmpty(device.Stats.Name) ? device.Stats.DeviceClass.ToString() : device.Stats.Name;
                    rows.Add(device.Id.Value, row = new Row(device.Id.Value, name));
                    AddReason(row.Reasons, CoreShipMapper.DeviceInertReason(device.Stats.DeviceClass));
                }

                BuildsOf(row, build).Add(build.Id.Value);
            }
        }

        return rows.Values.ToList();
    }

    // The satellite's weapons map as ship weapons on its barrel (phase 5, step 12c). Its devices and stats go to the ship,
    // so its components give their own reasons.
    private static List<Row> SatelliteBuilds(IDatabase database, List<string> errors)
    {
        var rows = new SortedDictionary<int, Row>();
        foreach (var build in database.SatelliteBuildList)
        {
            var row = new Row(build.Id.Value, build.Satellite.Name);
            rows.Add(row.Id, row);

            foreach (var item in build.Components)
            {
                try
                {
                    var component = new ComponentInfo(item.Component).CreateComponent(1);
                    AddComponentReasons(component, item.Component, database.ShipSettings, row.Reasons);
                }
                catch (Exception e)
                {
                    errors.Add($"satellite build {build.Id.Value}, component {item.Component.Id.Value}: {e.Message}");
                    row.Reasons.Add("build error");
                }
            }
        }

        foreach (var build in database.ShipBuildList)
        {
            foreach (var satellite in new[] { build.LeftSatelliteBuild, build.RightSatelliteBuild }.Where(item => item != null).Distinct())
                if (rows.TryGetValue(satellite.Id.Value, out var row))
                    BuildsOf(row, build).Add(build.Id.Value);
        }

        return rows.Values.ToList();
    }

    // Ship features: hull barrels are supported; armor regeneration waits for step 13.
    private static List<Row> Ships(IDatabase database)
    {
        var rows = new SortedDictionary<int, Row>();
        foreach (var ship in database.ShipList)
        {
            var row = new Row(ship.Id.Value, ship.Name);
            rows.Add(row.Id, row);
            if (ship.Features.Regeneration)
                row.Reasons.Add(ArmorRepairReason);
        }

        foreach (var build in database.ShipBuildList)
            if (rows.TryGetValue(build.Ship.Id.Value, out var row))
                BuildsOf(row, build).Add(build.Id.Value);

        return rows.Values.ToList();
    }

    private static HashSet<int> BuildsOf(Row row, ShipBuild build) =>
        build.Ship.ShipType == ShipType.Drone ? row.DroneBuilds : row.ShipBuilds;

    private static void AddReason(SortedSet<string> reasons, string reason)
    {
        if (reason != null)
            reasons.Add(reason);
    }

    private static string Status(Row row, Dictionary<string, DeferredItem> deferred)
    {
        if (row.Reasons.Count == 0) return "supported";
        return row.Reasons.All(deferred.ContainsKey) ? "deferred" : "inert";
    }

    private static string Cell(string text) => string.IsNullOrEmpty(text) ? string.Empty : text.Replace("|", "/").Replace("\n", " ");

    private static Dictionary<string, DeferredItem> LoadDeferred(StringBuilder report)
    {
        var result = new Dictionary<string, DeferredItem>(StringComparer.Ordinal);
        if (!File.Exists(DeferredListPath))
        {
            report.AppendLine($"Coverage: no deferred list at {DeferredListPath}; nothing is deferred.");
            return result;
        }

        var list = JsonUtility.FromJson<DeferredList>(File.ReadAllText(DeferredListPath));
        foreach (var item in list?.items ?? Array.Empty<DeferredItem>())
            if (!string.IsNullOrEmpty(item.reason))
                result[item.reason] = item;

        return result;
    }

    // The deferred list file: each entry names one inert reason exactly as the report prints it.
    [Serializable]
    private sealed class DeferredList
    {
        public DeferredItem[] items;
    }

    [Serializable]
    private sealed class DeferredItem
    {
        public string reason;
        public string decision;
        public string note;
    }

    private sealed class ReasonInfo
    {
        public int Rows;
        public readonly HashSet<int> ShipBuilds = new HashSet<int>();
    }

    private sealed class Row
    {
        public Row(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public readonly int Id;
        public readonly string Name;
        public readonly SortedSet<string> Reasons = new SortedSet<string>(StringComparer.Ordinal);
        public readonly HashSet<int> ShipBuilds = new HashSet<int>();
        public readonly HashSet<int> DroneBuilds = new HashSet<int>();
        public readonly HashSet<int> SatelliteBuilds = new HashSet<int>();
    }

    private sealed class ComponentWeapon : IWeaponData
    {
        public ComponentWeapon(WeaponData data)
        {
            Weapon = data.Weapon;
            Ammunition = data.Ammunition;
            Stats = data.StatModifier;
        }

        public Weapon Weapon { get; }
        public Ammunition Ammunition { get; }
        public WeaponStatModifier Stats { get; }
        public int KeyBinding => 0;
    }
}
