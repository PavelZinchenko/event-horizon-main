using UnityEngine;
using Zenject;
using Services.Settings;

namespace ShipEditor.UI
{
	[RequireComponent(typeof(MeshRenderer))]
    public class Background : MonoBehaviour
    {
        [System.Serializable]
        public struct ColorMatrix
        {
            public Vector3 R;
            public Vector3 G;
            public Vector3 B;
        }

		[InjectOptional] IGameSettings _settings;

		[SerializeField] private Material _material;
		[SerializeField] private Material _materialHQ;
		[SerializeField] private ColorMatrix _nebulaColor;
		[SerializeField] private CameraController _camera;

        private MeshRenderer _meshRenderer;
        private Material _materialCopy;

        public void Awake()
        {
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
            InitializeMaterial();
        }

        private void InitializeMaterial()
        {
            if (_materialCopy)
                Destroy(_materialCopy);

			_meshRenderer.material = _settings != null && _settings.QualityMode > 0 ? _materialHQ : _material;
			_materialCopy = _meshRenderer.material;

            SetNebulaColor();
        }

        private void SetNebulaColor()
        {
            var color = _nebulaColor;
            _materialCopy.SetVector("_NebulaR", color.R);
            _materialCopy.SetVector("_NebulaG", color.G);
            _materialCopy.SetVector("_NebulaB", color.B);
        }

        private void LateUpdate()
        {
            UpdateSize();
            UpdateMaterial();
        }

        private void UpdateSize()
        {
            var scale = new Vector3(_camera.Width, _camera.Height, 1.0f);
			var offset = _camera.Offset;
            transform.localScale = scale;
			transform.localPosition = new Vector3(offset.x, offset.y, transform.localPosition.z);
        }

        private void UpdateMaterial()
        {
            _materialCopy.SetVector("_Size", new Vector2(_camera.Width, _camera.Height));
            _materialCopy.SetVector("_Offset", transform.position);
        }

        private void OnDestroy()
        {
            _meshRenderer.material = null;
            Destroy(_materialCopy);
        }
    }
}
