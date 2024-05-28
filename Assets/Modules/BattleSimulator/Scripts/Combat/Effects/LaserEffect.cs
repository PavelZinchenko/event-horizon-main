using UnityEngine;

namespace Combat.Effects
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserEffect : EffectBase
    {
        [SerializeField] private Texture2D _texture;
        [SerializeField] private float _thickness = 1.0f;
        [SerializeField] private float _borderSize = 0.2f;

        private LineRenderer _lineRenderer;

        protected override void OnInitialize()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        protected override void SetColor(Color color)
        {
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }

        protected override void OnDispose() {}

        protected override void OnGameObjectDestroyed()
        {
            GameObject.Destroy(_mesh);
        }

        protected override void UpdateSize()
        {
            var size = Size*Scale;
            UpdateLine(size, _thickness);
        }

        protected override void UpdateLife()
        {
            Opacity = Life;
        }

        private void UpdateLine(float size, float thikness)
        {
            if (!_lineRenderer) return;

            if (size < 2 * _borderSize)
            {
                _lineRenderer.enabled = false;
                return;
            }

            _lineRenderer.enabled = true;
            _lineRenderer.startWidth = thikness;
            _lineRenderer.endWidth = thikness;
            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, new Vector3(_borderSize, 0, 0));
            _lineRenderer.SetPosition(2, new Vector3(size - _borderSize, 0, 0));
            _lineRenderer.SetPosition(3, new Vector3(size, 0, 0));
        }

        private Mesh _mesh;
    }
}
