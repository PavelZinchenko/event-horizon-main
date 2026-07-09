using GameServices.GameManager;
using GameServices.Gui;
using Services.Localization;
using Services.Messenger;
using Services.Storage;
using Session;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Linq;

namespace Gui.MainMenu
{
    public class SettingsProgress : MonoBehaviour
    {
        [SerializeField] GameObject _deleteProgressPanel;

        [Inject] private readonly ILocalization _localization;
        [Inject] private readonly ISessionData _session;
        [Inject] private readonly IGameDataManager _gameDataManager;
        [InjectOptional] private readonly GuiHelper _guiHelper;

        [Inject]
        private void Initialize(IMessenger messenger)
        {
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

            var deleteProgress = buttons != null ? buttons.Find("DeleteProgress") : null;
            if (controller != null && buttons != null && deleteProgress == null)
            {
                controller.CreateDeleteProgressNavigationButton(buttons, cloudButton, modButton);
                deleteProgress = buttons.Find("DeleteProgress");
            }

            if (deleteProgress != null)
            {
                deleteProgress.SetSiblingIndex(modButton != null
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

        private void CreateDeleteProgressNavigationButton(Transform parent, GameObject cloudTemplate, GameObject modButton)
        {
            var buttonObject = new GameObject("DeleteProgress", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.layer = parent.gameObject.layer;
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            if (cloudTemplate != null && cloudTemplate.transform is RectTransform cloudRect)
                rect.sizeDelta = cloudRect.sizeDelta;
            else
                rect.sizeDelta = new Vector2(96, 96);

            var templateImage = cloudTemplate != null ? cloudTemplate.GetComponent<Image>() : null;
            var image = buttonObject.GetComponent<Image>();
            if (templateImage != null)
            {
                image.sprite = templateImage.sprite;
                image.type = templateImage.type;
                image.color = templateImage.color;
            }
            else
            {
                image.color = new Color(0.12f, 0.12f, 0.16f, 0.9f);
            }

            var templateLayout = cloudTemplate != null ? cloudTemplate.GetComponent<LayoutElement>() : null;
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

        private void OnSessionCreated()
        {
            //if (gameObject.activeSelf)
            //    _deleteProgressPanel.gameObject.SetActive(_session.IsGameStarted());
        }
    }
}
