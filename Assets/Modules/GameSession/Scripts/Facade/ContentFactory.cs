using GameDatabase;
using Session.Content;

namespace Session
{
    public class ContentFactory
    {
        private readonly IDatabase _database;
        private readonly StarsValueChangedSignal.Trigger _starsValueChangedTrigger;
        private readonly FuelValueChangedSignal.Trigger _fuelValueChangedTrigger;
        private readonly MoneyValueChangedSignal.Trigger _moneyValueChangedTrigger;
        private readonly TokensValueChangedSignal.Trigger _tokensValueChangedTrigger;
        private readonly PlayerPositionChangedSignal.Trigger _playerPositionChangedTrigger;
        private readonly NewStarSecuredSignal.Trigger _newStarExploredTrigger;
        private readonly PlayerSkillsResetSignal.Trigger _playerSkillResetTrigger;
        private readonly ResourcesChangedSignal.Trigger _specialResourcesChangedTrigger;

		public ContentFactory(IDatabase database,
			StarsValueChangedSignal.Trigger starsValueChangedTrigger,
			FuelValueChangedSignal.Trigger fuelValueChangedTrigger,
			MoneyValueChangedSignal.Trigger moneyValueChangedTrigger,
			TokensValueChangedSignal.Trigger tokensValueChangedTrigger,
			PlayerPositionChangedSignal.Trigger playerPositionChangedTrigger,
			NewStarSecuredSignal.Trigger newStarExploredTrigger,
			PlayerSkillsResetSignal.Trigger playerSkillResetTrigger,
			ResourcesChangedSignal.Trigger specialResourcesChangedTrigger)
		{
			_database = database;
			_starsValueChangedTrigger = starsValueChangedTrigger;
			_fuelValueChangedTrigger = fuelValueChangedTrigger;
			_moneyValueChangedTrigger = moneyValueChangedTrigger;
			_tokensValueChangedTrigger = tokensValueChangedTrigger;
			_playerPositionChangedTrigger = playerPositionChangedTrigger;
			_newStarExploredTrigger = newStarExploredTrigger;
			_playerSkillResetTrigger = playerSkillResetTrigger;
			_specialResourcesChangedTrigger = specialResourcesChangedTrigger;
		}

		public ContentFactoryObsolete GetObsoleteContentFactory() => new ContentFactoryObsolete(_database);

		public AchievementData CreateAchievementData(Model.SaveGameData data) => new AchievementData(data);
        public BossData CreateBossData(Model.SaveGameData data) => new BossData(data);
		public CommonObjectData CreateCommonObjectData(Model.SaveGameData data) => new CommonObjectData(data);
		public EventData CreateEventData(Model.SaveGameData data) => new EventData(data);
		public GameData CreateGameData(Model.SaveGameData data) => new GameData(data);
		public IapData CreateIapData(Model.SaveGameData data) => new IapData(data, _starsValueChangedTrigger);
		public InventoryData CreateInventoryData(Model.SaveGameData data) => new InventoryData(data);
		public FleetData CreateFleetData(Model.SaveGameData data) => new FleetData(data);
		public RegionData CreateRegionData(Model.SaveGameData data) => new RegionData(data);
		public ResearchData CreateResearchData(Model.SaveGameData data) => new ResearchData(data);
		public ResourcesData CreateResourcesData(Model.SaveGameData data) => new ResourcesData(data,
			_fuelValueChangedTrigger, _moneyValueChangedTrigger,_starsValueChangedTrigger, _tokensValueChangedTrigger, _specialResourcesChangedTrigger);
		public ShopData CreateShopData(Model.SaveGameData data) => new ShopData(data);
		public StarMapData CreateStarMapData(Model.SaveGameData data) => new StarMapData(data,
			_playerPositionChangedTrigger, _newStarExploredTrigger);
		public StatisticsData CreateStatisticsData(Model.SaveGameData data) => new StatisticsData(data);
		public UpgradesData CreateUpgradesData(Model.SaveGameData data) => new UpgradesData(data, _playerSkillResetTrigger);
		public WormholeData CreateWormholeData(Model.SaveGameData data) => new WormholeData(data);
		public PvpData CreatePvpData(Model.SaveGameData data) => new PvpData(data);
		public SocialData CreateSocialData(Model.SaveGameData data) => new SocialData(data);
		public QuestData CreateQuestData(Model.SaveGameData data) => new QuestData(data);
	}
}
