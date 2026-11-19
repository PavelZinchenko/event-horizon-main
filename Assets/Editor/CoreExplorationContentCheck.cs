using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Combat.Integration.Core;
using CombatSimulation.Modes;
using Constructor;
using Game.Exploration;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;
using Services.Resources;
using UnityEditor;
using UnityEngine;

// Builds Main's exploration level on the core for a sample of planets of each type and level (exploration plan,
// step 6), maps every spawner's ship and reports the inert devices by reason and every exception. The player flies
// build 34; the planets have no session, so nothing is completed.
public static class CoreExplorationContentCheck
{
    [MenuItem("Tools/Combat/Check Exploration Content")]
    public static void Run()
    {
        var database = UnityDatabaseFactory.Create();
        database.LoadDefault();
        var mapper = new CoreShipMapper(database, Resources.Load<ResourceLocator>("ResourceLocator"));
        var player = new ShipBuilder(database.GetShipBuild(new ItemId<ShipBuild>(PlayerBuild))).Build(database.ShipSettings);

        var failures = new StringBuilder();
        var reasons = new Dictionary<string, int>();
        var roles = new Dictionary<ExplorationShipRole, int>();
        int planets = 0, spawners = 0, zones = 0;

        foreach (var type in PlanetTypes)
        foreach (var level in Levels)
        {
            ++planets;
            var faction = type == PlanetType.Infected ? database.ExplorationSettings.InfectedPlanetFaction : Faction.Empty;
            var seed = 1000 * (int)type + level;
            var exploration = new ExplorationData(database, new Planet(type, level, seed, faction, MaxObjectives, Color.gray));
            var name = $"{type} level {level} seed {seed}";

            CoreExplorationSetup setup;
            try
            {
                setup = CoreExplorationSetup.Create(exploration, database, mapper, player, Array.Empty<int>(), 1);
            }
            catch (Exception e)
            {
                failures.AppendLine($"{name}: setup: {e.Message}");
                continue;
            }

            zones += setup.Layout.Zones.Count;
            foreach (var spawner in setup.Layout.Spawners)
            {
                ++spawners;
                roles.TryGetValue(spawner.Role, out var count);
                roles[spawner.Role] = count + 1;
                try
                {
                    var mapping = setup.Content.Map(spawner.Role, spawner.ContentKey, spawner.Ordinal);
                    foreach (var device in mapping.Devices.Where(device => device.IsInert))
                    {
                        reasons.TryGetValue(device.InertReason, out var inert);
                        reasons[device.InertReason] = inert + 1;
                    }
                }
                catch (Exception e)
                {
                    failures.AppendLine($"{name}: {spawner.Role} key {spawner.ContentKey} ordinal {spawner.Ordinal}: {e.Message}");
                }
            }
        }

        var report = new StringBuilder();
        report.AppendLine($"CoreExplorationContentCheck: {planets} planets, {spawners} spawners, {zones} gas clouds");
        report.AppendLine("Spawners by role: " + string.Join(", ", roles.OrderBy(item => item.Key).Select(item => $"{item.Key} {item.Value}")));
        report.AppendLine($"Inert devices by reason (each spawner counts): {reasons.Values.Sum()}");
        foreach (var item in reasons.OrderByDescending(item => item.Value))
            report.AppendLine($"  {item.Value,5}  {item.Key}");
        report.AppendLine($"Exceptions: {failures.ToString().Count(c => c == '\n')}");
        report.Append(failures);

        if (failures.Length > 0) Debug.LogError(report.ToString());
        else Debug.Log(report.ToString());
    }

    // Asteroid planets have no exploration (ExplorationData.Objectives). Terran outposts need a level above 10.
    private static readonly PlanetType[] PlanetTypes = { PlanetType.Gas, PlanetType.Barren, PlanetType.Terran, PlanetType.Infected };
    private static readonly int[] Levels = { 1, 15, 50, 100, 300, 1000 };

    // Legacy Planet: 8 to 15 objectives; the most gives the most outposts and hives.
    private const int MaxObjectives = 15;
    private const int PlayerBuild = 34;
}
