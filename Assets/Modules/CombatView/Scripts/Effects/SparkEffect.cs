using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.SparkEffect (migration phase 5b, step 3): one spark of a Spark element. It flies
    // out in a random direction at the element size per lifetime, shrinks and fades, and turns by the turn rate. The
    // randomness uses the effect random source.
    [RequireComponent(typeof(SpriteRenderer))]
    public class SparkEffect : EffectBase, IEffectElement
    {
        private float _speed;
        private float _sparkSize;
        private float _turnRate;
        private Vector2 _velocity;
        private float _angularVelocity;

        public void Configure(EffectKind kind, Sprite image)
        {
            GetComponent<SpriteRenderer>().sprite = image;
        }

        public void Setup(EffectElement element)
        {
            _speed = element.Size;
            _sparkSize = element.ParticleSize;
            _turnRate = element.TurnRate;
        }

        public override void Restart() => Randomize();

        protected override void OnInitialize() => Randomize();
        protected override void OnDispose() { }
        protected override void OnGameObjectDestroyed() { }

        protected override void UpdateLife()
        {
            Opacity = Life;
            Scale = Life;
            Position = Position;
        }

        protected override void UpdatePosition()
        {
            var time = 1f - Life;
            var position = Position + _velocity * time * Size;
            if (_turnRate > 0.01f || _turnRate < -0.01f)
                position = EffectMath.Rotate(position, _turnRate * time);

            EffectMath.Move(transform, position);
            transform.localEulerAngles = new Vector3(0, 0, Rotation + _angularVelocity * time);
        }

        protected override void UpdateSize()
        {
            transform.localScale = Scale * _sparkSize * Vector3.one;
        }

        private void Randomize()
        {
            _velocity = EffectMath.Direction(EffectMath.Random.Next(0, 360)) * _speed;
            _angularVelocity = EffectMath.Random.Next(-360, 360) * _speed;
        }
    }
}
