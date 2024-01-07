using System;
using Combat.Domain;
using Combat.Scene;
using Constructor.Ships;
using Domain.Player;
using Domain.Quests;
using Economy;
using Economy.ItemType;
using Economy.Products;
using Galaxy;
using Galaxy.StarContent;
using Game;
using Game.Exploration;
using GameModel.Quests;
using GameServices;
using GameServices.Database;
using GameServices.Economy;
using GameServices.GameManager;
using GameServices.Gui;
using GameServices.Multiplayer;
using GameServices.Player;
using GameServices.Quests;
using GameServices.Random;
using GameServices.Research;
using GameServices.Settings;
using GameStateMachine;
using GameStateMachine.States;
using Services.Advertisements;
using Services.Gui;
using Services.InAppPurchasing;
using Services.Input;
using Services.InternetTime;
using Services.Messenger;
using Session;
using Session.Content;
using UnityEngine;
using Zenject;
using CommonComponents.Signals;
using PlayerInventory = GameServices.Player.PlayerInventory;

namespace Installers
{
    public class GameInstaller : MonoInstaller<GameInstaller>
    {
        [SerializeField] GameModel.Config _config;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<RandomGenerator>().AsSingle();

#if LICENSE_OPENSOURCE || IAP_DISABLED
            Container.BindInterfacesTo<InAppPurchasingStub>().AsSingle();
#elif IAP_UNITY
			Container.BindInterfacesTo<InAppPurchasing>().AsSingle();
#endif

            Container.Bind<IapPurchaseProcessor>().AsSingle();

            Container.Bind<GameModel.Config>().FromInstance(_config);

            Container.Bind<IGameDataManager>().To<GameDataManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<StarMap>().AsSingle();

            Container.BindInterfacesAndSelfTo<Research>().AsSingle();

            Container.Bind<OfflineMultiplayer>().AsSingle().NonLazy();

            Container.Bind<ItemTypeFactory>().AsCached();
            Container.Bind<ProductFactory>().AsCached();
            Container.Bind<LootGenerator>().AsCached();
            Container.Bind<ModificationFactory>().AsCached();

            Container.BindFactory<CombatModelBuilder, CombatModelBuilder.Factory>();
            Container.BindSignal<ShipCreatedSignal>();
            Container.BindTrigger<ShipCreatedSignal.Trigger>();
            Container.BindSignal<ShipDestroyedSignal>();
            Container.BindTrigger<ShipDestroyedSignal.Trigger>();

            Container.Bind<Cheats>().AsCached();
            Container.Bind<DatabaseCodesProcessor>().AsCached();

            Container.Bind<GuiHelper>().AsCached();
            Container.Bind<HolidayManager>().AsSingle();
            Container.Bind<NotificationManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<GameTime>().AsSingle().NonLazy();

            Container.BindInterfacesTo<Technologies>().AsSingle();
            Container.Bind<Skills>().AsSingle();
            Container.Bind<InventoryFactory>().AsSingle();
            Container.Bind<Planet.Factory>().AsCached();

            Container.BindInterfacesTo<SignalsTranslator>().AsSingle().NonLazy();

            BindPlayerData();
            BindQuestManager();
            BindStarContent();
            BindDatabase();
            BindStateMachine();
            BindLegacyServices();
            BindSignals();
        }

        private void BindPlayerData()
        {
            Container.BindInterfacesAndSelfTo<PlayerSkills>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerFleet>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerResources>().AsSingle();
            Container.BindInterfacesAndSelfTo<MotherShip>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerInventory>().AsSingle();
            Container.BindInterfacesAndSelfTo<SupplyShip>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DailyReward>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<StarMapManager>().AsSingle().NonLazy();
        }

        private void BindQuestManager()
        {
            Container.BindInterfacesTo<QuestManagerContext>().AsSingle();
            Container.BindInterfacesTo<QuestBuilderContext>().AsSingle();
            Container.BindInterfacesTo<QuestManagerEventProvider>().AsSingle();
            Container.BindInterfacesTo<QuestDataProvider>().AsSingle();
            Container.BindInterfacesTo<StarMapDataProvider>().AsSingle();
            Container.BindInterfacesTo<GameDataProvider>().AsSingle();
            Container.BindInterfacesTo<CharacterDataProvider>().AsSingle();
            Container.BindInterfacesTo<InventoryDataProvider>().AsSingle();
            Container.BindInterfacesTo<LootItemFactory>().AsSingle();

            Container.Bind<QuestCombatModelFacctory>().AsSingle();
            Container.Bind<QuestFactory>().AsSingle();
            Container.Bind<RequirementsFactory>().AsSingle();

            Container.BindInterfacesTo<QuestManager>().AsSingle().NonLazy();

            Container.BindSignal<QuestListChangedSignal>();
            Container.BindTrigger<QuestListChangedSignal.Trigger>();
            Container.BindSignal<QuestActionRequiredSignal>();
            Container.BindTrigger<QuestActionRequiredSignal.Trigger>();
            Container.BindSignal<QuestEventSignal>();
            Container.BindTrigger<QuestEventSignal.Trigger>();

            //Container.Bind<CharacterFactory>();
            //Container.Bind<ConditionFactory>();
            //Container.Bind<QuestBuilderFactory>();
            //Container.Bind<NodeFactory>();
        }

