using UnityEngine;
using UnityEngine.UI;
using Gui.Theme;

namespace Gui
{
	public class ImagesColorSelector : MonoBehaviour
	{
        [SerializeField] private ThemeColor _enabledColor = ThemeColor.Window;
        [SerializeField] private ThemeColor _disabledColor = ThemeColor.BackgroundDark;
        
        private Color EnabledColor => UiTheme.Current.GetColor(_enabledColor);
		private Color DisabledColor => UiTheme.Current.GetColor(_disabledColor);

        public int TotalCount
		{
			get { return _totalCount; }
			set 
			{
				if (_totalCount != value)
				{
					_totalCount = value;
					UpdateColors();
				}
			}
		}

		public int EnabledCount
		{
			get { return _enabledCount; }
			set 
			{
				if (_enabledCount != value)
				{
					_enabledCount = value;
					UpdateColors();
				}
			}
		}
		
		private void Awake()
		{
			UpdateColors();
		}

		private void UpdateColors()
		{
			var index = 0;
			Image template = null;
			foreach (Transform item in transform)
			{
				var image = item.GetComponent<Image>();
				if (image == null)
					continue;
				template = image;
				if (index >= _totalCount)
					item.gameObject.SetActive(false);
				else
				{
					item.gameObject.SetActive(true);
					image.color = index < _enabledCount ? EnabledColor : DisabledColor;
				}

				index++;
			}

			while (index < _totalCount)
			{
				var image = (Image)GameObject.Instantiate(template);
				image.rectTransform.SetParent(template.rectTransform.parent);
				image.rectTransform.localScale = Vector3.one;
				image.color = index < _enabledCount ? EnabledColor : DisabledColor;
				index++;
			}
		}

		private int _enabledCount = -1;
		private int _totalCount = -1;
	}
}
