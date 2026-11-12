using CombatView.Effects;
using UnityEngine;

namespace CombatView.Views
{
    // Copied from legacy Combat.Component.View.CircularSawView (migration phase 5b, step 5): the saw sprite spins in
    // world space. Changes: the local ColorMode; BulletViewSetup initializes it (legacy did it itself); the renderer
    // owns the root pose, so UpdatePosition does not move the root.
    public class CircularSawView : BaseView
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

        public void Initialize(Sprite sprite, Color color, ColorMode colorMode)
        {
            _baseColor = color;
            _colorMode = colorMode;
            _spriteRenderer.sprite = sprite;
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

        protected override void UpdatePosition(Vector2 position) {}

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
