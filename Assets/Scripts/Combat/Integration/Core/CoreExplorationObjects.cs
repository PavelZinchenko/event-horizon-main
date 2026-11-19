using System;
using System.Collections.Generic;
using CombatSimulation.Modes;
using CombatSimulation.Physics;
using CombatView;
using CombatView.Views;
using Game.Exploration;
using Services.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;
using EnvironmentObjectType = Game.Exploration.ExplorationData.EnvironmentObjectType;
using Object = UnityEngine.Object;

namespace Combat.Integration.Core
{
    // The still objects of an exploration level (exploration plan, step 7, E7–E9): presentation only, outside the core.
    // The prefabs (Resources/CombatView/Exploration) are the legacy Combat/Exploration ones without their collider and
    // body. Each object is drawn as CorePlanet draws the planet: the image nearest to the anchor. The legacy reference is
    // ExplorationSceneManager and SpaceObjectFactory.CreatePlanetary*.
    public sealed class CoreExplorationObjects : IDisposable
    {
        // E7: the dockable objectives at their layout positions, except those completed before the battle (legacy skips
        // them); a view stays after its completion, as legacy. E9: the legacy craters. E8: a cloud for each zone.
        public static CoreExplorationObjects Create(CoreExplorationSetup setup, ExplorationData exploration,
            IResourceLocator resourceLocator, UnityEngine.SceneManagement.Scene scene)
        {
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            if (exploration == null) throw new ArgumentNullException(nameof(exploration));
            if (resourceLocator == null) throw new ArgumentNullException(nameof(resourceLocator));

            var objects = new CoreExplorationObjects(setup.Layout.Torus, scene);
            objects.AddObjectives(setup.Layout, exploration, resourceLocator);
            objects.AddCraters(setup.Layout, exploration);
            objects.AddGasClouds(setup.Layout, exploration);
            Debug.Log($"CoreExplorationObjects: {objects._items.Count} views");
            return objects;
        }

        private CoreExplorationObjects(Torus torus, UnityEngine.SceneManagement.Scene scene)
        {
            _torus = torus;
            _scene = scene;
        }

        // Once per frame after the renderer, as CorePlanet.
        public void Render(Vector2 anchor, float deltaTime)
        {
            foreach (var item in _items)
            {
                if (!item.Root)
                    continue;

                item.Rotation += item.AngularVelocity * deltaTime;
                item.Root.transform.SetPositionAndRotation(Placement.NearestImage(_torus, anchor, item.Position),
                    Quaternion.Euler(0f, 0f, Mathf.Repeat(item.Rotation, 360f)));
                item.View.UpdateView(deltaTime);
            }
        }

        public void Dispose()
        {
            // The scene unload can destroy the roots first.
            foreach (var item in _items)
                if (item.Root)
                    Object.Destroy(item.Root);
            _items.Clear();
        }

        // Legacy CreateObjectives: the prefab, rotation, spin and colour of each type. The layout gives the position and
        // the angle; CoreExplorationSetup.ObjectiveSize gives the size, as for the docking radius.
        private void AddObjectives(ExplorationLayout layout, ExplorationData exploration, IResourceLocator resourceLocator)
        {
            var solid = exploration.HasSolidGround;
            var defaultColor = Color.Lerp(exploration.PlanetColor, Color.white, 0.5f);
            var groundColor = Color.Lerp(exploration.PlanetColor, Color.black, 0.1f);
            foreach (var placed in layout.Objectives)
            {
                if (placed.Kind != ExplorationObjectiveKind.Dockable)
                    continue;

                var objective = exploration.Objectives[placed.Id];
                if (exploration.IsCompleted(objective))
                    continue;

                var size = CoreExplorationSetup.ObjectiveSize(exploration, objective);
                var rotation = placed.Angle * Mathf.Rad2Deg;
                switch (objective.Type)
                {
                    case ObjectiveType.Container:
                        if (solid)
                            Add(ContainerPrefab, placed.Position, rotation, size, defaultColor);
                        else
                            Add(BoxPrefab, placed.Position, 0f, size, defaultColor);
                        break;
                    case ObjectiveType.Meteorite:
                        Add(solid ? RockPrefab : AsteroidPrefab, placed.Position, 0f, size, exploration.PlanetColor);
                        break;
                    case ObjectiveType.ShipWreck:
                        var wreck = Add(ShipWreckPrefab, placed.Position, rotation, size, defaultColor);
                        if (wreck == null)
                            continue;
                        var renderer = wreck.Root.GetComponentInChildren<SpriteRenderer>(true);
                        if (renderer)
                            renderer.sprite = resourceLocator.GetSprite(exploration.GetShipWreck(objective.Seed).ModelImage);
                        // Legacy: a floating wreck spins up to 0.1°/s, from the scene's shared random.
                        if (!solid)
                            wreck.AngularVelocity =
                                NextSigned(new System.Random(unchecked(objective.Seed + WreckSpinSalt))) * 0.1f;
                        break;
                    case ObjectiveType.Minerals:
                    case ObjectiveType.MineralsRare:
                        if (solid)
                            Add(objective.Type == ObjectiveType.Minerals ? VolcanoPrefab : Volcano2Prefab, placed.Position,
                                rotation, size, groundColor);
                        else
                            Add(VortexPrefab, placed.Position, 0f, size, exploration.GetGasCloudColor(objective.Id));
                        break;
                    default:
                        Debug.LogWarning($"CoreExplorationObjects: objective {objective.Id} ({objective.Type}) has no view");
                        continue;
                }
            }
        }

