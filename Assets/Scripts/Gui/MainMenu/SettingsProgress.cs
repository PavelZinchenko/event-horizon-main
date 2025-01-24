using GameServices.GameManager;
using GameServices.Gui;
using Services.Localization;
using Services.Messenger;
using Services.Storage;
using Session;
using UnityEngine;
using Zenject;

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
            OnSessionCreated();
        }

        private void OnSessionCreated()
        {
            //if (gameObject.activeSelf)
            //    _deleteProgressPanel.gameObject.SetActive(_session.IsGameStarted());
        }
    }
}
