using System.Linq;
using GameStateMachine.States;
using Services.Gui;
using Services.Messenger;
using Session;
using Constructor.Ships;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;
using GameServices.GameManager;
using GameServices.Gui;
using GameServices.Settings;
using Services.InAppPurchasing;
using Services.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Services.Resources;

namespace Gui.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        [Inject] private readonly IInAppPurchasing _inAppPurchasing;
        [Inject] private readonly IGameDataManager _gameDataManager;
        [Inject] private readonly GameSettings _gameSettings;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly GuiHelper _guiHelper;
        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly IResourceLocator _resourceLocator;
        [Inject] private readonly OpenGameSettingsSignal.Trigger _openSettingsTrigger;

        [Inject]
        private void Initialize(
            StartGameSignal.Trigger startGameTrigger,
            StartQuickBattleSignal.Trigger startBattleTrigger,
			OpenEhopediaSignal.Trigger openEchopediaTrigger,
			OpenShipEditorSignal.Trigger openShipEditorTrigger,
			IMessenger messenger,
            ISessionData gameSession,
            IGuiManager guiManager)
        {
            _startGameTrigger = startGameTrigger;
            _startBattleTrigger = startBattleTrigger;
			_openShipEditorTrigger = openShipEditorTrigger;
            _openEchopediaTrigger = openEchopediaTrigger;
            _gameSession = gameSession;
            _guiManager = guiManager;

            _inputField.text = _gameSettings.EditorText;

            messenger.AddListener(EventType.SessionCreated, UpdateButtons);
            messenger.AddListener(EventType.DatabaseLoaded, OnDatabaseLoaded);
            OnDatabaseLoaded();
        }

        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _continueGameButton;
        [SerializeField] private Button _constructorButton;
        [SerializeField] private Button _reloadDatabaseButton;
        [SerializeField] private InputField _inputField;
        [SerializeField] private GameObject _animatedBackground;
        [SerializeField] private GameObject _credits;
        [SerializeField] private BackgroundImage _backgroundImage;

        public void StartGame()
        {
            _startGameTrigger.Fire();
        }
        
        public void StartBattle()
        {
            _guiManager.OpenWindow(Common.WindowNames.SelectDifficultyDialog, OnDialogClosed);
        }

		public void OpenSettings()
		{
			_openSettingsTrigger.Fire();
		}

        public void OpenConstructor()
        {
            _gameSettings.EditorText = _inputField.text;

            int shipId;
            ShipBuild build = null;
            if (!int.TryParse(_inputField.text.Replace("*", string.Empty), out shipId))
                UnityEngine.Debug.Log("invalid id: " + _inputField.text);
            else
                build = _database.GetShipBuild(new ItemId<ShipBuild>(shipId));

            if (build == null)
            {
                UnityEngine.Debug.Log("ship not found: " + _inputField.text);
                build = _database.ShipBuildList.FirstOrDefault();
            }

            if (build == null)
                return;

			var ship = new EditorModeShip(build, _database);
			_openShipEditorTrigger.Fire(ship);
        }
        
        public void ReloadDatabase()
        {
            _gameDataManager.LoadMod(_database.Id, true);
        }

        public void ShowPrivacyPolicy()
        {
            Application.OpenURL("https://zipagames.com/policy.html");
        }

        public void Echopedia()
        {
            _openEchopediaTrigger.Fire();
        }

        public void Exit()
        {
#if UNITY_STANDALONE
            _guiHelper.ShowConfirmation(_localization.GetString("$ExitConfirmation"), Application.Quit);
#elif !UNITY_WEBGL
            Application.Quit();
#endif
        }

        public void RestorePurchases()
        {
            _inAppPurchasing.RestorePurchases();
            _gameDataManager.RestorePurchases();
        }

        private void OnDialogClosed(WindowExitCode result)
        {
            _gameSettings.EditorText = _inputField.text;

            switch (result)
            {
                case WindowExitCode.Option1:
                    _startBattleTrigger.Fire(true, _inputField.text);
                    break;
                case WindowExitCode.Option2:
                    _startBattleTrigger.Fire(false, _inputField.text);
                    break;
            }
        }

        private void OnDatabaseLoaded()
        {
            var backgroundImage = _database.UiSettings.MainMenuBackgroundImage;
            if (backgroundImage)
            {
                var sprite = _resourceLocator.GetSprite(backgroundImage);
                _backgroundImage.gameObject.SetActive(true);
                _backgroundImage.SetImage(sprite?.texture);
                _animatedBackground.SetActive(false);
            }
            else
            {
                _backgroundImage.gameObject.SetActive(false);
                _animatedBackground.SetActive(true);
            }

            if (_database.UiSettings.NoCreditsText)
                _credits.gameObject.SetActive(false);

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            var gameExists = _gameSession.IsGameStarted();
            _startGameButton.gameObject.SetActive(!gameExists);
            _continueGameButton.gameObject.SetActive(gameExists);
            _constructorButton.gameObject.SetActive(_database.IsEditable);
            _reloadDatabaseButton.gameObject.SetActive(_database.IsEditable);
        }

		private OpenShipEditorSignal.Trigger _openShipEditorTrigger;
		private StartGameSignal.Trigger _startGameTrigger;
        private StartQuickBattleSignal.Trigger _startBattleTrigger;
        private OpenEhopediaSignal.Trigger _openEchopediaTrigger;
        private ISessionData _gameSession;
        private IGuiManager _guiManager;
    }
}
