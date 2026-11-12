using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.FlickerEffect (migration phase 5b, step 3). The flicker uses the effect random
    // source instead of UnityEngine.Random.
    [RequireComponent(typeof(Renderer))]
    public class FlickerEffect : EffectBase
    {
        [SerializeField] private bool _lifeAffectsSize;
        [SerializeField] private bool _randomSize;
        [SerializeField] private bool _randomOpacity;

        protected override void OnInitialize() { }
        protected override void OnDispose() { }
        protected override void OnGameObjectDestroyed() { }

        protected override void UpdateLife() { }

        protected override void OnBeforeUpdate(float deltaTime)
        {
            if (_randomSize || _lifeAffectsSize)
            {
                var size = _randomSize ? 0.5f + EffectMath.NextFloat() : 1.0f;
                Scale = _lifeAffectsSize ? Life * size : size;
            }

            if (_randomOpacity)
                Opacity = 0.5f + EffectMath.NextFloat();
        }
    }
}
