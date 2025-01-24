using System;
using GameDatabase;
using GameServices.Gui;
using Session;
using GameServices.Settings;
using GameServices.SceneManager;
using Services.IapStorage;
using Services.InAppPurchasing;
using Services.Localization;
using Services.Storage;
using UnityEngine;
using CommonComponents.Signals;
using Zenject;

namespace GameServices.GameManager
{
    public class GameDataManager : MonoBehaviour, IGameDataManager
    {
        [Inject]
        private void Initialize(
            IDataStorage localStorage,
            ICloudStorage cloudStorage,
            ILocalization localization,
            SessionData sessionData,
            IIapStorage iapStorage,
            GameSettings gameSettings,
            IapPurchaseProcessor iapProcessor,
            IDatabase database,
            ISavegameExporter exporter,
            ShowMessageSignal.Trigger showMessageTrigger,
            SceneLoadedSignal sceneLoadedSignal,
            InAppPurchaseCompletedSignal inAppPurchaseCompletedSignal,
            SessionAboutToSaveSignal.Trigger sessionAboutToSaveTrigger)
        {
            _database = database;
            _localStorage = localStorage;
            _cloudStorage = cloudStorage;
            _localization = localization;
            _sessionData = sessionData;
            _iapProcessor = iapProcessor;
            _iapStorage = iapStorage;
            _gameSettings = gameSettings;
            _exporter = exporter;
            _showMessageTrigger = showMessageTrigger;
            _sceneLoadedSignal = sceneLoadedSignal;
            _inAppPurchaseCompletedSignal = inAppPurchaseCompletedSignal;
            _sessionAboutToSaveTrigger = sessionAboutToSaveTrigger;

            _sceneLoadedSignal.Event += OnLevelLoaded;
            _inAppPurchaseCompletedSignal.Event += OnIapCompleted;
        }

        ~GameDataManager()
        {
            UnityEngine.Debug.Log("GameDataManager: destructor");
        }

        public void RestorePurchases()
        {
            _iapStorage.Read(OnIapDataReceived);
        }

        private void OnIapDataReceived(IapData storedData)
        {
            if (storedData.SupporterPack > 0)
                _iapProcessor.TryProcessPurchase(IapPurchaseProcessor.SupporterPack_Id);

            if (storedData.PurchasedStars > _sessionData.Purchases.PurchasedStars)
            {
                var extraStars = storedData.PurchasedStars - _sessionData.Purchases.PurchasedStars;
                _sessionData.Purchases.PurchasedStars = storedData.PurchasedStars;
                _sessionData.Resources.Stars += extraStars;
            }
        }

        public void LoadMod(string id = null, bool allowReload = false)
        {
            if (!allowReload && _database.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                return;

            string error;

            try
            {
                SaveSession();

                if (!_database.TryLoad(id, out error))
                    throw new Exception(error);

                if (_localStorage.TryLoad(_sessionData, _database.Id))
                    UnityEngine.Debug.Log("Saved game loaded");
                else if (_database.IsEditable && _localStorage.TryImportOriginalSave(_sessionData, _database.Id))
                    UnityEngine.Debug.Log("Original saved game imported");
                else
                    _sessionData.CreateNewGame(_database.Id);

                _gameSettings.ActiveMod = _database.Id;
                _showMessageTrigger.Fire(_localization.GetString("$DatabaseLoaded"));
            }
            catch (Exception e)
            {
                _showMessageTrigger.Fire(_localization.GetString("$DatabaseLoadingError", e.Message));
                _database.LoadDefault();
                if (!_localStorage.TryLoad(_sessionData, string.Empty))
                    _sessionData.CreateNewGame(string.Empty);
            }
        }

        public void CreateNewGame()
        {
            _sessionData.CreateNewGame(_database.Id);
        }

        public void SaveGameToCloud(string filename)
        {
            UnityEngine.Debug.Log("GameDataManager.SaveGameToCloud: " + filename);
            _localStorage.Save(_sessionData);
            _cloudStorage.Save(filename, _sessionData);
        }

        public void SaveGameToCloud(ISavedGame game)
        {
            UnityEngine.Debug.Log("GameDataManager.SaveGameToCloud: " + game.Name);
            _sessionAboutToSaveTrigger.Fire();
            _localStorage.Save(_sessionData);
            game.Save(_sessionData);
        }

        public void LoadGameFromCloud(ISavedGame game)
        {
            UnityEngine.Debug.Log("GameDataManager.LoadGameFromCloud: " + game.Name);
            game.Load(_sessionData, _database.Id);
        }

        public void LoadGameFromLocalCopy()
        {
            UnityEngine.Debug.Log("GameDataManager.LoadGameFromLocalCopy");
            _cloudStorage.TryLoadFromCopy(_sessionData, _database.Id);
        }

        public void ExportProgress(ISavegameExporter.FileExportedCallback callback)
        {
            _exporter.Export(callback);
        }

        public void ImportProgress(ISavegameExporter.FileImportedCallback callback)
        {
            _exporter.Import(_sessionData, _database.Id, callback);
        }

        private void SaveSession()
        {
            if (!_sessionData.IsGameStarted())
                return;

            UnityEngine.Debug.Log("SessionData.TimePlayed = " + _sessionData.TimePlayed);
            UnityEngine.Debug.Log("GameStartTime = " + _sessionData.Game.GameStartTime);

            _sessionAboutToSaveTrigger.Fire();
            _localStorage.Save(_sessionData);
        }

        //private void SaveDatabaseToCloud()
        //{
        //    _autoSaveTime = Time.time;

        //    if (!_sessionData.IsGameStarted)
        //        return;

        //    _sessionAboutToSaveTrigger.Fire();
        //    _localStorage.Save(_sessionData);
        //    _cloudStorage.QuickSave("autosave", _localization.GetString("$AutoSave"), _sessionData);
        //}

        private void Start()
        {
            _autoSaveTime = Time.time;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                SaveSession();
        }

        private void OnApplicationFocus(bool focusStatus)
        {
            if (!focusStatus)
                SaveSession();
        }

        private void OnDisable()
        {
            SaveSession();
            Cleanup();
        }

        private void OnLevelLoaded(GameScene scene)
        {
            if (scene == GameScene.StarMap)
            {
                SaveSession();
            }
            //else if (Time.time - _autoSaveTime > 30f && _gameSettings.AutoSave &&
            //    _levelLoader.Current == LevelName.MainMenu)
            //{
            //    SaveDatabaseToCloud();
            //}

            Cleanup();
        }

        private void OnIapCompleted()
        {
            SaveSession();
        }

        private void Cleanup()
        {
            UnityEngine.Debug.Log("Cleanup");
            Resources.UnloadUnusedAssets();
            System.GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        private IDataStorage _localStorage;
        private ICloudStorage _cloudStorage;
        private ILocalization _localization;
        private SessionData _sessionData;
        private IIapStorage _iapStorage;
        private ISavegameExporter _exporter;
        private IapPurchaseProcessor _iapProcessor;
        private GameSettings _gameSettings;
        private SceneLoadedSignal _sceneLoadedSignal;
        private InAppPurchaseCompletedSignal _inAppPurchaseCompletedSignal;
        private SessionAboutToSaveSignal.Trigger _sessionAboutToSaveTrigger;
        private ShowMessageSignal.Trigger _showMessageTrigger;
        private IDatabase _database;

        private float _autoSaveTime;
    }

    public class SessionAboutToSaveSignal : SmartWeakSignal<SessionAboutToSaveSignal> {}
}
