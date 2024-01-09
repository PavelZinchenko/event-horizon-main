using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Zenject;
using Services.Resources;
using Constructor;
using GameDatabase.Model;

namespace ShipEditor.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class DraggableComponent : MonoBehaviour, IDragHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler
	{
		[Inject] private readonly IResourceLocator _resourceLocator;

		[SerializeField] private ComponentImage _icon;
		[SerializeField] private CanvasTransformHelper _helper;

		[SerializeField] private UnityEvent<Content, Vector2> _dropped;
		[SerializeField] private UnityEvent<Content, Vector2> _dragging;

		private RectTransform _rectTransform;

		private Content _content;

		private RectTransform RectTransform
		{
			get
			{
				if (_rectTransform == null) 
					_rectTransform = GetComponent<RectTransform>();

				return _rectTransform;
			}
		}

		public void Initialize(Content content, PointerEventData eventData)
		{
			_content = content;
			var blockSize = _helper.GetCellSize();

			gameObject.SetActive(true);
            var size = content.Layout.Size * blockSize;
            RectTransform.position = eventData.position;
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            _icon.SetIcon(_resourceLocator.GetSprite(content.Icon), content.Layout.Data, content.Layout.Size, content.Color);

            eventData.pointerDrag = gameObject;
            ExecuteEvents.Execute<IBeginDragHandler>(gameObject, eventData, ExecuteEvents.beginDragHandler);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform.position = eventData.position;
			_dragging?.Invoke(_content, _helper.ScreenToWorld(eventData.position));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
			gameObject.SetActive(false);
			_dropped?.Invoke(_content, _helper.ScreenToWorld(eventData.position));
		}

		public readonly struct Content
		{
			public readonly ComponentInfo Component;
			public readonly int KeyBinding;
			public readonly int Behaviour;

			public Layout Layout => Component.Data.Layout;
			public SpriteId Icon => Component.Data.Icon;
			public Color Color => Component.Data.Color;

			public Content(ComponentInfo component, int keyBinding = 0, int behaviour = 0)
			{
				Component = component;
				KeyBinding = keyBinding;
				Behaviour = behaviour;
			}
		}
	}
}
