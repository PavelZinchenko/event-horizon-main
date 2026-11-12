using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.TrailEffect (migration phase 5b, step 4): an engine trail (ShipTrail,
    // DroneTrail). The creator sets Pool before the first tick; legacy injected the pool. Without a pool it draws no
    // trail.
    public class TrailEffect : EffectBase
    {
        [SerializeField] private float _duration = 1.0f;

        public TrailPool Pool { get; set; }

        protected override void OnInitialize() {}

        protected override void OnDispose()
        {
            if (_trailRenderer != null)
                Pool.ReleaseTrailRenderer(_trailRenderer);

            _trailRenderer = null;
            Pool = null;
        }

        protected override void OnGameObjectDestroyed() {}

        protected override void SetColor(Color color)
        {
            if (_trailRenderer != null)
            {
                _trailRenderer.startColor = color;
                _trailRenderer.endColor = new Color(color.r, color.g, color.b, 0);
            }
        }

        protected override void UpdateLife()
        {
            if (Life > 0.01f)
            {
                _lastActivationTime = Time.time;

                if (_trailRenderer == null && Pool != null)
                {
                    _trailRenderer = Pool.CreateTrailRenderer(transform, Size, Size*2, Color, _duration);
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

        // A jump of more than 3 units (a new image at the wrap) starts a new trail.
        protected override void OnAfterUpdate(float deltaTime)
        {
            if (_trailRenderer == null)
                return;

            var position = (Vector2)transform.position;
            if (Vector2.Distance(_lastPosition, position) > 3.0f)
                _trailRenderer.Clear();

            _lastPosition = position;
        }

        private Vector2 _lastPosition;
        private float _lastActivationTime;
        private TrailRenderer _trailRenderer;
    }
}
