using CombatView.Effects;
using UnityEngine;

namespace CombatView.Views
{
    // Copied from legacy Combat.Component.View.MeshView (migration phase 5b, step 8): a lit mesh that spins on a random
    // axis. Changes: the effect random source instead of UnityEngine.Random, and the colour through a property block, so
    // the renderer keeps the shared prefab material.
    [RequireComponent(typeof(MeshFilter))]
    public class MeshView : BaseView
    {
        [SerializeField] private Color _baseColor = Color.white;
        [SerializeField] private float _rotationSpeed = 1.0f;

        public override void Dispose() { }

        public override void UpdateView(float elapsedTime)
        {
            _time += elapsedTime;
            transform.localEulerAngles = new Vector3(_rotationVector.x, _rotationVector.y, _rotationVector.z * _time);

            base.UpdateView(elapsedTime);
        }

        protected override void OnGameObjectCreated()
        {
            _rotationVector = new Vector3(EffectMath.Random.Next(360), EffectMath.Random.Next(360),
                _rotationSpeed * (20.5f + 39.5f * EffectMath.NextFloat()));
        }

        protected override void OnGameObjectDestroyed() { }

        protected override void UpdateLife(float life) { }
        protected override void UpdatePosition(Vector2 position) { }
        protected override void UpdateRotation(float rotation) { }
        protected override void UpdateSize(float size) { }

        protected override void UpdateColor(Color color)
        {
            var renderer = GetComponent<MeshRenderer>();
            _block ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(_block);
            _block.SetColor(ColorProperty, color * _baseColor);
            renderer.SetPropertyBlock(_block);
        }

        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        private float _time;
        private Vector3 _rotationVector;
        private MaterialPropertyBlock _block;
    }
}
