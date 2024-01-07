using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ShipEditor.UI
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
		[SerializeField] private UnityEvent<PointerEventData> _dragStarted;

		[SerializeField] private float _thresholdX = 0.1f;
		[SerializeField] private float _thresholdY = 0.1f;

		private bool _dragging;
		private Vector2 _startPosition;

		public void OnBeginDrag(PointerEventData eventData)
        {
			_startPosition = eventData.pressPosition;
			ExecuteEvents.ExecuteHierarchy<IBeginDragHandler>(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }

		public void OnDrag(PointerEventData eventData)
		{
			ExecuteEvents.ExecuteHierarchy<IDragHandler>(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);

			if (_dragging) return;

			var delta = (eventData.position - _startPosition);
			delta.x /= Screen.width;
			delta.y /= Screen.height;

			if (Math.Abs(delta.x) < _thresholdX && Math.Abs(delta.y) < _thresholdY) return;
			_dragging = true;

			ExecuteEvents.Execute<IEndDragHandler>(gameObject, eventData, ExecuteEvents.endDragHandler);
			_dragStarted?.Invoke(eventData);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_dragging = false;
			ExecuteEvents.ExecuteHierarchy<IEndDragHandler>(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
		}
	}
}
