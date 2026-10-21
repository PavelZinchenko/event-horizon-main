using System;
using System.Collections.Generic;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CombatView
{
    // Draws one core battle: ship and bullet sprites at interpolated poses (migration phase 3c, step 4).
    // Events decide when a view exists; snapshots give its poses. Nothing is written back to the simulation.
    // Beams, homing bullets and drones have no own views yet (phases 4-5); they get the generic sprite.
    public sealed class BattleRenderer : IDisposable
    {
        // The root goes into the given scene: the game loads scenes additively and never changes the active
        // scene, so a plain new GameObject would outlive the combat scene.
        public BattleRenderer(Torus torus, int playerSide, Sprite genericSprite, Scene scene)
        {
            _torus = torus;
            _playerSide = playerSide;
            _genericSprite = genericSprite;
            _root = new GameObject("CoreBattleViews");
            SceneManager.MoveGameObjectToScene(_root, scene);
        }

        // Legacy rule: the ship's transform scale is its model scale.
        public void AddShip(EntityId id, Sprite sprite, float scale)
        {
            if (_ships.ContainsKey(id))
                return;

            var renderer = CreateRenderer("Ship", sprite ? sprite : _genericSprite, ShipSortingOrder);
            renderer.transform.localScale = Vector3.one * scale;
            _ships.Add(id, new EntityView(renderer));
        }

        public void AddDrone(EntityId id, ShipDefinition definition)
        {
            WarnOnce(ref _droneWarned, "drone");
            AddShip(id, _genericSprite, ScaleToDiameter(_genericSprite, 2f * definition.HullRadius));
        }

        public void RemoveShip(EntityId id, RemovalReason reason)
        {
            if (!_ships.TryGetValue(id, out var view))
                return;

            _ships.Remove(id);
            if (reason == RemovalReason.Death && view.HasPose)
                _fades.Add(new Fade(view.Renderer));
            else
                Object.Destroy(view.Renderer.gameObject);
        }

        public void AddBullet(in BattleEvent spawn)
        {
            if (_bullets.ContainsKey(spawn.EntityId))
                return;

            float diameter;
            if (spawn.IsBeam)
            {
                WarnOnce(ref _beamWarned, "beam");
                diameter = GenericBulletDiameter;
            }
            else
            {
                if (spawn.BulletDefinition.IsHoming)
                    WarnOnce(ref _homingWarned, "homing bullet");
                diameter = 2f * spawn.BulletDefinition.Radius;
            }

            var view = _bulletPool.Count > 0 ? _bulletPool.Pop() : new EntityView(CreateRenderer("Bullet", _genericSprite, BulletSortingOrder));
            view.Renderer.color = spawn.Side == _playerSide ? PlayerBulletColor : EnemyBulletColor;
            view.Renderer.transform.localScale = Vector3.one * ScaleToDiameter(_genericSprite, diameter);
            _bullets.Add(spawn.EntityId, view);
        }

        public void RemoveBullet(EntityId id)
        {
            if (!_bullets.TryGetValue(id, out var view))
                return;

            _bullets.Remove(id);
            view.Clear();
            _bulletPool.Push(view);
        }

        // After every step. Copies the poses; the snapshot spans are valid only until the next step.
        public void Capture(BattleSnapshot snapshot)
        {
            foreach (var ship in snapshot.Ships)
                if (_ships.TryGetValue(ship.Id, out var view))
                    view.Push(ship.Position, ship.Angle);

            foreach (var bullet in snapshot.Bullets)
                if (_bullets.TryGetValue(bullet.Id, out var view))
                    view.Push(bullet.Position, bullet.Angle);
        }

        // Once per frame. The controlled ship goes to its image nearest to the anchor, and the anchor follows it,
        // so its drawn path never jumps at the wrap. Every other view goes to its image nearest to the new anchor.
        // Without a controlled ship the anchor stays. Alpha is the share of the next tick that has passed.
        public void Render(float alpha, EntityId controlled, ref Vector2 anchor, float deltaTime)
        {
            if (!controlled.IsNull && _ships.TryGetValue(controlled, out var controlledView) && controlledView.HasPose)
                anchor = Place(controlledView, alpha, anchor);

            foreach (var pair in _ships)
                if (pair.Key != controlled)
                    Place(pair.Value, alpha, anchor);

            foreach (var pair in _bullets)
                Place(pair.Value, alpha, anchor);

            UpdateFades(deltaTime);
        }

        // The pose of the last Render, in Unity units and degrees.
        public bool TryGetShipPose(EntityId id, out Vector2 position, out float degrees)
        {
            if (_ships.TryGetValue(id, out var view) && view.HasPose)
            {
                position = view.Position;
                degrees = view.Degrees;
                return true;
            }

            position = default;
            degrees = 0f;
            return false;
        }

        public void Dispose()
        {
            // The scene unload can destroy the root first.
            if (_root)
                Object.Destroy(_root);

            _ships.Clear();
            _bullets.Clear();
            _bulletPool.Clear();
            _fades.Clear();
        }

        private Vector2 Place(EntityView view, float alpha, Vector2 reference)
        {
            if (!view.HasPose)
                return reference;

            view.Position = Placement.NearestImage(_torus, reference, view.Pose.SamplePosition(alpha, _torus));
            view.Degrees = view.Pose.SampleAngle(alpha) * Mathf.Rad2Deg;
            view.Renderer.transform.SetPositionAndRotation(view.Position, Quaternion.Euler(0f, 0f, view.Degrees));
            return view.Position;
        }

        private void UpdateFades(float deltaTime)
        {
            for (var i = _fades.Count - 1; i >= 0; --i)
            {
                var fade = _fades[i];
                fade.Time += deltaTime;
                var progress = fade.Time / FadeDuration;
                if (progress >= 1f || !fade.Renderer)
                {
                    if (fade.Renderer)
                        Object.Destroy(fade.Renderer.gameObject);
                    _fades.RemoveAt(i);
                    continue;
                }

                var color = fade.Renderer.color;
                color.a = 1f - progress;
                fade.Renderer.color = color;
                fade.Renderer.transform.localScale = fade.Scale * (1f + (FadeScale - 1f) * progress);
                _fades[i] = fade;
            }
        }

        private SpriteRenderer CreateRenderer(string name, Sprite sprite, int sortingOrder)
        {
            var gameObject = new GameObject(name);
            gameObject.SetActive(false);
            gameObject.transform.SetParent(_root.transform, false);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static float ScaleToDiameter(Sprite sprite, float diameter)
        {
            if (!sprite)
                return diameter;

            var size = sprite.bounds.size;
            var extent = Mathf.Max(size.x, size.y);
            return extent > 0f ? diameter / extent : diameter;
        }

        private static void WarnOnce(ref bool warned, string kind)
        {
            if (warned)
                return;

            warned = true;
            Debug.LogWarning($"BattleRenderer: no view for {kind} yet; the generic sprite is used");
        }

        private sealed class EntityView
        {
            public EntityView(SpriteRenderer renderer)
            {
                Renderer = renderer;
            }

            public SpriteRenderer Renderer { get; }
            public PoseTrack Pose;
            public bool HasPose { get; private set; }
            public Vector2 Position { get; set; }
            public float Degrees { get; set; }

            // The first pose after a spawn starts the track, so a new view does not slide in from an old pose.
            public void Push(Vec2 position, float angle)
            {
                if (HasPose)
                {
                    Pose.Push(position, angle);
                    return;
                }

                Pose.Reset(position, angle);
                HasPose = true;
                Renderer.gameObject.SetActive(true);
            }

            public void Clear()
            {
                HasPose = false;
                Renderer.gameObject.SetActive(false);
            }
        }

        private struct Fade
        {
            public Fade(SpriteRenderer renderer)
            {
                Renderer = renderer;
                Scale = renderer.transform.localScale;
                Time = 0f;
            }

            public SpriteRenderer Renderer;
            public Vector3 Scale;
            public float Time;
        }

        private const int ShipSortingOrder = 0;
        private const int BulletSortingOrder = 1;
        private const float FadeDuration = 0.4f;
        private const float FadeScale = 1.3f;
        private const float GenericBulletDiameter = 0.3f;

        private static readonly Color PlayerBulletColor = new Color(0.5f, 0.9f, 1f);
        private static readonly Color EnemyBulletColor = new Color(1f, 0.5f, 0.4f);

        private bool _droneWarned;
        private bool _beamWarned;
        private bool _homingWarned;

        private readonly Torus _torus;
        private readonly int _playerSide;
        private readonly Sprite _genericSprite;
        private readonly GameObject _root;
        private readonly Dictionary<EntityId, EntityView> _ships = new Dictionary<EntityId, EntityView>();
        private readonly Dictionary<EntityId, EntityView> _bullets = new Dictionary<EntityId, EntityView>();
        private readonly Stack<EntityView> _bulletPool = new Stack<EntityView>();
        private readonly List<Fade> _fades = new List<Fade>();
    }
}
