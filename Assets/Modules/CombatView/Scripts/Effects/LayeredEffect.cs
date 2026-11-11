using UnityEngine;

namespace CombatView.Effects
{
    // Copied from legacy Combat.Effects.LayeredEffect (migration phase 5b, step 8): the Smoke and SmokeAdditive prefabs,
    // a few sprite layers with a random tint, offset, angle, scale and spin each. Change: legacy replayed one random
    // state at every update; the copy draws the values once per run from the effect random source.
    [RequireComponent(typeof(SpriteRenderer))]
    public class LayeredEffect : EffectBase
    {
        [SerializeField] private SpriteRenderer[] _children;
        [SerializeField] private float _rotationSpeed;

        protected override void SetColor(Color color)
        {
            base.SetColor(color);

            for (var i = 0; i < _children.Length; ++i)
            {
                var tint = _layers[i].Tint;
                _children[i].color = new Color(color.r * tint.r, color.g * tint.g, color.b * tint.b, color.a * tint.a);
            }
        }

        protected override void OnInitialize()
        {
            if (_layers == null || _layers.Length != _children.Length)
                _layers = new Layer[_children.Length];

            for (var i = 0; i < _children.Length; ++i)
            {
                EffectMath.Move(_children[i].transform, new Vector2(EffectMath.NextFloat() * 0.4f - 0.2f, EffectMath.NextFloat() * 0.4f - 0.2f));
                _layers[i] = new Layer
                {
                    Tint = new Color(Variation(), Variation(), Variation(), Variation()),
                    RotationFactor = EffectMath.NextFloatSigned(),
                    Angle = EffectMath.Random.Next(360),
                    SpinFactor = EffectMath.NextFloatSigned(),
                    Scale = EffectMath.NextFloat() * 0.4f + 0.8f,
                };
            }

            UpdateChildrenPosition();
        }

        protected override void UpdateSize()
        {
            gameObject.transform.localScale = Size * Scale * Vector3.one;
        }

        private void UpdateChildrenPosition()
        {
            for (var i = 0; i < _children.Length; ++i)
            {
                var layer = _layers[i];
                var rotation = Rotation * layer.RotationFactor + layer.Angle + layer.SpinFactor * Time.time * _rotationSpeed;
                _children[i].transform.localEulerAngles = new Vector3(0, 0, rotation);
                _children[i].transform.localScale = Vector3.one * layer.Scale;
            }
        }

        protected override void OnDispose() {}
        protected override void OnGameObjectDestroyed() {}

        protected override void OnAfterUpdate(float deltaTime)
        {
            UpdateChildrenPosition();
        }

        protected override void UpdateLife()
        {
            Opacity = 1.0f - Mathf.Pow(1.0f - Life, 4f);
        }

        private static float Variation() => EffectMath.NextFloat() * 0.2f + 0.9f;

        private struct Layer
        {
            public Color Tint;
            public float RotationFactor;
            public float Angle;
            public float SpinFactor;
            public float Scale;
        }

        private Layer[] _layers;
    }
}
