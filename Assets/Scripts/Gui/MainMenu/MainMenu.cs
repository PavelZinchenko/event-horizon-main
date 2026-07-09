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
using System.Collections;

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
            StartCoroutine(ConfigureQuickBattleFleetToggle());
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
            var useMyFleet = _useMyFleetToggle != null && _useMyFleetToggle.isOn;

            switch (result)
            {
                case WindowExitCode.Option1:
                    _startBattleTrigger.Fire(new QuickCombatState.Settings
                    {
                        EasyMode = true,
                        UsePlayerFleet = useMyFleet,
                        TestShipId = _inputField.text
                    });
                    break;
                case WindowExitCode.Option2:
                    _startBattleTrigger.Fire(new QuickCombatState.Settings
                    {
                        EasyMode = false,
                        UsePlayerFleet = useMyFleet,
                        TestShipId = _inputField.text
                    });
                    break;
            }
        }

        private IEnumerator ConfigureQuickBattleFleetToggle()
        {
            yield return null;
            var dialog = GameObject.Find(Common.WindowNames.SelectDifficultyDialog);
            if (dialog == null)
                yield break;

            var existing = dialog.transform.Find("UseMyFleet");
            if (existing != null)
            {
                _useMyFleetToggle = existing.GetComponent<Toggle>();
                yield break;
            }

            var row = new GameObject("UseMyFleet", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle), typeof(LayoutElement));
            row.layer = dialog.layer;
            row.transform.SetParent(dialog.transform, false);
            row.transform.SetAsLastSibling();
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 62f);
            var rowLayout = row.GetComponent<LayoutElement>();
            rowLayout.minHeight = 62f;
            rowLayout.preferredHeight = 62f;
            row.GetComponent<Image>().color = new Color(0.025f, 0.13f, 0.19f, 0.96f);

            var checkBackground = new GameObject("CheckBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkBackground.layer = row.layer;
            var checkRect = checkBackground.GetComponent<RectTransform>();
            checkRect.SetParent(rowRect, false);
            checkRect.anchorMin = checkRect.anchorMax = new Vector2(0f, 0.5f);
            checkRect.pivot = new Vector2(0f, 0.5f);
            checkRect.anchoredPosition = new Vector2(18f, 0f);
            checkRect.sizeDelta = new Vector2(36f, 36f);
            checkBackground.GetComponent<Image>().color = new Color(0.05f, 0.3f, 0.4f, 1f);

            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkmark.layer = row.layer;
            var markRect = checkmark.GetComponent<RectTransform>();
            markRect.SetParent(checkRect, false);
            markRect.anchorMin = new Vector2(0.2f, 0.2f);
            markRect.anchorMax = new Vector2(0.8f, 0.8f);
            markRect.offsetMin = Vector2.zero;
            markRect.offsetMax = Vector2.zero;
            checkmark.GetComponent<Image>().color = new Color(0.3f, 0.95f, 1f, 1f);

            var label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            label.layer = row.layer;
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.SetParent(rowRect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(70f, 0f);
            labelRect.offsetMax = new Vector2(-16f, 0f);
            var labelText = label.GetComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 24;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.color = Color.white;
            labelText.text = "使用我的舰队";

            _useMyFleetToggle = row.GetComponent<Toggle>();
            _useMyFleetToggle.targetGraphic = row.GetComponent<Image>();
            _useMyFleetToggle.graphic = checkmark.GetComponent<Image>();
            _useMyFleetToggle.isOn = false;
            _useMyFleetToggle.interactable = _gameSession.IsGameStarted();
            labelText.color = _useMyFleetToggle.interactable ? Color.white : new Color(0.55f, 0.58f, 0.62f);
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
        private Toggle _useMyFleetToggle;
    }
}
