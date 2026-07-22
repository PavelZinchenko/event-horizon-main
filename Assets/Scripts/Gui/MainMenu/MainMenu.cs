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
using GameServices.Multiplayer;
using Gui.Common;

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
            IGuiManager guiManager,
            MultiplayerSession multiplayer,
            GameServices.Player.PlayerFleet playerFleet)
        {
            _startGameTrigger = startGameTrigger;
            _startBattleTrigger = startBattleTrigger;
			_openShipEditorTrigger = openShipEditorTrigger;
            _openEchopediaTrigger = openEchopediaTrigger;
            _gameSession = gameSession;
            _guiManager = guiManager;
            _multiplayer = multiplayer;
            _playerFleet = playerFleet;
            _multiplayer.StatusChanged += OnMultiplayerStatusChanged;
            _multiplayer.BattleReady += OnMultiplayerBattleReady;

            _inputField.text = _gameSettings.EditorText;
            ThreeBodyUiPalette.Configure(_database.UiSettings);
            ApplyThreeBodyBranding();

            messenger.AddListener(EventType.SessionCreated, UpdateButtons);
            messenger.AddListener(EventType.DatabaseLoaded, OnDatabaseLoaded);
            OnDatabaseLoaded();
        }

        private void OnDestroy()
        {
            if (_multiplayer == null) return;
            _multiplayer.StatusChanged -= OnMultiplayerStatusChanged;
            _multiplayer.BattleReady -= OnMultiplayerBattleReady;
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
            // The difficulty window is pooled.  Never keep references to controls
            // or overlays from an earlier opening of the quick-combat dialog.
            _useMyFleetToggle = null;
            _useConfiguredAlliesToggle = null;
            _configureAllyFleetButton = null;
            if (_enemyFleetPanel != null) Destroy(_enemyFleetPanel);
            if (_allyFleetPanel != null) Destroy(_allyFleetPanel);
            _enemyFleetPanel = null;
            _allyFleetPanel = null;
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
            if (_allyFleetPanel != null)
                Destroy(_allyFleetPanel);
            _enemyFleetPanel = null;
            _allyFleetPanel = null;
            _quickEnemyCountTexts.Clear();
            _quickAllyCountTexts.Clear();
            _gameSettings.EditorText = _inputField.text;
            var useMyFleet = _useMyFleetToggle != null && _useMyFleetToggle.isOn;
            var useConfiguredAllies = _useConfiguredAlliesToggle != null && _useConfiguredAlliesToggle.isOn;
            var enemyFleetSpec = string.Join(",", _quickEnemyCounts.Where(item => item.Value > 0)
                .OrderBy(item => item.Key).Select(item => item.Key + ":" + item.Value));
            var allyFleetSpec = string.Join(",", _quickAllyCounts.Where(item => item.Value > 0)
                .OrderBy(item => item.Key).Select(item => item.Key + ":" + item.Value));

            switch (result)
            {
                case WindowExitCode.Option1:
                    _startBattleTrigger.Fire(new QuickCombatState.Settings
                    {
                        EasyMode = true,
                        UsePlayerFleet = useMyFleet,
                        UseConfiguredAllies = useConfiguredAllies,
                        EnemyFleetSpec = enemyFleetSpec,
                        AllyFleetSpec = allyFleetSpec,
                        TestShipId = _inputField.text
                    });
                    break;
                case WindowExitCode.Option2:
                    _startBattleTrigger.Fire(new QuickCombatState.Settings
                    {
                        EasyMode = false,
                        UsePlayerFleet = useMyFleet,
                        UseConfiguredAllies = useConfiguredAllies,
                        EnemyFleetSpec = enemyFleetSpec,
                        AllyFleetSpec = allyFleetSpec,
                        TestShipId = _inputField.text
                    });
                    break;
            }
        }

        private IEnumerator ConfigureQuickBattleFleetToggle()
        {
            GameObject dialog = null;
            // Depending on frame rate and whether the pooled window was used
            // before, it may be instantiated several frames after OpenWindow.
            for (var frame = 0; frame < 30 && dialog == null; frame++)
            {
                yield return null;
                dialog = GameObject.Find(Common.WindowNames.SelectDifficultyDialog);
            }
            if (dialog == null)
                yield break;

            var existing = dialog.transform.Find("UseMyFleet");
            if (existing != null)
            {
                _useMyFleetToggle = existing.GetComponent<Toggle>();
                _useMyFleetToggle.interactable = _gameSession.IsGameStarted();
                CreateEnemyFleetButton(dialog.transform);
                CreateAllyFleetToggle(dialog.transform);
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
            row.GetComponent<Image>().color = ThreeBodyUiPalette.PanelSoft;

            var checkBackground = new GameObject("CheckBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkBackground.layer = row.layer;
            var checkRect = checkBackground.GetComponent<RectTransform>();
            checkRect.SetParent(rowRect, false);
            checkRect.anchorMin = checkRect.anchorMax = new Vector2(0f, 0.5f);
            checkRect.pivot = new Vector2(0f, 0.5f);
            checkRect.anchoredPosition = new Vector2(18f, 0f);
            checkRect.sizeDelta = new Vector2(36f, 36f);
            checkBackground.GetComponent<Image>().color = ThreeBodyUiPalette.ButtonDim;

            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkmark.layer = row.layer;
            var markRect = checkmark.GetComponent<RectTransform>();
            markRect.SetParent(checkRect, false);
            markRect.anchorMin = new Vector2(0.2f, 0.2f);
            markRect.anchorMax = new Vector2(0.8f, 0.8f);
            markRect.offsetMin = Vector2.zero;
            markRect.offsetMax = Vector2.zero;
            checkmark.GetComponent<Image>().color = ThreeBodyUiPalette.Accent;

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
            CreateAllyFleetToggle(dialog.transform);
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
            buttonObject.GetComponent<Image>().color = ThreeBodyUiPalette.Button;
            buttonObject.GetComponent<Button>().onClick.AddListener(OpenEnemyFleetPanel);
            var label = CreateRuntimeText(buttonObject.transform, "Label", "配置敌方舰队", 24, TextAnchor.MiddleCenter);
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
        }

        private void CreateAllyFleetToggle(Transform dialog)
        {
            var existing = dialog.Find("UseConfiguredAllies");
            if (existing != null)
            {
                _useConfiguredAlliesToggle = existing.GetComponent<Toggle>();
                _useConfiguredAlliesToggle.interactable = true;
                _useConfiguredAlliesToggle.onValueChanged.RemoveAllListeners();
                _useConfiguredAlliesToggle.onValueChanged.AddListener(_ => RefreshAllyFleetButton());
                CreateAllyFleetButton(dialog);
                return;
            }

            var row = new GameObject("UseConfiguredAllies", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle), typeof(LayoutElement));
            row.layer = dialog.gameObject.layer;
            row.transform.SetParent(dialog, false);
            row.transform.SetAsLastSibling();
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 62f);
            var rowLayout = row.GetComponent<LayoutElement>();
            rowLayout.minHeight = 62f;
            rowLayout.preferredHeight = 62f;
            row.GetComponent<Image>().color = ThreeBodyUiPalette.PanelSoft;

            var checkBackground = new GameObject("CheckBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkBackground.layer = row.layer;
            var checkRect = checkBackground.GetComponent<RectTransform>();
            checkRect.SetParent(rowRect, false);
            checkRect.anchorMin = checkRect.anchorMax = new Vector2(0f, 0.5f);
            checkRect.pivot = new Vector2(0f, 0.5f);
            checkRect.anchoredPosition = new Vector2(18f, 0f);
            checkRect.sizeDelta = new Vector2(36f, 36f);
            checkBackground.GetComponent<Image>().color = ThreeBodyUiPalette.ButtonDim;

            var checkmark = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkmark.layer = row.layer;
            var markRect = checkmark.GetComponent<RectTransform>();
            markRect.SetParent(checkRect, false);
            markRect.anchorMin = new Vector2(0.2f, 0.2f);
            markRect.anchorMax = new Vector2(0.8f, 0.8f);
            markRect.offsetMin = Vector2.zero;
            markRect.offsetMax = Vector2.zero;
            checkmark.GetComponent<Image>().color = ThreeBodyUiPalette.Accent;

            var label = CreateRuntimeText(row.transform, "Label", "启用配置友军", 24, TextAnchor.MiddleLeft);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(70f, 0f);
            label.rectTransform.offsetMax = new Vector2(-16f, 0f);

            _useConfiguredAlliesToggle = row.GetComponent<Toggle>();
            _useConfiguredAlliesToggle.targetGraphic = row.GetComponent<Image>();
            _useConfiguredAlliesToggle.graphic = checkmark.GetComponent<Image>();
            _useConfiguredAlliesToggle.isOn = false;
            _useConfiguredAlliesToggle.onValueChanged.AddListener(_ => RefreshAllyFleetButton());
            CreateAllyFleetButton(dialog);
        }

        private void CreateAllyFleetButton(Transform dialog)
        {
            var existing = dialog.Find("ConfigureAllyFleet");
            if (existing == null)
            {
                var buttonObject = new GameObject("ConfigureAllyFleet", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
                buttonObject.layer = dialog.gameObject.layer;
                buttonObject.transform.SetParent(dialog, false);
                buttonObject.transform.SetAsLastSibling();
                var layout = buttonObject.GetComponent<LayoutElement>();
                layout.minHeight = layout.preferredHeight = 62f;
                var label = CreateRuntimeText(buttonObject.transform, "Label", "配置友军舰队", 24, TextAnchor.MiddleCenter);
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
                _configureAllyFleetButton = buttonObject.GetComponent<Button>();
            }
            else
            {
                _configureAllyFleetButton = existing.GetComponent<Button>();
            }

            _configureAllyFleetButton.onClick.RemoveAllListeners();
            _configureAllyFleetButton.onClick.AddListener(OpenAllyFleetPanel);
            RefreshAllyFleetButton();
        }

        private void RefreshAllyFleetButton()
        {
            if (_configureAllyFleetButton == null)
                return;

            var enabled = _useConfiguredAlliesToggle != null && _useConfiguredAlliesToggle.isOn;
            // Keep the configuration page reachable even before the checkbox is
            // enabled. Opening it implicitly enables the configured ally fleet.
            _configureAllyFleetButton.interactable = true;
            var image = _configureAllyFleetButton.GetComponent<Image>();
            if (image != null)
                image.color = enabled ? ThreeBodyUiPalette.Button : ThreeBodyUiPalette.ButtonDim;
        }

        private void OpenEnemyFleetPanel()
        {
            OpenFleetPanel(false);
        }

        private void OpenAllyFleetPanel()
        {
            if (_useConfiguredAlliesToggle != null)
                _useConfiguredAlliesToggle.isOn = true;
            OpenFleetPanel(true);
        }

        private void OpenFleetPanel(bool ally)
        {
            var existingPanel = ally ? _allyFleetPanel : _enemyFleetPanel;
            if (existingPanel != null)
            {
                existingPanel.SetActive(true);
                return;
            }

            var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
            if (canvas == null) return;
            var fleetPanel = new GameObject(ally ? "QuickAllyFleetPanel" : "QuickEnemyFleetPanel", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasRenderer), typeof(Image));
            fleetPanel.layer = canvas.gameObject.layer;
            if (ally) _allyFleetPanel = fleetPanel; else _enemyFleetPanel = fleetPanel;
            var root = fleetPanel.GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            root.anchorMin = new Vector2(0.08f, 0.06f); root.anchorMax = new Vector2(0.92f, 0.94f);
            root.offsetMin = root.offsetMax = Vector2.zero;
            fleetPanel.GetComponent<Image>().color = ThreeBodyUiPalette.PanelDeep;
            var overlayCanvas = fleetPanel.GetComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = canvas.sortingOrder + 100;
            fleetPanel.transform.SetAsLastSibling();

            var title = CreateRuntimeText(root, "Title", ally ? "指定友军舰队" : "指定敌方舰队", 30, TextAnchor.MiddleCenter);
            title.rectTransform.anchorMin = new Vector2(0f, 0.91f); title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(20f, 0f); title.rectTransform.offsetMax = new Vector2(-20f, 0f);

            var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask), typeof(ScrollRect));
            viewportObject.layer = fleetPanel.layer;
            var viewport = viewportObject.GetComponent<RectTransform>();
            viewport.SetParent(root, false);
            viewport.anchorMin = new Vector2(0.03f, 0.12f); viewport.anchorMax = new Vector2(0.97f, 0.9f);
            viewport.offsetMin = viewport.offsetMax = Vector2.zero;
            viewportObject.GetComponent<Image>().color = ThreeBodyUiPalette.Panel;
            viewportObject.GetComponent<Mask>().showMaskGraphic = true;

            var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.layer = fleetPanel.layer;
            var content = contentObject.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0.5f, 1f); content.offsetMin = content.offsetMax = Vector2.zero;
            var group = contentObject.GetComponent<VerticalLayoutGroup>();
            group.spacing = 5f; group.padding = new RectOffset(8, 8, 8, 8); group.childForceExpandHeight = false;
            contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = viewportObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;

            foreach (var build in _database.ShipBuildList.Where(GameStateMachine.States.QuickCombatState.IsConfigurableQuickBattleBuild)
                         .GroupBy(item => item.Ship.Id.Value).Select(items => items.First())
                         .OrderBy(item => (int)item.Ship.SizeClass).ThenBy(item => item.Ship.Id.Value))
                CreateFleetRow(content, build, ally);

            var done = CreateRuntimeButton(root, "Done", "完成", new Vector2(0.54f, 0.02f), new Vector2(0.95f, 0.105f));
            done.onClick.AddListener(() => fleetPanel.SetActive(false));
            var clear = CreateRuntimeButton(root, "Clear", "清空", new Vector2(0.05f, 0.02f), new Vector2(0.46f, 0.105f));
            clear.onClick.AddListener(() =>
            {
                GetFleetCounts(ally).Clear();
                RefreshFleetCounts(ally);
            });
        }

        private void CreateFleetRow(RectTransform parent, ShipBuild build, bool ally)
        {
            var row = new GameObject("Ship_" + build.Id.Value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            row.layer = parent.gameObject.layer; row.transform.SetParent(parent, false);
            row.GetComponent<Image>().color = ThreeBodyUiPalette.PanelSoft;
            row.GetComponent<LayoutElement>().preferredHeight = 58f;
            var name = CreateRuntimeText(row.transform, "Name", _localization.GetString(build.Ship.Name), 20, TextAnchor.MiddleLeft);
            name.rectTransform.anchorMin = new Vector2(0.02f, 0f); name.rectTransform.anchorMax = new Vector2(0.63f, 1f);
            name.rectTransform.offsetMin = name.rectTransform.offsetMax = Vector2.zero;
            var minus = CreateRuntimeButton(row.GetComponent<RectTransform>(), "Minus", "−", new Vector2(0.65f, 0.12f), new Vector2(0.75f, 0.88f));
            var count = CreateRuntimeText(row.transform, "Count", "0", 22, TextAnchor.MiddleCenter);
            count.rectTransform.anchorMin = new Vector2(0.76f, 0f); count.rectTransform.anchorMax = new Vector2(0.87f, 1f);
            count.rectTransform.offsetMin = count.rectTransform.offsetMax = Vector2.zero;
            GetFleetCountTexts(ally)[build.Id.Value] = count;
            var plus = CreateRuntimeButton(row.GetComponent<RectTransform>(), "Plus", "+", new Vector2(0.88f, 0.12f), new Vector2(0.98f, 0.88f));
            minus.onClick.AddListener(() => SetFleetCount(build.Id.Value, -1, ally));
            plus.onClick.AddListener(() => SetFleetCount(build.Id.Value, 1, ally));
            RefreshFleetCount(build.Id.Value, ally);
        }

        private Dictionary<int, int> GetFleetCounts(bool ally) => ally ? _quickAllyCounts : _quickEnemyCounts;

        private Dictionary<int, Text> GetFleetCountTexts(bool ally) => ally ? _quickAllyCountTexts : _quickEnemyCountTexts;

        private void SetFleetCount(int id, int delta, bool ally)
        {
            var counts = GetFleetCounts(ally);
            counts.TryGetValue(id, out var count);
            count = Mathf.Clamp(count + delta, 0, 99);
            if (count == 0) counts.Remove(id); else counts[id] = count;
            RefreshFleetCount(id, ally);
        }

        private void RefreshFleetCounts(bool ally)
        {
            foreach (var id in GetFleetCountTexts(ally).Keys.ToArray()) RefreshFleetCount(id, ally);
        }

        private void RefreshFleetCount(int id, bool ally)
        {
            var counts = GetFleetCounts(ally);
            if (GetFleetCountTexts(ally).TryGetValue(id, out var text))
                text.text = counts.TryGetValue(id, out var count) ? count.ToString() : "0";
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
            go.GetComponent<Image>().color = ThreeBodyUiPalette.Button;
            var text = CreateRuntimeText(rect, "Label", label, 22, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            return go.GetComponent<Button>();
        }

        private void OnDatabaseLoaded()
        {
            ThreeBodyUiPalette.Configure(_database.UiSettings);
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
            CreateBrandText(root.transform, template, "Developers", "策划&文案：白墨\n程序开发：V0idream\n舰船设计：Aqua\n音乐：巡洋舰零售\n特效：堂桔诃德\n测试群：908948524", 28, new Vector2(0, 0.12f), new Vector2(1, 0.66f), ThreeBodyUiPalette.AccentSoft);
            CreateBrandText(root.transform, template, "OriginalAuthor", "原作者：Pavel Zinchenko（Event Horizon）", 22, new Vector2(0, 0.01f), new Vector2(1, 0.14f), new Color(0.72f, 0.76f, 0.82f));
            HideMultiplayerEntry();
        }

        private static void HideMultiplayerEntry()
        {
            var entry = GameObject.Find("ThreeBodyMultiplayerButton");
            if (entry != null)
                entry.SetActive(false);
        }

        private void CreateMultiplayerButton(Canvas canvas)
        {
            if (GameObject.Find("ThreeBodyMultiplayerButton") != null) return;
            var button = CreateRuntimeButton(canvas.GetComponent<RectTransform>(), "ThreeBodyMultiplayerButton", $"联机  ·  {AppConfig.version}",
                new Vector2(0.72f, 0.08f), new Vector2(0.96f, 0.16f));
            button.onClick.AddListener(OpenMultiplayerPanel);
        }

        private void OpenMultiplayerPanel()
        {
            if (_multiplayerPanel != null)
            {
                _multiplayerPanel.SetActive(true);
                RefreshMultiplayerPanel();
                return;
            }
            var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
            if (canvas == null) return;

            _multiplayerPanel = new GameObject("MultiplayerPanel", typeof(RectTransform), typeof(Canvas),
                typeof(GraphicRaycaster), typeof(CanvasRenderer), typeof(Image));
            _multiplayerPanel.layer = canvas.gameObject.layer;
            var root = _multiplayerPanel.GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            root.anchorMin = new Vector2(0.2f, 0.22f); root.anchorMax = new Vector2(0.8f, 0.78f);
            root.offsetMin = root.offsetMax = Vector2.zero;
            _multiplayerPanel.GetComponent<Image>().color = ThreeBodyUiPalette.PanelDeep;
            var overlay = _multiplayerPanel.GetComponent<Canvas>();
            overlay.overrideSorting = true; overlay.sortingOrder = canvas.sortingOrder + 150;

            _multiplayerTitle = CreateRuntimeText(root, "Title", $"舰队联机 · {AppConfig.version}", 34, TextAnchor.MiddleCenter);
            _multiplayerTitle.rectTransform.anchorMin = new Vector2(0.04f, 0.83f); _multiplayerTitle.rectTransform.anchorMax = new Vector2(0.96f, 0.98f);
            _multiplayerTitle.rectTransform.offsetMin = _multiplayerTitle.rectTransform.offsetMax = Vector2.zero;
            _multiplayerHint = CreateRuntimeText(root, "Hint", "主机：在 SakuraFRP 中把 TCP 隧道转发到本机 8779 端口\n客机：输入 SakuraFRP 提供的域名/IP:端口",
                20, TextAnchor.MiddleCenter);
            _multiplayerHint.rectTransform.anchorMin = new Vector2(0.05f, 0.61f); _multiplayerHint.rectTransform.anchorMax = new Vector2(0.95f, 0.84f);
            _multiplayerHint.rectTransform.offsetMin = _multiplayerHint.rectTransform.offsetMax = Vector2.zero;

            var inputObject = new GameObject("FrpAddress", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
            inputObject.layer = root.gameObject.layer;
            var inputRect = inputObject.GetComponent<RectTransform>(); inputRect.SetParent(root, false);
            inputRect.anchorMin = new Vector2(0.08f, 0.48f); inputRect.anchorMax = new Vector2(0.92f, 0.59f);
            inputRect.offsetMin = inputRect.offsetMax = Vector2.zero;
            inputObject.GetComponent<Image>().color = ThreeBodyUiPalette.PanelSoft;
            var inputText = CreateRuntimeText(inputRect, "Text", PlayerPrefs.GetString("ThreeBody.MultiplayerAddress", string.Empty), 22, TextAnchor.MiddleLeft);
            inputText.rectTransform.anchorMin = Vector2.zero; inputText.rectTransform.anchorMax = Vector2.one;
            inputText.rectTransform.offsetMin = new Vector2(14f, 0); inputText.rectTransform.offsetMax = new Vector2(-14f, 0);
            _multiplayerAddress = inputObject.GetComponent<InputField>();
            _multiplayerAddressObject = inputObject;
            _multiplayerAddress.textComponent = inputText;
            _multiplayerAddress.text = inputText.text;
            var placeholder = CreateRuntimeText(inputRect, "Placeholder", "例如：example.sakurafrp.com:12345", 20, TextAnchor.MiddleLeft);
            placeholder.rectTransform.anchorMin = Vector2.zero; placeholder.rectTransform.anchorMax = Vector2.one;
            placeholder.rectTransform.offsetMin = new Vector2(14f, 0); placeholder.rectTransform.offsetMax = new Vector2(-14f, 0);
            placeholder.color = ThreeBodyUiPalette.TextMuted;
            _multiplayerAddress.placeholder = placeholder;

            var host = CreateRuntimeButton(root, "Host", "作为主机（本地 8779）", new Vector2(0.08f, 0.32f), new Vector2(0.48f, 0.44f));
            host.onClick.AddListener(() => BeginMultiplayer(true));
            var client = CreateRuntimeButton(root, "Client", "作为客机连接", new Vector2(0.52f, 0.32f), new Vector2(0.92f, 0.44f));
            client.onClick.AddListener(() => BeginMultiplayer(false));
            _multiplayerHostButton = host;
            _multiplayerClientButton = client;

            _multiplayerStatus = CreateRuntimeText(root, "Status", _multiplayer.Status, 19, TextAnchor.MiddleCenter);
            _multiplayerStatus.rectTransform.anchorMin = new Vector2(0.05f, 0.14f); _multiplayerStatus.rectTransform.anchorMax = new Vector2(0.95f, 0.3f);
            _multiplayerStatus.rectTransform.offsetMin = _multiplayerStatus.rectTransform.offsetMax = Vector2.zero;
            _multiplayerReadyButton = CreateRuntimeButton(root, "Ready", "准备", new Vector2(0.08f, 0.025f), new Vector2(0.48f, 0.125f));
            _multiplayerReadyButton.onClick.AddListener(() => _multiplayer.SetReady());
            _multiplayerReadyLabel = _multiplayerReadyButton.GetComponentInChildren<Text>();
            var close = CreateRuntimeButton(root, "Close", "关闭", new Vector2(0.52f, 0.025f), new Vector2(0.92f, 0.125f));
            close.onClick.AddListener(CloseMultiplayerPanel);
            RefreshMultiplayerPanel();
        }

        private void BeginMultiplayer(bool host)
        {
            if (!_gameSession.IsGameStarted() || !_playerFleet.ActiveShipGroup.Ships.Any())
            {
                OnMultiplayerStatusChanged("请先进入存档并配置本地出战舰队");
                return;
            }
            SetMultiplayerRoleButtons(false);
            if (host) _multiplayer.Host();
            else
            {
                var address = _multiplayerAddress != null ? _multiplayerAddress.text.Trim() : string.Empty;
                PlayerPrefs.SetString("ThreeBody.MultiplayerAddress", address); PlayerPrefs.Save();
                _multiplayer.Connect(address);
            }
            RefreshMultiplayerPanel();
        }

        private void OnMultiplayerStatusChanged(string status)
        {
            if (_multiplayerStatus != null) _multiplayerStatus.text = status;
            if (_multiplayer != null && !_multiplayer.IsConnecting && !_multiplayer.IsActive &&
                (status == "未连接" || status.StartsWith("主机启动失败：") ||
                 status.StartsWith("连接失败：") || status.StartsWith("连接超时：") ||
                 status.StartsWith("连接已断开：")))
                SetMultiplayerRoleButtons(true);
            RefreshMultiplayerPanel();
        }

        private void CloseMultiplayerPanel()
        {
            _multiplayer.Disconnect();
            if (_multiplayerPanel != null) _multiplayerPanel.SetActive(false);
        }

        private void SetMultiplayerRoleButtons(bool interactable)
        {
            if (_multiplayerHostButton != null) _multiplayerHostButton.interactable = interactable;
            if (_multiplayerClientButton != null) _multiplayerClientButton.interactable = interactable;
        }

        private void RefreshMultiplayerPanel()
        {
            if (_multiplayerPanel == null || !_multiplayerPanel.activeSelf || _multiplayer == null)
                return;

            var connected = _multiplayer.IsActive;
            var hostWaitingForGuest = _multiplayer.IsWaitingForGuest;
            var waitingForConnection = (_multiplayer.IsConnecting || hostWaitingForGuest) && !connected;
            var inLobby = _multiplayer.IsInLobby;

            if (_multiplayerTitle != null)
                _multiplayerTitle.text = connected || hostWaitingForGuest
                    ? $"等待大厅 · {AppConfig.version}"
                    : $"舰队联机 · {AppConfig.version}";
            if (_multiplayerHint != null)
            {
                _multiplayerHint.text = connected
                    ? inLobby
                        ? "双方舰队与自定义贴图已同步。双方点击准备后，由主机开始战斗。"
                        : "已建立连接，正在交换舰队配置和自定义贴图…"
                    : hostWaitingForGuest
                        ? "本地 8779 端口已可用。请保持 SakuraFRP TCP 隧道运行，等待客机通过公网地址加入。"
                    : waitingForConnection
                        ? "主机正在本地 8779 端口等待 SakuraFRP 转发过来的客机连接。"
                        : "主机：在 SakuraFRP 中把 TCP 隧道转发到本机 8779 端口\n客机：输入 SakuraFRP 提供的域名/IP:端口";
            }

            if (_multiplayerAddressObject != null)
                _multiplayerAddressObject.SetActive(!connected && !hostWaitingForGuest);
            if (_multiplayerHostButton != null)
                _multiplayerHostButton.gameObject.SetActive(!connected && !hostWaitingForGuest);
            if (_multiplayerClientButton != null)
                _multiplayerClientButton.gameObject.SetActive(!connected && !hostWaitingForGuest);
            SetMultiplayerRoleButtons(!connected && !_multiplayer.IsConnecting && !hostWaitingForGuest);

            if (_multiplayerReadyButton != null)
            {
                _multiplayerReadyButton.gameObject.SetActive(inLobby);
                _multiplayerReadyButton.interactable = !_multiplayer.IsReady;
            }
            if (_multiplayerReadyLabel != null)
                _multiplayerReadyLabel.text = _multiplayer.IsReady ? "已准备" : "准备";
            if (_multiplayerStatus != null)
                _multiplayerStatus.text = _multiplayer.Status;
        }

        private void OnMultiplayerBattleReady()
        {
            _startBattleTrigger.Fire(new QuickCombatState.Settings
            {
                Multiplayer = true,
                UsePlayerFleet = true,
                EasyMode = false,
                TestShipId = string.Empty,
                EnemyFleetSpec = string.Empty,
                AllyFleetSpec = string.Empty,
            });
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
        private Toggle _useConfiguredAlliesToggle;
        private GameObject _enemyFleetPanel;
        private GameObject _allyFleetPanel;
        private Button _configureAllyFleetButton;
        private readonly Dictionary<int, int> _quickEnemyCounts = new();
        private readonly Dictionary<int, Text> _quickEnemyCountTexts = new();
        private readonly Dictionary<int, int> _quickAllyCounts = new();
        private readonly Dictionary<int, Text> _quickAllyCountTexts = new();
        private MultiplayerSession _multiplayer;
        private GameServices.Player.PlayerFleet _playerFleet;
        private GameObject _multiplayerPanel;
        private GameObject _multiplayerAddressObject;
        private InputField _multiplayerAddress;
        private Text _multiplayerStatus;
        private Text _multiplayerTitle;
        private Text _multiplayerHint;
        private Button _multiplayerHostButton;
        private Button _multiplayerClientButton;
        private Button _multiplayerReadyButton;
        private Text _multiplayerReadyLabel;
    }
}
