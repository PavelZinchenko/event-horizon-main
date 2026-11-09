using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Services.TrailRendererPool (migration phase 5b, step 4), without the MonoBehaviour and
    // Zenject. A released trail stays where it is and fades out; it is reused only after its time has passed. The
    // renderer root destroys the trails.
    public sealed class TrailPool : IDisposable
    {
        public TrailPool(TrailRenderer prefab, Transform root)
        {
            _prefab = prefab;
            _storage = new GameObject("Trails").transform;
            _storage.SetParent(root, false);
        }

        // Null without a prefab.
        public TrailRenderer CreateTrailRenderer(Transform parent, float startWidth, float endWidth, Color color, float duration)
        {
            if (!_prefab)
                return null;

            TrailRenderer renderer = null;

            for (int i = 0; i < _trailRenderers.Count; ++i)
            {
                var item = _trailRenderers[i];
                if (!item.Renderer)
                {
                    _trailRenderers.RemoveAt(i--);
                    continue;
                }

                if (Time.time - item.LastUpdateTime < item.Renderer.time)
                    continue;

                renderer = item.Renderer;
                _trailRenderers.RemoveAt(i);
                break;
            }

            if (renderer == null)
                renderer = Object.Instantiate(_prefab);

            renderer.transform.SetParent(parent, false);
            renderer.transform.localPosition = Vector3.zero;
            renderer.Clear();
            renderer.startWidth = startWidth;
            renderer.endWidth = endWidth;
            renderer.time = duration;
            renderer.startColor = color;
            renderer.endColor = new Color(color.r, color.g, color.b, 0);
            return renderer;
        }

        public void ReleaseTrailRenderer(TrailRenderer renderer)
        {
            if (!renderer)
                return;

            // The scene unload can destroy the storage first.
            if (!_storage)
            {
                Object.Destroy(renderer.gameObject);
                return;
            }

            renderer.transform.SetParent(_storage, true);
            _trailRenderers.Add(new TrailRendererInfo { Renderer = renderer, LastUpdateTime = Time.time });
        }

        public void Dispose()
        {
            _trailRenderers.Clear();
        }

        private readonly TrailRenderer _prefab;
        private readonly Transform _storage;
        private readonly List<TrailRendererInfo> _trailRenderers = new List<TrailRendererInfo>();

        private struct TrailRendererInfo
        {
            public TrailRenderer Renderer;
            public float LastUpdateTime;
        }
    }
}
