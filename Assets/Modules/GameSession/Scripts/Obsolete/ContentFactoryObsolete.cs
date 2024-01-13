using GameDatabase;
using Session.ContentObsolete;

namespace Session
{
    public class ContentFactoryObsolete
    {
        private readonly IDatabase _database;

		public ContentFactoryObsolete(IDatabase database) => _database = database;

		public AchievementData CreateAchievementData(byte[] buffer)
        {
            return new AchievementData(buffer);
        }

        public BossData CreateBossData(byte[] buffer)
        {
            return new BossData(buffer);
        }

        public CommonObjectData CreateCommonObjectData(byte[] buffer)
        {
            return new CommonObjectData(buffer);
        }

        public EventData CreateEventData(byte[] buffer)
        {
            return new EventData(buffer);
        }

        public GameData CreateGameData(byte[] buffer)
        {
            return new GameData(buffer);
        }

        public InAppPurchasesData CreateInAppPurchasesData(byte[] buffer)
        {
            return new InAppPurchasesData(buffer);
        }

        public InventoryData CreateInventoryData(byte[] buffer)
        {
            return new InventoryData(buffer);
        }

        public FleetData CreateFleetData(byte[] buffer)
        {
            return new FleetData(_database, buffer);
        }

        public RegionData CreateRegionData(byte[] buffer, int gameSeed)
        {
            return new RegionData(gameSeed, buffer);
        }

        public ResearchData CreateResearchData(byte[] buffer)
        {
            return new ResearchData(buffer);
        }

        public ResourcesData CreateResourcesData(byte[] buffer)
        {
            return new ResourcesData(buffer);
        }

        public ShopData CreateShopData(byte[] buffer)
        {
            return new ShopData(buffer);
        }

        public StarMapData CreateStarMapData(byte[] buffer)
        {
            return new StarMapData(buffer);
        }

        public StatisticsData CreateStatisticsData(byte[] buffer)
        {
            return new StatisticsData(buffer);
        }

        public UpgradesData CreateUpgradesData(byte[] buffer)
        {
            return new UpgradesData(buffer);
        }

        public WormholeData CreateWormholeData(byte[] buffer)
        {
            return new WormholeData(buffer);
        }

        public PvpData CreatePvpData(byte[] buffer)
        {
            return new PvpData(buffer);
        }

        public SocialData CreateSocialData(byte[] buffer)
        {
            return new SocialData(buffer);
        }

        public QuestData CreateQuestData(byte[] buffer)
        {
            return new QuestData(buffer);
        }
    }
}
