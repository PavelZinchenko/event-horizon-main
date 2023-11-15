using UnityEngine;
using UnityEngine.Events;

namespace Map
{
    public class MapNavigator : MonoBehaviour
    {
        [SerializeField] private MapScaler _mapScaler;
        [SerializeField] private ScreenCenter _screenCenter;
        [SerializeField] private float _movementSpeed = 5;
        [SerializeField] private float _cooldownTime = 2;
        [SerializeField] private float _minDistance = 0.05f;

        [SerializeField] private UnityEvent _positionChanged;

        private Vector2 _position;
        private Vector2 _focusedPosition;
        private float _cooldown;

        public Vector2 Center => -_position;

        private void Update()
        {
            if (_cooldown > 0)
            {
                _cooldown -= Time.deltaTime;
                return;
            }

            var focus = /*_screenCenter.Position*/ - _focusedPosition;
            var distance = Vector2.Distance(_position, focus);
            if (distance >= _minDistance)
            {
                _position = Vector2.Lerp(_position, focus, _movementSpeed * Time.deltaTime);
                UpdatePosition();
                _positionChanged?.Invoke();
            }
        }

        public void OnCenterPointMoved()
        {
            UpdatePosition();
        }

        public void Wait() => _cooldown = _cooldownTime;
        public void Go() => _cooldown = 0f;

        public void SetFocus(Vector2 position)
        {
            _focusedPosition = position;
        }

        public void MoveImmediately()
        {
            _position = -_focusedPosition;
            UpdatePosition();
            _positionChanged?.Invoke();
        }

        public void OnMove(Vector2 offset)
        {
            _position += offset;
            _cooldown = _cooldownTime;
            UpdatePosition();
            _positionChanged?.Invoke();
        }

        public void OnZoom()
        {
            UpdatePosition();
            _positionChanged?.Invoke();
        }

        private void UpdatePosition()
        {
            transform.localPosition = _position + _screenCenter.Position;
        }
    }
}
