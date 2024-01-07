using UnityEngine;
using UnityEngine.UI;

namespace ShipEditor.UI
{
	[RequireComponent(typeof(Canvas))]
	public class CanvasTransformHelper : MonoBehaviour
	{
		[SerializeField] private float _cellSize = 1.0f;

		private Canvas _canvas;
		private Camera _camera;
		private RectTransform _rectTransform;

		private void Awake()
		{
			_camera = Camera.main;
			_canvas = GetComponent<Canvas>();
			_rectTransform = GetComponent<RectTransform>();
		}

		public Vector2 GetCellSize()
		{
			var screenPointZero = _camera.WorldToScreenPoint(Vector3.zero);
			var screenPointOne = _camera.WorldToScreenPoint(Vector3.one);
			var canvasRect = _rectTransform.rect;
			var scale = new Vector2(canvasRect.width / Screen.width, canvasRect.height / Screen.height);

			return new Vector2(screenPointOne.x - screenPointZero.x, screenPointOne.y - screenPointZero.y) * scale;
		}

		public Vector3 ScreenToWorld(Vector2 position)
		{
			return _camera.ScreenToWorldPoint(position);
		}
	}
}
