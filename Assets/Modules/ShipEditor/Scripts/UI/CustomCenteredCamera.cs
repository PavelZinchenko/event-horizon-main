using UnityEngine;

namespace ShipEditor.UI
{
	[RequireComponent(typeof(Camera))]
	public class CustomCenteredCamera : MonoBehaviour
	{
		[SerializeField] private Vector2 _center = Vector2.zero;

		private Camera _camera;
		private float _orthographicSize;
		private int _width;
		private int _height;

		private void Awake()
		{
			_camera = GetComponent<Camera>();
		}

		private void LateUpdate()
		{
			var sameSize = _width == Screen.width && _height == Screen.height;
			var sameOrthographicSize = Mathf.Approximately(_orthographicSize, _camera.orthographicSize);

			if (sameSize && sameOrthographicSize) return;

			_width = Screen.width;
			_height = Screen.height;
			_orthographicSize = _camera.orthographicSize;

			var aspect = _camera.aspect;
			var height = _orthographicSize;
			var width = height * aspect;

			var left = -width * 0.5f * (1f + _center.x);
			var right = width * 0.5f * (1f - _center.x);
			var top = -height * 0.5f * (1f + _center.y);
			var bottom = height * 0.5f * (1f - _center.y);

			var matrix = Matrix4x4.Ortho(left, right, top, bottom, _camera.nearClipPlane, _camera.farClipPlane);
			_camera.projectionMatrix = matrix;
		}
	}
}
