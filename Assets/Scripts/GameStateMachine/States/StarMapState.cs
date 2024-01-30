using System.Collections.Generic;
using GameServices.SceneManager;
using Combat.Domain;
using CommonComponents.Signals;
using Session;
using Zenject;
using GameServices.Quests;
using Galaxy;
using GameServices.Player;
using Services.Gui;
using Services.Audio;
using Constructor.Ships;
using Domain.Quests;
using Game.Exploration;
using GameDatabase.DataModel;
using GameModel.Quests;
using UniRx;

namespace GameStateMachine.States
{
    public class StarMapState : BaseState, IQuestActionProcessor
    {
        [Inject]
        public StarMapState(
			IStateMachine stateMachine,
            GameStateFactory gameStateFactory,
			IQuestManager questManager,
			ISessionData session,
            IGuiManager guiManager,
            MotherShip motherShip,
            PlayerResources playerResources,
			StarData starData,
            QuestCombatModelFacctory questCombatModelFacctory,
            InventoryFactory inventoryFactory,
            RetreatSignal retreatSignal,
            IMusicPlayer musicPlayer,
			StartTravelSignal startTravelSignal, 
			StartBattleSignal startBattleSignal,
			QuestActionRequiredSignal questActionRequiredSignal,
			QuestEventSignal.Trigger questEventTrigger,
            OpenSkillTreeSignal openSkillTreeSignal,
            OpenShipEditorSignal openShipEditorSignal,
            OpenShopSignal openShopSignal,
            OpenWorkshopSignal openWorkshopSignal,
			OpenShipyardSignal openShipyardSignal,
            OpenEhopediaSignal openEhopediaSignal,
            StartExplorationSignal startExplorationSignal,
            PlayerPositionChangedSignal playerPositionChangedSignal,
            ExitSignal exitSignal,
            EscapeKeyPressedSignal escapeKeyPressedSignal)
            : base(stateMachine, gameStateFactory)
        {
			_questManager = questManager;
            _questEventTrigger = questEventTrigger;
			_session = session;
			_starData = starData;
            _motherShip = motherShip;
            _playerResources = playerResources;
            _inventoryFactory = inventoryFactory;
            _guiManager = guiManager;
            _musicPlayer = musicPlayer;
            _questCombatModelFacctory = questCombatModelFacctory;
            _retreatSignal = retreatSignal;
			_retreatSignal.Event += OnRetreat;
            _startTravelSignal = startTravelSignal;
            _startTravelSignal.Event += OnStartTravel;
			_startBattleSignal = startBattleSignal;
            _startBattleSignal.Event += OnStartBattle;
			_questActionRequiredSignal = questActionRequiredSignal;
			_questActionRequiredSignal.Event += OnQuestActionRequired;
            _exitSignal = exitSignal;
            _exitSignal.Event += OnExit;
            _openSkillTreeSignal = openSkillTreeSignal;
            _openSkillTreeSignal.Event += OnOpenSkillTree;
            _openShipEditorSignal = openShipEditorSignal;
            _openShipEditorSignal.Event += OnOpenShipEditor;
            _openShopSignal = openShopSignal;
            _openShopSignal.Event += OnOpenShop;
            _openWorkshopSignal = openWorkshopSignal;
            _openWorkshopSignal.Event += OnOpenWorkshop;
            _escapeKeyPressedSignal = escapeKeyPressedSignal;
            _escapeKeyPressedSignal.Event += OnEscapePressed;
            _playerPositionChangedSignal = playerPositionChangedSignal;
            _playerPositionChangedSignal.Event += OnPlayerPositionChanged;
            _openShipyardSignal = openShipyardSignal;
            _openShipyardSignal.Event += OnOpenShipyard;
            _startExplorationSignal = startExplorationSignal;
            _startExplorationSignal.Event += OnStartExploration;
            _openEhopediaSignal = openEhopediaSignal;
            _openEhopediaSignal.Event += OnOpenEhopedia;
        }

		public override StateType Type => StateType.StarMap;

		public override IEnumerable<GameScene> RequiredScenes { get { yield return GameScene.StarMap; } }

		protected override void OnActivate()
		{
            _musicPlayer.Play(AudioTrackType.Game);

            if (!string.IsNullOrEmpty(DesiredWindowOnActivate))
            {
                var id = DesiredWindowOnActivate;
                DesiredWindowOnActivate = string.Empty;
                _guiManager.OpenWindow(id);
            }

            CheckStatus();
        }

