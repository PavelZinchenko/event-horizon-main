//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using GameDatabase.DataModel;
using GameDatabase.Storage;
using GameDatabase.Model;

namespace GameDatabase
{
	public partial interface IDatabase
	{
		CombatSettings CombatSettings { get; }
		DatabaseSettings DatabaseSettings { get; }
		DebugSettings DebugSettings { get; }
		ExplorationSettings ExplorationSettings { get; }
		FactionsSettings FactionsSettings { get; }
		FrontierSettings FrontierSettings { get; }
		GalaxySettings GalaxySettings { get; }
		ShipModSettings ShipModSettings { get; }
		ShipSettings ShipSettings { get; }
		SkillSettings SkillSettings { get; }
		SpecialEventSettings SpecialEventSettings { get; }
		UiSettings UiSettings { get; }

		IEnumerable<AmmunitionObsolete> AmmunitionObsoleteList { get; }
		IEnumerable<Component> ComponentList { get; }
		IEnumerable<ComponentMod> ComponentModList { get; }
		IEnumerable<ComponentStats> ComponentStatsList { get; }
		IEnumerable<Device> DeviceList { get; }
		IEnumerable<DroneBay> DroneBayList { get; }
		IEnumerable<Faction> FactionList { get; }
		IEnumerable<GameObjectPrefab> GameObjectPrefabList { get; }
		IEnumerable<Satellite> SatelliteList { get; }
		IEnumerable<SatelliteBuild> SatelliteBuildList { get; }
		IEnumerable<Ship> ShipList { get; }
		IEnumerable<ShipBuild> ShipBuildList { get; }
		IEnumerable<Skill> SkillList { get; }
		IEnumerable<Technology> TechnologyList { get; }
		IEnumerable<BehaviorTreeModel> BehaviorTreeList { get; }
		IEnumerable<Character> CharacterList { get; }
		IEnumerable<CombatRules> CombatRulesList { get; }
		IEnumerable<Fleet> FleetList { get; }
		IEnumerable<LootModel> LootList { get; }
		IEnumerable<QuestModel> QuestList { get; }
		IEnumerable<QuestItem> QuestItemList { get; }
		IEnumerable<Ammunition> AmmunitionList { get; }
		IEnumerable<BulletPrefab> BulletPrefabList { get; }
		IEnumerable<VisualEffect> VisualEffectList { get; }
		IEnumerable<Weapon> WeaponList { get; }

		AmmunitionObsolete GetAmmunitionObsolete(ItemId<AmmunitionObsolete> id);
		Component GetComponent(ItemId<Component> id);
		ComponentMod GetComponentMod(ItemId<ComponentMod> id);
		ComponentStats GetComponentStats(ItemId<ComponentStats> id);
		Device GetDevice(ItemId<Device> id);
		DroneBay GetDroneBay(ItemId<DroneBay> id);
		Faction GetFaction(ItemId<Faction> id);
		GameObjectPrefab GetGameObjectPrefab(ItemId<GameObjectPrefab> id);
		Satellite GetSatellite(ItemId<Satellite> id);
		SatelliteBuild GetSatelliteBuild(ItemId<SatelliteBuild> id);
		Ship GetShip(ItemId<Ship> id);
		ShipBuild GetShipBuild(ItemId<ShipBuild> id);
		Skill GetSkill(ItemId<Skill> id);
		Technology GetTechnology(ItemId<Technology> id);
		BehaviorTreeModel GetBehaviorTree(ItemId<BehaviorTreeModel> id);
		Character GetCharacter(ItemId<Character> id);
		CombatRules GetCombatRules(ItemId<CombatRules> id);
		Fleet GetFleet(ItemId<Fleet> id);
		LootModel GetLoot(ItemId<LootModel> id);
		QuestModel GetQuest(ItemId<QuestModel> id);
		QuestItem GetQuestItem(ItemId<QuestItem> id);
		Ammunition GetAmmunition(ItemId<Ammunition> id);
		BulletPrefab GetBulletPrefab(ItemId<BulletPrefab> id);
		VisualEffect GetVisualEffect(ItemId<VisualEffect> id);
		Weapon GetWeapon(ItemId<Weapon> id);

