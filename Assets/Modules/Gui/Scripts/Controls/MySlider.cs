namespace UnityEngine.UIElements
{
	public class MySlider : Slider
	{
		public MySlider()
		{
			_dragger = this.Q(null, draggerUssClassName);
			_dragContainer = this.Q(null, trackerUssClassName);
		}

		private VisualElement _dragger;
		private VisualElement _dragContainer;

		public override void HandleEvent(EventBase evt)
		{
			base.HandleEvent(evt);
			if (evt is IPointerEvent || evt is IMouseEvent)
				AdjustPosition();
		}

		private void AdjustPosition()
		{
			var width = _dragContainer.contentRect.width - _dragger.contentRect.width;
			var position = _dragger.transform.position;

			if (position.x < 0)
				position.x = 0;
			else if (position.x > width)
				position.x = width;
			else
				return;
				
			_dragger.transform.position = position;
		}

		public new class UxmlFactory : UxmlFactory<MySlider, UxmlTraits> { }
	}
}
