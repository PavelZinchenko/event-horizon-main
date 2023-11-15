using GameServices.Settings;
using Map;
using Services.Messenger;
using UnityEngine;
using Zenject;

namespace Scripts.Galaxy
{
    public class Background : MonoBehaviour
    {
        [Inject] GameSettings _settings;

        [SerializeField] private ScreenCenter _center;
        [SerializeField] private MapScaler _mapScaler;
        [SerializeField] private Transform _target;
        [SerializeField] private Material _material;
        [SerializeField] private Material _materialHQ;
        [SerializeField] private float _minScale = 0.1f;
        [SerializeField] private float _maxScale = 5f;

        private Vector2 _offset;
        private Vector2 _lastTargetPosition;

        private void OnValidate()
        {
            if (_maxScale < _minScale) _maxScale = _minScale;
        }

        [Inject]
        void Initialize(IMessenger messenger)
        {
            messenger.AddListener<int>(EventType.GraphicsQualityChanged, OnQualityChanged);
        }

        public void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            InitializeMaterial();
        }

        private void OnQualityChanged(int _)
        {
            InitializeMaterial();
        }

        private void InitializeMaterial()
        {
            if (_materialCopy)
                Destroy(_materialCopy);

            _meshRenderer.material = _settings.QualityMode > 0 ? _materialHQ : _material;
            _materialCopy = _meshRenderer.material;
        }

        private void LateUpdate()
        {
            UpdateSize();
            UpdateMaterial();
        }

        private void UpdateSize()
        {
            var camera = Camera.main;
            var aspect = camera.aspect;
            float cameraHeight = camera.orthographicSize * 2;
            var cameraSize = new Vector2(aspect * cameraHeight, cameraHeight);

            var scale = new Vector3(cameraSize.x, cameraSize.y, 1.0f);
            transform.localScale = scale;
        }

        private void UpdateMaterial()
        {
            var camera = Camera.main;
            var height = 2f*camera.orthographicSize;
            var width = height * camera.aspect;

            var scale = Camera.main.orthographicSize;//Mathf.Sqrt(Camera.main.orthographicSize / 5f);

            var position = (Vector2)_target.localPosition;
            var offset = position - _lastTargetPosition;
            _lastTargetPosition = position;
            _offset += offset / scale;

            _materialCopy.SetVector("_Size", new Vector2(width, height));
            _materialCopy.SetVector("_Position", _target.localPosition);
            _materialCopy.SetVector("_CenterPosition", _center.Position);
            _materialCopy.SetVector("_Offset", -_offset * scale);
        }

        private void OnDestroy()
        {
            _meshRenderer.material = null;
            Destroy(_materialCopy);
        }

        private Material _materialCopy;
        private MeshRenderer _meshRenderer;
    }
}
