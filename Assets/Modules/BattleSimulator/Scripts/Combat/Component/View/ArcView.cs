using UnityEngine;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using System.Collections.Generic;

namespace Combat.Component.View
{
    public class ArcView : BaseView
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private PolygonCollider2D _collider;

        [SerializeField] private float _alphaScale = 1.0f;
        [SerializeField] private Color _startColor = Color.white;
        [SerializeField] private Color _endColor = Color.white;
        [SerializeField] private Color _baseColor = Color.white;
        [SerializeField] private ColorMode _colorMode = ColorMode.TakeFromOwner;

        [HideInInspector][SerializeField] private float _growthRate = 1;

        public void Initialize(Color baseColor, ColorMode colorMode, float margins, float growthRate)
        {
            _growthRate = growthRate;

            var maxAngle = Mathf.PI / 2 * (1f - margins);
            var halfNumPoints = 1 + Mathf.FloorToInt(8*(1f - margins));
            
            _baseColor = baseColor;
            _colorMode = colorMode;

            CreateShape(maxAngle, halfNumPoints);
            CreateCollider(maxAngle, halfNumPoints);
        }

        public override void Dispose() {}
        protected override void OnGameObjectCreated() {}
        protected override void OnGameObjectDestroyed() {}

        protected override void UpdateColor(Color color)
        {
            if (!_lineRenderer) return;

            color = _colorMode.Apply(_baseColor, color);
            color.a *= _alphaScale;
            _lineRenderer.startColor = color * _startColor;
            _lineRenderer.endColor = color * _endColor;
        }

        protected override void UpdateLife(float life)
        {
            Opacity = Life;
            Size = 1 + _growthRate*(1f - Life);
        }

        protected override void UpdatePosition(Vector2 position) {}
        protected override void UpdateRotation(float rotation) {}
        protected override void UpdateSize(float size)
        {
            _lineRenderer.transform.localScale = Vector3.one * size;
        }

        private void CreateShape(float maxAngle, int halfNumPoints)
        {
            if (!_lineRenderer) return;

            _lineRenderer.positionCount = halfNumPoints * 2 + 1;

            for (var i = -halfNumPoints; i <= halfNumPoints; ++i)
            {
                var angle = maxAngle * (i / (float)halfNumPoints);

                _lineRenderer.SetPosition(halfNumPoints + i, new Vector3(Mathf.Cos(angle) - 1f, Mathf.Sin(angle), 0));
            }
        }

        private void CreateCollider(float maxAngle, int halfNumPoints)
        {
            if (!_collider) return;

            var path = new List<Vector2>();
            for (var i = -halfNumPoints; i <= halfNumPoints; ++i)
            {
                var angle = maxAngle * (i / (float)halfNumPoints);
                path.Add(new Vector2(Mathf.Cos(angle) - 1f, Mathf.Sin(angle)));
            }

            _collider.pathCount = 1;
            _collider.SetPath(0, path.ToArray());
        }
    }
}
