using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Combat.Integration.Core;
using CombatSimulation.Core;
using Constructor;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Resources;
using UnityEditor;
using UnityEngine;

// Maps every built-in ship build to a core ship (phase 4, step 3a) and reports:
// 1. builds that map, and builds with at least one working cannon or beam ("armed");
// 2. behaviour trees that at least one armed ship or drone names directly (AI coverage gate). Trees reached only
//    through SubTree nodes do not count: a battle need not run those branches (owner, 2026-10-29);
// 3. the inert reasons (D1) with counts, to order the work of steps 4 and 5.
// A build uses its CustomAI, else the legacy default: StarbaseAI for starbases, EnemyAI for other ships. A drone uses its
// bay's tree for its behaviour, else the drone build's CustomAI, else the default drone tree (ControllerFactory).
public static class CoreShipBuildCheck
{
    [MenuItem("Tools/Combat/Check Ship Builds")]
    public static void Run()
    {
        var database = UnityDatabaseFactory.Create();
        database.LoadDefault();
        var mapper = new CoreShipMapper(database, Resources.Load<ResourceLocator>("ResourceLocator"));
        var settings = database.CombatSettings;

        var failures = new StringBuilder();
        var reasons = new Dictionary<string, int>();
        var armedTrees = new HashSet<BehaviorTreeModel>();
        int builds = 0, mapped = 0, armed = 0;

        foreach (var build in database.ShipBuildList.OrderBy(item => item.Id.Value))
        {
            if (build.Ship.ShipType == ShipType.Drone)
                continue;

            ++builds;
            IShipSpecification specification;
            CoreShipMapping mapping;
            try
            {
                specification = new ShipBuilder(build).Build(database.ShipSettings);
                mapping = mapper.Map(specification, isDrone: false);
            }
            catch (Exception e)
            {
                failures.AppendLine($"build {build.Id.Value}: {e.Message}");
                continue;
            }

            ++mapped;
            CountReasons(mapping, reasons);
            if (IsArmed(mapping.Definition))
            {
                ++armed;
                var tree = build.CustomAI ?? (build.Ship.ShipType == ShipType.Starbase ? settings.StarbaseAI : settings.EnemyAI);
                if (tree != null) armedTrees.Add(tree);
            }

            // Bay devices follow the platforms, devices, clonning centers and replicator in device order.
            var bays = mapping.Definition.Devices.OfType<DroneBayDefinition>().ToList();
            var index = 0;
            foreach (var item in specification.DroneBays)
            {
                var drone = bays[index++].Drone;
                CountReasons(mapper.Mappings[drone], reasons);
                if (!IsArmed(drone))
                    continue;

                var tree = (item.Behaviour == DroneBehaviour.Defensive ? item.DroneBay.DefensiveDroneAI : item.DroneBay.OffensiveDroneAI)
                    ?? item.Drone.CustomAI;
                if (tree == null && !item.DroneBay.ImprovedAi)
                    tree = item.Behaviour == DroneBehaviour.Aggressive ? settings.OffensiveDroneAI : settings.DefensiveDroneAI;
                if (tree != null) armedTrees.Add(tree);
            }
        }

        var trees = database.BehaviorTreeList.Count();
        var report = new StringBuilder();
        report.AppendLine($"CoreShipBuildCheck: {builds} builds (drones excluded), {mapped} mapped, {builds - mapped} failed, {armed} armed.");
        report.AppendLine($"Trees used by an armed ship or drone: {armedTrees.Count} of {trees}; without: {trees - armedTrees.Count}.");
        report.AppendLine($"Armed trees: {string.Join(", ", armedTrees.Select(tree => tree.Id.Value).OrderBy(id => id))}");
        report.AppendLine("Inert devices by reason (ships and drones, counted per build):");
        foreach (var item in reasons.OrderByDescending(item => item.Value).ThenBy(item => item.Key))
            report.AppendLine($"  {item.Value,5}  {item.Key}");
        if (failures.Length > 0)
            report.Append("Failures:\n").Append(failures);

        if (failures.Length > 0) Debug.LogError(report.ToString());
        else Debug.Log(report.ToString());
    }

    private static bool IsArmed(ShipDefinition definition) =>
        definition.Devices.Any(device => device is CannonDefinition || device is BeamDefinition);

    private static void CountReasons(CoreShipMapping mapping, Dictionary<string, int> reasons)
    {
        foreach (var device in mapping.Devices)
        {
            if (!device.IsInert) continue;
            reasons.TryGetValue(device.InertReason, out var count);
            reasons[device.InertReason] = count + 1;
        }
    }
}
