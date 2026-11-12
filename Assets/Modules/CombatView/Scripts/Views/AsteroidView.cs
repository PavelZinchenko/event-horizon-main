using CombatView.Effects;
using UnityEngine;

namespace CombatView.Views
{
    // Copied from legacy Combat.Component.View.AsteroidView (migration phase 5b, step 8): the rock or planet sphere,
    // built once per instance with Perlin noise on its radius.
    public class AsteroidView : MeshView
    {
        [SerializeField] private int _detailLevel = 6;
        [SerializeField] private float _meshSize = 1.0f;
        [SerializeField] private float _noiseFrequency = 5f;
        [SerializeField] private float _noisePower = 0.2f;

        protected override void OnGameObjectCreated()
        {
            base.OnGameObjectCreated();

            var meshFilter = GetComponent<MeshFilter>();
            _mesh = meshFilter.mesh;
            EffectMath.CreateSphere(_mesh, _meshSize, _detailLevel, _noiseFrequency, _noisePower);
        }

        protected override void OnGameObjectDestroyed()
        {
            base.OnGameObjectDestroyed();
            Destroy(_mesh);
        }

        private Mesh _mesh;
    }
}
