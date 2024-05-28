using Combat.Component.Helpers;
using GameDatabase.DataModel;
using Services.Resources;
using UnityEngine;

namespace Combat.Effects
{
    public class SmokeEffect : EffectBase, IEffectPrefabInitializer
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _rotationMaxSpeed = 10;
        [SerializeField] private float _colorVariation = 0.2f;
        [SerializeField] private float _positionVariation = 0.4f;
        [SerializeField] private float _sizeVariation = 0.4f;

        [SerializeField] [HideInInspector] private float _randomFactor;

        private Color _randomColor;
        private float _randomAngle;
        private float _randomScale;
        private float _randomValue1;
        private float _randomValue2;

        public void Initialize(VisualEffectElement data, IResourceLocator resourceLocator)
        {
            var sprite = resourceLocator.GetSprite(data.Image);
            _spriteRenderer.sprite = sprite;
            _randomFactor = data.ParticleSize;
        }

        public override void Restart()
        {
            OnInitialize();
        }

        protected override void SetColor(Color color)
        {
            _spriteRenderer.color = new Color(
                color.r + color.r*((_randomColor.r - 0.5f) * _colorVariation)*_randomFactor, 
                color.g + color.g*((_randomColor.g - 0.5f) * _colorVariation)*_randomFactor,
                color.b + color.b*((_randomColor.b - 0.5f) * _colorVariation)*_randomFactor,
                color.a + color.a*((_randomColor.a - 0.5f) * _colorVariation)*_randomFactor);
        }

        protected override void OnInitialize()
        {
            var random = new System.Random(GetHashCode() + (int)System.DateTime.UtcNow.Ticks);
            _randomColor = new Color(random.NextFloat(), random.NextFloat(), random.NextFloat(), random.NextFloat());
            _randomAngle = random.Next(360);
            _randomScale = 1f + (random.NextFloat() - 0.5f)*_sizeVariation*_randomFactor;
            _randomValue1 = 2*random.NextFloat() - 1;
            _randomValue2 = 2*random.NextFloat() - 1;

            _spriteRenderer.gameObject.Move(new Vector2(
                (random.NextFloat() - 0.5f)*_positionVariation*_randomFactor,
                (random.NextFloat() - 0.5f)*_positionVariation*_randomFactor));

            UpdateChildrenPosition();
        }

        protected override void UpdateSize()
        {
            gameObject.transform.localScale = Size * Scale * Vector3.one;
        }

        private void UpdateChildrenPosition()
        {
            var rotation = Rotation*_randomValue1 + _randomAngle;
            rotation += _randomValue2*Time.time*_rotationMaxSpeed*_randomFactor;
            _spriteRenderer.transform.localEulerAngles = new Vector3(0, 0, rotation);
            _spriteRenderer.transform.localScale = Vector3.one*_randomScale;
        }

        protected override void OnDispose() {}
        protected override void OnGameObjectDestroyed() {}

        protected override void OnAfterUpdate()
        {
            UpdateChildrenPosition();
        }

        protected override void UpdateLife()
        {
            var live = Life*2;
            Opacity = 1.0f - (1.0f - live) * (1.0f - live) * (1.0f - live) * (1.0f - live);
        }
    }
}
