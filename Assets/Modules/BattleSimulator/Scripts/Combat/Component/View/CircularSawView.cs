using Combat.Component.Helpers;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using Services.Resources;
using System;
using UnityEngine;

namespace Combat.Component.View
{
    public class CircularSawView : BaseView, IBulletPrefabInitializer
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _rotationSpeed = 5.0f;
        [SerializeField] private Color _baseColor = Color.white;
        [SerializeField] private ColorMode _colorMode = ColorMode.TakeFromOwner;

        private float _initialSize;

        public override void UpdateView(float elapsedTime)
        {
            base.UpdateView(elapsedTime);
            Rotation += _rotationSpeed * elapsedTime * 360;
        }

        public override void Dispose() {}

        public void Initialize(BulletPrefab data, IResourceLocator resourceLocator)
        {
            _baseColor = data.MainColor;
            _colorMode = data.MainColorMode;
            _spriteRenderer.sprite = resourceLocator.GetSprite(data.Image);
        }

        protected override void OnGameObjectCreated()
        {
            _initialSize = _spriteRenderer.transform.localScale.z;
        }

        protected override void OnGameObjectDestroyed() {}

        protected override void UpdateColor(Color color)
        {
            color = _colorMode.Apply(_baseColor, color);
            _spriteRenderer.color = color;
        }

        protected override void UpdatePosition(Vector2 position) 
        {
            transform.localPosition = position;
        }

        protected override void UpdateRotation(float rotation)
        {
            _spriteRenderer.transform.eulerAngles = new Vector3(0, 0, rotation);
        }

        protected override void UpdateSize(float size)
        {
            _spriteRenderer.transform.localScale = _initialSize*size*Vector3.one;
        }

        protected override void UpdateLife(float life)
        {
        }
    }
}
