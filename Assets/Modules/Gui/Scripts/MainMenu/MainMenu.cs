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

[RequireComponent(typeof(Gui.Generated.MainMenuDocumentModel))]
public class MainMenu : MonoBehaviour
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

    [SerializeField] private VisualTreeAsset _itemsListTemplate;
    [SerializeField] private UnityEngine.Events.UnityEvent _openSettingsButtonClicked;

    private Gui.Generated.MainMenuDocumentModel _model;

    [Inject]
    private void Initialize(IMessenger messenger)
    {
        messenger.AddListener(EventType.SessionCreated, UpdateButtons);
        messenger.AddListener(EventType.DatabaseLoaded, UpdateButtons);
    }

    private void Awake()
    {
        _model = GetComponent<Gui.Generated.MainMenuDocumentModel>();
        UpdateButtons();
    }

    private void OnEnable()
    {
        _model.NewGame_button.RegisterCallback<ClickEvent>(OnNewGameButtonClicked);
        _model.Continue_button.RegisterCallback<ClickEvent>(OnNewGameButtonClicked);
        _model.QuickCombat_button.RegisterCallback<ClickEvent>(OnQuickCombatButtonClicked);
        _model.Settings_button.RegisterCallback<ClickEvent>(OnSettingsButtonClicked);
        _model.Constructor_button.RegisterCallback<ClickEvent>(OnConstructorButtonClicked);
        _model.RestorePurchases_button.RegisterCallback<ClickEvent>(OnRestorePurchasesButtonClicked);
        _model.Exit_button.RegisterCallback<ClickEvent>(OnExitButtonClicked);
        _model.PrivacyPolicy_button.RegisterCallback<ClickEvent>(OnPrivacyPolicyClicked);
    }

    private void OnDisable()
    {
        _model.NewGame_button.UnregisterCallback<ClickEvent>(OnNewGameButtonClicked);
        _model.Continue_button.UnregisterCallback<ClickEvent>(OnNewGameButtonClicked);
        _model.QuickCombat_button.UnregisterCallback<ClickEvent>(OnQuickCombatButtonClicked);
        _model.Settings_button.UnregisterCallback<ClickEvent>(OnSettingsButtonClicked);
        _model.Constructor_button.UnregisterCallback<ClickEvent>(OnConstructorButtonClicked);
        _model.RestorePurchases_button.UnregisterCallback<ClickEvent>(OnRestorePurchasesButtonClicked);
        _model.Exit_button.UnregisterCallback<ClickEvent>(OnExitButtonClicked);
        _model.PrivacyPolicy_button.UnregisterCallback<ClickEvent>(OnPrivacyPolicyClicked);
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
        if (_model.Constructor_button_input.focusController.focusedElement == _model.Constructor_button_input)
            return;

        var text = _model.Constructor_button_input.text;
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

    private void OnPrivacyPolicyClicked(ClickEvent e)
    {
        Application.OpenURL("https://zipagames.com/policy.html");
    }

    private void OnExitButtonClicked(ClickEvent e)
    {
        ExitGame();
    }

    private void OnDialogClosed(WindowExitCode result)
    {
        var text = _model.Constructor_button_input.text;
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

        _model.Constructor_button_input.value = _gameSettings.EditorText;

#if !UNITY_ANDROID && !UNITY_STANDALONE
        _model.Exit_button.style.display = DisplayStyle.None;
#endif

#if !UNITY_IOS
        _model.RestorePurchases.style.display = DisplayStyle.None;
#endif

#if !UNITY_ANDROID && !UNITY_IOS
        _model.PrivacyPolicy.style.display = DisplayStyle.None;
#endif

        _model.NewGame_button.style.display = !gameExists ? DisplayStyle.Flex : DisplayStyle.None;
        _model.Continue_button.style.display = gameExists ? DisplayStyle.Flex : DisplayStyle.None;
        _model.Constructor_button.style.display = _database.IsEditable ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
