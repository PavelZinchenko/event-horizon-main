using Services.Settings;
using GameStateMachine.States;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.MainMenu
{
    public class SettingsCombat : MonoBehaviour
    {
        [SerializeField] Slider _cameraZoomSlider;
        [SerializeField] Toggle _centerOnPlayerToggle;
		[SerializeField] Toggle _showDamageToogle;
		[SerializeField] Toggle _enemyTransmissions;

		[Inject] private readonly IGameSettings _gameSettings;
        [Inject] private readonly ConfigureControlsSignal.Trigger _configureControlsTrigger;

        public void SetCameraZoom(float value)
        {
            _gameSettings.CameraZoom = value;
        }

        public void ConfigureControls()
        {
            _configureControlsTrigger.Fire();
        }

        public void SetCenterOnPlayer(bool enabled)
        {
            _gameSettings.CenterOnPlayer = enabled;
        }

		public void ShowEnemyTransmissions(bool enabled)
		{
			_gameSettings.ShowEnemyMessages = enabled;
		}

        public void SetShowDamage(bool enabled)
        {
            _gameSettings.ShowDamage = enabled;
        }

        private void OnEnable()
        {
            _cameraZoomSlider.value = _gameSettings.CameraZoom;
            _centerOnPlayerToggle.isOn = _gameSettings.CenterOnPlayer;
            _showDamageToogle.isOn = _gameSettings.ShowDamage;
			_enemyTransmissions.isOn = _gameSettings.ShowEnemyMessages;
            CreateCombatMapSizeSelector();
        }

        private void CreateCombatMapSizeSelector()
        {
            var existing = transform.Find("CombatMapSize");
            if (existing != null)
            {
                UpdateCombatMapSizeText(existing);
                return;
            }

            var template = transform.Find("EnemyTransmissions");
            if (template == null)
                return;

            var selector = Instantiate(template.gameObject, transform);
            selector.name = "CombatMapSize";
            selector.SetActive(true);

            var toggle = selector.GetComponentInChildren<Toggle>(true);
            Graphic targetGraphic = null;
            if (toggle != null)
            {
                targetGraphic = toggle.targetGraphic;
                toggle.onValueChanged.RemoveAllListeners();
                toggle.enabled = false;
            }

            var button = selector.AddComponent<Button>();
            button.targetGraphic = targetGraphic;
            button.onClick.AddListener(() =>
            {
                ThreeBody.CombatMapSizeSettings.Next();
                UpdateCombatMapSizeText(selector.transform);
            });

            UpdateCombatMapSizeText(selector.transform);
            selector.transform.SetSiblingIndex(template.GetSiblingIndex() + 1);
        }

        private static void UpdateCombatMapSizeText(Transform selector)
        {
            var label = selector.Find("Text")?.GetComponent<Text>();
            if (label != null)
                label.text = ThreeBody.CombatMapSizeSettings.GetDisplayText();
        }

    }
}
