using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.CommonEffect (migration phase 5b, step 3). As a record element (Flash,
    // FlashAdditive, Sprite) it takes the element image, as legacy SpriteEffectInitializer did.
    [RequireComponent(typeof(Renderer))]
    public class CommonEffect : EffectBase, IEffectElement
    {
        [SerializeField] private bool _lifeAffectsOpacity = true;
        [SerializeField] private bool _lifeAffectsSize = false;

        public void Configure(EffectKind kind, Sprite image)
        {
            if (Renderer is SpriteRenderer spriteRenderer)
                spriteRenderer.sprite = image;
        }

        public void Setup(EffectElement element) { }

        protected override void OnInitialize() { }
        protected override void OnDispose() { }
        protected override void OnGameObjectDestroyed() { }

        protected override void UpdateLife()
        {
            if (_lifeAffectsOpacity)
                Opacity = Life;
            if (_lifeAffectsSize)
                Scale = Life;
        }
    }
}
