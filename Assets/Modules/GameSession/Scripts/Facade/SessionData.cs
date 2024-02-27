using System;
using Session.Content;
using System.Collections.Generic;
using Session.Utils;
using Zenject;

namespace Session
{
	public class SessionData : ISessionData, Services.Storage.ISerializableGameData, ITickable, IDataChangedCallback
	{
		private readonly SessionSerializer _serializer;
		private readonly ContentFactory _contentFactory;
		private readonly SessionDataLoadedSignal.Trigger _dataLoadedTrigger;
		private readonly SessionCreatedSignal.Trigger _sessionCreatedTrigger;
		private Model.SaveGameData _content;
		private double _timePlayed;
		private bool _dataChanged;
		private long _dataVersion;

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

        public SessionData(
			ContentFactory contentFactory, 
			SessionDataLoadedSignal.Trigger dataLoadedTrigger, 
			SessionCreatedSignal.Trigger sesionCreatedTrigger)
		{
			_contentFactory = contentFactory;
			_sessionCreatedTrigger = sesionCreatedTrigger;
			_dataLoadedTrigger = dataLoadedTrigger;
			_serializer = new SessionSerializer(_contentFactory.GetObsoleteContentFactory());
		}

		public long GameId { get; private set; }
		public string ModId { get; private set; }
		public long TimePlayed { get => (long)_timePlayed; private set => _timePlayed = value; }

		public void Tick()
		{
			if (_content != null && _content.Game.StartTime > 0)
				_timePlayed += UnityEngine.Time.deltaTime;
		}

		public long DataVersion => _dataVersion;

		public IEnumerable<byte> Serialize()
		{
			_dataChanged = false;
			return _serializer.Serialize(_content);
		}

		public bool TryDeserialize(long gameId, long timePlayed, long dataVersion, string modId, byte[] data, int startIndex)
		{
			try
			{
				if (!_serializer.TryDeserialize(data, startIndex, out _content)) 
					return false;

				InitializeContent(_content);

				GameId = gameId;
				TimePlayed = timePlayed;
				ModId = modId;
				_dataVersion = dataVersion;
				_dataChanged = false;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				return false;
			}

			try
			{
				_dataLoadedTrigger.Fire();
				_sessionCreatedTrigger.Fire();
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}

			return true;
		}

		public void CreateNewGame(string modId, bool keepPurchases = true)
		{
			var content = new Model.SaveGameData(null);

			if (_content != null && keepPurchases)
			{
				content.Iap.RemoveAds = _content.Iap.RemoveAds;
				content.Iap.SupporterPack = _content.Iap.SupporterPack;
				content.Iap.Stars = _content.Iap.Stars;
				content.Resources.Stars = _content.Iap.Stars;
			}

			_content = content;

			GameId = DateTime.UtcNow.Ticks;
			TimePlayed = 0;
			ModId = modId;
			_dataVersion = 0;
			_dataChanged = false;

			InitializeContent(_content);

			_dataLoadedTrigger.Fire();
			_sessionCreatedTrigger.Fire();
		}

		private void InitializeContent(Model.SaveGameData data)
		{
			_gameData = _contentFactory.CreateGameData(data);
			_starMapData = _contentFactory.CreateStarMapData(data);
			_inventoryData = _contentFactory.CreateInventoryData(data);
			_fleetData = _contentFactory.CreateFleetData(data);
			_shopData = _contentFactory.CreateShopData(data);
			_eventData = _contentFactory.CreateEventData(data);
			_bossData = _contentFactory.CreateBossData(data);
			_regionData = _contentFactory.CreateRegionData(data);
			_iapData = _contentFactory.CreateIapData(data);
			_pvpData = _contentFactory.CreatePvpData(data);
			_wormholeData = _contentFactory.CreateWormholeData(data);
			_achievementData = _contentFactory.CreateAchievementData(data);
			_commonObjectData = _contentFactory.CreateCommonObjectData(data);
			_researchData = _contentFactory.CreateResearchData(data);
			_statisticsData = _contentFactory.CreateStatisticsData(data);
			_resourcesData = _contentFactory.CreateResourcesData(data);
			_upgradesData = _contentFactory.CreateUpgradesData(data);
			_socialData = _contentFactory.CreateSocialData(data);
			_questData = _contentFactory.CreateQuestData(data);
            _presetsData = _contentFactory.CreatePresetsData(data);
			_content.Parent = this;
		}

		public void OnDataChanged()
		{
			if (_dataChanged) return;

			_dataChanged = true;
			_dataVersion++;
		}
	}
}
