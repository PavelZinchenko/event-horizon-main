using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace ShipEditor.UI
{
    public class NavigationPanel : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler
    {
        [Range(0f,1f)][SerializeField] private float _mouseScrollSensibility = 0.1f;

		[SerializeField] private UnityEvent<PointerEventData> _dragged;
		[SerializeField] private UnityEvent<Vector2> _clicked;
		[SerializeField] private UnityEvent<Vector2> _moved;
		[SerializeField] private UnityEvent<float> _zoomed;
		[SerializeField] private float _dragThreshold = 0.5f;

		private readonly TouchCollection _touches = new();
        //private readonly Dictionary<int, Vector2> _touches = new();
        private Vector2 _velocity;
        private Vector2 _position;
		private float _touchZoomDistance;

		private bool Interactable { get; set; } = true;

        public void OnPointerClick(PointerEventData data)
        {
            if (!Interactable)
                return;

            if (!data.dragging)
                _clicked?.Invoke(Camera.main.ScreenToWorldPoint(data.position));
        }

		public void OnPointerUp(PointerEventData eventData)
		{
			_touches.Remove(eventData.pointerId);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_touches.Update(eventData.pointerId, eventData.position);
			_position = _touches.AveragePosition;
			_touchZoomDistance = 0;
		}

		public void OnDrag(PointerEventData data)
        {
			_touches.Update(data.pointerId, data.position);
        }

        public void OnBeginDrag(PointerEventData data)
        {
			if (_touches.GetElapsedTime(data.pointerId) >= _dragThreshold)
				_dragged?.Invoke(data);

            _position = _touches.AveragePosition;
            _touchZoomDistance = 0;
        }

        public void OnEndDrag(PointerEventData data)
        {
            _position = _touches.AveragePosition;
            _touchZoomDistance = 0;
        }

        public void OnScroll(PointerEventData data)
        {
            UpdateZoom(-_mouseScrollSensibility * (data.scrollDelta.y + data.scrollDelta.x));
        }

        public void OnWindowActivated(bool active)
        {
            _touches.Clear();
        }

        private void Update()
        {
            _velocity *= 1 - Time.deltaTime*5;

            var count = _touches.Count;
            if (count > 0)
            {
                var lastPosition = _position;
                _position = _touches.AveragePosition;
                var delta = (Vector2) Camera.main.ScreenToWorldPoint(_position) -
                            (Vector2) Camera.main.ScreenToWorldPoint(lastPosition);
                _moved?.Invoke(delta);
                var deltaTime = Mathf.Max(0.01f, Time.deltaTime);
                _velocity = _velocity*0.9f + 0.2f*delta/deltaTime;

                var speedLimit = Camera.main.orthographicSize*10;
                _velocity.x = Mathf.Clamp(_velocity.x, -speedLimit, speedLimit);
                _velocity.y = Mathf.Clamp(_velocity.y, -speedLimit, speedLimit);

                if (count > 1)
                {
                    var lastDistance = _touchZoomDistance;
                    _touchZoomDistance = _touches.AverageDistanceTo(_position);
                    if (lastDistance > 0.001f)
                        UpdateZoom(1f - _touchZoomDistance/lastDistance);
                }
            }
            else if (_velocity.magnitude > 0.1f)
            {
                _moved?.Invoke(_velocity*Time.deltaTime);
            }
        }

        private void UpdateZoom(float zoom)
        {
            _zoomed?.Invoke(1f + zoom);
        }
	}

	public class TouchCollection
	{
		private readonly List<TouchData> _touches = new();
		private Vector2 _summaryPosition = Vector2.zero;

		public int Count => _touches.Count();

		public void Clear()
		{
			_touches.Clear();
			_summaryPosition = Vector2.zero;
		}

		public void Update(int id, Vector2 position)
		{
			var time = Time.realtimeSinceStartup;
			var index = GetIndex(id);
			if (index < 0)
			{
				_touches.Add(new TouchData { Id = id, Position = position, PressTime = time, UpdateTime = time });
			}
			else
			{
				var data = _touches[index];
				_summaryPosition -= data.Position;
				data.Position = position;
				data.UpdateTime = time;
				_touches[index] = data;
			}

			_summaryPosition += position;
		}

		public void Remove(int id)
		{
			var index = GetIndex(id);
			if (index < 0) return;

			_summaryPosition -= _touches[index].Position;

			var lastIndex = _touches.Count - 1;
			if (index < lastIndex)
				_touches[index] = _touches[lastIndex];

			_touches.RemoveAt(lastIndex);
		}

		private int GetIndex(int id)
		{
			for (int i = 0; i < _touches.Count; ++i)
				if (_touches[i].Id == id) return i;
			return -1;
		}

		public float GetElapsedTime(int id)
		{
			var index = GetIndex(id);
			if (index < 0) return 0;

			return Time.realtimeSinceStartup - _touches[index].PressTime;
		}

		public float AverageDistanceTo(Vector2 position)
		{
			if (Count == 0) return 0;

			float totalDistance = 0;
			for (int i = 0; i < _touches.Count; ++i)
				totalDistance += Vector2.Distance(_touches[i].Position, position);

			return totalDistance / Count;
		}

		public Vector2 AveragePosition => Count > 0 ? _summaryPosition / Count : Vector2.zero;

		private struct TouchData
		{
			public int Id;
			public Vector2 Position;
			public float UpdateTime;
			public float PressTime;
		}
	}
}
