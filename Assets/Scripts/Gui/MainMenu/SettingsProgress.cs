using System;
using System.Collections.Generic;
using System.Linq;
using Constructor.Ships;
using GameDatabase;
using GameServices.GameManager;
using GameServices.Gui;
using GameServices.Developer;
using GameStateMachine.States;
using Gui.Windows;
using Services.Localization;
using Services.Messenger;
using Services.Gui;
using Services.Storage;
using Session;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Gui.Common;

namespace Gui.MainMenu
{
    public class SettingsProgress : MonoBehaviour
    {
        [SerializeField] GameObject _deleteProgressPanel;

        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly ISessionData _session;
        [Inject] private readonly IGameDataManager _gameDataManager;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly OpenShipEditorSignal.Trigger _openShipEditorTrigger;
        [Inject] private readonly GuiHelper _guiHelper;

        [Inject]
        private void Initialize(IMessenger messenger)
        {
            ThreeBodyUiPalette.Configure(_database.UiSettings);
            messenger.AddListener(EventType.SessionCreated, OnSessionCreated);
        }

        public void DeleteProgress()
        {
            _guiHelper?.ShowConfirmation(_localization.GetString("$DeleteConfirmationText"), CreateNewGame);
        }

        public void ExportProgress()
        {
            _gameDataManager.ExportProgress(OnFileExported);
        }

        public void ImportProgress()
        {
            _gameDataManager.ImportProgress(OnFileImported);
        }

        public void OpenDefaultShipEditor()
        {
            var build = _database.ShipBuildList.FirstOrDefault();
            if (build == null)
            {
                _guiHelper?.ShowMessageBox("当前模组没有可编辑舰船");
                return;
            }

            _openShipEditorTrigger?.Fire(new EditorModeShip(build, _database));

            // Settings is an additive modal state.  MainMenu records the
            // editor request while suspended, then opens it as soon as this
            // window closes and MainMenu resumes.
            var settingsWindow = GetComponentInParent<AnimatedWindow>();
            settingsWindow?.Close(WindowExitCode.Ok);
        }

        private void OnFileImported(ISavegameExporter.Result result)
        {
            if (result == ISavegameExporter.Result.InvalidFormat)
                _guiHelper.ShowMessageBox(_localization.GetString("$InvalidSavegame"));
            else if (result == ISavegameExporter.Result.Success)
                _guiHelper.ShowMessageBox(_localization.GetString("$CloudGameLoaded"));
        }

        private void OnFileExported(bool success)
        {
            if (success)
                _guiHelper.ShowMessageBox(_localization.GetString("$CloudGameSaved"));
        }

        private void CreateNewGame()
        {
            _gameDataManager.CreateNewGame();
        }

        private void OnEnable()
        {
            FactionDeveloperSettings.Apply(_database);
            ApplyThreeBodySettingsLayout();
            OnSessionCreated();
        }