        IImageData GetImage(string name);
        IAudioClipData GetAudioClip(string name);
        string GetLocalization(string language);
	}

    public partial class Database : IDatabase
    {
		public const int VersionMajor = 1;
		public const int VersionMinor = 6;

		public CombatSettings CombatSettings { get; private set; }
		public DatabaseSettings DatabaseSettings { get; private set; }
		public DebugSettings DebugSettings { get; private set; }
		public ExplorationSettings ExplorationSettings { get; private set; }
		public FactionsSettings FactionsSettings { get; private set; }
		public FrontierSettings FrontierSettings { get; private set; }
		public GalaxySettings GalaxySettings { get; private set; }
		public ShipModSettings ShipModSettings { get; private set; }
		public ShipSettings ShipSettings { get; private set; }
		public SkillSettings SkillSettings { get; private set; }
		public SpecialEventSettings SpecialEventSettings { get; private set; }
		public UiSettings UiSettings { get; private set; }

		public IEnumerable<AmmunitionObsolete> AmmunitionObsoleteList => _ammunitionObsoleteMap.Values;
		public IEnumerable<Component> ComponentList => _componentMap.Values;
		public IEnumerable<ComponentMod> ComponentModList => _componentModMap.Values;
		public IEnumerable<ComponentStats> ComponentStatsList => _componentStatsMap.Values;
		public IEnumerable<Device> DeviceList => _deviceMap.Values;
		public IEnumerable<DroneBay> DroneBayList => _droneBayMap.Values;
		public IEnumerable<Faction> FactionList => _factionMap.Values;
		public IEnumerable<GameObjectPrefab> GameObjectPrefabList => _gameObjectPrefabMap.Values;
		public IEnumerable<Satellite> SatelliteList => _satelliteMap.Values;
		public IEnumerable<SatelliteBuild> SatelliteBuildList => _satelliteBuildMap.Values;
		public IEnumerable<Ship> ShipList => _shipMap.Values;
		public IEnumerable<ShipBuild> ShipBuildList => _shipBuildMap.Values;
		public IEnumerable<Skill> SkillList => _skillMap.Values;
		public IEnumerable<Technology> TechnologyList => _technologyMap.Values;
		public IEnumerable<BehaviorTreeModel> BehaviorTreeList => _behaviorTreeMap.Values;
		public IEnumerable<Character> CharacterList => _characterMap.Values;
		public IEnumerable<CombatRules> CombatRulesList => _combatRulesMap.Values;
		public IEnumerable<Fleet> FleetList => _fleetMap.Values;
		public IEnumerable<LootModel> LootList => _lootMap.Values;
		public IEnumerable<QuestModel> QuestList => _questMap.Values;
		public IEnumerable<QuestItem> QuestItemList => _questItemMap.Values;
		public IEnumerable<Ammunition> AmmunitionList => _ammunitionMap.Values;
		public IEnumerable<BulletPrefab> BulletPrefabList => _bulletPrefabMap.Values;
		public IEnumerable<VisualEffect> VisualEffectList => _visualEffectMap.Values;
		public IEnumerable<Weapon> WeaponList => _weaponMap.Values;

