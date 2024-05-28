using Combat.Services;
using UnityEngine;

namespace Combat.Component.View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShipView : BaseView
    {
        [SerializeField] private SpriteRenderer[] _extraRenderers;

        private Material _defaultMaterial;
        private SpriteRenderer _renderer;

        public override void ApplyHsv(float hue, float saturation, MaterialCache materialCache)
        {
            _defaultMaterial = materialCache.GetDefaultMaterial();
            var material = materialCache.GetHsvMaterial(hue, saturation);

            Renderer.sharedMaterial = material;

            if (_extraRenderers != null && _extraRenderers.Length > 0)
                foreach (var item in _extraRenderers)
                    item.sharedMaterial = material;
        }

        public override void Dispose()
        {
            if (this && _defaultMaterial)
            {
                Renderer.sharedMaterial = _defaultMaterial;

                if (_extraRenderers != null && _extraRenderers.Length > 0)
                    foreach (var item in _extraRenderers)
                        item.sharedMaterial = _defaultMaterial;
            }
        }

        protected SpriteRenderer Renderer => _renderer == null ? _renderer = GetComponent<SpriteRenderer>() : _renderer;

        protected override void OnGameObjectCreated() {}

        protected override void OnGameObjectDestroyed()
        {
        }

        protected override void UpdateLife(float life) {}
        protected override void UpdatePosition(Vector2 position) {}
        protected override void UpdateRotation(float rotation) {}
        protected override void UpdateSize(float size) {}

        protected override void UpdateColor(Color color)
        {
            Renderer.color = color;
        }
    }
}