        public static void ApplyThreeBodySettingsLayout()
        {
            var controller = FindObjectsByType<SettingsProgress>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault();
            var accountButton = GameObject.Find("Canvas/Settings/Buttons/Account");
            var cloudButton = GameObject.Find("Canvas/Settings/Buttons/LoadSave");
            var modButton = GameObject.Find("Canvas/Settings/Buttons/Database");
            var buttons = accountButton != null ? accountButton.transform.parent : null;

            if (buttons != null && modButton != null)
            {
                var accountIndex = accountButton.transform.GetSiblingIndex();
                modButton.transform.SetSiblingIndex(accountIndex);
            }

            accountButton?.SetActive(false);
            cloudButton?.SetActive(false);

            var defaultShip = buttons != null ? buttons.Find("DefaultShip") : null;
            if (controller != null && buttons != null && defaultShip == null)
            {
                controller.CreateDefaultShipNavigationButton(buttons, cloudButton, modButton);
                defaultShip = buttons.Find("DefaultShip");
            }

            var factionEditor = buttons != null ? buttons.Find("FactionEditor") : null;
            if (controller != null && buttons != null && factionEditor == null)
            {
                controller.CreateFactionEditorNavigationButton(buttons, cloudButton, modButton);
                factionEditor = buttons.Find("FactionEditor");
            }

            var deleteProgress = buttons != null ? buttons.Find("DeleteProgress") : null;
            if (controller != null && buttons != null && deleteProgress == null)
            {
                controller.CreateDeleteProgressNavigationButton(buttons, cloudButton, modButton);
                deleteProgress = buttons.Find("DeleteProgress");
            }

            if (defaultShip != null)
                defaultShip.SetSiblingIndex(modButton != null ? modButton.transform.GetSiblingIndex() + 1 : buttons.childCount - 1);

            if (factionEditor != null)
                factionEditor.SetSiblingIndex(defaultShip != null
                    ? defaultShip.GetSiblingIndex() + 1
                    : modButton != null
                        ? modButton.transform.GetSiblingIndex() + 1
                        : buttons.childCount - 1);

            if (deleteProgress != null)
            {
                deleteProgress.SetSiblingIndex(factionEditor != null
                    ? factionEditor.GetSiblingIndex() + 1
                    : defaultShip != null
                        ? defaultShip.GetSiblingIndex() + 1
                    : modButton != null
                        ? modButton.transform.GetSiblingIndex() + 1
                        : buttons.childCount - 1);

                if (deleteProgress.Find("ProhibitedIcon") is RectTransform icon)
                {
                    icon.anchorMin = new Vector2(0.18f, 0.18f);
                    icon.anchorMax = new Vector2(0.82f, 0.82f);
                    icon.offsetMin = Vector2.zero;
                    icon.offsetMax = Vector2.zero;
                    icon.localPosition = Vector3.zero;
                    icon.localRotation = Quaternion.identity;
                    icon.localScale = Vector3.one;
                }
            }

            GameObject.Find("Canvas/Settings/Panels/LoadSave")?.SetActive(false);
            if (controller != null && controller._deleteProgressPanel != null)
                controller._deleteProgressPanel.SetActive(false);

            foreach (var account in FindObjectsOfType<SettingsAccount>(true))
                account.gameObject.SetActive(false);
            foreach (var loadSave in FindObjectsOfType<SettingsLoadSave>(true))
                loadSave.gameObject.SetActive(false);
        }

        private void CreateDefaultShipNavigationButton(Transform parent, GameObject cloudTemplate, GameObject modButton)
        {
            var buttonObject = CreateNavigationButton("DefaultShip", parent, cloudTemplate);

            var iconObject = new GameObject("ShipIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.layer = buttonObject.layer;
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(buttonObject.transform, false);
            iconRect.anchorMin = new Vector2(0.18f, 0.18f);
            iconRect.anchorMax = new Vector2(0.82f, 0.82f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            var icon = iconObject.GetComponent<Image>();
            icon.sprite = Resources.Load<Sprite>("Textures/GUI/select_ship_icon") ?? Resources.Load<Sprite>("Textures/GUI/ship");
            icon.preserveAspect = true;
            icon.color = ThreeBodyUiPalette.Accent;
            icon.raycastTarget = false;

            buttonObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                _guiHelper?.ShowConfirmation("仅供开发人员调试使用，是否继续编辑默认舰船布局？", OpenDefaultShipEditor);
            });
            buttonObject.transform.SetSiblingIndex(modButton != null ? modButton.transform.GetSiblingIndex() + 1 : parent.childCount - 1);
        }

