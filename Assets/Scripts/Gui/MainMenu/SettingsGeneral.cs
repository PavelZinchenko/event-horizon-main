using System;
using System.Collections.Generic;
using GameDatabase;
using GameStateMachine.States;
using Services.Settings;
using Services.Audio;
using Services.Localization;
using Services.Messenger;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Gui.MainMenu
{
    public class SettingsGeneral : MonoBehaviour
    {
        [SerializeField] Slider _soundVolumeSlider;
        [SerializeField] Slider _musicVolumeSlider;
        [SerializeField] Toggle _runInBackgroundToggle;
        [SerializeField] private Dropdown _languagesDropdown;

        [SerializeField] Toggle _lowQualityToggle;
        [SerializeField] Toggle _mediumQualityToggle;
        [SerializeField] Toggle _highQualityToggle;
        [SerializeField] Toggle _fullScreenModeToogle;

        [Inject] private readonly ISoundPlayer _soundPlayer;
        [Inject] private readonly IMusicPlayer _musicPlayer;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IGameSettings _gameSettings;
        [Inject] private readonly IDatabase _database;
		[Inject] private readonly ReloadUiSignal.Trigger _reloadGuiTrigger;

		[Inject]
        private void Initialize(IMessenger messenger)
        {
            _localizations = _localization.LoadLocalizationList();
        }

        public void OnLanguageChanged(int value)
        {
            var language = _localizations[value].folder;
            if (language.Equals(_gameSettings.Language, StringComparison.OrdinalIgnoreCase))
                return;

            _gameSettings.Language = language;
            _localization.Initialize(language, _database);
            _reloadGuiTrigger.Fire();
        }

        public void SetSoundVolume(float value)
        {
            _soundPlayer.Volume = value;
        }

        public void SetMusicVolume(float value)
        {
            _musicPlayer.Volume = value;
        }

        public void SetGraphicQuality(int quality)
        {
            _gameSettings.QualityMode = quality;
            QualitySettings.SetQualityLevel(quality < 0 ? 0 : 1);
        }

        public void RunInBackground(bool enabled)
        {
            _gameSettings.RunInBackground = enabled;
            Application.runInBackground = enabled;
        }

        public void SetFullScreenMode(bool enabled)
        {
            if (enabled)
            {
                var resolution = Screen.currentResolution;
                Screen.SetResolution(resolution.width, resolution.height, true);
            }
            else
            {
                Screen.fullScreen = false;
            }
        }

        private void InitializeLanguageDropdown()
        {
            int selectedIndex = 0;
            var selectedLanguage = _gameSettings.Language;
            var options = new List<Dropdown.OptionData>();
            for (var i = 0; i < _localizations.Count; ++i)
            {
                var item = _localizations[i];
                var sprite = Resources.Load<Sprite>("Textures/GUI/Flags/" + item.icon);
                options.Add(new Dropdown.OptionData(item.name, sprite));

                if (item.folder == selectedLanguage)
                    selectedIndex = i;
            }

            _languagesDropdown.options = options;
            _languagesDropdown.value = selectedIndex > 0 ? selectedIndex : 0;
        }

        private void OnEnable()
        {
            _musicVolumeSlider.value = _musicPlayer.Volume;
            _soundVolumeSlider.value = _soundPlayer.Volume;
            _runInBackgroundToggle.isOn = _gameSettings.RunInBackground;

            if (_gameSettings.QualityMode < 0)
                _lowQualityToggle.isOn = true;
            else if (_gameSettings.QualityMode > 0)
                _highQualityToggle.isOn = true;
            else
                _mediumQualityToggle.isOn = true;

            InitializeLanguageDropdown();

#if UNITY_STANDALONE
            _fullScreenModeToogle.isOn = Screen.fullScreen;
#endif
        }

        private List<XmlLanguageInfo> _localizations;
    }
}
