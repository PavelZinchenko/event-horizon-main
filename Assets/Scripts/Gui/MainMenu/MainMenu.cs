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
using System.Collections.Generic;

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
            if (_enemyFleetPanel != null)
                Destroy(_enemyFleetPanel);
            _enemyFleetPanel = null;
            _quickEnemyCountTexts.Clear();
            _gameSettings.EditorText = _inputField.text;
            var useMyFleet = _useMyFleetToggle != null && _useMyFleetToggle.isOn;
            var enemyFleetSpec = string.Join(",", _quickEnemyCounts.Where(item => item.Value > 0)
                .OrderBy(item => item.Key).Select(item => item.Key + ":" + item.Value));

            switch (result)
            {
                case WindowExitCode.Option1:
                    _startBattleTrigger.Fire(new QuickCombatState.Settings
                    {
                        EasyMode = true,
                        UsePlayerFleet = useMyFleet,
                        EnemyFleetSpec = enemyFleetSpec,
                        TestShipId = _inputField.text
                    });
                    break;
                case WindowExitCode.Option2:
                    _startBattleTrigger.Fire(new QuickCombatState.Settings
                    {
                        EasyMode = false,
                        UsePlayerFleet = useMyFleet,
                        EnemyFleetSpec = enemyFleetSpec,
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
                CreateEnemyFleetButton(dialog.transform);
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
            CreateEnemyFleetButton(dialog.transform);
        }

        private void CreateEnemyFleetButton(Transform dialog)
        {
            var existing = dialog.Find("ConfigureEnemyFleet");
            if (existing != null)
            {
                var existingButton = existing.GetComponent<Button>();
                existingButton.onClick.RemoveAllListeners();
                existingButton.onClick.AddListener(OpenEnemyFleetPanel);
                return;
            }
            var buttonObject = new GameObject("ConfigureEnemyFleet", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.layer = dialog.gameObject.layer;
            buttonObject.transform.SetParent(dialog, false);
            buttonObject.transform.SetAsLastSibling();
            var layout = buttonObject.GetComponent<LayoutElement>();
            layout.minHeight = layout.preferredHeight = 62f;
            buttonObject.GetComponent<Image>().color = new Color(0.03f, 0.28f, 0.38f, 0.98f);
            buttonObject.GetComponent<Button>().onClick.AddListener(OpenEnemyFleetPanel);
            var label = CreateRuntimeText(buttonObject.transform, "Label", "配置敌方舰队", 24, TextAnchor.MiddleCenter);
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        }

        private void OpenEnemyFleetPanel()
        {
            if (_enemyFleetPanel != null) { _enemyFleetPanel.SetActive(true); return; }
            var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
            if (canvas == null) return;
            _enemyFleetPanel = new GameObject("QuickEnemyFleetPanel", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasRenderer), typeof(Image));
            _enemyFleetPanel.layer = canvas.gameObject.layer;
            var root = _enemyFleetPanel.GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            root.anchorMin = new Vector2(0.08f, 0.06f); root.anchorMax = new Vector2(0.92f, 0.94f);
            root.offsetMin = root.offsetMax = Vector2.zero;
            _enemyFleetPanel.GetComponent<Image>().color = new Color(0.015f, 0.07f, 0.11f, 0.99f);
            var overlayCanvas = _enemyFleetPanel.GetComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = canvas.sortingOrder + 100;
            _enemyFleetPanel.transform.SetAsLastSibling();

            var title = CreateRuntimeText(root, "Title", "指定敌方舰队", 30, TextAnchor.MiddleCenter);
            title.rectTransform.anchorMin = new Vector2(0f, 0.91f); title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(20f, 0f); title.rectTransform.offsetMax = new Vector2(-20f, 0f);

            var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask), typeof(ScrollRect));
            viewportObject.layer = _enemyFleetPanel.layer;
            var viewport = viewportObject.GetComponent<RectTransform>();
            viewport.SetParent(root, false);
            viewport.anchorMin = new Vector2(0.03f, 0.12f); viewport.anchorMax = new Vector2(0.97f, 0.9f);
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            viewportObject.GetComponent<Image>().color = new Color(0.02f, 0.12f, 0.17f, 0.98f);
            viewportObject.GetComponent<Mask>().showMaskGraphic = true;

            var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.layer = _enemyFleetPanel.layer;
            var content = contentObject.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f); content.offsetMin = content.offsetMax = Vector2.zero;
            var group = contentObject.GetComponent<VerticalLayoutGroup>();
            group.spacing = 5f; group.padding = new RectOffset(8, 8, 8, 8); group.childForceExpandHeight = false;
            contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = viewportObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;

            foreach (var build in _database.ShipBuildList.Where(item => item != null && item.Ship != null)
                         .GroupBy(item => item.Ship.Id.Value).Select(items => items.First())
                         .OrderBy(item => (int)item.Ship.SizeClass).ThenBy(item => item.Ship.Id.Value))
                CreateEnemyFleetRow(content, build);

            var done = CreateRuntimeButton(root, "Done", "完成", new Vector2(0.54f, 0.02f), new Vector2(0.95f, 0.105f));
            done.onClick.AddListener(() => _enemyFleetPanel.SetActive(false));
            var clear = CreateRuntimeButton(root, "Clear", "清空", new Vector2(0.05f, 0.02f), new Vector2(0.46f, 0.105f));
            clear.onClick.AddListener(() => { _quickEnemyCounts.Clear(); RefreshEnemyFleetCounts(); });
        }

        private void CreateEnemyFleetRow(RectTransform parent, ShipBuild build)
        {
            var row = new GameObject("Ship_" + build.Id.Value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            row.layer = parent.gameObject.layer; row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = new Color(0.03f, 0.19f, 0.25f, 0.95f);
            row.GetComponent<LayoutElement>().preferredHeight = 58f;
            var name = CreateRuntimeText(row.transform, "Name", _localization.GetString(build.Ship.Name), 20, TextAnchor.MiddleLeft);
            name.rectTransform.anchorMin = new Vector2(0.02f, 0f); name.rectTransform.anchorMax = new Vector2(0.63f, 1f);
            name.rectTransform.offsetMin = name.rectTransform.offsetMax = Vector2.zero;
            var minus = CreateRuntimeButton(row.GetComponent<RectTransform>(), "Minus", "−", new Vector2(0.65f, 0.12f), new Vector2(0.75f, 0.88f));
            var count = CreateRuntimeText(row.transform, "Count", "0", 22, TextAnchor.MiddleCenter);
            count.rectTransform.anchorMin = new Vector2(0.76f, 0f); count.rectTransform.anchorMax = new Vector2(0.87f, 1f);
            count.rectTransform.offsetMin = count.rectTransform.offsetMax = Vector2.zero;
            _quickEnemyCountTexts[build.Id.Value] = count;
            var plus = CreateRuntimeButton(row.GetComponent<RectTransform>(), "Plus", "+", new Vector2(0.88f, 0.12f), new Vector2(0.98f, 0.88f));
            minus.onClick.AddListener(() => SetEnemyFleetCount(build.Id.Value, -1));
            plus.onClick.AddListener(() => SetEnemyFleetCount(build.Id.Value, 1));
            RefreshEnemyFleetCount(build.Id.Value);
        }

        private void SetEnemyFleetCount(int id, int delta)
        {
            _quickEnemyCounts.TryGetValue(id, out var count);
            count = Mathf.Clamp(count + delta, 0, 99);
            if (count == 0) _quickEnemyCounts.Remove(id); else _quickEnemyCounts[id] = count;
            RefreshEnemyFleetCount(id);
        }

        private void RefreshEnemyFleetCounts()
        {
            foreach (var id in _quickEnemyCountTexts.Keys.ToArray()) RefreshEnemyFleetCount(id);
        }

        private void RefreshEnemyFleetCount(int id)
        {
            if (_quickEnemyCountTexts.TryGetValue(id, out var text))
                text.text = _quickEnemyCounts.TryGetValue(id, out var count) ? count.ToString() : "0";
        }

        private static Text CreateRuntimeText(Transform parent, string name, string value, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.layer = parent.gameObject.layer; go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = fontSize; text.alignment = alignment; text.color = Color.white;
            return text;
        }

        private static Button CreateRuntimeButton(RectTransform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.layer = parent.gameObject.layer; var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.04f, 0.36f, 0.48f, 1f);
            var text = CreateRuntimeText(rect, "Label", label, 22, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            return go.GetComponent<Button>();
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
            rect.anchorMin = new Vector2(0.04f, 0.5f);
            rect.anchorMax = new Vector2(0.62f, 0.8f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CreateBrandText(root.transform, template, "Title", "三体视界", 76, new Vector2(0, 0.6f), new Vector2(1, 1), Color.white);
            CreateBrandText(root.transform, template, "Developers", "策划&文案：白墨\n程序开发：V0idream\n舰船设计：Aqua\n音乐：巡洋舰零售\n测试群：908948524", 28, new Vector2(0, 0.08f), new Vector2(1, 0.62f), new Color(0.55f, 0.9f, 1f));
            CreateBrandText(root.transform, template, "OriginalAuthor", "原作者：Pavel Zinchenko（Event Horizon）", 22, new Vector2(0, 0f), new Vector2(1, 0.12f), new Color(0.72f, 0.76f, 0.82f));
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
        private GameObject _enemyFleetPanel;
        private readonly Dictionary<int, int> _quickEnemyCounts = new();
        private readonly Dictionary<int, Text> _quickEnemyCountTexts = new();
    }
}
