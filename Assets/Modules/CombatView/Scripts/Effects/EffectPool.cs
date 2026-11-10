using System;
using System.Collections.Generic;
using CombatSimulation.Physics;
using UnityEngine;

namespace CombatView.Effects
{
    // The live effects of one battle (migration phase 5b, step 2). It ticks every effect once per frame and gives the
    // dead ones back to the prefab pool. An effect at a world point sits under a host that goes to the point's image
    // nearest to the anchor, so its own Position stays an offset and its legacy motion code is unchanged. An effect on
    // a view moves with its parent; its owner disposes it before it destroys the parent.
    // Step 3: a visual effect record plays as one CompositeEffect, whose elements come from a template per element type
    // and image.
    public sealed class EffectPool : IDisposable
    {
        public EffectPool(PrefabPool prefabs, Torus torus, Transform root, CameraShake shake)
        {
            _prefabs = prefabs;
            _torus = torus;
            _root = root;
            _shake = shake;
            _templates = new EffectTemplates(prefabs);
        }

        // Null without a prefab or without an effect component on it.
        public IEffect Create(GameObject prefab, Vec2 point)
        {
            if (!prefab)
                return null;

            var host = TakeHost();
            var effect = Instantiate(prefab, host, null);
            if (effect == null)
            {
                ReleaseHost(host);
                return null;
            }

            _effects.Add(new Item(effect, (Component)effect, host, point));
            return effect;
        }

        public IEffect Create(GameObject prefab, Transform parent)
        {
            if (!prefab || !parent)
                return null;

            var effect = Instantiate(prefab, parent, null);
            if (effect != null)
                _effects.Add(new Item(effect, (Component)effect, parent, null));
            return effect;
        }

        // A record at a world point. Null without a record.
        public IEffect Create(EffectRecord record, Vec2 point)
        {
            if (record == null)
                return null;

            var host = TakeHost();
            var root = TakeRoot(host, WorldRecordZ);
            var effect = new CompositeEffect(record, root, this);
            _effects.Add(new Item(effect, root, host, point));
            return effect;
        }

        // A record on a view. Null without a record or a parent.
        public IEffect Create(EffectRecord record, Transform parent)
        {
            if (record == null || !parent)
                return null;

            var root = TakeRoot(parent, 0f);
            var effect = new CompositeEffect(record, root, this);
            _effects.Add(new Item(effect, root, parent, null));
            return effect;
        }

        // Step 6: a world effect goes to a new point, as legacy ShowHitEffectAction moved its one effect to each hit.
        public void MoveTo(IEffect effect, Vec2 point)
        {
            var index = IndexOf(effect);
            if (index >= 0 && _effects[index].Point.HasValue)
                _effects[index] = new Item(effect, _effects[index].Owner, _effects[index].Host, point);
        }

        // Step 6, legacy CompositeEffect.Detach: an effect on a view stays in the world at the point, turned by the
        // degrees, at its own size; the parent's scale no longer applies. Its owner can then release the parent.
        public void Detach(IEffect effect, Vec2 point, float degrees)
        {
            var index = IndexOf(effect);
            if (index < 0 || _effects[index].Point.HasValue || !_effects[index].Owner)
                return;

            var host = TakeHost();
            var transform = ((Component)_effects[index].Owner).transform;
            transform.SetParent(host, false);
            transform.localPosition = new Vector3(0f, 0f, WorldRecordZ);
            effect.Position = Vector2.zero;
            effect.Rotation = degrees;
            _effects[index] = new Item(effect, _effects[index].Owner, host, point);
        }

        // After the views are placed, so an effect on a view follows it in the same frame.
        public void Update(float deltaTime, Vector2 anchor)
        {
            for (var i = _effects.Count - 1; i >= 0; --i)
            {
                var item = _effects[i];
                if (item.Owner && item.Effect.IsAlive)
                {
                    if (item.Point.HasValue)
                        item.Host.position = Placement.NearestImage(_torus, anchor, item.Point.Value);
                    item.Effect.Tick(deltaTime);
                }

                // The effect disposed itself, its owner disposed it, or its parent view was destroyed with it.
                if (item.Owner && item.Effect.IsAlive)
                    continue;

                if (item.Point.HasValue)
                    ReleaseHost(item.Host);
                _effects.RemoveAt(i);
            }
        }

