using Constructor.Ships;
using Services.Localization;
using Services.Resources;
using UnityEngine;
using UnityEngine.UI;

namespace ShipEditor.UI
{
    public class ShipItem : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _name;
		[SerializeField] private Text _classText;
		[SerializeField] private Text _levelText;

        public void Initialize(IShip ship, IResourceLocator resourceLocator, ILocalization localization)
        {
            Ship = ship;
            var icon = resourceLocator.GetSprite(ship.Model.IconImage);
            if (!(ship is EditorModeShip))
                icon = PlayerShipTextureOverrides.Get(ship.Model.OriginalShip.Id.Value,
                    icon ?? resourceLocator.GetSprite(ship.Model.ModelImage));
            if (icon != null)
            {
                _icon.sprite = icon;
                _icon.rectTransform.localScale = 1.4f * ship.Model.IconScale * Vector3.one;
            }
            else
            {
                _icon.sprite = resourceLocator.GetSprite(ship.Model.ModelImage);
                _icon.rectTransform.localScale = Vector3.one;
            }

            _icon.color = ship.ColorScheme.HsvColor;

			var shipName = localization.GetString(ship.Model.OriginalShip.Name);
            // Database-editor mode exposes every ShipBuild.  Show the build id
            // as a variant label so AI/default layouts can be selected safely.
            _name.text = ship is EditorModeShip editorShip
                ? shipName + " · 改型 " + editorShip.BuildId
                : localization.GetString(ship.Name);
			_classText.text = ship.Model.SizeClass.ToString(localization);
			var level = ship.Experience.Level;
			_levelText.text = level > 0 ? level.ToString() : "0";
		}

		public IShip Ship { get; private set; }
    }
}
