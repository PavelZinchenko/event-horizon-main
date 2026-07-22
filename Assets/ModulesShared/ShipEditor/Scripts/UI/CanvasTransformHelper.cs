using UnityEngine;

namespace ShipEditor.UI
{
	[RequireComponent(typeof(Canvas))]
	public class CanvasTransformHelper : MonoBehaviour
	{
        [SerializeField] private ShipView _shipView;
        
        private Canvas _canvas;
		private Camera _camera;
		private RectTransform _rectTransform;

		private void Awake()
		{
			_canvas = GetComponent<Canvas>();
			_camera = _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
			_rectTransform = GetComponent<RectTransform>();
		}

        public float GetShipRotation() => _shipView.transform.eulerAngles.z - _camera.transform.eulerAngles.z;
        public float GetShipWorldRotation() => _shipView.transform.eulerAngles.z;

        public Vector2 GetCellSize() => GetUnitSquare() *_shipView.Scale;
        public float GetWorldCellSize() => _shipView.CellSize;

        public Vector2 GetUnitSquare()
		{
			var screenPointZero = _camera.WorldToScreenPoint(Vector3.zero);
			var screenPointOne = _camera.WorldToScreenPoint(_camera.transform.up + _camera.transform.right);
			var canvasRect = _rectTransform.rect;
			var scale = new Vector2(canvasRect.width / Screen.width, canvasRect.height / Screen.height);

			return new Vector2(screenPointOne.x - screenPointZero.x, screenPointOne.y - screenPointZero.y) * scale;
		}

		public Vector3 ScreenToWorld(Vector2 position)
		{
			// CameraController uses an off-centre custom projection matrix so that
			// the ship occupies the free area beside the component panel. Unity's
			// ScreenToWorldPoint may use the camera's orthographicSize instead of
			// that matrix on some Unity/Android combinations. The resulting small
			// screen-space error becomes dozens of cells on a Titan-sized layout.
			// Invert the active projection explicitly so pointer and grid always
			// use the exact same camera transform.
			var viewport = _camera.ScreenToViewportPoint(position);
			var clip = new Vector4(viewport.x * 2f - 1f, viewport.y * 2f - 1f, 0f, 1f);
			var cameraPoint = _camera.projectionMatrix.inverse * clip;
			if (Mathf.Abs(cameraPoint.w) > Mathf.Epsilon)
				cameraPoint /= cameraPoint.w;
			var world = _camera.cameraToWorldMatrix * cameraPoint;
			world.z = _shipView.transform.position.z;
			return world;
		}
	}
}
