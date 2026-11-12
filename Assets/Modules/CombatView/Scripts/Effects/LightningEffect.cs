using CombatView.Views;
using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.LightningEffect (migration phase 5b, step 3): a Lightning or LightningStrike
    // element. Lightning draws a new line in a new direction every frame; LightningStrike draws one line that fades.
    // Size is the diameter, so the line is half of it long; the particle size is its width. The element image goes to
    // the line through a property block, so the instances share the prefab material, as legacy instances shared the
    // template's.
    [RequireComponent(typeof(LightningView), typeof(LineRenderer))]
    public class LightningEffect : ViewEffect, IEffectElement
    {
        [HideInInspector][SerializeField] private Texture2D _texture;

        private LightningView _view;
        private float _thickness;
        private float _direction;
        private MaterialPropertyBlock _block;

        private LightningView View => _view ? _view : (_view = GetComponent<LightningView>());

        public override float Size { get => View.Size * 2; set => View.Size = value * 0.5f; }

        public void Configure(EffectKind kind, Sprite image)
        {
            _texture = image ? image.texture : null;
            View.Animated = kind == EffectKind.Lightning;
            View.Fading = kind == EffectKind.LightningStrike;
        }

        public void Setup(EffectElement element)
        {
            View.Initialize(element.Color, element.ColorMode);
            _thickness = element.ParticleSize;
        }

        public override void OnParentSizeChanged()
        {
            var parentSize = transform.parent.lossyScale.z;
            transform.localScale = Vector3.one / parentSize;
            View.Thickness = _thickness * parentSize;
        }

        public override void Restart() => RandomizeRotation();

        public override void Initialize(PrefabPool pool)
        {
            base.Initialize(pool);
            if (_block == null) _block = new MaterialPropertyBlock();
            EffectMath.SetTexture(GetComponent<LineRenderer>(), _texture, _block);
            _direction = EffectMath.Random.Next(0, 360);
        }

        protected override void UpdateView(float deltaTime)
        {
            base.UpdateView(deltaTime);

            if (View.Animated)
                RandomizeRotation();
        }

        protected override void UpdateRotation()
        {
            transform.localEulerAngles = new Vector3(0, 0, Rotation + _direction);
        }

        private void RandomizeRotation()
        {
            _direction = EffectMath.Random.Next(0, 360);
            UpdateRotation();
        }
    }
}
