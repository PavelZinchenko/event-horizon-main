using System;
using System.Collections.Generic;
using Session.Content;

namespace Session
{
	public class SessionDataStub : ISessionData
	{
		private GameData _gameData;
		private StarMapData _starMapData;
		private InventoryData _inventoryData;
		private FleetData _fleetData;
		private ShopData _shopData;
		private EventData _eventData;
		private BossData _bossData;
		private RegionData _regionData;
		private IapData _iapData;
		private PvpData _pvpData;
		private WormholeData _wormholeData;
		private AchievementData _achievementData;
		private CommonObjectData _commonObjectData;
		private ResearchData _researchData;
		private StatisticsData _statisticsData;
		private ResourcesData _resourcesData;
		private UpgradesData _upgradesData;
		private SocialData _socialData;
		private QuestData _questData;
        private ShipPresetsData _presetsData;

        public long GameId { get; private set; }
		public long TimePlayed { get; private set; }
		public long DataVersion { get; private set; }
		public string ModId { get; private set; }

		public IEnumerable<byte> Serialize()
		{
			throw new NotImplementedException();
		}

		public bool TryDeserialize(long gameId, long timePlayed, long dataVersion, string modId, byte[] data, int startIndex)
		{
			throw new NotImplementedException();
		}

		public void CreateNewGame(string modId, bool keepPurchases = true)
		{
			throw new NotImplementedException();
		}

		public IGameData Game => _gameData;
		public IStarMapData StarMap => _starMapData;
		public IInventoryData Inventory => _inventoryData;
		public IFleetData Fleet => _fleetData;
		public IShopData Shop => _shopData;
		public IEventData Events => _eventData;
		public IBossData Bosses => _bossData;
		public IRegionData Regions => _regionData;
		public IIapData Purchases => _iapData;
		public IPvpData Pvp => _pvpData;
		public IWormholeData Wormholes => _wormholeData;
		public IAchievementData Achievements => _achievementData;
		public ICommonObjectData CommonObjects => _commonObjectData;
		public IResearchData Research => _researchData;
		public IStatisticsData Statistics => _statisticsData;
		public IResourcesData Resources => _resourcesData;
		public IUpgradesData Upgrades => _upgradesData;
		public ISocialData Social => _socialData;
		public IQuestData Quests => _questData;
        public IShipPresetsData ShipPresets => _presetsData;
    }
}
