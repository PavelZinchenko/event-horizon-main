using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.LaserEffect (migration phase 5b, step 3): the new Laser element (owner question
    // 10), which replaces the hard-coded repair bot beam. A straight beam along local x, Size long and the particle size
    // wide. The element image goes along it in thirds, as legacy LaserView: the end thirds keep their length (the
    // border), and the middle third stretches. The border is 0.2 of the width, the ratio of the legacy Laser prefab.
    [RequireComponent(typeof(LineRenderer))]
    public class LaserEffect : EffectBase, IEffectElement
    {
        [SerializeField] private Texture2D _texture;
        [SerializeField] private float _thickness = 1.0f;
        [SerializeField] private float _borderSize = 0.2f;

        private LineRenderer _lineRenderer;
        private MaterialPropertyBlock _block;

        public void Configure(EffectKind kind, Sprite image)
        {
            _texture = image ? image.texture : null;
        }

        public void Setup(EffectElement element)
        {
            _thickness = element.ParticleSize;
            _borderSize = BorderRatio * element.ParticleSize;
        }

        protected override void OnInitialize()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_block == null) _block = new MaterialPropertyBlock();
            EffectMath.SetTexture(_lineRenderer, _texture, _block);
        }

        protected override void SetColor(Color color)
        {
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }

        protected override void OnDispose() { }
        protected override void OnGameObjectDestroyed() { }

        protected override void UpdateSize()
        {
            UpdateLine(Size * Scale, _thickness);
        }

        protected override void UpdateLife()
        {
            Opacity = Life;
        }

        private void UpdateLine(float size, float thickness)
        {
            if (!_lineRenderer) return;

            if (size < 2 * _borderSize)
            {
                _lineRenderer.enabled = false;
                return;
            }

            _lineRenderer.enabled = true;
            _lineRenderer.startWidth = thickness;
            _lineRenderer.endWidth = thickness;
            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, new Vector3(_borderSize, 0, 0));
            _lineRenderer.SetPosition(2, new Vector3(size - _borderSize, 0, 0));
            _lineRenderer.SetPosition(3, new Vector3(size, 0, 0));
        }

        private const float BorderRatio = 0.2f;
    }
}
