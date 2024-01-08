using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{
    [RequireComponent(typeof(RectTransform))]
    public class ScreenCenter : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        private bool _initialized;
        private Vector2 _position;

        private void OnRectTransformDimensionsChange()
        {
            if (!_initialized) return;
            UpdatePosition();
        }

        public Vector2 NormalizedPosition
        {
            get
            {
                EnsureInitialized();
                return new Vector2(_position.x / _camera.aspect, _position.y);
            }
        }

        public Vector2 Position
        {
            get
            {
                EnsureInitialized();
                return _position * _camera.orthographicSize;

            }
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            Assert.IsTrue(_camera.orthographic);
            UpdatePosition();
            _initialized = true;
        }

        private void UpdatePosition()
        {
            if (!_camera) return;
            var rectTransform = GetComponent<RectTransform>();
            _position = _camera.ScreenToWorldPoint(rectTransform.position) / _camera.orthographicSize;
        }
    }
}
