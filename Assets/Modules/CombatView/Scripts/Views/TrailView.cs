using UnityEngine;
using CombatView.Effects;

namespace CombatView.Views
{
    // Copied from legacy Combat.Component.View.TrailView (migration phase 5b, step 5): the rocket trail. Changes: the
    // creator sets Pool, as for TrailEffect (legacy injected it); without a pool it draws no trail. The colour goes to
    // the trail's vertex colours, as in TrailEffect, not to a new material per trail.
    public class TrailView : BaseView
    {
        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private float _alpha = 1.0f;
        [SerializeField] private bool _useObjectScale = false;
        [SerializeField] private Color _baseColor = Color.white;
        [SerializeField] private ColorMode _colorMode = ColorMode.TakeFromOwner;

        public TrailPool Pool { get; set; }

        public void Initialize(Color color, ColorMode colorMode)
        {
            _baseColor = color;
            _colorMode = colorMode;
        }

        public override void Dispose()
        {
            if (_trailRenderer != null)
                Pool.ReleaseTrailRenderer(_trailRenderer);

            _trailRenderer = null;
        }

        // A jump of more than 1 unit (a new image at the wrap) starts a new trail.
        public override void UpdateView(float elapsedTime)
        {
            base.UpdateView(elapsedTime);

            if (_trailRenderer == null)
                return;

            var position = (Vector2)transform.position;
            if (Vector2.Distance(_lastPosition, position) > 1.0f)
                _trailRenderer.Clear();

            _lastPosition = position;
        }

        protected override void OnGameObjectCreated() { }
        protected override void OnGameObjectDestroyed() { }

        protected override void UpdateLife(float life)
        {
            if (life > 0.01f)
            {
                _lastActivationTime = Time.time;

                if (_trailRenderer == null && Pool != null)
                {
                    _trailRenderer = Pool.CreateTrailRenderer(transform, TrailSize, TrailSize*0.5f, _trailColor, _duration);
                    _lastPosition = transform.position;
                }
            }
            else if (Time.time - _lastActivationTime > 0.1f)
            {
                if (_trailRenderer != null)
                {
                    Pool.ReleaseTrailRenderer(_trailRenderer);
                    _trailRenderer = null;
                }
            }
        }

        protected override void UpdatePosition(Vector2 position) {}
        protected override void UpdateRotation(float rotation) {}

        protected override void UpdateSize(float size)
        {
            _trailSize = size;
            if (_trailRenderer != null)
            {
                _trailRenderer.startWidth = TrailSize;
                _trailRenderer.endWidth = TrailSize*0.5f;
            }
        }

        protected override void UpdateColor(Color color)
        {
            _trailColor = _colorMode.Apply(_baseColor, color);
            _trailColor.a *= _alpha;
            if (_trailRenderer != null)
            {
                _trailRenderer.startColor = _trailColor;
                _trailRenderer.endColor = new Color(_trailColor.r, _trailColor.g, _trailColor.b, 0);
            }
        }

        private float TrailSize { get { return _useObjectScale ? _trailSize * transform.lossyScale.z : _trailSize; } }

        private Color _trailColor = Color.white;
        private float _trailSize = 1.0f;
        private Vector2 _lastPosition;
        private float _lastActivationTime;
        private TrailRenderer _trailRenderer;
    }
}
