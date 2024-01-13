//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session.Model
{
	public class SaveGameData : IDataChangedCallback
	{
		private readonly IDataChangedCallback _parent;
		private GameData _game;
		private Achievements _achievements;
		private BossData _bosses;
		private CommonObjectData _common;
		private EventData _events;
		private FleetData _fleet;
		private IapData _iap;
		private PvpData _pvp;
		private Inventory _inventory;
		private QuestData _quests;
		private RegionData _regions;
		private ResearchData _research;
		private ResourcesData _resources;
		private ShopData _shop;
		private SocialData _social;
		private StarMapData _starMap;
		private Statistics _statistics;
		private UpgradesData _upgrades;
		private WormholeData _wormholes;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		public SaveGameData(IDataChangedCallback parent)
		{
			_parent = parent;
			_game = new GameData(this);
			_achievements = new Achievements(this);
			_bosses = new BossData(this);
			_common = new CommonObjectData(this);
			_events = new EventData(this);
			_fleet = new FleetData(this);
			_iap = new IapData(this);
			_pvp = new PvpData(this);
			_inventory = new Inventory(this);
			_quests = new QuestData(this);
			_regions = new RegionData(this);
			_research = new ResearchData(this);
			_resources = new ResourcesData(this);
			_shop = new ShopData(this);
			_social = new SocialData(this);
			_starMap = new StarMapData(this);
			_statistics = new Statistics(this);
			_upgrades = new UpgradesData(this);
			_wormholes = new WormholeData(this);
		}

		public SaveGameData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_game = new GameData(reader, this);
			_achievements = new Achievements(reader, this);
			_bosses = new BossData(reader, this);
			_common = new CommonObjectData(reader, this);
			_events = new EventData(reader, this);
			_fleet = new FleetData(reader, this);
			_iap = new IapData(reader, this);
			_pvp = new PvpData(reader, this);
			_inventory = new Inventory(reader, this);
			_quests = new QuestData(reader, this);
			_regions = new RegionData(reader, this);
			_research = new ResearchData(reader, this);
			_resources = new ResourcesData(reader, this);
			_shop = new ShopData(reader, this);
			_social = new SocialData(reader, this);
			_starMap = new StarMapData(reader, this);
			_statistics = new Statistics(reader, this);
			_upgrades = new UpgradesData(reader, this);
			_wormholes = new WormholeData(reader, this);
			_parent = parent;
			DataChanged = false;
		}

		public GameData Game => _game;
		public Achievements Achievements => _achievements;
		public BossData Bosses => _bosses;
		public CommonObjectData Common => _common;
		public EventData Events => _events;
		public FleetData Fleet => _fleet;
		public IapData Iap => _iap;
		public PvpData Pvp => _pvp;
		public Inventory Inventory => _inventory;
		public QuestData Quests => _quests;
		public RegionData Regions => _regions;
		public ResearchData Research => _research;
		public ResourcesData Resources => _resources;
		public ShopData Shop => _shop;
		public SocialData Social => _social;
		public StarMapData StarMap => _starMap;
		public Statistics Statistics => _statistics;
		public UpgradesData Upgrades => _upgrades;
		public WormholeData Wormholes => _wormholes;

		public void Serialize(SessionDataWriter writer)
		{
			_game.Serialize(writer);
			_achievements.Serialize(writer);
			_bosses.Serialize(writer);
			_common.Serialize(writer);
			_events.Serialize(writer);
			_fleet.Serialize(writer);
			_iap.Serialize(writer);
			_pvp.Serialize(writer);
			_inventory.Serialize(writer);
			_quests.Serialize(writer);
			_regions.Serialize(writer);
			_research.Serialize(writer);
			_resources.Serialize(writer);
			_shop.Serialize(writer);
			_social.Serialize(writer);
			_starMap.Serialize(writer);
			_statistics.Serialize(writer);
			_upgrades.Serialize(writer);
			_wormholes.Serialize(writer);
			DataChanged = false;
		}

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
