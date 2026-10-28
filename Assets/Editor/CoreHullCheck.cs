using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Combat.Integration.Core;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using GameDatabase;
using Services.Resources;
using UnityEditor;
using UnityEngine;

// Builds the core hull of every built-in ship from Ship.Collider and from the sprite fallback (phase 4, step 2),
// adds it to a physics body of mass 1, and reports part count, inertia and hull radius. Lists every failure.
public static class CoreHullCheck
{
    [MenuItem("Tools/Combat/Check Ship Hulls")]
    public static void Run()
    {
        var database = UnityDatabaseFactory.Create();
        database.LoadDefault();
        var builder = new CoreHullBuilder(Resources.Load<ResourceLocator>("ResourceLocator"));

        var report = new StringBuilder();
        int checkedShips = 0, failures = 0, fallbacks = 0;
        foreach (var ship in database.ShipList.OrderBy(ship => ship.Id.Value))
        {
            ++checkedShips;
            var name = $"{ship.Id.Value} ({ship.ModelImage.Id}, scale {ship.ModelScale})";
            if (ship.Collider.Count == 0) ++fallbacks;

            var dataOk = TryMeasure(() => builder.Build(ship), out var data, out var dataError);
            var spriteOk = TryMeasure(() => builder.FromSprite(ship), out var sprite, out var spriteError);
            if (!dataOk || !spriteOk)
            {
                ++failures;
                report.AppendLine($"{name}: FAILED data: {dataError ?? data}; sprite: {spriteError ?? sprite}");
                continue;
            }

            report.AppendLine($"{name}: data {data}; sprite {sprite}");
        }

        var summary = $"CoreHullCheck: {checkedShips} ships, {failures} failed, {fallbacks} without Ship.Collider.\n{report}";
        if (failures > 0) Debug.LogError(summary);
        else Debug.Log(summary);
    }

    private static bool TryMeasure(Func<IReadOnlyList<HullShape>> build, out string result, out string error)
    {
        try
        {
            var hull = build();
            var definition = new ShipDefinition(hull, 1f, new EngineDefinition(1f, 1f, 1f, 1f), 1f,
                new ResourceDefinition(1f), new ResourceDefinition(1f));

            var world = new PhysicsWorld(new WorldSettings());
            var body = world.CreateBody(BodyDef.Create(Vec2.Zero, 1f));
            foreach (var part in definition.HullParts)
            {
                switch (part.Type)
                {
                    case HullPartType.Circle: world.AddCircle(body, part.Circle); break;
                    case HullPartType.Capsule: world.AddCapsule(body, part.Capsule); break;
                    default: world.AddPolygon(body, part.Polygon); break;
                }
            }

            var inertia = world.GetInertia(body);
            if (!(inertia > 0f) || float.IsInfinity(inertia))
                throw new InvalidOperationException($"inertia {inertia}");

            result = $"{definition.HullParts.Count} parts, inertia {inertia:0.###}, radius {definition.HullRadius:0.###}";
            error = null;
            return true;
        }
        catch (Exception e)
        {
            result = null;
            error = e.Message;
            return false;
        }
    }
}
