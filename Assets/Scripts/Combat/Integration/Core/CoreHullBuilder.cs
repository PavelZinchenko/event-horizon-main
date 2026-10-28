using System;
using System.Collections.Generic;
using Collider2DOptimization;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Resources;
using UnityEngine;

namespace Combat.Integration.Core
{
    // Builds a core hull from Ship.Collider, scaled by ModelScale (phase 4, D5). A ship whose field has no valid
    // shape falls back to the legacy rule of ShipFactory.CreateShipObject: SizeClass.Undefined gives a circle fitted
    // to the sprite; any other size class gives the sprite's physics shape, reduced with ColliderTolerance.
    // Main thread only: the fallback reads the sprite. The core receives shapes only.
    public sealed class CoreHullBuilder
    {
        public CoreHullBuilder(IResourceLocator resourceLocator)
        {
            _resourceLocator = resourceLocator;
        }

        public IReadOnlyList<HullShape> Build(Ship ship)
        {
            var scale = ship.ModelScale;
            var shapes = new List<HullShape>();
            for (var i = 0; i < ship.Collider.Count; ++i)
            {
                var shape = ship.Collider[i];
                var points = new Vec2[shape.Points.Count];
                for (var j = 0; j < points.Length; ++j)
                    points[j] = new Vec2(shape.Points[j].x * scale, shape.Points[j].y * scale);

                if (TryCreate(ship, points, shape.Radius * scale, out var hullShape))
                    shapes.Add(hullShape);
            }

            return shapes.Count > 0 ? shapes : FromSprite(ship);
        }

        // The legacy collider of a ship without a prefab.
        public IReadOnlyList<HullShape> FromSprite(Ship ship)
        {
            var scale = ship.ModelScale;
            var sprite = _resourceLocator.GetSprite(ship.ModelImage);
            if (sprite == null)
                return new[] { HullShape.Circle(Vec2.Zero, 0.5f * scale) };

            var shapes = new List<HullShape>();
            if (ship.SizeClass != SizeClass.Undefined)
            {
                var paths = new List<Vec2[]>();
                var path = new List<Vector2>();
                for (var i = 0; i < sprite.GetPhysicsShapeCount(); ++i)
                {
                    sprite.GetPhysicsShape(i, path);
                    var reduced = ShapeOptimizationHelper.DouglasPeuckerReduction(path, ship.ColliderTolerance);
                    if (reduced.Count < 3) continue;

                    var points = new Vec2[reduced.Count];
                    for (var j = 0; j < points.Length; ++j)
                        points[j] = new Vec2(reduced[j].x * scale, reduced[j].y * scale);
                    paths.Add(points);
                }

                for (var i = 0; i < paths.Count; ++i)
                    if (!ConvexDecomposition.IsHole(paths, i) && TryCreate(ship, paths[i], 0f, out var outline))
                        shapes.Add(outline);
            }

            if (shapes.Count == 0)
            {
                // A circle around the sprite bounds, as CircleCollider2D sizes itself on a sprite.
                var bounds = sprite.bounds;
                var center = new Vec2(bounds.center.x * scale, bounds.center.y * scale);
                shapes.Add(HullShape.Circle(center, Mathf.Max(bounds.extents.x, bounds.extents.y) * scale));
            }

            return shapes;
        }

        private bool TryCreate(Ship ship, IReadOnlyList<Vec2> points, float radius, out HullShape shape)
        {
            try
            {
                shape = new HullShape(points, radius);
                return true;
            }
            catch (ArgumentException e)
            {
                if (_reported.Add(ship.Id.Value))
                    Debug.LogWarning($"CoreHullBuilder: ship {ship.Id.Value} ({ship.ModelImage.Id}) has an invalid collider shape, skipped: {e.Message}");
                shape = null;
                return false;
            }
        }

        private readonly IResourceLocator _resourceLocator;
        private readonly HashSet<int> _reported = new HashSet<int>();
    }
}
