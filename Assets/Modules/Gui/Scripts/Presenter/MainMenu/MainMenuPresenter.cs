using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using GameDatabase;
using GameDatabase.Model;
using GameDatabase.DataModel;
using GameServices.Settings;
using GameStateMachine.States;
using Constructor.Ships;
using Services.Messenger;
using Services.Gui;
using Services.InAppPurchasing;
using GameServices.GameManager;
using Session;

namespace Gui.Presenter.MainMenu
{
    public partial class MainMenuPresenter : PresenterBase
    {
        [Inject] private readonly IInAppPurchasing _inAppPurchasing;
        [Inject] private readonly IGameDataManager _gameDataManager;
        [Inject] private readonly GameSettings _gameSettings;
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly IGuiManager _guiManager;
        [Inject] private readonly ISessionData _gameSession;

        [Inject] private readonly OpenConstructorSignal.Trigger _openConstructorTrigger;
        [Inject] private readonly StartGameSignal.Trigger _startGameTrigger;
        [Inject] private readonly StartQuickBattleSignal.Trigger _startBattleTrigger;

        [SerializeField] private UnityEngine.Events.UnityEvent _openSettingsButtonClicked;

        [Inject]
        private void Initialize(IMessenger messenger)
        {
            messenger.AddListener(EventType.SessionCreated, UpdateButtons);
            messenger.AddListener(EventType.DatabaseLoaded, UpdateButtons);
        }

        private void Awake()
        {
            UpdateButtons();
        }

        private void OnEnable()
        {
            MainMenu_NewGame_button.RegisterCallback<ClickEvent>(OnNewGameButtonClicked);
            MainMenu_Continue_button.RegisterCallback<ClickEvent>(OnNewGameButtonClicked);
            MainMenu_QuickCombat_button.RegisterCallback<ClickEvent>(OnQuickCombatButtonClicked);
            MainMenu_Settings_button.RegisterCallback<ClickEvent>(OnSettingsButtonClicked);
            MainMenu_Constructor_button.RegisterCallback<ClickEvent>(OnConstructorButtonClicked);
            MainMenu_RestorePurchases_button.RegisterCallback<ClickEvent>(OnRestorePurchasesButtonClicked);
            MainMenu_Exit_button.RegisterCallback<ClickEvent>(OnExitButtonClicked);
        }

        private void OnDisable()
        {
            MainMenu_NewGame_button.UnregisterCallback<ClickEvent>(OnNewGameButtonClicked);
            MainMenu_Continue_button.UnregisterCallback<ClickEvent>(OnNewGameButtonClicked);
            MainMenu_QuickCombat_button.UnregisterCallback<ClickEvent>(OnQuickCombatButtonClicked);
            MainMenu_Settings_button.UnregisterCallback<ClickEvent>(OnSettingsButtonClicked);
            MainMenu_Constructor_button.UnregisterCallback<ClickEvent>(OnConstructorButtonClicked);
            MainMenu_RestorePurchases_button.UnregisterCallback<ClickEvent>(OnRestorePurchasesButtonClicked);
            MainMenu_Exit_button.UnregisterCallback<ClickEvent>(OnExitButtonClicked);
        }

        public void ExitGame()
        {
            Debug.LogError("ExitGame");
            Application.Quit();
        }

        private void OnNewGameButtonClicked(ClickEvent e)
        {
            _startGameTrigger.Fire();
        }

        private void OnQuickCombatButtonClicked(ClickEvent e)
        {
            _guiManager.OpenWindow(Gui.Common.WindowNames.SelectDifficultyDialog, OnDialogClosed);
        }

        private void OnSettingsButtonClicked(ClickEvent e)
        {
            _openSettingsButtonClicked?.Invoke();
        }

        private void OnConstructorButtonClicked(ClickEvent e)
        {
            if (MainMenu_Constructor_button_input.focusController.focusedElement == MainMenu_Constructor_button_input)
                return;

            var text = MainMenu_Constructor_button_input.text;
            _gameSettings.EditorText = text;

            int shipId;
            ShipBuild build = null;
            if (!int.TryParse(text.Replace("*", string.Empty), out shipId))
                UnityEngine.Debug.Log("invalid id: " + text);
            else
                build = _database.GetShipBuild(new ItemId<ShipBuild>(shipId));

            if (build == null)
            {
                UnityEngine.Debug.Log("ship not found: " + text);
                build = _database.ShipBuildList.FirstOrDefault();
            }

            if (build == null)
                return;

            _openConstructorTrigger.Fire(new EditorModeShip(build, _database));
        }

        private void OnRestorePurchasesButtonClicked(ClickEvent e)
        {
            _inAppPurchasing.RestorePurchases();
            _gameDataManager.RestorePurchases();
        }

        private void OnExitButtonClicked(ClickEvent e)
        {
            ExitGame();
        }

        private void OnDialogClosed(WindowExitCode result)
        {
            var text = MainMenu_Constructor_button_input.text;
            _gameSettings.EditorText = text;

            switch (result)
            {
                case WindowExitCode.Option1:
                    _startBattleTrigger.Fire(true, text);
                    break;
                case WindowExitCode.Option2:
                    _startBattleTrigger.Fire(false, text);
                    break;
            }
        }

        private void UpdateButtons()
        {
            var gameExists = _gameSession.IsGameStarted();

            MainMenu_Constructor_button_input.value = _gameSettings.EditorText;

#if !UNITY_ANDROID && !UNITY_STANDALONE
        MainMenu_Exit_button.style.display = DisplayStyle.None;
#endif

#if !UNITY_IOS
            MainMenu_RestorePurchases.style.display = DisplayStyle.None;
#endif

            MainMenu_NewGame_button.style.display = !gameExists ? DisplayStyle.Flex : DisplayStyle.None;
            MainMenu_Continue_button.style.display = gameExists ? DisplayStyle.Flex : DisplayStyle.None;
            MainMenu_Constructor_button.style.display = _database.IsEditable ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
