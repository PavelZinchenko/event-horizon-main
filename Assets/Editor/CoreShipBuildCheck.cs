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
// 2. behaviour trees that an armed ship or drone reaches from its root tree, SubTree nodes included (AI coverage gate,
//    owner, 2026-10-31). Only 8 trees are roots, so the direct-name rule of 2026-10-29 could never pass. Live trees are
//    the ones any build, drone bay or combat setting reaches; the rest are dead and stay out of the count;
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

        var rootTrees = new List<BehaviorTreeModel>
        {
            settings.EnemyAI, settings.AutopilotAI, settings.CloneAI,
            settings.DefensiveDroneAI, settings.OffensiveDroneAI, settings.StarbaseAI,
        };
        rootTrees.AddRange(database.ShipBuildList.Select(build => build.CustomAI));
        rootTrees.AddRange(database.DroneBayList.SelectMany(bay => new[] { bay.Stats.DefensiveDroneAI, bay.Stats.OffensiveDroneAI }));

        var liveTrees = Reach(rootTrees);
        var coveredTrees = Reach(armedTrees);
        var uncovered = liveTrees.Where(tree => !coveredTrees.Contains(tree)).ToList();
        var dead = database.BehaviorTreeList.Where(tree => !liveTrees.Contains(tree)).ToList();

        var report = new StringBuilder();
        report.AppendLine($"CoreShipBuildCheck: {builds} builds (drones excluded), {mapped} mapped, {builds - mapped} failed, {armed} armed.");
        report.AppendLine($"Trees: {database.BehaviorTreeList.Count()}, live {liveTrees.Count}, dead {dead.Count}.");
        report.AppendLine($"Live trees reached by an armed ship or drone: {coveredTrees.Count} of {liveTrees.Count}; without: {uncovered.Count} (gate limit 10).");
        report.AppendLine($"Armed root trees: {TreeList(armedTrees)}");
        report.AppendLine($"Live trees without an armed ship or drone: {TreeList(uncovered)}");
        report.AppendLine($"Dead trees: {TreeList(dead)}");
        report.AppendLine("Inert devices by reason (ships and drones, counted per build):");
        foreach (var item in reasons.OrderByDescending(item => item.Value).ThenBy(item => item.Key))
            report.AppendLine($"  {item.Value,5}  {item.Key}");
        if (failures.Length > 0)
            report.Append("Failures:\n").Append(failures);

        if (failures.Length > 0) Debug.LogError(report.ToString());
        else Debug.Log(report.ToString());
    }

    // Trees reachable from the roots through SubTree nodes, roots included.
    private static HashSet<BehaviorTreeModel> Reach(IEnumerable<BehaviorTreeModel> roots)
    {
        var result = new HashSet<BehaviorTreeModel>();
        var trees = new Stack<BehaviorTreeModel>(roots.Where(tree => tree != null));
        var nodes = new Stack<BehaviorTreeNode>();
        while (trees.Count > 0)
        {
            var tree = trees.Pop();
            if (!result.Add(tree)) continue;

            nodes.Push(tree.RootNode);
            while (nodes.Count > 0)
            {
                var children = Children(nodes.Pop(), out var subTree);
                foreach (var child in children)
                    if (child != null) nodes.Push(child);
                if (subTree != null) trees.Push(subTree);
            }
        }

        return result;
    }

    private static IEnumerable<BehaviorTreeNode> Children(BehaviorTreeNode node, out BehaviorTreeModel subTree)
    {
        subTree = null;
        switch (node)
        {
            case BehaviorTreeNode_SubTree item: subTree = item.BehaviourTree; return Array.Empty<BehaviorTreeNode>();
            case BehaviorTreeNode_Selector item: return item.Nodes;
            case BehaviorTreeNode_Sequence item: return item.Nodes;
            case BehaviorTreeNode_Parallel item: return item.Nodes;
            case BehaviorTreeNode_RandomSelector item: return item.Nodes;
            case BehaviorTreeNode_ParallelSequence item: return item.Nodes;
            case BehaviorTreeNode_IfThenElse item: return item.Nodes;
            case BehaviorTreeNode_Invertor item: return new[] { item.Node };
            case BehaviorTreeNode_Cooldown item: return new[] { item.Node };
            case BehaviorTreeNode_Execute item: return new[] { item.Node };
            case BehaviorTreeNode_PreserveTarget item: return new[] { item.Node };
            default: return Array.Empty<BehaviorTreeNode>();
        }
    }

    private static string TreeList(IEnumerable<BehaviorTreeModel> trees) =>
        string.Join(", ", trees.Select(tree => tree.Id.Value).OrderBy(id => id));

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