        private void OnPlayerPositionChanged(int position)
        {
			if (Condition == GameStateCondition.Active)
				CheckStatus();
        }

        private void CheckStatus()
        {
            UpdateQuests();
            if (CheckStarGuardian()) return;
        }

        private void OnRetreat()
		{
			LoadStateAdditive(StateFactory.CreateRetreatState());
		}

        private void OnStartTravel(int destination)
        {
            var requiredFuel = _motherShip.CalculateRequiredFuel(_motherShip.Position, destination);
			bool showConfirmation = _motherShip.ViewMode == ViewMode.GalaxyMap;
			if (requiredFuel > 1 && _playerResources.Fuel < requiredFuel) showConfirmation = true;
			if (requiredFuel >= 3) showConfirmation = true;
			
			if (showConfirmation)
				LoadStateAdditive(StateFactory.CreateDialogState(Gui.StarMap.WindowNames.FlightConfirmationDialog,
					new WindowArgs(destination), code => OnFlightConfirmationDialogClosed(destination, code)));
			else
				LoadStateAdditive(StateFactory.CreateTravelState(destination));
		}

		private void OnFlightConfirmationDialogClosed(int destination, WindowExitCode code)
        {
            if (code == WindowExitCode.Ok)
                LoadStateAdditive(StateFactory.CreateTravelState(destination));
        }

		private void OnStartBattle(ICombatModel combatModel, System.Action<ICombatModel> onCompletedAction)
        {
			LoadState(StateFactory.CreateCombatState(combatModel, onCompletedAction));
        }

        private void OnStartExploration(Planet planet)
        {
            LoadState(StateFactory.CreateExplorationState(planet));
        }

        private void OnQuestActionRequired()
		{
			UpdateQuests();
		}

        private void OnEscapePressed()
        {
			if (Condition != GameStateCondition.Active)
				return;

            if (_motherShip.ViewMode != ViewMode.StarMap)
                _motherShip.ViewMode = ViewMode.StarMap;
            else
                OnExit();
        }

        private void OnExit()
        {
			LoadState(StateFactory.CreateMainMenuState());
        }

        private void OnOpenSkillTree()
        {
            LoadState(StateFactory.CreateSkillTreeState());
        }

        private void OnOpenShipEditor(IShip ship)
        {
            LoadState(StateFactory.CreateShipEditorState(new ShipEditorState.Context { Ship = ship, NextState = this }));
            DesiredWindowOnActivate = Gui.StarMap.WindowNames.HangarWindow;
        }

        private void OnOpenShop(IInventory marketInventory, IInventory playerInventory)
        {
            LoadStateAdditive(StateFactory.CreateDialogState(Gui.StarMap.WindowNames.MarketDialog, new WindowArgs(marketInventory, playerInventory)));
        }

        private void OnOpenWorkshop(Faction faction, int level)
        {
            LoadStateAdditive(StateFactory.CreateDialogState(Gui.StarMap.WindowNames.WorkshopDialog, new WindowArgs(faction, level)));
        }

        private void OnOpenShipyard(Faction faction, int level)
        {
            LoadStateAdditive(StateFactory.CreateDialogState(Gui.StarMap.WindowNames.ShipyardWindow, new WindowArgs(faction, level)));
        }

        public void OnOpenEhopedia()
        {
            LoadStateAdditive(StateFactory.CreateEchopediaState());
        }

        private bool CheckStarGuardian()
		{
			if (Condition != GameStateCondition.Active)
				return false;

			var star = _session.StarMap.PlayerPosition;
			var guardian = _starData.GetOccupant(star);

		    if (guardian.IsAggressive)
		    {
		        UnityEngine.Debug.Log("Attacked by occupants");
		        guardian.Attack();
                return true;
            }

            return false;
        }

		private void UpdateQuests()
		{
			if (Condition != GameStateCondition.Active)
				return;

		    if (_questManager.ActionRequired)
		        _questManager.InvokeAction(this);
		}

        public void ShowUiDialog(IUserInteraction userInteraction)
        {
            LoadStateAdditive(StateFactory.CreateQuestState(userInteraction));
        }

        public void Retreat()
        {
            OnRetreat();
        }

        public void SetCharacterRelations(int characterId, int value, bool additive)
        {
            _session.Quests.SetCharacterRelations(characterId, additive ? value + _session.Quests.GetCharacterRelations(characterId) : value);
        }

