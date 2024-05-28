using UnityEngine;
using Utilites;

namespace Combat.Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LayeredEffect : EffectBase
    {
        [SerializeField] private SpriteRenderer[] _children;
        [SerializeField] private float _rotationSpeed;

        protected override void SetColor(Color color)
        {
            base.SetColor(color);
            var random = new RandomState { Value = _randomState };

            foreach (var child in _children)
                child.color = new Color(
                    color.r*(random.NextFloat() * 0.2f + 0.9f), 
                    color.g*(random.NextFloat() * 0.2f + 0.9f),
                    color.b*(random.NextFloat() * 0.2f + 0.9f),
                    color.a*(random.NextFloat() * 0.2f + 0.9f));
        }

        protected override void OnInitialize()
        {
            var random = new System.Random();
            _randomState = RandomState.FromTickCount().Value;

            foreach (var child in _children)
                child.gameObject.Move(new Vector2(random.NextFloat()*0.4f - 0.2f, random.NextFloat()*0.4f - 0.2f));

            UpdateChildrenPosition();
        }

        protected override void UpdateSize()
        {
            gameObject.transform.localScale = Size * Scale * Vector3.one;
        }

        private void UpdateChildrenPosition()
        {
            var random = new RandomState { Value = _randomState };
            foreach (var child in _children)
            {
                var rotation = Rotation * (2 * random.NextFloat() - 1) + random.Next() % 360;
                rotation += (2 * random.NextFloat() - 1) * Time.time * _rotationSpeed;
                child.transform.localEulerAngles = new Vector3(0, 0, rotation);
                child.transform.localScale = Vector3.one * (random.NextFloat() * 0.4f + 0.8f);
            }
        }

        protected override void OnDispose() {}
        protected override void OnGameObjectDestroyed() {}

        protected override void OnAfterUpdate()
        {
            UpdateChildrenPosition();
        }

        protected override void UpdateLife()
        {
            Opacity = 1.0f - Mathf.Pow(1.0f - Life, 4f);
        }

        private ulong _randomState;
    }
}