		public AmmunitionObsolete GetAmmunitionObsolete(ItemId<AmmunitionObsolete> id) { return (_ammunitionObsoleteMap.TryGetValue(id.Value, out var item)) ? item : AmmunitionObsolete.DefaultValue; }
		public Component GetComponent(ItemId<Component> id) { return (_componentMap.TryGetValue(id.Value, out var item)) ? item : Component.DefaultValue; }
		public ComponentMod GetComponentMod(ItemId<ComponentMod> id) { return (_componentModMap.TryGetValue(id.Value, out var item)) ? item : ComponentMod.DefaultValue; }
		public ComponentStats GetComponentStats(ItemId<ComponentStats> id) { return (_componentStatsMap.TryGetValue(id.Value, out var item)) ? item : ComponentStats.DefaultValue; }
		public Device GetDevice(ItemId<Device> id) { return (_deviceMap.TryGetValue(id.Value, out var item)) ? item : Device.DefaultValue; }
		public DroneBay GetDroneBay(ItemId<DroneBay> id) { return (_droneBayMap.TryGetValue(id.Value, out var item)) ? item : DroneBay.DefaultValue; }
		public Faction GetFaction(ItemId<Faction> id) { return (_factionMap.TryGetValue(id.Value, out var item)) ? item : Faction.DefaultValue; }
		public GameObjectPrefab GetGameObjectPrefab(ItemId<GameObjectPrefab> id) { return (_gameObjectPrefabMap.TryGetValue(id.Value, out var item)) ? item : GameObjectPrefab.DefaultValue; }
		public Satellite GetSatellite(ItemId<Satellite> id) { return (_satelliteMap.TryGetValue(id.Value, out var item)) ? item : Satellite.DefaultValue; }
		public SatelliteBuild GetSatelliteBuild(ItemId<SatelliteBuild> id) { return (_satelliteBuildMap.TryGetValue(id.Value, out var item)) ? item : SatelliteBuild.DefaultValue; }
		public Ship GetShip(ItemId<Ship> id) { return (_shipMap.TryGetValue(id.Value, out var item)) ? item : Ship.DefaultValue; }
		public ShipBuild GetShipBuild(ItemId<ShipBuild> id) { return (_shipBuildMap.TryGetValue(id.Value, out var item)) ? item : ShipBuild.DefaultValue; }
		public Skill GetSkill(ItemId<Skill> id) { return (_skillMap.TryGetValue(id.Value, out var item)) ? item : Skill.DefaultValue; }
		public Technology GetTechnology(ItemId<Technology> id) { return (_technologyMap.TryGetValue(id.Value, out var item)) ? item : Technology.DefaultValue; }
		public BehaviorTreeModel GetBehaviorTree(ItemId<BehaviorTreeModel> id) { return (_behaviorTreeMap.TryGetValue(id.Value, out var item)) ? item : BehaviorTreeModel.DefaultValue; }
		public Character GetCharacter(ItemId<Character> id) { return (_characterMap.TryGetValue(id.Value, out var item)) ? item : Character.DefaultValue; }
		public CombatRules GetCombatRules(ItemId<CombatRules> id) { return (_combatRulesMap.TryGetValue(id.Value, out var item)) ? item : CombatRules.DefaultValue; }
		public Fleet GetFleet(ItemId<Fleet> id) { return (_fleetMap.TryGetValue(id.Value, out var item)) ? item : Fleet.DefaultValue; }
		public LootModel GetLoot(ItemId<LootModel> id) { return (_lootMap.TryGetValue(id.Value, out var item)) ? item : LootModel.DefaultValue; }
		public QuestModel GetQuest(ItemId<QuestModel> id) { return (_questMap.TryGetValue(id.Value, out var item)) ? item : QuestModel.DefaultValue; }
		public QuestItem GetQuestItem(ItemId<QuestItem> id) { return (_questItemMap.TryGetValue(id.Value, out var item)) ? item : QuestItem.DefaultValue; }
		public Ammunition GetAmmunition(ItemId<Ammunition> id) { return (_ammunitionMap.TryGetValue(id.Value, out var item)) ? item : Ammunition.DefaultValue; }
		public BulletPrefab GetBulletPrefab(ItemId<BulletPrefab> id) { return (_bulletPrefabMap.TryGetValue(id.Value, out var item)) ? item : BulletPrefab.DefaultValue; }
		public VisualEffect GetVisualEffect(ItemId<VisualEffect> id) { return (_visualEffectMap.TryGetValue(id.Value, out var item)) ? item : VisualEffect.DefaultValue; }
		public Weapon GetWeapon(ItemId<Weapon> id) { return (_weaponMap.TryGetValue(id.Value, out var item)) ? item : Weapon.DefaultValue; }

        public IImageData GetImage(string name) { return _images.TryGetValue(name, out var image) ? image : ImageData.Empty; }
        public IAudioClipData GetAudioClip(string name) { return _audioClips.TryGetValue(name, out var audioClip) ? audioClip : AudioClipData.Empty; }
        public string GetLocalization(string language) { return _localizations.TryGetValue(language, out var data) ? data : null; }

