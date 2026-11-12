using System;
using System.Collections.Generic;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using CombatView;
using CombatView.Views;
using UnityEngine;
using UnityEngine.SceneManagement;
using CoreEntityId = CombatSimulation.Core.EntityId;
using Object = UnityEngine.Object;

namespace Combat.Integration.Core
{
    // Phase 5, step 14d (H10). The adapter draws the legacy asteroid layout from the battle seed (owner question 3), and
    // this class draws the core asteroids. Phase 5b, step 8: the new prefab (Resources/CombatView/Objects/Asteroid), the
    // legacy one without its body, collider and physics. Its views build the rock mesh in Awake; its MeshView spins the
    // mesh on the effect random source, presentation only.
    public sealed class CoreAsteroids : IDisposable
    {
        // Legacy CombatManager.Initialize.
        public const int Count = 10;

        // Legacy CombatManager.Initialize and FindFreePlace, in its order per asteroid: size, position, velocity. Legacy
        // creates the asteroids before any ship, so FindFreePlace takes its first candidate: whole numbers around (0, 0),
        // which wrap to a uniform point of the torus. powerMultiplier: Experience.LevelToPowerMultiplier of the enemy level.
        public static IReadOnlyList<AsteroidSetup> Layout(ulong seed, Torus torus, float powerMultiplier)
        {
            var random = new System.Random(unchecked((int)(uint)seed ^ Salt));
            var halfWidth = (int)torus.Width / 2;
            var halfHeight = (int)torus.Height / 2;
            var asteroids = new AsteroidSetup[Count];
            for (var i = 0; i < Count; ++i)
            {
                var size = MinSize + (MaxSize - MinSize) * (float)random.NextDouble();
                var position = torus.Wrap(new Vec2(random.Next(-halfWidth, halfWidth), random.Next(-halfHeight, halfHeight)));
                var velocity = InsideUnitCircle(random) * (MaxSpeed / size);
                asteroids[i] = new AsteroidSetup(position, velocity, size, size * size * MassFactor,
                    size * size * HitPointFactor * powerMultiplier, powerMultiplier);
            }

            return asteroids;
        }

        // Null without the prefab.
        public static CoreAsteroids Create(Torus torus, UnityEngine.SceneManagement.Scene scene)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            if (prefab)
                return new CoreAsteroids(prefab, torus, scene);

            Debug.LogWarning($"CoreAsteroids: prefab {PrefabPath} not found; asteroids are not drawn");
            return null;
        }

        private CoreAsteroids(GameObject prefab, Torus torus, UnityEngine.SceneManagement.Scene scene)
        {
            _prefab = prefab;
            _torus = torus;
            _scene = scene;
        }

        // After every step. A view starts at the first snapshot that lists its asteroid; the snapshot span is valid only
        // until the next step.
        public void Capture(BattleSnapshot snapshot)
        {
            foreach (var asteroid in snapshot.Asteroids)
            {
                if (!_views.TryGetValue(asteroid.Id, out var view))
                {
                    view = CreateView(asteroid.Size);
                    view.Pose.Reset(asteroid.Position, asteroid.Angle);
                    _views.Add(asteroid.Id, view);
                    continue;
                }

                view.Pose.Push(asteroid.Position, asteroid.Angle);
            }
        }

        // Once per frame after the renderer: the interpolated pose at the image nearest to the anchor, as every other view.
        public void Render(float alpha, Vector2 anchor, float deltaTime)
        {
            foreach (var view in _views.Values)
            {
                if (!view.Root)
                    continue;

                var position = Placement.NearestImage(_torus, anchor, view.Pose.SamplePosition(alpha, _torus));
                view.Root.transform.SetPositionAndRotation(position,
                    Quaternion.Euler(0f, 0f, view.Pose.SampleAngle(alpha) * Mathf.Rad2Deg));
                view.View.UpdateView(deltaTime);
            }
        }

        // The asteroid died (BattleEventKind.AsteroidRemoval). Size: its diameter, for the death glow.
        public bool Remove(CoreEntityId id, out float size)
        {
            size = 0f;
            if (!_views.TryGetValue(id, out var view))
                return false;

            _views.Remove(id);
            size = view.Size;
            if (view.Root)
                Object.Destroy(view.Root);
            return true;
        }

        public void Dispose()
        {
            // The scene unload can destroy the roots first.
            foreach (var view in _views.Values)
                if (view.Root)
                    Object.Destroy(view.Root);
            _views.Clear();
        }

        private Item CreateView(float size)
        {
            var root = Object.Instantiate(_prefab);
            SceneManager.MoveGameObjectToScene(root, _scene);
            root.transform.localScale = Vector3.one * size;
            return new Item(root, root.GetComponent<IView>(), size);
        }

        // Legacy UnityEngine.Random.insideUnitCircle: a uniform point of the unit disc.
        private static Vec2 InsideUnitCircle(System.Random random)
        {
            while (true)
            {
                var x = 2f * (float)random.NextDouble() - 1f;
                var y = 2f * (float)random.NextDouble() - 1f;
                if (x * x + y * y <= 1f)
                    return new Vec2(x, y);
            }
        }

        private sealed class Item
        {
            public Item(GameObject root, IView view, float size)
            {
                Root = root;
                View = view;
                Size = size;
            }

            public readonly GameObject Root;
            public readonly IView View;
            public readonly float Size;
            public PoseTrack Pose;
        }

        private const string PrefabPath = "CombatView/Objects/Asteroid";
        private const int Salt = 0x41535452;
        private const float MinSize = 2f;
        private const float MaxSize = 5f;
        private const float MaxSpeed = 10f;
        private const float MassFactor = 5f;
        private const float HitPointFactor = 100f;

        private readonly GameObject _prefab;
        private readonly Torus _torus;
        private readonly UnityEngine.SceneManagement.Scene _scene;
        private readonly Dictionary<CoreEntityId, Item> _views = new Dictionary<CoreEntityId, Item>();
    }
}
