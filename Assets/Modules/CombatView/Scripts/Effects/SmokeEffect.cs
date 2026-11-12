using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.SmokeEffect (migration phase 5b, step 3): one smoke puff of a Smoke or
    // SmokeAdditive element, with a random colour, offset, size and spin scaled by the element's particle size. The
    // randomness uses the effect random source.
    public class SmokeEffect : EffectBase, IEffectElement
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _rotationMaxSpeed = 10;
        [SerializeField] private float _colorVariation = 0.2f;
        [SerializeField] private float _positionVariation = 0.4f;
        [SerializeField] private float _sizeVariation = 0.4f;

        private float _randomFactor;
        private Color _randomColor;
        private float _randomAngle;
        private float _randomScale;
        private float _randomValue1;
        private float _randomValue2;

        public void Configure(EffectKind kind, Sprite image)
        {
            _spriteRenderer.sprite = image;
        }

        public void Setup(EffectElement element)
        {
            _randomFactor = element.ParticleSize;
        }

        public override void Restart()
        {
            OnInitialize();
        }

        protected override void SetColor(Color color)
        {
            _spriteRenderer.color = new Color(
                color.r + color.r * ((_randomColor.r - 0.5f) * _colorVariation) * _randomFactor,
                color.g + color.g * ((_randomColor.g - 0.5f) * _colorVariation) * _randomFactor,
                color.b + color.b * ((_randomColor.b - 0.5f) * _colorVariation) * _randomFactor,
                color.a + color.a * ((_randomColor.a - 0.5f) * _colorVariation) * _randomFactor);
        }

        protected override void OnInitialize()
        {
            _randomColor = new Color(EffectMath.NextFloat(), EffectMath.NextFloat(), EffectMath.NextFloat(), EffectMath.NextFloat());
            _randomAngle = EffectMath.Random.Next(360);
            _randomScale = 1f + (EffectMath.NextFloat() - 0.5f) * _sizeVariation * _randomFactor;
            _randomValue1 = EffectMath.NextFloatSigned();
            _randomValue2 = EffectMath.NextFloatSigned();

            EffectMath.Move(_spriteRenderer.transform, new Vector2(
                (EffectMath.NextFloat() - 0.5f) * _positionVariation * _randomFactor,
                (EffectMath.NextFloat() - 0.5f) * _positionVariation * _randomFactor));

            UpdateChildrenPosition();
        }

        protected override void UpdateSize()
        {
            transform.localScale = Size * Scale * Vector3.one;
        }

        private void UpdateChildrenPosition()
        {
            var rotation = Rotation * _randomValue1 + _randomAngle;
            rotation += _randomValue2 * Time.time * _rotationMaxSpeed * _randomFactor;
            _spriteRenderer.transform.localEulerAngles = new Vector3(0, 0, rotation);
            _spriteRenderer.transform.localScale = Vector3.one * _randomScale;
        }

        protected override void OnDispose() { }
        protected override void OnGameObjectDestroyed() { }

        protected override void OnAfterUpdate(float deltaTime)
        {
            UpdateChildrenPosition();
        }

        protected override void UpdateLife()
        {
            var live = Life * 2;
            Opacity = 1.0f - (1.0f - live) * (1.0f - live) * (1.0f - live) * (1.0f - live);
        }
    }
}
