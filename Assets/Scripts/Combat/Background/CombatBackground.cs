using GameServices.Settings;
using Services.Messenger;
using UnityEngine;
using Zenject;

namespace Combat.Background
{
    public class CombatBackground : MonoBehaviour
    {
        [System.Serializable]
        public struct ColorMatrix
        {
            public Vector3 R;
            public Vector3 G;
            public Vector3 B;
        }

        [Inject] GameSettings _settings;

        [SerializeField] private Material _material;
        [SerializeField] private Material _materialHQ;
        [SerializeField] private ColorMatrix[] _nebulaColors;

        private bool _outOfTimeMode;
        private MeshRenderer _meshRenderer;
        private Material _materialCopy;

        [Inject]
        void Initialize(IMessenger messenger)
        {
            messenger.AddListener<int>(EventType.GraphicsQualityChanged, OnQualityChanged);
        }

        public bool OutOfTimeMode 
        {
            get => _outOfTimeMode;
            set 
            {
                if (_outOfTimeMode == value) return;
                _outOfTimeMode = value;
                _materialCopy.SetInt("_NebulaColorMode", value ? 1 : 0);
            }
        }

        public void Awake()
        {
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
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

            SetNebulaColor();
        }

        private void SetNebulaColor()
        {
            var color = _nebulaColors[Random.Range(0,_nebulaColors.Length)];
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
            float cameraHeight = UnityEngine.Camera.main.orthographicSize * 2;
            Vector2 cameraSize = new Vector2(UnityEngine.Camera.main.aspect * cameraHeight, cameraHeight);

            var scale = new Vector3(cameraSize.x, cameraSize.y, 1.0f);
            transform.localScale = scale;
        }

        private void UpdateMaterial()
        {
            var camera = UnityEngine.Camera.main;

            var height = 2f * camera.orthographicSize;
            var width = height * camera.aspect;

            _materialCopy.SetVector("_Size", new Vector2(width, height));
            _materialCopy.SetVector("_Offset", transform.position);
        }

        private void OnDestroy()
        {
            _meshRenderer.material = null;
            Destroy(_materialCopy);
        }
    }
}
