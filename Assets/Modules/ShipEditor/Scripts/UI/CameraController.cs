using UnityEngine;

namespace ShipEditor.UI
{
	[RequireComponent(typeof(Camera))]
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private RectTransform _focus;
		[SerializeField] private float _smoothTime;

		private Camera _camera;
		private float _orthographicSize;
		private float _zoomVelocity;
		private int _screenWidth;
		private int _screenHeight;
		private Vector2 _focusPoint;
		private Vector2 _focusVelocity;

		public float Width => Height * Aspect;
		public float Height => 2*_orthographicSize;
		public float Aspect => (float)_screenWidth / _screenHeight;
		public RectTransform Focus { get => _focus; set => _focus = value; }
		public float OrthographicSize { get => _camera.orthographicSize; set => _camera.orthographicSize = value; }
		public Vector2 Position { get => transform.localPosition; set => transform.localPosition = new Vector3(value.x, value.y, transform.localPosition.z); }
		public Vector2 Offset => new Vector2((0.5f - _focusPoint.x)*Width, (0.5f - _focusPoint.y)*Height);

		private void Awake()
		{
			_camera = GetComponent<Camera>();
		}

		private void Update()
		{
			if (!TryUpdateDimensions()) return;

			var halfHeight = _orthographicSize;
			var halfWidth = halfHeight * Aspect;

			var x = (1.0f - 2*_focusPoint.x) * halfWidth;
			var y = (1.0f - 2*_focusPoint.y) * halfHeight;

			var left = x - halfWidth;
			var right = x + halfWidth;
			var top = y - halfHeight;
			var bottom = y + halfHeight;

			var matrix = Matrix4x4.Ortho(left, right, top, bottom, _camera.nearClipPlane, _camera.farClipPlane);
			_camera.projectionMatrix = matrix;
		}

		private bool TryUpdateDimensions()
		{
			var dataChanged = false;

			var screenSizeChanged = _screenWidth != Screen.width || _screenHeight != Screen.height;
			if (screenSizeChanged)
			{
				_screenWidth = Screen.width;
				_screenHeight = Screen.height;
				dataChanged = true;
			}

			var orthographicSizeChanged = !Mathf.Approximately(_orthographicSize, _camera.orthographicSize);
			if (orthographicSizeChanged)
			{
				_orthographicSize = CalculateOrthographicSize(_camera.orthographicSize);
				dataChanged = true;
			}

			var focus = 0.5f * (_focus.anchorMin + _focus.anchorMax);
			if (focus != _focusPoint)
			{
				_focusPoint = CalculateFocusPoint(focus);
				dataChanged = true;
			}

			return dataChanged;
		}

		private Vector2 CalculateFocusPoint(Vector2 value)
		{
			if (_smoothTime > 0)
				return Vector2.SmoothDamp(_focusPoint, value, ref _focusVelocity, _smoothTime);

			return value;
		}

		private float CalculateOrthographicSize(float value)
		{
			if (_smoothTime > 0)
				return Mathf.SmoothDamp(_orthographicSize, value, ref _zoomVelocity, _smoothTime);

			return value;
		}
	}
}
