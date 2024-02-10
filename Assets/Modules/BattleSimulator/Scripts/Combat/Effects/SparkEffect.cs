using UnityEngine;
using Combat.Component.Helpers;
using GameDatabase.DataModel;
using Services.Resources;

namespace Combat.Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SparkEffect : EffectBase, IEffectPrefabInitializer
    {
        [HideInInspector][SerializeField] private float _speed;
        [HideInInspector][SerializeField] private float _sparkSize;

        private Vector2 _velocity;
        private float _angularVelocity;

        public void Initialize(VisualEffectElement data, IResourceLocator resourceLocator)
        {
            var renderer = GetComponent<SpriteRenderer>();
            var sprite = resourceLocator.GetSprite(data.Image);
            renderer.sprite = sprite;

            _speed = data.Size;
            _sparkSize = data.ParticleSize;
        }

        public override void Restart() => Randomize();

        protected override void OnInitialize() => Randomize();
        protected override void OnDispose() {}
        protected override void OnGameObjectDestroyed() {}

        protected override void UpdateLife()
        {
            Opacity = Life;
            Scale = Life;
            Position = Position;
        }

        protected override void UpdatePosition()
        {
            gameObject.Move(Position + _velocity * (1f - Life) * Size);
            gameObject.transform.localEulerAngles = new Vector3(0, 0, Rotation + _angularVelocity * (1f - Life));
        }

        protected override void UpdateSize()
        {
            gameObject.transform.localScale = Scale * _sparkSize * Vector3.one;
        }

        private void Randomize()
        {
            _velocity = RotationHelpers.Direction(Random.Range(0, 360)) * _speed;
            _angularVelocity = Random.Range(-360, 360) * _speed;
        }
    }
}
