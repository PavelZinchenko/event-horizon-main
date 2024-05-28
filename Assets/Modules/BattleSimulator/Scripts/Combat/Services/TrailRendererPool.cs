using System.Collections.Generic;
using Services.Settings;
using UnityEngine;
using Zenject;

namespace Combat.Services
{
    public class TrailRendererPool : MonoBehaviour
    {
        [SerializeField] private TrailRenderer _prefab;

        [Inject]
        private void Initialize(IGameSettings gameSettings)
        {
            _powerSavingMode = gameSettings.QualityMode < 0;
        }

        public TrailRenderer CreateTrailRenderer(Transform parent, float startWidth, float endWidth, Color color, float duration)
        {
            if (_powerSavingMode)
                return null;

            TrailRenderer renderer = null;

            for (int i = 0; i < _trailRenderers.Count; ++i)
            {
                var item = _trailRenderers[i];
                if (Time.time - item.LastUpdateTime < item.Renderer.time)
                    continue;

                renderer = item.Renderer;
                _trailRenderers.QuickRemove(i);
                break;
            }

            if (renderer == null)
                renderer = GameObject.Instantiate<TrailRenderer>(_prefab);

            renderer.transform.parent = parent;
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
            if (!this)
            {
                GameObject.DestroyImmediate(renderer.gameObject);
                return;
            }

            renderer.transform.parent = transform;
            _trailRenderers.Add(new TrailRendererInfo { Renderer = renderer, LastUpdateTime = Time.time });
        }

        private void OnDestroy()
        {
            foreach (var item in _trailRenderers)
            {
                foreach (var material in item.Renderer.materials)
                    GameObject.DestroyImmediate(material);

                GameObject.Destroy(item.Renderer.gameObject);
            }

            _trailRenderers.Clear();
        }


        private readonly List<TrailRendererInfo> _trailRenderers = new();
        private bool _powerSavingMode;

        private struct TrailRendererInfo
        {
            public TrailRenderer Renderer;
            public float LastUpdateTime;
        }
    }
}