        // E9, legacy CreateEnvironment: the craters at the legacy positions, sizes and rotations. Legacy starts the player
        // at (0, 0), the core at the layout's start, so each position moves by the start.
        private void AddCraters(ExplorationLayout layout, ExplorationData exploration)
        {
            var color = Color.Lerp(exploration.PlanetColor, Color.black, 0.1f);
            foreach (var item in CoreExplorationSetup.LegacyEnvironment(exploration))
            {
                string prefab;
                switch (item.Type)
                {
                    case EnvironmentObjectType.SmallCrater: prefab = CraterPrefab; break;
                    case EnvironmentObjectType.BigCrater: prefab = Crater2Prefab; break;
                    default: continue;
                }

                var position = _torus.Wrap(layout.Start + new Vec2(item.Position.x, item.Position.y));
                Add(prefab, position, item.Rotation, item.Size, color);
            }
        }

        // E8: a cloud for each zone, at its layout position. Legacy body size: 1.5 × (5 to 14), with a collider radius of
        // 0.5 at scale 1, so the scale is twice the zone radius. Legacy rotation: twice the drawn size, in degrees; spin:
        // up to 0.1°/s. Here both come from the zone's index, presentation only.
        private void AddGasClouds(ExplorationLayout layout, ExplorationData exploration)
        {
            var zones = layout.Zones;
            for (var i = 0; i < zones.Count; ++i)
            {
                var zone = zones[i];
                var random = new System.Random(unchecked(exploration.Seed + GasCloudSalt + i));
                var item = Add(GasCloudPrefab, zone.Position, random.Next(360), 2f * zone.Radius,
                    exploration.GetGasCloudColor(i));
                if (item != null)
                    item.AngularVelocity = NextSigned(random) * 0.1f;
            }
        }

        // Null without the prefab.
        private Item Add(string path, Vec2 position, float rotation, float size, Color color)
        {
            var prefab = Load(path);
            if (!prefab)
                return null;

            var root = Object.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(root, _scene);
            root.transform.localScale = Vector3.one * size;

            var view = root.GetComponent<IView>();
            view.Color = color;

            var item = new Item(root, view, position, rotation);
            _items.Add(item);
            return item;
        }

        private GameObject Load(string path)
        {
            if (_prefabs.TryGetValue(path, out var prefab))
                return prefab;

            prefab = Resources.Load<GameObject>(path);
            if (!prefab)
                Debug.LogWarning($"CoreExplorationObjects: prefab {path} not found; its objects are not drawn");
            _prefabs.Add(path, prefab);
            return prefab;
        }

        private static float NextSigned(System.Random random) => 2f * (float)random.NextDouble() - 1f;

        private sealed class Item
        {
            public Item(GameObject root, IView view, Vec2 position, float rotation)
            {
                Root = root;
                View = view;
                Position = position;
                Rotation = rotation;
            }

            public readonly GameObject Root;
            public readonly IView View;
            public readonly Vec2 Position;

            // Degrees, and degrees per second.
            public float Rotation;
            public float AngularVelocity;
        }

        private const string PrefabFolder = "CombatView/Exploration/";
        private const string ContainerPrefab = PrefabFolder + "Container";
        private const string BoxPrefab = PrefabFolder + "Box";
        private const string RockPrefab = PrefabFolder + "Rock";
        private const string AsteroidPrefab = PrefabFolder + "Asteroid";
        private const string ShipWreckPrefab = PrefabFolder + "ShipWreck";
        private const string VolcanoPrefab = PrefabFolder + "Volcano";
        private const string Volcano2Prefab = PrefabFolder + "Volcano2";
        private const string VortexPrefab = PrefabFolder + "Vortex";
        private const string CraterPrefab = PrefabFolder + "Crater";
        private const string Crater2Prefab = PrefabFolder + "Crater2";
        private const string GasCloudPrefab = PrefabFolder + "GasCloud";

        private const int WreckSpinSalt = 0x5752434B;
        private const int GasCloudSalt = 0x47415343;

        private readonly Torus _torus;
        private readonly UnityEngine.SceneManagement.Scene _scene;
        private readonly List<Item> _items = new List<Item>();
        private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
    }
}