        public void Dispose()
        {
            foreach (var item in _effects)
                if (item.Owner)
                    item.Effect.Dispose();
            _effects.Clear();
            _hosts.Clear();
            _roots.Clear();
        }

        // One effect of a record element under the record root; an empty effect without a prefab.
        internal IEffect CreateElement(EffectElement element, Transform root)
        {
            if (element.Kind == EffectKind.Shake)
                return new ShakeEffect(_shake, root);

            var template = _templates.Get(element.Kind, element.Image);
            var effect = template ? Instantiate(template, root, element) : null;
            return (IEffect)effect ?? EmptyEffect.Instance;
        }

        internal void ReleaseRoot(Transform root)
        {
            if (!root)
                return;

            root.gameObject.SetActive(false);
            _roots.Push(root);
        }

        private IEffectComponent Instantiate(GameObject prefab, Transform parent, EffectElement element)
        {
            var instance = _prefabs.Get(prefab, parent);
            var effect = instance.GetComponent<IEffectComponent>();
            if (effect == null)
            {
                Debug.LogError($"EffectPool: {prefab.name} has no effect component");
                _prefabs.Release(instance);
                return null;
            }

            // Legacy EffectFactory: the element values go to the instance before its Initialize, and the element starts
            // at the record root whatever its prefab's position (the Wave prefab has z -3).
            if (element != null)
            {
                instance.GetComponent<IEffectElement>()?.Setup(element);
                instance.transform.localPosition = Vector3.zero;
            }
            effect.Initialize(_prefabs);
            return effect;
        }

        // The newest first: a bullet's effects are usually recent.
        private int IndexOf(IEffect effect)
        {
            for (var i = _effects.Count - 1; i >= 0; --i)
                if (_effects[i].Effect == effect)
                    return i;
            return -1;
        }

        private Transform TakeHost()
        {
            Transform host = null;
            while (_hosts.Count > 0 && !host)
                host = _hosts.Pop();
            if (!host)
            {
                host = new GameObject("Effect").transform;
                host.SetParent(_root, false);
            }

            host.gameObject.SetActive(true);
            return host;
        }

        private void ReleaseHost(Transform host)
        {
            if (!host)
                return;

            host.gameObject.SetActive(false);
            _hosts.Push(host);
        }

        // Legacy CompositeEffect.Create: a world record at z -3, a record on a body at its origin.
        private Transform TakeRoot(Transform parent, float z)
        {
            Transform root = null;
            while (_roots.Count > 0 && !root)
                root = _roots.Pop();
            if (!root)
                root = new GameObject("VisualEffect").transform;

            root.SetParent(parent, false);
            root.localPosition = new Vector3(0f, 0f, z);
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one;
            root.gameObject.SetActive(true);
            return root;
        }

        private readonly struct Item
        {
            public Item(IEffect effect, UnityEngine.Object owner, Transform host, Vec2? point)
            {
                Effect = effect;
                Owner = owner;
                Host = host;
                Point = point;
            }

            public readonly IEffect Effect;

            // Unity's destroyed check: the effect's component, or the root of a record.
            public readonly UnityEngine.Object Owner;

            // A world effect: its host and its point. An effect on a view: its parent and null.
            public readonly Transform Host;
            public readonly Vec2? Point;
        }

        // Legacy EmptyEffect: an element without a prefab.
        private sealed class EmptyEffect : IEffect
        {
            public static readonly EmptyEffect Instance = new EmptyEffect();

            public bool Visible { get => false; set { } }
            public Vector2 Position { get => default; set { } }
            public float Rotation { get => 0f; set { } }
            public float Size { get => 0f; set { } }
            public Color Color { get => default; set { } }
            public float Life { get => 0f; set { } }
            public bool IsAlive => false;
            public void Run(float lifetime, Vector2 velocity, float angularVelocity) { }
            public void Tick(float deltaTime) { }
            public void Dispose() { }
            public void Restart() { }
            public void OnParentSizeChanged() { }
        }

        private const float WorldRecordZ = -3f;

        private readonly PrefabPool _prefabs;
        private readonly Torus _torus;
        private readonly Transform _root;
        private readonly CameraShake _shake;
        private readonly EffectTemplates _templates;
        private readonly List<Item> _effects = new List<Item>();
        private readonly Stack<Transform> _hosts = new Stack<Transform>();
        private readonly Stack<Transform> _roots = new Stack<Transform>();
    }
}
