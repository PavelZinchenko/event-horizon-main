using GameDatabase.Enums;
using GameDatabase.Extensions;
using Services.Localization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Zenject;

namespace ShipEditor.UI
{
	public class ControlsPanel : MonoBehaviour
	{
	    [Inject] private readonly ILocalization _localization;

		[SerializeField] private GameObject _automaticPanel;
		[SerializeField] private GameObject _keyBindingLabel;
		[SerializeField] private Toggle _automaticButton;
		[SerializeField] private RadioGroup _keyGroup;
		[SerializeField] private RadioGroup _droneBehaviourGroup;
		[SerializeField] private Text _energyLevelText;
		[SerializeField] private Slider _energyLevelSlider;

		[SerializeField] private UnityEvent _controlsChanged;

		public int KeyBinding => _keyBinding;
        public int ComponentMode => _componentMode;

		public void Clear()
		{
			_keyGroup.Value = _keyBinding = 0;
		}

		public void Initialize(GameDatabase.DataModel.Component component, int key, int defaultKey, int componentMode)
		{
			var activationType = component.GetActivationType();
			if (activationType == ActivationType.None)
			{
				gameObject.SetActive(activationType != ActivationType.None);
				return;
			}
			else
			{
				gameObject.SetActive(true);
			}

			_suppressEvents = true;

			_automaticPanel.gameObject.SetActive(false);
			_keyBindingLabel.gameObject.SetActive(false);
			_keyGroup.gameObject.SetActive(false);
			_energyLevelText.gameObject.SetActive(false);
			_energyLevelSlider.gameObject.SetActive(false);
            _keyBinding = -1;
		    _componentMode = componentMode;

            _droneBehaviourGroup.gameObject.SetActive(component.DroneBay != null);
            _droneBehaviourGroup.Value = _componentMode;

            if (activationType == ActivationType.Mixed && key < 0)
			{
				_automaticPanel.gameObject.SetActive(true);
				_automaticButton.isOn = true;
				_energyLevelText.gameObject.SetActive(true);
				var level = -key;
				_energyLevelSlider.gameObject.SetActive(true);
				_energyLevelSlider.value = level;
				UpdateLevelText(level);
			}
			else if (activationType == ActivationType.Mixed && key >= 0)
			{
				_automaticPanel.gameObject.SetActive(true);
				_automaticButton.isOn = false;
				_keyGroup.gameObject.SetActive(true);
				_keyGroup.Value = _keyBinding = key;
			}
			else if (activationType == ActivationType.Manual)
			{
				_automaticPanel.gameObject.SetActive(false);
				_keyBindingLabel.gameObject.SetActive(true);
				_keyGroup.gameObject.SetActive(true);
				_keyGroup.Value = _keyBinding = key >= 0 ? key : defaultKey;
            }

		    _suppressEvents = false;
		}
        
        public void OnKeyBindingChanged(int value)
        {
            _keyBinding = value;

            if (!_suppressEvents)
                _controlsChanged?.Invoke();
		}

	    public void OnComponentModeChanged(int value)
	    {
            _componentMode = value;

            if (!_suppressEvents)
                _controlsChanged?.Invoke();
	    }
		
		public void OnActivationModeChanged(bool value)
		{
			_keyGroup.gameObject.SetActive(!value);
			_energyLevelText.gameObject.SetActive(value);
			_energyLevelSlider.gameObject.SetActive(value);
			OnKeyBindingChanged(value ? -Mathf.RoundToInt(_energyLevelSlider.value) : _keyGroup.Value);
		}

		public void OnSliderValueChanged(float value)
		{
			var level = Mathf.RoundToInt(value);
			UpdateLevelText(level);
			OnKeyBindingChanged(-level);
		}

		private void UpdateLevelText(int value)
		{
			_energyLevelText.text = _localization.GetString("$MinEnergy", Mathf.RoundToInt(value-1)*10);
		}

		private int _keyBinding;
	    private int _componentMode;
	    private bool _suppressEvents;
	}
}
