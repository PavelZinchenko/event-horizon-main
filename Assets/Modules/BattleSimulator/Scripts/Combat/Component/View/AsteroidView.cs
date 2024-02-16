using UnityEngine;

namespace Combat.Component.View
{
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
            Primitives.CreateSphere(_mesh, _meshSize, _detailLevel, _noiseFrequency, _noisePower);
        }

        protected override void OnGameObjectDestroyed()
        {
            base.OnGameObjectDestroyed();
            GameObject.Destroy(_mesh);
        }

        private Mesh _mesh;
    }
}