        private void CreateDeleteProgressNavigationButton(Transform parent, GameObject cloudTemplate, GameObject modButton)
        {
            var buttonObject = CreateNavigationButton("DeleteProgress", parent, cloudTemplate);

            var iconObject = new GameObject("ProhibitedIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.layer = buttonObject.layer;
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(buttonObject.transform, false);
            iconRect.anchorMin = new Vector2(0.18f, 0.18f);
            iconRect.anchorMax = new Vector2(0.82f, 0.82f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            var icon = iconObject.GetComponent<Image>();
            icon.sprite = Resources.Load<Sprite>("Textures/GUI/cross2");
            icon.preserveAspect = true;
            icon.color = new Color(1f, 0.2f, 0.16f, 1f);
            icon.raycastTarget = false;

            buttonObject.GetComponent<Button>().onClick.AddListener(DeleteProgress);
            buttonObject.transform.SetSiblingIndex(modButton != null ? modButton.transform.GetSiblingIndex() + 1 : parent.childCount - 1);
        }

        private void CreateFactionEditorNavigationButton(Transform parent, GameObject cloudTemplate, GameObject modButton)
        {
            var buttonObject = CreateNavigationButton("FactionEditor", parent, cloudTemplate);
            var iconObject = new GameObject("FactionIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.layer = buttonObject.layer;
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(buttonObject.transform, false);
            iconRect.anchorMin = new Vector2(0.18f, 0.18f);
            iconRect.anchorMax = new Vector2(0.82f, 0.82f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            var icon = iconObject.GetComponent<Image>();
            icon.sprite = Resources.Load<Sprite>("Textures/GUI/faction") ?? Resources.Load<Sprite>("Textures/GUI/ship");
            icon.preserveAspect = true;
            icon.color = new Color(0.95f, 0.78f, 0.22f, 1f);
            icon.raycastTarget = false;

            buttonObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                _guiHelper?.ShowConfirmation("仅供开发人员调试使用，是否继续编辑势力生成规则？", OpenFactionEditor);
            });
            buttonObject.transform.SetSiblingIndex(modButton != null ? modButton.transform.GetSiblingIndex() + 1 : parent.childCount - 1);
        }

        private void OpenFactionEditor()
        {
            CloseFactionEditor();
            var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
            if (canvas == null)
                return;

            _factionEditorPanel = new GameObject("ThreeBodyFactionEditor", typeof(RectTransform), typeof(Canvas),
                typeof(GraphicRaycaster), typeof(CanvasRenderer), typeof(Image));
            _factionEditorPanel.layer = canvas.gameObject.layer;
            var root = _factionEditorPanel.GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            root.anchorMin = new Vector2(0.07f, 0.05f);
            root.anchorMax = new Vector2(0.93f, 0.95f);
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            var panelColor = (Color)_database.UiSettings.BackgroundDark;
            panelColor.a = 0.985f;
            _factionEditorPanel.GetComponent<Image>().color = panelColor;
            var overlay = _factionEditorPanel.GetComponent<Canvas>();
            overlay.overrideSorting = true;
            overlay.sortingOrder = canvas.sortingOrder + 200;

            var title = CreateFactionEditorText(root, "Title", "势力编辑（开发者）", 30, TextAnchor.MiddleCenter);
            SetAnchors(title.rectTransform, new Vector2(0.03f, 0.91f), new Vector2(0.97f, 0.985f));
            var hint = CreateFactionEditorText(root, "Hint", "空间站、星区和舰队刷新范围会在新生成的星图/星区中生效。", 17, TextAnchor.MiddleCenter);
            hint.color = ThreeBodyUiPalette.AccentSoft;
            SetAnchors(hint.rectTransform, new Vector2(0.03f, 0.855f), new Vector2(0.97f, 0.92f));

            var scrollObject = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.layer = root.gameObject.layer;
            var scrollRect = scrollObject.GetComponent<RectTransform>();
            scrollRect.SetParent(root, false);
            SetAnchors(scrollRect, new Vector2(0.035f, 0.13f), new Vector2(0.965f, 0.845f));
            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
            viewportObject.layer = root.gameObject.layer;
            var viewport = viewportObject.GetComponent<RectTransform>();
            viewport.SetParent(scrollRect, false);
            SetAnchors(viewport, Vector2.zero, Vector2.one);
            var viewportColor = (Color)_database.UiSettings.BackgroundDark;
            viewportColor.a = 0.12f;
            viewportObject.GetComponent<Image>().color = viewportColor;
            viewportObject.GetComponent<Mask>().showMaskGraphic = true;

            var contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentObject.layer = root.gameObject.layer;
            var content = contentObject.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0, 0);
            var group = contentObject.GetComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset(12, 12, 10, 10);
            group.spacing = 8;
            group.childControlWidth = true;
            group.childForceExpandWidth = true;
            group.childControlHeight = false;
            group.childForceExpandHeight = false;
            contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = content;

            _factionEditorRows.Clear();
            foreach (var faction in _database.FactionList.OrderBy(item => item.Id.Value))
                _factionEditorRows.Add(CreateFactionEditorRow(content, faction));

            var save = CreateFactionEditorButton(root, "Save", "保存", new Vector2(0.04f, 0.035f), new Vector2(0.31f, 0.105f));
            save.onClick.AddListener(SaveFactionEditor);
            var reset = CreateFactionEditorButton(root, "Reset", "恢复默认", new Vector2(0.36f, 0.035f), new Vector2(0.63f, 0.105f));
            reset.onClick.AddListener(() =>
            {
                FactionDeveloperSettings.ResetAll(_database);
                OpenFactionEditor();
            });
            var close = CreateFactionEditorButton(root, "Close", "关闭", new Vector2(0.68f, 0.035f), new Vector2(0.95f, 0.105f));
            close.onClick.AddListener(CloseFactionEditor);
        }

        private FactionEditorRow CreateFactionEditorRow(Transform parent, GameDatabase.DataModel.Faction faction)
        {
            var values = FactionDeveloperSettings.Read(faction);
            var rowObject = new GameObject("Faction_" + faction.Id.Value, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
            rowObject.layer = parent.gameObject.layer;
            rowObject.transform.SetParent(parent, false);
            var rowColor = (Color)_database.UiSettings.SelectionColor;
            rowColor.a = 0.70f;
            rowObject.GetComponent<Image>().color = rowColor;
            rowObject.GetComponent<LayoutElement>().preferredHeight = 124;
            var layout = rowObject.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 6, 6);
            layout.spacing = 4;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            var row = new FactionEditorRow { Faction = faction, Values = values };
            var name = CreateFactionEditorText(rowObject.transform, "Name", _localization.GetString(faction.Name) + "  [" + faction.Id.Value + "]", 20, TextAnchor.MiddleLeft);
            name.color = ThreeBodyUiPalette.AccentSoft;
            name.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

            var toggles = CreateFactionEditorLine(rowObject.transform, "Toggles", 31);
            row.TerritoriesButton = CreateFactionEditorToggle(toggles, "星区", row.Values.HasTerritories);
            row.StarbasesButton = CreateFactionEditorToggle(toggles, "空间站", row.Values.HasStarbases);
            row.WanderingButton = CreateFactionEditorToggle(toggles, "舰队刷新", row.Values.AllowsWanderingShips);
            row.TerritoriesButton.onClick.AddListener(() => { row.Values.HasTerritories = !row.Values.HasTerritories; RefreshFactionToggle(row.TerritoriesButton, "星区", row.Values.HasTerritories); });
            row.StarbasesButton.onClick.AddListener(() => { row.Values.HasStarbases = !row.Values.HasStarbases; RefreshFactionToggle(row.StarbasesButton, "空间站", row.Values.HasStarbases); });
            row.WanderingButton.onClick.AddListener(() => { row.Values.AllowsWanderingShips = !row.Values.AllowsWanderingShips; RefreshFactionToggle(row.WanderingButton, "舰队刷新", row.Values.AllowsWanderingShips); });

            var home = CreateFactionEditorLine(rowObject.transform, "HomeRange", 31);
            CreateFactionEditorLineText(home, "星区范围", 18, 150);
            row.HomeMin = CreateFactionEditorInput(home, values.HomeStarDistance, 110);
            CreateFactionEditorLineText(home, "至", 18, 35);
            row.HomeMax = CreateFactionEditorInput(home, values.HomeStarDistanceMax, 110);

            var spawn = CreateFactionEditorLine(rowObject.transform, "SpawnRange", 31);
            CreateFactionEditorLineText(spawn, "舰队刷新范围", 18, 150);
            row.SpawnMin = CreateFactionEditorInput(spawn, values.WanderingShipsDistance, 110);
            CreateFactionEditorLineText(spawn, "至", 18, 35);
            row.SpawnMax = CreateFactionEditorInput(spawn, values.WanderingShipsDistanceMax, 110);
            return row;
        }

        private void SaveFactionEditor()
        {
            foreach (var row in _factionEditorRows)
            {
                var values = row.Values;
                values.HomeStarDistance = ReadFactionNumber(row.HomeMin, values.HomeStarDistance);
                values.HomeStarDistanceMax = ReadFactionNumber(row.HomeMax, values.HomeStarDistanceMax);
                values.WanderingShipsDistance = ReadFactionNumber(row.SpawnMin, values.WanderingShipsDistance);
                values.WanderingShipsDistanceMax = ReadFactionNumber(row.SpawnMax, values.WanderingShipsDistanceMax);
                FactionDeveloperSettings.Save(row.Faction, values);
            }
            PlayerPrefs.Save();
            _guiHelper?.ShowMessageBox("势力规则已保存。重新创建星图或进入新星区后将使用这些设置。");
        }

        private void CloseFactionEditor()
        {
            if (_factionEditorPanel != null)
                Destroy(_factionEditorPanel);
            _factionEditorPanel = null;
            _factionEditorRows.Clear();
        }

        private static Transform CreateFactionEditorLine(Transform parent, string name, float preferredHeight)
        {
            var line = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            line.layer = parent.gameObject.layer;
            line.transform.SetParent(parent, false);
            line.GetComponent<LayoutElement>().preferredHeight = preferredHeight;
            var layout = line.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 7;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            return line.transform;
        }

        private Text CreateFactionEditorText(Transform parent, string name, string value, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);
            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = _database.UiSettings.TextColor;
            return text;
        }

