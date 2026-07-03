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
using System.Text.RegularExpressions;

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
            ApplyThreeBodyBranding();

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

            ShipBuild build = null;

            var matches = Regex.Matches(_inputField.text, @"\d+");
            if (matches.Count > 0)
                build = _database.GetShipBuild(new ItemId<ShipBuild>(int.Parse(matches[0].Value)));

            build ??= _database.ShipBuildList.FirstOrDefault();

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
                var modSprite = _resourceLocator.GetSprite(backgroundImage);
                if (modSprite != null)
                {
                    _backgroundImage.gameObject.SetActive(true);
                    _backgroundImage.SetImage(modSprite.texture);
                    _animatedBackground.SetActive(false);
                    UpdateButtons();
                    return;
                }
            }

            var preview5Background = Resources.Load<Texture2D>("Textures/Preview5/main_background_preview5");
            if (preview5Background != null)
            {
                _backgroundImage.gameObject.SetActive(true);
                _backgroundImage.SetImage(preview5Background);
                _animatedBackground.SetActive(false);
                UpdateButtons();
                return;
            }

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

        private void ApplyThreeBodyBranding()
        {
            if (GameObject.Find("ThreeBodyBranding") != null)
                return;

            var template = _credits != null ? _credits.GetComponentInChildren<Text>(true) : null;
            var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
            if (canvas == null)
                return;

            if (_credits != null)
                _credits.SetActive(false);

            var root = new GameObject("ThreeBodyBranding", typeof(RectTransform));
            root.layer = canvas.gameObject.layer;
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(canvas.transform, false);
            rect.anchorMin = new Vector2(0.04f, 0.57f);
            rect.anchorMax = new Vector2(0.62f, 0.87f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CreateBrandText(root.transform, template, "Title", "三体视界", 76, new Vector2(0, 0.53f), new Vector2(1, 1), Color.white);
            CreateBrandText(root.transform, template, "Developers", "开发者：白墨 & 空梦", 32, new Vector2(0, 0.27f), new Vector2(1, 0.55f), new Color(0.55f, 0.9f, 1f));
            CreateBrandText(root.transform, template, "OriginalAuthor", "原作者：Pavel Zinchenko（Event Horizon）", 23, new Vector2(0, 0), new Vector2(1, 0.28f), new Color(0.72f, 0.76f, 0.82f));
        }

        private static void CreateBrandText(
            Transform parent,
            Text template,
            string name,
            string value,
            int fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            gameObject.layer = parent.gameObject.layer;
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = gameObject.GetComponent<Text>();
            text.font = template != null ? template.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = fontSize;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = color;

            var outline = gameObject.GetComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.9f);
            outline.effectDistance = new Vector2(2, -2);
        }

		private OpenShipEditorSignal.Trigger _openShipEditorTrigger;
		private StartGameSignal.Trigger _startGameTrigger;
        private StartQuickBattleSignal.Trigger _startBattleTrigger;
        private OpenEhopediaSignal.Trigger _openEchopediaTrigger;
        private ISessionData _gameSession;
        private IGuiManager _guiManager;
    }
}