        public void SetFactionRelations(int starId, int value, bool additive)
        {
            _session.Quests.SetFactionRelations(starId, additive ? value + _session.Quests.GetFactionRelations(starId) : value);
        }

        public void StartQuest(QuestModel quest)
        {
            _questManager.StartQuest(quest);
        }

        public void OpenShipyard(Faction faction, int level)
        {
            OnOpenShipyard(faction, level);
        }

        public void OpenWorkshop(Faction faction, int level)
        {
            OnOpenWorkshop(faction, level);
        }

        public void CaptureStarBase(int starId, bool capture)
        {
            _starData.GetRegion(starId).IsCaptured = capture;
        }

        public void ChangeFaction(int starId, Faction faction)
        {
            _starData.GetRegion(starId).Faction = faction;
        }

        public void SuppressOccupants(int starId, bool destroy)
        {
            _starData.GetOccupant(starId).Suppress(destroy);
        }

        public void StartCombat(QuestEnemyData enemyData, ILoot specialLoot)
        {
            StartCombat(_questCombatModelFacctory.CreateCombatModel(enemyData, specialLoot));
        }

        public void AttackOccupants(int starId)
        {
			StartCombat(_starData.GetOccupant(starId).CreateCombatModelBuilder().Build());
        }

		public void AttackStarbase(int starId)
		{
			StartCombat(_starData.GetStarbase(starId).CreateCombatModelBuilder().Build());
		}

		public void StartTrading(ILoot merchantItems)
        {
            OnOpenShop(_inventoryFactory.CreateQuestInventory(merchantItems), _inventoryFactory.CreatePlayerInventory());
        }

		private void StartCombat(ICombatModel model)
		{
			LoadState(StateFactory.CreateCombatState(model, value => _questEventTrigger.Fire(new CombatEventData(value.IsVictory()))));
		}

        private readonly IQuestManager _questManager;
		private readonly ISessionData _session;
		private readonly StarData _starData;
        private readonly MotherShip _motherShip;
        private readonly PlayerResources _playerResources;
        private readonly InventoryFactory _inventoryFactory;
        private readonly IGuiManager _guiManager;
        private readonly IMusicPlayer _musicPlayer;
        private readonly QuestCombatModelFacctory _questCombatModelFacctory;
        private readonly RetreatSignal _retreatSignal;
        private readonly StartTravelSignal _startTravelSignal;
        private readonly StartBattleSignal _startBattleSignal;
		private readonly QuestActionRequiredSignal _questActionRequiredSignal;
        private readonly QuestEventSignal.Trigger _questEventTrigger;
        private readonly OpenSkillTreeSignal _openSkillTreeSignal;
		private readonly OpenShipEditorSignal _openShipEditorSignal;
        private readonly OpenShopSignal _openShopSignal;
        private readonly OpenWorkshopSignal _openWorkshopSignal;
        private readonly OpenShipyardSignal _openShipyardSignal;
        private readonly OpenEhopediaSignal _openEhopediaSignal;
        private readonly ExitSignal _exitSignal;
        private readonly EscapeKeyPressedSignal _escapeKeyPressedSignal;
        private readonly PlayerPositionChangedSignal _playerPositionChangedSignal;
        private readonly StartExplorationSignal _startExplorationSignal;

        private string DesiredWindowOnActivate { get; set; }

        public class Factory : Factory<StarMapState> { }
    }

	public class RetreatSignal : SmartWeakSignal<RetreatSignal> {}
	public class StartTravelSignal : SmartWeakSignal<StartTravelSignal, int> {}
	public class StartBattleSignal : SmartWeakSignal<StartBattleSignal, ICombatModel, System.Action<ICombatModel>> {}
	public class OpenSkillTreeSignal : SmartWeakSignal<OpenSkillTreeSignal> {}
	public class OpenShipEditorSignal : SmartWeakSignal<OpenShipEditorSignal, IShip> {}
	public class OpenShopSignal : SmartWeakSignal<OpenShopSignal, IInventory, IInventory> {}
	public class OpenWorkshopSignal : SmartWeakSignal<OpenWorkshopSignal, Faction, int> {}
    public class OpenShipyardSignal : SmartWeakSignal<OpenShipyardSignal, Faction, int> {}
    public class StartExplorationSignal : SmartWeakSignal<StartExplorationSignal, Planet> {}
    public class OpenEhopediaSignal : SmartWeakSignal<OpenEhopediaSignal> {}
}