        private void Clear()
        {
			_ammunitionObsoleteMap.Clear();
			_componentMap.Clear();
			_componentModMap.Clear();
			_componentStatsMap.Clear();
			_deviceMap.Clear();
			_droneBayMap.Clear();
			_factionMap.Clear();
			_gameObjectPrefabMap.Clear();
			_satelliteMap.Clear();
			_satelliteBuildMap.Clear();
			_shipMap.Clear();
			_shipBuildMap.Clear();
			_skillMap.Clear();
			_technologyMap.Clear();
			_behaviorTreeMap.Clear();
			_characterMap.Clear();
			_combatRulesMap.Clear();
			_fleetMap.Clear();
			_lootMap.Clear();
			_questMap.Clear();
			_questItemMap.Clear();
			_ammunitionMap.Clear();
			_bulletPrefabMap.Clear();
			_visualEffectMap.Clear();
			_weaponMap.Clear();

			CombatSettings = null;
			DatabaseSettings = null;
			DebugSettings = null;
			ExplorationSettings = null;
			FactionsSettings = null;
			FrontierSettings = null;
			GalaxySettings = null;
			ShipModSettings = null;
			ShipSettings = null;
			SkillSettings = null;
			SpecialEventSettings = null;
			UiSettings = null;

			_images.Clear();
			_audioClips.Clear();
			_localizations.Clear();
        }

		private readonly Dictionary<int, AmmunitionObsolete> _ammunitionObsoleteMap = new();
		private readonly Dictionary<int, Component> _componentMap = new();
		private readonly Dictionary<int, ComponentMod> _componentModMap = new();
		private readonly Dictionary<int, ComponentStats> _componentStatsMap = new();
		private readonly Dictionary<int, Device> _deviceMap = new();
		private readonly Dictionary<int, DroneBay> _droneBayMap = new();
		private readonly Dictionary<int, Faction> _factionMap = new();
		private readonly Dictionary<int, GameObjectPrefab> _gameObjectPrefabMap = new();
		private readonly Dictionary<int, Satellite> _satelliteMap = new();
		private readonly Dictionary<int, SatelliteBuild> _satelliteBuildMap = new();
		private readonly Dictionary<int, Ship> _shipMap = new();
		private readonly Dictionary<int, ShipBuild> _shipBuildMap = new();
		private readonly Dictionary<int, Skill> _skillMap = new();
		private readonly Dictionary<int, Technology> _technologyMap = new();
		private readonly Dictionary<int, BehaviorTreeModel> _behaviorTreeMap = new();
		private readonly Dictionary<int, Character> _characterMap = new();
		private readonly Dictionary<int, CombatRules> _combatRulesMap = new();
		private readonly Dictionary<int, Fleet> _fleetMap = new();
		private readonly Dictionary<int, LootModel> _lootMap = new();
		private readonly Dictionary<int, QuestModel> _questMap = new();
		private readonly Dictionary<int, QuestItem> _questItemMap = new();
		private readonly Dictionary<int, Ammunition> _ammunitionMap = new();
		private readonly Dictionary<int, BulletPrefab> _bulletPrefabMap = new();
		private readonly Dictionary<int, VisualEffect> _visualEffectMap = new();
		private readonly Dictionary<int, Weapon> _weaponMap = new();
	
        private readonly Dictionary<string, IImageData> _images = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IAudioClipData> _audioClips = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _localizations = new(StringComparer.OrdinalIgnoreCase);

		public class Loader
        {
		    public static void Load(Database database, DatabaseContent content)
            {
				var loader = new Loader(database, content);
				loader.Load();
            }

            private Loader(Database database, DatabaseContent content)
            {
                _database = database;
                _content = content;
            }

