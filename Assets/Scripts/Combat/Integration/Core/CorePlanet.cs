using System;
using Combat.Component.View;
using CombatSimulation.Physics;
using CombatView;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Combat.Integration.Core
{
    // The legacy background planet (phase 4, step 10, "Planet (10)"): presentation only, outside the core. The legacy
    // prefab is reused; its views build their mesh in Awake and need only UpdateView. Colour, position and size come
    // from the battle seed (PL2), so both duel peers draw the same planet; the tilt and spin stay in the prefab's
    // MeshView on UnityEngine.Random.
    public sealed class CorePlanet : IDisposable
    {
        // Null without the prefab.
        public static CorePlanet Create(ulong seed, Torus torus, UnityEngine.SceneManagement.Scene scene)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            if (!prefab)
            {
                Debug.LogWarning($"CorePlanet: prefab {PrefabPath} not found; no planet");
                return null;
            }

            // Legacy CombatManager.OnCombatStarted: r, b, x, y, size, in this order.
            var random = new System.Random(unchecked((int)(uint)seed ^ Salt));
            var r = (float)random.NextDouble();
            var g = Mathf.Sqrt(1f - r * r);
            var b = (float)random.NextDouble();
            var color = Color.Lerp(new Color(r, g, b), Color.gray, 0.5f);
            var position = new Vec2(torus.Width * (float)random.NextDouble(), torus.Height * (float)random.NextDouble());
            var size = 30 + (float)random.NextDouble() * 10;

            var root = Object.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(root, scene);
            root.transform.localScale = Vector3.one * size;

            // The core owns the physics; the legacy body is never initialized. The adapter requires the rigidbody,
            // so it goes first.
            Object.DestroyImmediate(root.GetComponent<Combat.Component.Body.RigidBodyAdapter>());
            Object.DestroyImmediate(root.GetComponent<Rigidbody2D>());

            var view = root.GetComponent<IView>();
            view.Color = color;

            Debug.Log($"CorePlanet: colour {color}, position ({position.X:F2}, {position.Y:F2}), size {size:F2}");
            return new CorePlanet(root, view, position, torus);
        }

        private CorePlanet(GameObject root, IView view, Vec2 position, Torus torus)
        {
            _root = root;
            _view = view;
            _position = position;
            _torus = torus;
        }

        // Once per frame after the renderer: the image nearest to the anchor, as every other view (PL3).
        public void Render(Vector2 anchor, float deltaTime)
        {
            if (!_root)
                return;

            _root.transform.position = Placement.NearestImage(_torus, anchor, _position);
            _view.UpdateView(deltaTime);
        }

        public void Dispose()
        {
            // The scene unload can destroy the root first.
            if (_root)
                Object.Destroy(_root);
        }

        private const string PrefabPath = "Combat/Objects/Planet";
        private const int Salt = 0x504C4E54;

        private readonly GameObject _root;
        private readonly IView _view;
        private readonly Vec2 _position;
        private readonly Torus _torus;
    }
}