        private void BindStarContent()
        {
            Container.BindInterfacesAndSelfTo<StarData>().AsSingle();
            Container.Bind<Occupants>().AsSingle();
            Container.Bind<Boss>().AsSingle();
            Container.Bind<Ruins>().AsSingle();
            Container.Bind<Challenge>().AsSingle();
            Container.Bind<LocalEvent>().AsSingle();
            Container.Bind<Survival>().AsSingle();
            Container.Bind<Wormhole>().AsSingle();
            Container.Bind<StarBase>().AsSingle();
            Container.Bind<XmasTree>().AsSingle();
            Container.Bind<Hive>().AsSingle();
        }

        private void BindDatabase()
        {
#if EDITOR_MODE
            Container.BindInterfacesAndSelfTo<SessionDataStub>()AsSingle();
#else
            Container.BindInterfacesAndSelfTo<SessionData>().AsSingle();
#endif
            Container.Bind<ContentFactory>().AsCached();

            Container.BindSignal<SessionCreatedSignal>();
            Container.BindTrigger<SessionCreatedSignal.Trigger>();
            Container.BindSignal<SessionDataLoadedSignal>();
            Container.BindTrigger<SessionDataLoadedSignal.Trigger>();

            Container.BindSignal<PlayerPositionChangedSignal>();
            Container.BindTrigger<PlayerPositionChangedSignal.Trigger>();
            Container.BindSignal<NewStarSecuredSignal>();
            Container.BindTrigger<NewStarSecuredSignal.Trigger>();
            Container.BindSignal<MoneyValueChangedSignal>();
            Container.BindTrigger<MoneyValueChangedSignal.Trigger>();
            Container.BindSignal<FuelValueChangedSignal>();
            Container.BindTrigger<FuelValueChangedSignal.Trigger>();
            Container.BindSignal<StarsValueChangedSignal>();
            Container.BindTrigger<StarsValueChangedSignal.Trigger>();
            Container.BindSignal<PlayerSkillsResetSignal>();
            Container.BindTrigger<PlayerSkillsResetSignal.Trigger>();
            Container.BindSignal<TokensValueChangedSignal>();
            Container.BindTrigger<TokensValueChangedSignal.Trigger>();
            Container.BindSignal<ResourcesChangedSignal>();
            Container.BindTrigger<ResourcesChangedSignal.Trigger>();
        }