			public void Load()
            {
				foreach (var item in _content.AmmunitionObsoleteList)
					if (!item.Disabled && !_database._ammunitionObsoleteMap.ContainsKey(item.Id))
						AmmunitionObsolete.Create(item, this);
				foreach (var item in _content.ComponentList)
					if (!item.Disabled && !_database._componentMap.ContainsKey(item.Id))
						Component.Create(item, this);
				foreach (var item in _content.ComponentModList)
					if (!item.Disabled && !_database._componentModMap.ContainsKey(item.Id))
						ComponentMod.Create(item, this);
				foreach (var item in _content.ComponentStatsList)
					if (!item.Disabled && !_database._componentStatsMap.ContainsKey(item.Id))
						ComponentStats.Create(item, this);
				foreach (var item in _content.DeviceList)
					if (!item.Disabled && !_database._deviceMap.ContainsKey(item.Id))
						Device.Create(item, this);
				foreach (var item in _content.DroneBayList)
					if (!item.Disabled && !_database._droneBayMap.ContainsKey(item.Id))
						DroneBay.Create(item, this);
				foreach (var item in _content.FactionList)
					if (!item.Disabled && !_database._factionMap.ContainsKey(item.Id))
						Faction.Create(item, this);
				foreach (var item in _content.GameObjectPrefabList)
					if (!item.Disabled && !_database._gameObjectPrefabMap.ContainsKey(item.Id))
						GameObjectPrefab.Create(item, this);
				foreach (var item in _content.SatelliteList)
					if (!item.Disabled && !_database._satelliteMap.ContainsKey(item.Id))
						Satellite.Create(item, this);
				foreach (var item in _content.SatelliteBuildList)
					if (!item.Disabled && !_database._satelliteBuildMap.ContainsKey(item.Id))
						SatelliteBuild.Create(item, this);
				foreach (var item in _content.ShipList)
					if (!item.Disabled && !_database._shipMap.ContainsKey(item.Id))
						Ship.Create(item, this);
				foreach (var item in _content.ShipBuildList)
					if (!item.Disabled && !_database._shipBuildMap.ContainsKey(item.Id))
						ShipBuild.Create(item, this);
				foreach (var item in _content.SkillList)
					if (!item.Disabled && !_database._skillMap.ContainsKey(item.Id))
						Skill.Create(item, this);
				foreach (var item in _content.TechnologyList)
					if (!item.Disabled && !_database._technologyMap.ContainsKey(item.Id))
						Technology.Create(item, this);
				foreach (var item in _content.BehaviorTreeList)
					if (!item.Disabled && !_database._behaviorTreeMap.ContainsKey(item.Id))
						BehaviorTreeModel.Create(item, this);
				foreach (var item in _content.CharacterList)
					if (!item.Disabled && !_database._characterMap.ContainsKey(item.Id))
						Character.Create(item, this);
				foreach (var item in _content.CombatRulesList)
					if (!item.Disabled && !_database._combatRulesMap.ContainsKey(item.Id))
						CombatRules.Create(item, this);
				foreach (var item in _content.FleetList)
					if (!item.Disabled && !_database._fleetMap.ContainsKey(item.Id))
						Fleet.Create(item, this);
				foreach (var item in _content.LootList)
					if (!item.Disabled && !_database._lootMap.ContainsKey(item.Id))
						LootModel.Create(item, this);
				foreach (var item in _content.QuestList)
					if (!item.Disabled && !_database._questMap.ContainsKey(item.Id))
						QuestModel.Create(item, this);
				foreach (var item in _content.QuestItemList)
					if (!item.Disabled && !_database._questItemMap.ContainsKey(item.Id))
						QuestItem.Create(item, this);
				foreach (var item in _content.AmmunitionList)
					if (!item.Disabled && !_database._ammunitionMap.ContainsKey(item.Id))
						Ammunition.Create(item, this);
				foreach (var item in _content.BulletPrefabList)
					if (!item.Disabled && !_database._bulletPrefabMap.ContainsKey(item.Id))
						BulletPrefab.Create(item, this);
				foreach (var item in _content.VisualEffectList)
					if (!item.Disabled && !_database._visualEffectMap.ContainsKey(item.Id))
						VisualEffect.Create(item, this);
				foreach (var item in _content.WeaponList)
					if (!item.Disabled && !_database._weaponMap.ContainsKey(item.Id))
						Weapon.Create(item, this);

                foreach (var item in _content.Images)
                    if (!_database._images.ContainsKey(item.Key))
                        _database._images.Add(item.Key, item.Value);

                foreach (var item in _content.AudioClips)
                    if (!_database._audioClips.ContainsKey(item.Key))
                        _database._audioClips.Add(item.Key, item.Value);

                foreach (var item in _content.Localizations)
                    if (!_database._localizations.ContainsKey(item.Key))
                        _database._localizations.Add(item.Key, item.Value);

				if (_database.CombatSettings == null)
					_database.CombatSettings = CombatSettings.Create(_content.CombatSettings ?? new Serializable.CombatSettingsSerializable { ItemType = Enums.ItemType.CombatSettings }, this);
				if (_database.DatabaseSettings == null)
					_database.DatabaseSettings = DatabaseSettings.Create(_content.DatabaseSettings ?? new Serializable.DatabaseSettingsSerializable { ItemType = Enums.ItemType.DatabaseSettings }, this);
				if (_database.DebugSettings == null)
					_database.DebugSettings = DebugSettings.Create(_content.DebugSettings ?? new Serializable.DebugSettingsSerializable { ItemType = Enums.ItemType.DebugSettings }, this);
				if (_database.ExplorationSettings == null)
					_database.ExplorationSettings = ExplorationSettings.Create(_content.ExplorationSettings ?? new Serializable.ExplorationSettingsSerializable { ItemType = Enums.ItemType.ExplorationSettings }, this);
				if (_database.FactionsSettings == null)
					_database.FactionsSettings = FactionsSettings.Create(_content.FactionsSettings ?? new Serializable.FactionsSettingsSerializable { ItemType = Enums.ItemType.FactionsSettings }, this);
				if (_database.FrontierSettings == null)
					_database.FrontierSettings = FrontierSettings.Create(_content.FrontierSettings ?? new Serializable.FrontierSettingsSerializable { ItemType = Enums.ItemType.FrontierSettings }, this);
				if (_database.GalaxySettings == null)
					_database.GalaxySettings = GalaxySettings.Create(_content.GalaxySettings ?? new Serializable.GalaxySettingsSerializable { ItemType = Enums.ItemType.GalaxySettings }, this);
				if (_database.ShipModSettings == null)
					_database.ShipModSettings = ShipModSettings.Create(_content.ShipModSettings ?? new Serializable.ShipModSettingsSerializable { ItemType = Enums.ItemType.ShipModSettings }, this);
				if (_database.ShipSettings == null)
					_database.ShipSettings = ShipSettings.Create(_content.ShipSettings ?? new Serializable.ShipSettingsSerializable { ItemType = Enums.ItemType.ShipSettings }, this);
				if (_database.SkillSettings == null)
					_database.SkillSettings = SkillSettings.Create(_content.SkillSettings ?? new Serializable.SkillSettingsSerializable { ItemType = Enums.ItemType.SkillSettings }, this);
				if (_database.SpecialEventSettings == null)
					_database.SpecialEventSettings = SpecialEventSettings.Create(_content.SpecialEventSettings ?? new Serializable.SpecialEventSettingsSerializable { ItemType = Enums.ItemType.SpecialEventSettings }, this);
				if (_database.UiSettings == null)
					_database.UiSettings = UiSettings.Create(_content.UiSettings ?? new Serializable.UiSettingsSerializable { ItemType = Enums.ItemType.UiSettings }, this);
			}

