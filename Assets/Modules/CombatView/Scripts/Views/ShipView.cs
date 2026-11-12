using UnityEngine;

namespace CombatView.Views
{
    // Copied from legacy Combat.Component.View.ShipView (migration phase 5b, step 4): the view of the ship, satellite
    // and turret prefabs. HsvMaterials replaces the MaterialCache component.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShipView : BaseView
    {
        [SerializeField] private SpriteRenderer[] _extraRenderers;

        private Material _defaultMaterial;
        private SpriteRenderer _renderer;

        public SpriteRenderer Renderer => _renderer == null ? _renderer = GetComponent<SpriteRenderer>() : _renderer;

        public void ApplyHsv(float hue, float saturation, HsvMaterials materialCache)
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
