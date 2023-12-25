namespace UnityEngine.UIElements
{
	public class MyIntSlider : SliderInt
	{
		public MyIntSlider()
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
			position.x = width * (value - lowValue) / (highValue - lowValue);
			_dragger.transform.position = position;
		}

		public new class UxmlFactory : UxmlFactory<MyIntSlider, UxmlTraits> { }
	}
}