			public AmmunitionObsolete GetAmmunitionObsolete(ItemId<AmmunitionObsolete> id, bool notNull = false)
			{
				if (_database._ammunitionObsoleteMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetAmmunitionObsolete(id.Value);
                if (serializable != null && !serializable.Disabled) return AmmunitionObsolete.Create(serializable, this);

				var value = AmmunitionObsolete.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Component GetComponent(ItemId<Component> id, bool notNull = false)
			{
				if (_database._componentMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetComponent(id.Value);
                if (serializable != null && !serializable.Disabled) return Component.Create(serializable, this);

				var value = Component.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public ComponentMod GetComponentMod(ItemId<ComponentMod> id, bool notNull = false)
			{
				if (_database._componentModMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetComponentMod(id.Value);
                if (serializable != null && !serializable.Disabled) return ComponentMod.Create(serializable, this);

				var value = ComponentMod.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public ComponentStats GetComponentStats(ItemId<ComponentStats> id, bool notNull = false)
			{
				if (_database._componentStatsMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetComponentStats(id.Value);
                if (serializable != null && !serializable.Disabled) return ComponentStats.Create(serializable, this);

				var value = ComponentStats.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Device GetDevice(ItemId<Device> id, bool notNull = false)
			{
				if (_database._deviceMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetDevice(id.Value);
                if (serializable != null && !serializable.Disabled) return Device.Create(serializable, this);

				var value = Device.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public DroneBay GetDroneBay(ItemId<DroneBay> id, bool notNull = false)
			{
				if (_database._droneBayMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetDroneBay(id.Value);
                if (serializable != null && !serializable.Disabled) return DroneBay.Create(serializable, this);

				var value = DroneBay.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Faction GetFaction(ItemId<Faction> id, bool notNull = false)
			{
				if (_database._factionMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetFaction(id.Value);
                if (serializable != null && !serializable.Disabled) return Faction.Create(serializable, this);

				var value = Faction.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public GameObjectPrefab GetGameObjectPrefab(ItemId<GameObjectPrefab> id, bool notNull = false)
			{
				if (_database._gameObjectPrefabMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetGameObjectPrefab(id.Value);
                if (serializable != null && !serializable.Disabled) return GameObjectPrefab.Create(serializable, this);

				var value = GameObjectPrefab.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Satellite GetSatellite(ItemId<Satellite> id, bool notNull = false)
			{
				if (_database._satelliteMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetSatellite(id.Value);
                if (serializable != null && !serializable.Disabled) return Satellite.Create(serializable, this);

				var value = Satellite.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public SatelliteBuild GetSatelliteBuild(ItemId<SatelliteBuild> id, bool notNull = false)
			{
				if (_database._satelliteBuildMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetSatelliteBuild(id.Value);
                if (serializable != null && !serializable.Disabled) return SatelliteBuild.Create(serializable, this);

				var value = SatelliteBuild.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Ship GetShip(ItemId<Ship> id, bool notNull = false)
			{
				if (_database._shipMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetShip(id.Value);
                if (serializable != null && !serializable.Disabled) return Ship.Create(serializable, this);

				var value = Ship.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public ShipBuild GetShipBuild(ItemId<ShipBuild> id, bool notNull = false)
			{
				if (_database._shipBuildMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetShipBuild(id.Value);
                if (serializable != null && !serializable.Disabled) return ShipBuild.Create(serializable, this);

				var value = ShipBuild.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Skill GetSkill(ItemId<Skill> id, bool notNull = false)
			{
				if (_database._skillMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetSkill(id.Value);
                if (serializable != null && !serializable.Disabled) return Skill.Create(serializable, this);

				var value = Skill.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Technology GetTechnology(ItemId<Technology> id, bool notNull = false)
			{
				if (_database._technologyMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetTechnology(id.Value);
                if (serializable != null && !serializable.Disabled) return Technology.Create(serializable, this);

				var value = Technology.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public BehaviorTreeModel GetBehaviorTree(ItemId<BehaviorTreeModel> id, bool notNull = false)
			{
				if (_database._behaviorTreeMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetBehaviorTree(id.Value);
                if (serializable != null && !serializable.Disabled) return BehaviorTreeModel.Create(serializable, this);

				var value = BehaviorTreeModel.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Character GetCharacter(ItemId<Character> id, bool notNull = false)
			{
				if (_database._characterMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetCharacter(id.Value);
                if (serializable != null && !serializable.Disabled) return Character.Create(serializable, this);

				var value = Character.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public CombatRules GetCombatRules(ItemId<CombatRules> id, bool notNull = false)
			{
				if (_database._combatRulesMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetCombatRules(id.Value);
                if (serializable != null && !serializable.Disabled) return CombatRules.Create(serializable, this);

				var value = CombatRules.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Fleet GetFleet(ItemId<Fleet> id, bool notNull = false)
			{
				if (_database._fleetMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetFleet(id.Value);
                if (serializable != null && !serializable.Disabled) return Fleet.Create(serializable, this);

				var value = Fleet.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public LootModel GetLoot(ItemId<LootModel> id, bool notNull = false)
			{
				if (_database._lootMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetLoot(id.Value);
                if (serializable != null && !serializable.Disabled) return LootModel.Create(serializable, this);

				var value = LootModel.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public QuestModel GetQuest(ItemId<QuestModel> id, bool notNull = false)
			{
				if (_database._questMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetQuest(id.Value);
                if (serializable != null && !serializable.Disabled) return QuestModel.Create(serializable, this);

				var value = QuestModel.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public QuestItem GetQuestItem(ItemId<QuestItem> id, bool notNull = false)
			{
				if (_database._questItemMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetQuestItem(id.Value);
                if (serializable != null && !serializable.Disabled) return QuestItem.Create(serializable, this);

				var value = QuestItem.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Ammunition GetAmmunition(ItemId<Ammunition> id, bool notNull = false)
			{
				if (_database._ammunitionMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetAmmunition(id.Value);
                if (serializable != null && !serializable.Disabled) return Ammunition.Create(serializable, this);

				var value = Ammunition.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public BulletPrefab GetBulletPrefab(ItemId<BulletPrefab> id, bool notNull = false)
			{
				if (_database._bulletPrefabMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetBulletPrefab(id.Value);
                if (serializable != null && !serializable.Disabled) return BulletPrefab.Create(serializable, this);

				var value = BulletPrefab.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public VisualEffect GetVisualEffect(ItemId<VisualEffect> id, bool notNull = false)
			{
				if (_database._visualEffectMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetVisualEffect(id.Value);
                if (serializable != null && !serializable.Disabled) return VisualEffect.Create(serializable, this);

				var value = VisualEffect.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}
			public Weapon GetWeapon(ItemId<Weapon> id, bool notNull = false)
			{
				if (_database._weaponMap.TryGetValue(id.Value, out var item)) return item;
                var serializable = _content.GetWeapon(id.Value);
                if (serializable != null && !serializable.Disabled) return Weapon.Create(serializable, this);

				var value = Weapon.DefaultValue;
				if (notNull && value == null) throw new DatabaseException("Data not found " + id);
                return value;
			}


			public void AddAmmunitionObsolete(int id, AmmunitionObsolete item) { _database._ammunitionObsoleteMap.Add(id, item); }
			public void AddComponent(int id, Component item) { _database._componentMap.Add(id, item); }
			public void AddComponentMod(int id, ComponentMod item) { _database._componentModMap.Add(id, item); }
			public void AddComponentStats(int id, ComponentStats item) { _database._componentStatsMap.Add(id, item); }
			public void AddDevice(int id, Device item) { _database._deviceMap.Add(id, item); }
			public void AddDroneBay(int id, DroneBay item) { _database._droneBayMap.Add(id, item); }
			public void AddFaction(int id, Faction item) { _database._factionMap.Add(id, item); }
			public void AddGameObjectPrefab(int id, GameObjectPrefab item) { _database._gameObjectPrefabMap.Add(id, item); }
			public void AddSatellite(int id, Satellite item) { _database._satelliteMap.Add(id, item); }
			public void AddSatelliteBuild(int id, SatelliteBuild item) { _database._satelliteBuildMap.Add(id, item); }
			public void AddShip(int id, Ship item) { _database._shipMap.Add(id, item); }
			public void AddShipBuild(int id, ShipBuild item) { _database._shipBuildMap.Add(id, item); }
			public void AddSkill(int id, Skill item) { _database._skillMap.Add(id, item); }
			public void AddTechnology(int id, Technology item) { _database._technologyMap.Add(id, item); }
			public void AddBehaviorTree(int id, BehaviorTreeModel item) { _database._behaviorTreeMap.Add(id, item); }
			public void AddCharacter(int id, Character item) { _database._characterMap.Add(id, item); }
			public void AddCombatRules(int id, CombatRules item) { _database._combatRulesMap.Add(id, item); }
			public void AddFleet(int id, Fleet item) { _database._fleetMap.Add(id, item); }
			public void AddLoot(int id, LootModel item) { _database._lootMap.Add(id, item); }
			public void AddQuest(int id, QuestModel item) { _database._questMap.Add(id, item); }
			public void AddQuestItem(int id, QuestItem item) { _database._questItemMap.Add(id, item); }
			public void AddAmmunition(int id, Ammunition item) { _database._ammunitionMap.Add(id, item); }
			public void AddBulletPrefab(int id, BulletPrefab item) { _database._bulletPrefabMap.Add(id, item); }
			public void AddVisualEffect(int id, VisualEffect item) { _database._visualEffectMap.Add(id, item); }
			public void AddWeapon(int id, Weapon item) { _database._weaponMap.Add(id, item); }

            private readonly DatabaseContent _content;
			private readonly Database _database;
		}
	}
}

