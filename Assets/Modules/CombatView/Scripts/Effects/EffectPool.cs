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
    public sealed class EffectPool : IDisposable
    {
        public EffectPool(PrefabPool prefabs, Torus torus, Transform root)
        {
            _prefabs = prefabs;
            _torus = torus;
            _root = root;
        }

        // Null without a prefab or without an effect component on it.
        public IEffect Create(GameObject prefab, Vec2 point)
        {
            if (!prefab)
                return null;

            var host = _hosts.Count > 0 ? _hosts.Pop() : CreateHost();
            host.gameObject.SetActive(true);
            var effect = Create(prefab, host, point);
            if (effect == null)
                ReleaseHost(host);
            return effect;
        }

        public IEffect Create(GameObject prefab, Transform parent)
        {
            if (!prefab || !parent)
                return null;

            return Create(prefab, parent, null);
        }

        // After the views are placed, so an effect on a view follows it in the same frame.
        public void Update(float deltaTime, Vector2 anchor)
        {
            for (var i = _effects.Count - 1; i >= 0; --i)
            {
                var item = _effects[i];
                if (item.Component && item.Effect.IsAlive)
                {
                    if (item.Point.HasValue)
                        item.Host.position = Placement.NearestImage(_torus, anchor, item.Point.Value);
                    item.Effect.Tick(deltaTime);
                }

                // The effect disposed itself, its owner disposed it, or its parent view was destroyed with it.
                if (item.Component && item.Effect.IsAlive)
                    continue;

                if (item.Point.HasValue)
                    ReleaseHost(item.Host);
                _effects.RemoveAt(i);
            }
        }

        public void Dispose()
        {
            foreach (var item in _effects)
                if (item.Component)
                    item.Effect.Dispose();
            _effects.Clear();
            _hosts.Clear();
        }

        private IEffect Create(GameObject prefab, Transform parent, Vec2? point)
        {
            var instance = _prefabs.Get(prefab, parent);
            var effect = instance.GetComponent<IEffectComponent>();
            if (effect == null)
            {
                Debug.LogError($"EffectPool: {prefab.name} has no effect component");
                _prefabs.Release(instance);
                return null;
            }

            effect.Initialize(_prefabs);
            _effects.Add(new Item(effect, (Component)effect, parent, point));
            return effect;
        }

        private Transform CreateHost()
        {
            var host = new GameObject("Effect");
            host.transform.SetParent(_root, false);
            return host.transform;
        }

        private void ReleaseHost(Transform host)
        {
            if (!host)
                return;

            host.gameObject.SetActive(false);
            _hosts.Push(host);
        }

        private readonly struct Item
        {
            public Item(IEffectComponent effect, Component component, Transform host, Vec2? point)
            {
                Effect = effect;
                Component = component;
                Host = host;
                Point = point;
            }

            public readonly IEffectComponent Effect;

            // The same object as Effect, for Unity's destroyed check.
            public readonly Component Component;

            // A world effect: its host and its point. An effect on a view: its parent and null.
            public readonly Transform Host;
            public readonly Vec2? Point;
        }

        private readonly PrefabPool _prefabs;
        private readonly Torus _torus;
        private readonly Transform _root;
        private readonly List<Item> _effects = new List<Item>();
        private readonly Stack<Transform> _hosts = new Stack<Transform>();
    }
}
