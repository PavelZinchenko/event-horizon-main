using Constructor.Ships;
using Services.Localization;
using Services.Resources;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ShipEditor.UI
{
    public class ShipPresetItem : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private InputField _inputField;
        [SerializeField] private UnityEvent<IShipPreset> _presedSelectd;

        public IShipPreset ShipPreset { get; private set; }
        
        public void Initialize(IShipPreset preset, IResourceLocator resourceLocator, ILocalization localization)
        {
            ShipPreset = preset;
			_icon.sprite = resourceLocator.GetSprite(preset.Ship.ModelImage);
			_inputField.text = string.IsNullOrEmpty(preset.Name) ? localization.GetString(preset.Ship.Name) : preset.Name;
		}

        public void OnTextChanged(string text)
        {
            if (ShipPreset != null)
                ShipPreset.Name = text;
        }

        public void OnValueChanged(bool selected)
        {
            if (selected && ShipPreset != null)
                _presedSelectd?.Invoke(ShipPreset);
        }
    }
}
