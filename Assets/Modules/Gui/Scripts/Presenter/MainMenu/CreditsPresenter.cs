using UnityEngine;
using UnityEngine.UIElements;

namespace Gui.Presenter.MainMenu
{
    public partial class CreditsPresenter : PresenterBase
    {
		[SerializeField] private float _totalTime = 120f;

		private static float _startTime;

		private void Start()
		{
			if (_startTime == 0f)
				_startTime = Time.time;

			Credits_ScrollView_Title.style.display = AppConfig.alternativeTitle ? DisplayStyle.None : DisplayStyle.Flex;
			Credits_ScrollView_AlternativeTitle.style.display = AppConfig.alternativeTitle ? DisplayStyle.Flex : DisplayStyle.None;
			Credits_ScrollView.scrollOffset = new Vector2(0, float.MinValue);
		}

		private void Update()
		{
			var contentHeight = Credits_ScrollView.contentContainer.contentRect.height;
			var viewportHeight = Credits_ScrollView.contentViewport.contentRect.height;

			if (float.IsNaN(viewportHeight) || viewportHeight <= 0)
				return;

			var progress = (Time.time - _startTime) / _totalTime;
			progress -= Mathf.Floor(progress);

			var offset = new Vector2(0, (contentHeight + viewportHeight) * progress - viewportHeight);
			Credits_ScrollView.scrollOffset = offset;
		}
	}
}
