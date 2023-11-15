using GameDatabase.Enums;
using GameDatabase.Extensions;
using UnityEngine;

namespace Combat.Component.View
{
    public class ParticleSystemView : BaseView
    {
        [SerializeField] private Transform _view;
        [SerializeField] private bool _lifeAffectsSize = true;
        [Range(0f, 1f)][SerializeField] private float _minScale = 0.5f;
        [SerializeField] private float _growthRate = 5f;

        [SerializeField] private Color _baseColor = Color.white;
        [SerializeField] private ColorMode _colorMode = ColorMode.TakeFromOwner;

        private float _maturity = 0f;
        private float _initialSize;

        public void Initialize(Color color, ColorMode colorMode)
        {
            _baseColor = color;
            _colorMode = colorMode;
            Scale = 0;
        }

        public override void Dispose()
        {
            Opacity = 1.0f;
            Scale = 1.0f;
        }

        public override void UpdateView(float elapsedTime)
        {
            if (_maturity < 1f)
            {
                _maturity = Mathf.Min(1f, _maturity + _growthRate * elapsedTime);
                Scale = GetScale();
            }

            base.UpdateView(elapsedTime);
        }

        protected override void UpdateLife(float life)
        {
            //if (_lifeAffectsOpacity)
            //    Opacity = 1f - (1f - life) * (1f - life);

            if (_lifeAffectsSize)
                Scale = GetScale();
        }

        private float GetScale()
        {
            if (_lifeAffectsSize)
                return _maturity * (_minScale + (1f - (1f - Life) * (1f - Life)) * (1f - _minScale));
            else
                return _maturity;
        }

        protected override void UpdatePosition(Vector2 position)
        {
            _view.localPosition = position;
        }

        protected override void UpdateRotation(float rotation)
        {
            _view.localEulerAngles = new Vector3(0, 0, rotation);
        }

        protected override void UpdateSize(float size)
        {
            _view.localScale = _initialSize * size * Vector3.one;
        }

        protected override void UpdateColor(Color color)
        {
            color = _colorMode.Apply(_baseColor, color);

            //foreach (var spriteRenderer in _spriteRenderers)
            //    spriteRenderer.color = color;
            //foreach (var particleSystem in _particleSystems)
            //    particleSystem.main.startColor;

            //color.a *= _alphaScale;
            //_spriteRenderer.material.color = color;
        }

        protected override void OnGameObjectCreated()
        {
            _initialSize = transform.localScale.z;
        }

        protected override void OnGameObjectDestroyed()
        {
            _maturity = 0f;
        }
    }
}