        private void CreateFactionEditorLineText(Transform parent, string value, int fontSize, float width)
        {
            var text = CreateFactionEditorText(parent, value, value, fontSize, TextAnchor.MiddleLeft);
            text.gameObject.AddComponent<LayoutElement>().preferredWidth = width;
        }

        private Button CreateFactionEditorToggle(Transform parent, string title, bool value)
        {
            var button = CreateFactionEditorInlineButton(parent, title, value, 170);
            RefreshFactionToggle(button, title, value);
            return button;
        }

        private void RefreshFactionToggle(Button button, string title, bool value)
        {
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = title + "：" + (value ? "有" : "无");
                text.color = _database.UiSettings.ButtonTextColor;
            }
            var image = button.GetComponent<Image>();
            if (image != null)
            {
                var color = value ? (Color)_database.UiSettings.ButtonColor : (Color)_database.UiSettings.ButtonFocusColor;
                if (!value)
                    color.a = 0.92f;
                image.color = color;
            }
        }

        private Button CreateFactionEditorInlineButton(Transform parent, string name, bool value, float width)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.layer = parent.gameObject.layer;
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<LayoutElement>().preferredWidth = width;
            var text = CreateFactionEditorText(buttonObject.transform, "Label", string.Empty, 17, TextAnchor.MiddleCenter);
            text.color = _database.UiSettings.ButtonTextColor;
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
            return buttonObject.GetComponent<Button>();
        }

        private InputField CreateFactionEditorInput(Transform parent, int value, float width)
        {
            var inputObject = new GameObject("Number", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField), typeof(LayoutElement));
            inputObject.layer = parent.gameObject.layer;
            inputObject.transform.SetParent(parent, false);
            inputObject.GetComponent<Image>().color = _database.UiSettings.WindowColor;
            inputObject.GetComponent<LayoutElement>().preferredWidth = width;
            var text = CreateFactionEditorText(inputObject.transform, "Text", value.ToString(), 18, TextAnchor.MiddleCenter);
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(4, 0), new Vector2(-4, 0));
            var input = inputObject.GetComponent<InputField>();
            input.textComponent = text;
            input.contentType = InputField.ContentType.IntegerNumber;
            input.text = value.ToString();
            return input;
        }

        private Button CreateFactionEditorButton(RectTransform parent, string name, string value, Vector2 min, Vector2 max)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.layer = parent.gameObject.layer;
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetAnchors(rect, min, max);
            buttonObject.GetComponent<Image>().color = _database.UiSettings.ButtonColor;
            var text = CreateFactionEditorText(rect, "Label", value, 20, TextAnchor.MiddleCenter);
            text.color = _database.UiSettings.ButtonTextColor;
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
            return buttonObject.GetComponent<Button>();
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin ?? Vector2.zero;
            rect.offsetMax = offsetMax ?? Vector2.zero;
        }

        private static int ReadFactionNumber(InputField field, int fallback)
        {
            return field != null && int.TryParse(field.text, out var value) ? Mathf.Clamp(value, 0, 5000) : fallback;
        }

        private sealed class FactionEditorRow
        {
            public GameDatabase.DataModel.Faction Faction;
            public FactionDeveloperSettings.Values Values;
            public Button TerritoriesButton;
            public Button StarbasesButton;
            public Button WanderingButton;
            public InputField HomeMin;
            public InputField HomeMax;
            public InputField SpawnMin;
            public InputField SpawnMax;
        }

        private GameObject CreateNavigationButton(string name, Transform parent, GameObject template)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.layer = parent.gameObject.layer;
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            if (template != null && template.transform is RectTransform templateRect)
                rect.sizeDelta = templateRect.sizeDelta;
            else
                rect.sizeDelta = new Vector2(96, 96);

            var image = buttonObject.GetComponent<Image>();
            var templateImage = template != null ? template.GetComponent<Image>() : null;
            if (templateImage != null)
            {
                image.sprite = templateImage.sprite;
                image.type = templateImage.type;
            }
            image.color = _database.UiSettings.ButtonColor;

            var templateLayout = template != null ? template.GetComponent<LayoutElement>() : null;
            var layout = buttonObject.GetComponent<LayoutElement>();
            if (templateLayout != null)
            {
                layout.minWidth = templateLayout.minWidth;
                layout.minHeight = templateLayout.minHeight;
                layout.preferredWidth = templateLayout.preferredWidth;
                layout.preferredHeight = templateLayout.preferredHeight;
                layout.flexibleWidth = templateLayout.flexibleWidth;
                layout.flexibleHeight = templateLayout.flexibleHeight;
            }

            return buttonObject;
        }

        private void OnSessionCreated()
        {
            //if (gameObject.activeSelf)
            //    _deleteProgressPanel.gameObject.SetActive(_session.IsGameStarted());
        }

        private GameObject _factionEditorPanel;
        private readonly List<FactionEditorRow> _factionEditorRows = new List<FactionEditorRow>();
    }
}