        private void BindStateMachine()
        {
            Container.BindInterfacesTo<StateMachine>().AsSingle().NonLazy();
            Container.Bind<GameStateFactory>().AsTransient();

            Container.Bind<TravelState>().AsTransient();
            Container.BindFactory<int, TravelState, TravelState.Factory>();

			Container.Bind<RetreatState>().AsTransient();
			Container.BindFactory<RetreatState, RetreatState.Factory>();

            Container.Bind<InitializationState>().AsTransient();
            Container.BindFactory<InitializationState, InitializationState.Factory>();

            Container.Bind<EditorInitializationState>().AsTransient();
            Container.BindFactory<EditorInitializationState, EditorInitializationState.Factory>();

            Container.Bind<MainMenuState>().AsTransient();
            Container.BindFactory<MainMenuState, MainMenuState.Factory>();

            Container.Bind<StarMapState>().AsTransient();
            Container.BindFactory<StarMapState, StarMapState.Factory>();

            Container.Bind<StartingNewGameState>().AsTransient();
            Container.BindFactory<StartingNewGameState, StartingNewGameState.Factory>();
            
            Container.Bind<QuestState>().AsTransient();
			Container.BindFactory<IUserInteraction, QuestState, QuestState.Factory>();

            Container.Bind<SkillTreeState>().AsTransient();
            Container.BindFactory<SkillTreeState, SkillTreeState.Factory>();

            Container.Bind<DialogState>().AsTransient();
            Container.BindFactory<string, WindowArgs, Action<WindowExitCode>, DialogState, DialogState.Factory>();

            Container.Bind<TestingState>().AsTransient();
            Container.BindFactory<TestingState, TestingState.Factory>();

            Container.Bind<CombatState>().AsTransient();
            Container.BindFactory<ICombatModel, Action<ICombatModel>, CombatState, CombatState.Factory>();

			Container.Bind<QuickCombatState>().AsTransient();
			Container.BindFactory<QuickCombatState.Settings, QuickCombatState, QuickCombatState.Factory>();

			Container.Bind<ShipEditorState>().AsTransient();
			Container.BindFactory<ShipEditorState.Context, ShipEditorState, ShipEditorState.Factory>();

			Container.Bind<ExplorationState>().AsTransient();
            Container.BindFactory<Planet, ExplorationState, ExplorationState.Factory>();

            Container.Bind<EhopediaState>().AsTransient();
            Container.BindFactory<EhopediaState, EhopediaState.Factory>();

            Container.Bind<CombatRewardState>().AsTransient();
            Container.BindFactory<IReward, CombatRewardState, CombatRewardState.Factory>();

            Container.Bind<DailyRewardState>().AsTransient();
            Container.BindFactory<DailyRewardState, DailyRewardState.Factory>();

            Container.Bind<AnnouncementState>().AsTransient();
            Container.BindFactory<AnnouncementState, AnnouncementState.Factory>();

            Container.BindSignal<GameStateChangedSignal>();
            Container.BindTrigger<GameStateChangedSignal.Trigger>();
            Container.BindSignal<StartGameSignal>();
            Container.BindTrigger<StartGameSignal.Trigger>();
            Container.BindSignal<StartTravelSignal>();
            Container.BindTrigger<StartTravelSignal.Trigger>();
			Container.BindSignal<RetreatSignal>();
			Container.BindTrigger<RetreatSignal.Trigger>();
            Container.BindSignal<StartBattleSignal>();
            Container.BindTrigger<StartBattleSignal.Trigger>();
            Container.BindSignal<StartQuickBattleSignal>();
            Container.BindTrigger<StartQuickBattleSignal.Trigger>();
            Container.BindSignal<ExitSignal>();
            Container.BindTrigger<ExitSignal.Trigger>();
            Container.BindSignal<OpenSkillTreeSignal>();
            Container.BindTrigger<OpenSkillTreeSignal.Trigger>();
			Container.BindSignal<OpenShipEditorSignal>();
			Container.BindTrigger<OpenShipEditorSignal.Trigger>();
			Container.BindSignal<OpenShopSignal>();
            Container.BindTrigger<OpenShopSignal.Trigger>();
            Container.BindSignal<OpenWorkshopSignal>();
            Container.BindTrigger<OpenWorkshopSignal.Trigger>();
            Container.BindSignal<OpenEhopediaSignal>();
            Container.BindTrigger<OpenEhopediaSignal.Trigger>();
            Container.BindSignal<ConfigureControlsSignal>();
            Container.BindTrigger<ConfigureControlsSignal.Trigger>();
            Container.BindSignal<CombatCompletedSignal>();
            Container.BindTrigger<CombatCompletedSignal.Trigger>();
            Container.BindSignal<OpenShipyardSignal>();
            Container.BindTrigger<OpenShipyardSignal.Trigger>();
            Container.BindSignal<StartExplorationSignal>();
            Container.BindTrigger<StartExplorationSignal.Trigger>();
            Container.BindSignal<SupplyShipActivatedSignal>();
            Container.BindTrigger<SupplyShipActivatedSignal.Trigger>();
        }

        private void BindSignals()
        {
            Container.BindSignal<InAppPurchaseCompletedSignal>();
            Container.BindTrigger<InAppPurchaseCompletedSignal.Trigger>();
            Container.BindSignal<InAppPurchaseFailedSignal>();
            Container.BindTrigger<InAppPurchaseFailedSignal.Trigger>();
            Container.BindSignal<SupporterPackPurchasedSignal>();
            Container.BindTrigger<SupporterPackPurchasedSignal.Trigger>();
            Container.BindSignal<InAppItemListUpdatedSignal>();
            Container.BindTrigger<InAppItemListUpdatedSignal.Trigger>();
            Container.BindSignal<SessionAboutToSaveSignal>();
            Container.BindTrigger<SessionAboutToSaveSignal.Trigger>();
            Container.BindSignal<MultiplayerStatusChangedSignal>();
            Container.BindTrigger<MultiplayerStatusChangedSignal.Trigger>();
            Container.BindSignal<EnemyFleetLoadedSignal>();
            Container.BindTrigger<EnemyFleetLoadedSignal.Trigger>();
            Container.BindSignal<EnemyFoundSignal>();
            Container.BindTrigger<EnemyFoundSignal.Trigger>();
            Container.BindSignal<DailyRewardAwailableSignal>();
            Container.BindTrigger<DailyRewardAwailableSignal.Trigger>();
            Container.BindSignal<GameModel.BaseCapturedSignal>();
            Container.BindTrigger<GameModel.BaseCapturedSignal.Trigger>();
            Container.BindSignal<GameModel.RegionFleetDefeatedSignal>();
            Container.BindTrigger<GameModel.RegionFleetDefeatedSignal.Trigger>();
            Container.BindSignal<StarContentChangedSignal>();
            Container.BindTrigger<StarContentChangedSignal.Trigger>();
            Container.BindSignal<KeyBindingsChangedSignal>();
            Container.BindTrigger<KeyBindingsChangedSignal.Trigger>();
			Container.BindSignal<MouseEnabledSignal>();
			Container.BindTrigger<MouseEnabledSignal.Trigger>();
			Container.BindSignal<ReloadUiSignal>();
			Container.BindTrigger<ReloadUiSignal.Trigger>();
		}

        private void BindLegacyServices()
        {
            Container.BindInterfacesAndSelfTo<GameModel.RegionMap>().AsSingle();
        }
    }
}
