using UnityEngine;

namespace CombatView.Effects
{
    // The camera shake of the combat scene (migration phase 5b, step 3), legacy Scene.Shake and Scene.ViewRect: the
    // strongest shake wins, it decays by 2 x the real time, and the camera rect moves by it in a new random direction on
    // each read. The adapter adds Offset to the camera rect.
    public sealed class CameraShake
    {
        // The camera centre, in the drawn space of the views.
        public Vector2 ViewPoint { get; set; }

        public Vector2 Offset => _disturbance > 0f ? _disturbance * EffectMath.Direction(EffectMath.Random.Next(360)) : Vector2.zero;

        // Legacy EffectFactory.CreateDisturbance: weaker with the distance from the camera centre, at most 5.
        public void Shake(Vector2 position, float power)
        {
            var distance = Vector2.Distance(position, ViewPoint) + 20;
            _disturbance = Mathf.Max(_disturbance, Mathf.Min(2.0f * power / Mathf.Sqrt(distance), MaxAmplitude));
        }

        public void Update(float unscaledDeltaTime)
        {
            _disturbance = Mathf.Max(0f, _disturbance * (1 - 2 * unscaledDeltaTime));
        }

        private const float MaxAmplitude = 5f;
        private float _disturbance;
    }

    // Copied from legacy Combat.Effects.ShakeEffect (migration phase 5b, step 3): a Shake element shakes the camera once,
    // when it becomes visible, by its world size.
    public sealed class ShakeEffect : IEffect
    {
        public ShakeEffect(CameraShake shake, Transform parent)
        {
            _shake = shake;
            _parent = parent;
        }

        public bool Visible
        {
            get => _active;
            set
            {
                if (_active == value) return;

                // World size: a record on a bullet scales with the bullet (mapping table, S1).
                if (value && _parent)
                    _shake.Shake(_parent.position, _parent.lossyScale.z * Size);

                _active = value;
            }
        }

        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; }
        public float Life { get; set; }
        public bool IsAlive => true;

        public void Run(float lifetime, Vector2 velocity, float angularVelocity) { }
        public void Tick(float deltaTime) { }
        public void Dispose() { }
        public void Restart() { }
        public void OnParentSizeChanged() { }

        private bool _active;
        private readonly Transform _parent;
        private readonly CameraShake _shake;
    }
}
