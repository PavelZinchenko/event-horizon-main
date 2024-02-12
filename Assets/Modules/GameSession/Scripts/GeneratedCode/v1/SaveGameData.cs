//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using Session.Utils;

namespace Session.v1
{
	public class SaveGameData : IDataChangedCallback
	{
		private IDataChangedCallback _parent;

		private Model.GameData _game;
		private Model.Achievements _achievements;
		private v1.BossData _bosses;
		private Model.CommonObjectData _common;
		private v1.EventData _events;
		private Model.FleetData _fleet;
		private Model.IapData _iap;
		private Model.PvpData _pvp;
		private v1.Inventory _inventory;
		private Model.QuestData _quests;
		private v1.RegionData _regions;
		private Model.ResearchData _research;
		private Model.ResourcesData _resources;
		private v1.ShopData _shop;
		private Model.SocialData _social;
		private v1.StarMapData _starMap;
		private Model.Statistics _statistics;
		private Model.UpgradesData _upgrades;
		private Model.WormholeData _wormholes;

		public const int VersionMinor = 0;
		public const int VersionMajor = 1;

		public bool DataChanged { get; private set; }

		internal IDataChangedCallback Parent { get => _parent; set => _parent = value; }

		public SaveGameData(IDataChangedCallback parent)
		{
			_parent = parent;
			_game = new Model.GameData(this);
			_achievements = new Model.Achievements(this);
			_bosses = new v1.BossData(this);
			_common = new Model.CommonObjectData(this);
			_events = new v1.EventData(this);
			_fleet = new Model.FleetData(this);
			_iap = new Model.IapData(this);
			_pvp = new Model.PvpData(this);
			_inventory = new v1.Inventory(this);
			_quests = new Model.QuestData(this);
			_regions = new v1.RegionData(this);
			_research = new Model.ResearchData(this);
			_resources = new Model.ResourcesData(this);
			_shop = new v1.ShopData(this);
			_social = new Model.SocialData(this);
			_starMap = new v1.StarMapData(this);
			_statistics = new Model.Statistics(this);
			_upgrades = new Model.UpgradesData(this);
			_wormholes = new Model.WormholeData(this);
		}

		public SaveGameData(SessionDataReader reader, IDataChangedCallback parent)
		{
			_game = new Model.GameData(reader, this);
			_achievements = new Model.Achievements(reader, this);
			_bosses = new v1.BossData(reader, this);
			_common = new Model.CommonObjectData(reader, this);
			_events = new v1.EventData(reader, this);
			_fleet = new Model.FleetData(reader, this);
			_iap = new Model.IapData(reader, this);
			_pvp = new Model.PvpData(reader, this);
			_inventory = new v1.Inventory(reader, this);
			_quests = new Model.QuestData(reader, this);
			_regions = new v1.RegionData(reader, this);
			_research = new Model.ResearchData(reader, this);
			_resources = new Model.ResourcesData(reader, this);
			_shop = new v1.ShopData(reader, this);
			_social = new Model.SocialData(reader, this);
			_starMap = new v1.StarMapData(reader, this);
			_statistics = new Model.Statistics(reader, this);
			_upgrades = new Model.UpgradesData(reader, this);
			_wormholes = new Model.WormholeData(reader, this);
			_parent = parent;
			DataChanged = false;
		}

		public Model.GameData Game
		{
			get => _game;
			set
			{
				if (_game == value) return;
				_game = value;
				_game.Parent = this;
				OnDataChanged();
			}
		}
		public Model.Achievements Achievements
		{
			get => _achievements;
			set
			{
				if (_achievements == value) return;
				_achievements = value;
				_achievements.Parent = this;
				OnDataChanged();
			}
		}
		public v1.BossData Bosses
		{
			get => _bosses;
			set
			{
				if (_bosses == value) return;
				_bosses = value;
				_bosses.Parent = this;
				OnDataChanged();
			}
		}
		public Model.CommonObjectData Common
		{
			get => _common;
			set
			{
				if (_common == value) return;
				_common = value;
				_common.Parent = this;
				OnDataChanged();
			}
		}
		public v1.EventData Events
		{
			get => _events;
			set
			{
				if (_events == value) return;
				_events = value;
				_events.Parent = this;
				OnDataChanged();
			}
		}
		public Model.FleetData Fleet
		{
			get => _fleet;
			set
			{
				if (_fleet == value) return;
				_fleet = value;
				_fleet.Parent = this;
				OnDataChanged();
			}
		}
		public Model.IapData Iap
		{
			get => _iap;
			set
			{
				if (_iap == value) return;
				_iap = value;
				_iap.Parent = this;
				OnDataChanged();
			}
		}
		public Model.PvpData Pvp
		{
			get => _pvp;
			set
			{
				if (_pvp == value) return;
				_pvp = value;
				_pvp.Parent = this;
				OnDataChanged();
			}
		}
		public v1.Inventory Inventory
		{
			get => _inventory;
			set
			{
				if (_inventory == value) return;
				_inventory = value;
				_inventory.Parent = this;
				OnDataChanged();
			}
		}
		public Model.QuestData Quests
		{
			get => _quests;
			set
			{
				if (_quests == value) return;
				_quests = value;
				_quests.Parent = this;
				OnDataChanged();
			}
		}
		public v1.RegionData Regions
		{
			get => _regions;
			set
			{
				if (_regions == value) return;
				_regions = value;
				_regions.Parent = this;
				OnDataChanged();
			}
		}
		public Model.ResearchData Research
		{
			get => _research;
			set
			{
				if (_research == value) return;
				_research = value;
				_research.Parent = this;
				OnDataChanged();
			}
		}
		public Model.ResourcesData Resources
		{
			get => _resources;
			set
			{
				if (_resources == value) return;
				_resources = value;
				_resources.Parent = this;
				OnDataChanged();
			}
		}
		public v1.ShopData Shop
		{
			get => _shop;
			set
			{
				if (_shop == value) return;
				_shop = value;
				_shop.Parent = this;
				OnDataChanged();
			}
		}
		public Model.SocialData Social
		{
			get => _social;
			set
			{
				if (_social == value) return;
				_social = value;
				_social.Parent = this;
				OnDataChanged();
			}
		}
		public v1.StarMapData StarMap
		{
			get => _starMap;
			set
			{
				if (_starMap == value) return;
				_starMap = value;
				_starMap.Parent = this;
				OnDataChanged();
			}
		}
		public Model.Statistics Statistics
		{
			get => _statistics;
			set
			{
				if (_statistics == value) return;
				_statistics = value;
				_statistics.Parent = this;
				OnDataChanged();
			}
		}
		public Model.UpgradesData Upgrades
		{
			get => _upgrades;
			set
			{
				if (_upgrades == value) return;
				_upgrades = value;
				_upgrades.Parent = this;
				OnDataChanged();
			}
		}
		public Model.WormholeData Wormholes
		{
			get => _wormholes;
			set
			{
				if (_wormholes == value) return;
				_wormholes = value;
				_wormholes.Parent = this;
				OnDataChanged();
			}
		}

		public void OnDataChanged()
		{
			DataChanged = true;
			_parent?.OnDataChanged();
		}
	}
}
