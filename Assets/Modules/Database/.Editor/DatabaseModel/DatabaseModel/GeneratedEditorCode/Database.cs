//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using EditorDatabase.DataModel;
using EditorDatabase.Storage;
using EditorDatabase.Model;
using EditorDatabase.Enums;

namespace EditorDatabase
{
    public partial class Database
    {
		public const int VersionMajor = 1;
		public const int VersionMinor = 7;

		public Database(IDataStorage storage)
		{
            _serializer = new JsonSerializer();
            _content = new DatabaseContent(_serializer, storage);
		}

		public void Save(IDataStorage storage)
		{
            foreach (var item in _ammunitionObsoleteMap) item.Value.Save(_content.GetAmmunitionObsolete(item.Key));
            foreach (var item in _componentMap) item.Value.Save(_content.GetComponent(item.Key));
            foreach (var item in _componentGroupTagMap) item.Value.Save(_content.GetComponentGroupTag(item.Key));
            foreach (var item in _componentModMap) item.Value.Save(_content.GetComponentMod(item.Key));
            foreach (var item in _componentStatsMap) item.Value.Save(_content.GetComponentStats(item.Key));
            foreach (var item in _componentStatUpgradeMap) item.Value.Save(_content.GetComponentStatUpgrade(item.Key));
            foreach (var item in _deviceMap) item.Value.Save(_content.GetDevice(item.Key));
            foreach (var item in _droneBayMap) item.Value.Save(_content.GetDroneBay(item.Key));
            foreach (var item in _factionMap) item.Value.Save(_content.GetFaction(item.Key));
            foreach (var item in _gameObjectPrefabMap) item.Value.Save(_content.GetGameObjectPrefab(item.Key));
            foreach (var item in _satelliteMap) item.Value.Save(_content.GetSatellite(item.Key));
            foreach (var item in _satelliteBuildMap) item.Value.Save(_content.GetSatelliteBuild(item.Key));
            foreach (var item in _shipMap) item.Value.Save(_content.GetShip(item.Key));
            foreach (var item in _shipBuildMap) item.Value.Save(_content.GetShipBuild(item.Key));
            foreach (var item in _statUpgradeTemplateMap) item.Value.Save(_content.GetStatUpgradeTemplate(item.Key));
            foreach (var item in _technologyMap) item.Value.Save(_content.GetTechnology(item.Key));
            foreach (var item in _behaviorTreeMap) item.Value.Save(_content.GetBehaviorTree(item.Key));
            foreach (var item in _characterMap) item.Value.Save(_content.GetCharacter(item.Key));
            foreach (var item in _combatRulesMap) item.Value.Save(_content.GetCombatRules(item.Key));
            foreach (var item in _fleetMap) item.Value.Save(_content.GetFleet(item.Key));
            foreach (var item in _lootMap) item.Value.Save(_content.GetLoot(item.Key));
            foreach (var item in _questMap) item.Value.Save(_content.GetQuest(item.Key));
            foreach (var item in _questItemMap) item.Value.Save(_content.GetQuestItem(item.Key));
            foreach (var item in _ammunitionMap) item.Value.Save(_content.GetAmmunition(item.Key));
            foreach (var item in _bulletPrefabMap) item.Value.Save(_content.GetBulletPrefab(item.Key));
            foreach (var item in _visualEffectMap) item.Value.Save(_content.GetVisualEffect(item.Key));
            foreach (var item in _weaponMap) item.Value.Save(_content.GetWeapon(item.Key));
			_combatSettings?.Save(_content.CombatSettings);
			_databaseSettings?.Save(_content.DatabaseSettings);
			_debugSettings?.Save(_content.DebugSettings);
			_explorationSettings?.Save(_content.ExplorationSettings);
			_factionsSettings?.Save(_content.FactionsSettings);
			_galaxySettings?.Save(_content.GalaxySettings);
			_localizationSettings?.Save(_content.LocalizationSettings);
			_musicPlaylist?.Save(_content.MusicPlaylist);
			_shipModSettings?.Save(_content.ShipModSettings);
			_shipSettings?.Save(_content.ShipSettings);
			_skillSettings?.Save(_content.SkillSettings);
			_specialEventSettings?.Save(_content.SpecialEventSettings);
			_uiSettings?.Save(_content.UiSettings);
			_weaponSlots?.Save(_content.WeaponSlots);
		
			_content.Save(storage, _serializer);
		}

		public IEnumerable<IItemId> GetItemList(Type type)
        {
            if (type == typeof(AmmunitionObsolete)) return _content.AmmunitionObsoleteList.Select(item => new ItemId<AmmunitionObsolete>(item));
            if (type == typeof(Component)) return _content.ComponentList.Select(item => new ItemId<Component>(item));
            if (type == typeof(ComponentGroupTag)) return _content.ComponentGroupTagList.Select(item => new ItemId<ComponentGroupTag>(item));
            if (type == typeof(ComponentMod)) return _content.ComponentModList.Select(item => new ItemId<ComponentMod>(item));
            if (type == typeof(ComponentStats)) return _content.ComponentStatsList.Select(item => new ItemId<ComponentStats>(item));
            if (type == typeof(ComponentStatUpgrade)) return _content.ComponentStatUpgradeList.Select(item => new ItemId<ComponentStatUpgrade>(item));
            if (type == typeof(Device)) return _content.DeviceList.Select(item => new ItemId<Device>(item));
            if (type == typeof(DroneBay)) return _content.DroneBayList.Select(item => new ItemId<DroneBay>(item));
            if (type == typeof(Faction)) return _content.FactionList.Select(item => new ItemId<Faction>(item));
            if (type == typeof(GameObjectPrefab)) return _content.GameObjectPrefabList.Select(item => new ItemId<GameObjectPrefab>(item));
            if (type == typeof(Satellite)) return _content.SatelliteList.Select(item => new ItemId<Satellite>(item));
            if (type == typeof(SatelliteBuild)) return _content.SatelliteBuildList.Select(item => new ItemId<SatelliteBuild>(item));
            if (type == typeof(Ship)) return _content.ShipList.Select(item => new ItemId<Ship>(item));
            if (type == typeof(ShipBuild)) return _content.ShipBuildList.Select(item => new ItemId<ShipBuild>(item));
            if (type == typeof(StatUpgradeTemplate)) return _content.StatUpgradeTemplateList.Select(item => new ItemId<StatUpgradeTemplate>(item));
            if (type == typeof(Technology)) return _content.TechnologyList.Select(item => new ItemId<Technology>(item));
            if (type == typeof(BehaviorTreeModel)) return _content.BehaviorTreeList.Select(item => new ItemId<BehaviorTreeModel>(item));
            if (type == typeof(Character)) return _content.CharacterList.Select(item => new ItemId<Character>(item));
            if (type == typeof(CombatRules)) return _content.CombatRulesList.Select(item => new ItemId<CombatRules>(item));
            if (type == typeof(Fleet)) return _content.FleetList.Select(item => new ItemId<Fleet>(item));
            if (type == typeof(LootModel)) return _content.LootList.Select(item => new ItemId<LootModel>(item));
            if (type == typeof(QuestModel)) return _content.QuestList.Select(item => new ItemId<QuestModel>(item));
            if (type == typeof(QuestItem)) return _content.QuestItemList.Select(item => new ItemId<QuestItem>(item));
            if (type == typeof(Ammunition)) return _content.AmmunitionList.Select(item => new ItemId<Ammunition>(item));
            if (type == typeof(BulletPrefab)) return _content.BulletPrefabList.Select(item => new ItemId<BulletPrefab>(item));
            if (type == typeof(VisualEffect)) return _content.VisualEffectList.Select(item => new ItemId<VisualEffect>(item));
            if (type == typeof(Weapon)) return _content.WeaponList.Select(item => new ItemId<Weapon>(item));
            return Enumerable.Empty<IItemId>();
        }

        public IEnumerable<object> GetAllItems()
        {
            foreach (var item in _content.AmmunitionObsoleteList)
                yield return GetAmmunitionObsolete(item.Id);
            foreach (var item in _content.ComponentList)
                yield return GetComponent(item.Id);
            foreach (var item in _content.ComponentGroupTagList)
                yield return GetComponentGroupTag(item.Id);
            foreach (var item in _content.ComponentModList)
                yield return GetComponentMod(item.Id);
            foreach (var item in _content.ComponentStatsList)
                yield return GetComponentStats(item.Id);
            foreach (var item in _content.ComponentStatUpgradeList)
                yield return GetComponentStatUpgrade(item.Id);
            foreach (var item in _content.DeviceList)
                yield return GetDevice(item.Id);
            foreach (var item in _content.DroneBayList)
                yield return GetDroneBay(item.Id);
            foreach (var item in _content.FactionList)
                yield return GetFaction(item.Id);
            foreach (var item in _content.GameObjectPrefabList)
                yield return GetGameObjectPrefab(item.Id);
            foreach (var item in _content.SatelliteList)
                yield return GetSatellite(item.Id);
            foreach (var item in _content.SatelliteBuildList)
                yield return GetSatelliteBuild(item.Id);
            foreach (var item in _content.ShipList)
                yield return GetShip(item.Id);
            foreach (var item in _content.ShipBuildList)
                yield return GetShipBuild(item.Id);
            foreach (var item in _content.StatUpgradeTemplateList)
                yield return GetStatUpgradeTemplate(item.Id);
            foreach (var item in _content.TechnologyList)
                yield return GetTechnology(item.Id);
            foreach (var item in _content.BehaviorTreeList)
                yield return GetBehaviorTree(item.Id);
            foreach (var item in _content.CharacterList)
                yield return GetCharacter(item.Id);
            foreach (var item in _content.CombatRulesList)
                yield return GetCombatRules(item.Id);
            foreach (var item in _content.FleetList)
                yield return GetFleet(item.Id);
            foreach (var item in _content.LootList)
                yield return GetLoot(item.Id);
            foreach (var item in _content.QuestList)
                yield return GetQuest(item.Id);
            foreach (var item in _content.QuestItemList)
                yield return GetQuestItem(item.Id);
            foreach (var item in _content.AmmunitionList)
                yield return GetAmmunition(item.Id);
            foreach (var item in _content.BulletPrefabList)
                yield return GetBulletPrefab(item.Id);
            foreach (var item in _content.VisualEffectList)
                yield return GetVisualEffect(item.Id);
            foreach (var item in _content.WeaponList)
                yield return GetWeapon(item.Id);
            if (_content.CombatSettings != null)
				yield return CombatSettings;
            if (_content.DatabaseSettings != null)
				yield return DatabaseSettings;
            if (_content.DebugSettings != null)
				yield return DebugSettings;
            if (_content.ExplorationSettings != null)
				yield return ExplorationSettings;
            if (_content.FactionsSettings != null)
				yield return FactionsSettings;
            if (_content.GalaxySettings != null)
				yield return GalaxySettings;
            if (_content.LocalizationSettings != null)
				yield return LocalizationSettings;
            if (_content.MusicPlaylist != null)
				yield return MusicPlaylist;
            if (_content.ShipModSettings != null)
				yield return ShipModSettings;
            if (_content.ShipSettings != null)
				yield return ShipSettings;
            if (_content.SkillSettings != null)
				yield return SkillSettings;
            if (_content.SpecialEventSettings != null)
				yield return SpecialEventSettings;
            if (_content.UiSettings != null)
				yield return UiSettings;
            if (_content.WeaponSlots != null)
				yield return WeaponSlots;
        }

        public IItemId GetItemId(Type type, int id)
        {
            if (type == typeof(AmmunitionObsolete)) return GetAmmunitionObsoleteId(id);
            if (type == typeof(Component)) return GetComponentId(id);
            if (type == typeof(ComponentGroupTag)) return GetComponentGroupTagId(id);
            if (type == typeof(ComponentMod)) return GetComponentModId(id);
            if (type == typeof(ComponentStats)) return GetComponentStatsId(id);
            if (type == typeof(ComponentStatUpgrade)) return GetComponentStatUpgradeId(id);
            if (type == typeof(Device)) return GetDeviceId(id);
            if (type == typeof(DroneBay)) return GetDroneBayId(id);
            if (type == typeof(Faction)) return GetFactionId(id);
            if (type == typeof(GameObjectPrefab)) return GetGameObjectPrefabId(id);
            if (type == typeof(Satellite)) return GetSatelliteId(id);
            if (type == typeof(SatelliteBuild)) return GetSatelliteBuildId(id);
            if (type == typeof(Ship)) return GetShipId(id);
            if (type == typeof(ShipBuild)) return GetShipBuildId(id);
            if (type == typeof(StatUpgradeTemplate)) return GetStatUpgradeTemplateId(id);
            if (type == typeof(Technology)) return GetTechnologyId(id);
            if (type == typeof(BehaviorTreeModel)) return GetBehaviorTreeId(id);
            if (type == typeof(Character)) return GetCharacterId(id);
            if (type == typeof(CombatRules)) return GetCombatRulesId(id);
            if (type == typeof(Fleet)) return GetFleetId(id);
            if (type == typeof(LootModel)) return GetLootId(id);
            if (type == typeof(QuestModel)) return GetQuestId(id);
            if (type == typeof(QuestItem)) return GetQuestItemId(id);
            if (type == typeof(Ammunition)) return GetAmmunitionId(id);
            if (type == typeof(BulletPrefab)) return GetBulletPrefabId(id);
            if (type == typeof(VisualEffect)) return GetVisualEffectId(id);
            if (type == typeof(Weapon)) return GetWeaponId(id);
            return ItemId<Type>.Empty;
        }

        public object GetItem(ItemType type, int id)
        {
            switch (type)
            {
				case ItemType.AmmunitionObsolete: return GetAmmunitionObsolete(id);
				case ItemType.Component: return GetComponent(id);
				case ItemType.ComponentGroupTag: return GetComponentGroupTag(id);
				case ItemType.ComponentMod: return GetComponentMod(id);
				case ItemType.ComponentStats: return GetComponentStats(id);
				case ItemType.ComponentStatUpgrade: return GetComponentStatUpgrade(id);
				case ItemType.Device: return GetDevice(id);
				case ItemType.DroneBay: return GetDroneBay(id);
				case ItemType.Faction: return GetFaction(id);
				case ItemType.GameObjectPrefab: return GetGameObjectPrefab(id);
				case ItemType.Satellite: return GetSatellite(id);
				case ItemType.SatelliteBuild: return GetSatelliteBuild(id);
				case ItemType.Ship: return GetShip(id);
				case ItemType.ShipBuild: return GetShipBuild(id);
				case ItemType.StatUpgradeTemplate: return GetStatUpgradeTemplate(id);
				case ItemType.Technology: return GetTechnology(id);
				case ItemType.BehaviorTree: return GetBehaviorTree(id);
				case ItemType.Character: return GetCharacter(id);
				case ItemType.CombatRules: return GetCombatRules(id);
				case ItemType.Fleet: return GetFleet(id);
				case ItemType.Loot: return GetLoot(id);
				case ItemType.Quest: return GetQuest(id);
				case ItemType.QuestItem: return GetQuestItem(id);
				case ItemType.Ammunition: return GetAmmunition(id);
				case ItemType.BulletPrefab: return GetBulletPrefab(id);
				case ItemType.VisualEffect: return GetVisualEffect(id);
				case ItemType.Weapon: return GetWeapon(id);
				case ItemType.CombatSettings: return CombatSettings;
				case ItemType.DatabaseSettings: return DatabaseSettings;
				case ItemType.DebugSettings: return DebugSettings;
				case ItemType.ExplorationSettings: return ExplorationSettings;
				case ItemType.FactionsSettings: return FactionsSettings;
				case ItemType.GalaxySettings: return GalaxySettings;
				case ItemType.LocalizationSettings: return LocalizationSettings;
				case ItemType.MusicPlaylist: return MusicPlaylist;
				case ItemType.ShipModSettings: return ShipModSettings;
				case ItemType.ShipSettings: return ShipSettings;
				case ItemType.SkillSettings: return SkillSettings;
				case ItemType.SpecialEventSettings: return SpecialEventSettings;
				case ItemType.UiSettings: return UiSettings;
				case ItemType.WeaponSlots: return WeaponSlots;
                default: return null;
            }
        }


		public CombatSettings CombatSettings => _combatSettings ?? (_combatSettings = CombatSettings.Create(_content.CombatSettings, this));
		public DatabaseSettings DatabaseSettings => _databaseSettings ?? (_databaseSettings = DatabaseSettings.Create(_content.DatabaseSettings, this));
		public DebugSettings DebugSettings => _debugSettings ?? (_debugSettings = DebugSettings.Create(_content.DebugSettings, this));
		public ExplorationSettings ExplorationSettings => _explorationSettings ?? (_explorationSettings = ExplorationSettings.Create(_content.ExplorationSettings, this));
		public FactionsSettings FactionsSettings => _factionsSettings ?? (_factionsSettings = FactionsSettings.Create(_content.FactionsSettings, this));
		public GalaxySettings GalaxySettings => _galaxySettings ?? (_galaxySettings = GalaxySettings.Create(_content.GalaxySettings, this));
		public LocalizationSettings LocalizationSettings => _localizationSettings ?? (_localizationSettings = LocalizationSettings.Create(_content.LocalizationSettings, this));
		public MusicPlaylist MusicPlaylist => _musicPlaylist ?? (_musicPlaylist = MusicPlaylist.Create(_content.MusicPlaylist, this));
		public ShipModSettings ShipModSettings => _shipModSettings ?? (_shipModSettings = ShipModSettings.Create(_content.ShipModSettings, this));
		public ShipSettings ShipSettings => _shipSettings ?? (_shipSettings = ShipSettings.Create(_content.ShipSettings, this));
		public SkillSettings SkillSettings => _skillSettings ?? (_skillSettings = SkillSettings.Create(_content.SkillSettings, this));
		public SpecialEventSettings SpecialEventSettings => _specialEventSettings ?? (_specialEventSettings = SpecialEventSettings.Create(_content.SpecialEventSettings, this));
		public UiSettings UiSettings => _uiSettings ?? (_uiSettings = UiSettings.Create(_content.UiSettings, this));
		public WeaponSlots WeaponSlots => _weaponSlots ?? (_weaponSlots = WeaponSlots.Create(_content.WeaponSlots, this));

		public ItemId<AmmunitionObsolete> GetAmmunitionObsoleteId(int id) { return new ItemId<AmmunitionObsolete>(_content.GetAmmunitionObsolete(id)); }
        public AmmunitionObsolete GetAmmunitionObsolete(int id)
        {
            if (!_ammunitionObsoleteMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetAmmunitionObsolete(id);
                item = AmmunitionObsolete.Create(serializable, this);
                _ammunitionObsoleteMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Component> GetComponentId(int id) { return new ItemId<Component>(_content.GetComponent(id)); }
        public Component GetComponent(int id)
        {
            if (!_componentMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetComponent(id);
                item = Component.Create(serializable, this);
                _componentMap.Add(id, item);
            }
            return item;
        }

		public ItemId<ComponentGroupTag> GetComponentGroupTagId(int id) { return new ItemId<ComponentGroupTag>(_content.GetComponentGroupTag(id)); }
        public ComponentGroupTag GetComponentGroupTag(int id)
        {
            if (!_componentGroupTagMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetComponentGroupTag(id);
                item = ComponentGroupTag.Create(serializable, this);
                _componentGroupTagMap.Add(id, item);
            }
            return item;
        }

		public ItemId<ComponentMod> GetComponentModId(int id) { return new ItemId<ComponentMod>(_content.GetComponentMod(id)); }
        public ComponentMod GetComponentMod(int id)
        {
            if (!_componentModMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetComponentMod(id);
                item = ComponentMod.Create(serializable, this);
                _componentModMap.Add(id, item);
            }
            return item;
        }

		public ItemId<ComponentStats> GetComponentStatsId(int id) { return new ItemId<ComponentStats>(_content.GetComponentStats(id)); }
        public ComponentStats GetComponentStats(int id)
        {
            if (!_componentStatsMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetComponentStats(id);
                item = ComponentStats.Create(serializable, this);
                _componentStatsMap.Add(id, item);
            }
            return item;
        }

		public ItemId<ComponentStatUpgrade> GetComponentStatUpgradeId(int id) { return new ItemId<ComponentStatUpgrade>(_content.GetComponentStatUpgrade(id)); }
        public ComponentStatUpgrade GetComponentStatUpgrade(int id)
        {
            if (!_componentStatUpgradeMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetComponentStatUpgrade(id);
                item = ComponentStatUpgrade.Create(serializable, this);
                _componentStatUpgradeMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Device> GetDeviceId(int id) { return new ItemId<Device>(_content.GetDevice(id)); }
        public Device GetDevice(int id)
        {
            if (!_deviceMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetDevice(id);
                item = Device.Create(serializable, this);
                _deviceMap.Add(id, item);
            }
            return item;
        }

		public ItemId<DroneBay> GetDroneBayId(int id) { return new ItemId<DroneBay>(_content.GetDroneBay(id)); }
        public DroneBay GetDroneBay(int id)
        {
            if (!_droneBayMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetDroneBay(id);
                item = DroneBay.Create(serializable, this);
                _droneBayMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Faction> GetFactionId(int id) { return new ItemId<Faction>(_content.GetFaction(id)); }
        public Faction GetFaction(int id)
        {
            if (!_factionMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetFaction(id);
                item = Faction.Create(serializable, this);
                _factionMap.Add(id, item);
            }
            return item;
        }

		public ItemId<GameObjectPrefab> GetGameObjectPrefabId(int id) { return new ItemId<GameObjectPrefab>(_content.GetGameObjectPrefab(id)); }
        public GameObjectPrefab GetGameObjectPrefab(int id)
        {
            if (!_gameObjectPrefabMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetGameObjectPrefab(id);
                item = GameObjectPrefab.Create(serializable, this);
                _gameObjectPrefabMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Satellite> GetSatelliteId(int id) { return new ItemId<Satellite>(_content.GetSatellite(id)); }
        public Satellite GetSatellite(int id)
        {
            if (!_satelliteMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetSatellite(id);
                item = Satellite.Create(serializable, this);
                _satelliteMap.Add(id, item);
            }
            return item;
        }

		public ItemId<SatelliteBuild> GetSatelliteBuildId(int id) { return new ItemId<SatelliteBuild>(_content.GetSatelliteBuild(id)); }
        public SatelliteBuild GetSatelliteBuild(int id)
        {
            if (!_satelliteBuildMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetSatelliteBuild(id);
                item = SatelliteBuild.Create(serializable, this);
                _satelliteBuildMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Ship> GetShipId(int id) { return new ItemId<Ship>(_content.GetShip(id)); }
        public Ship GetShip(int id)
        {
            if (!_shipMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetShip(id);
                item = Ship.Create(serializable, this);
                _shipMap.Add(id, item);
            }
            return item;
        }

		public ItemId<ShipBuild> GetShipBuildId(int id) { return new ItemId<ShipBuild>(_content.GetShipBuild(id)); }
        public ShipBuild GetShipBuild(int id)
        {
            if (!_shipBuildMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetShipBuild(id);
                item = ShipBuild.Create(serializable, this);
                _shipBuildMap.Add(id, item);
            }
            return item;
        }

		public ItemId<StatUpgradeTemplate> GetStatUpgradeTemplateId(int id) { return new ItemId<StatUpgradeTemplate>(_content.GetStatUpgradeTemplate(id)); }
        public StatUpgradeTemplate GetStatUpgradeTemplate(int id)
        {
            if (!_statUpgradeTemplateMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetStatUpgradeTemplate(id);
                item = StatUpgradeTemplate.Create(serializable, this);
                _statUpgradeTemplateMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Technology> GetTechnologyId(int id) { return new ItemId<Technology>(_content.GetTechnology(id)); }
        public Technology GetTechnology(int id)
        {
            if (!_technologyMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetTechnology(id);
                item = Technology.Create(serializable, this);
                _technologyMap.Add(id, item);
            }
            return item;
        }

		public ItemId<BehaviorTreeModel> GetBehaviorTreeId(int id) { return new ItemId<BehaviorTreeModel>(_content.GetBehaviorTree(id)); }
        public BehaviorTreeModel GetBehaviorTree(int id)
        {
            if (!_behaviorTreeMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetBehaviorTree(id);
                item = BehaviorTreeModel.Create(serializable, this);
                _behaviorTreeMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Character> GetCharacterId(int id) { return new ItemId<Character>(_content.GetCharacter(id)); }
        public Character GetCharacter(int id)
        {
            if (!_characterMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetCharacter(id);
                item = Character.Create(serializable, this);
                _characterMap.Add(id, item);
            }
            return item;
        }

		public ItemId<CombatRules> GetCombatRulesId(int id) { return new ItemId<CombatRules>(_content.GetCombatRules(id)); }
        public CombatRules GetCombatRules(int id)
        {
            if (!_combatRulesMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetCombatRules(id);
                item = CombatRules.Create(serializable, this);
                _combatRulesMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Fleet> GetFleetId(int id) { return new ItemId<Fleet>(_content.GetFleet(id)); }
        public Fleet GetFleet(int id)
        {
            if (!_fleetMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetFleet(id);
                item = Fleet.Create(serializable, this);
                _fleetMap.Add(id, item);
            }
            return item;
        }

		public ItemId<LootModel> GetLootId(int id) { return new ItemId<LootModel>(_content.GetLoot(id)); }
        public LootModel GetLoot(int id)
        {
            if (!_lootMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetLoot(id);
                item = LootModel.Create(serializable, this);
                _lootMap.Add(id, item);
            }
            return item;
        }

		public ItemId<QuestModel> GetQuestId(int id) { return new ItemId<QuestModel>(_content.GetQuest(id)); }
        public QuestModel GetQuest(int id)
        {
            if (!_questMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetQuest(id);
                item = QuestModel.Create(serializable, this);
                _questMap.Add(id, item);
            }
            return item;
        }

		public ItemId<QuestItem> GetQuestItemId(int id) { return new ItemId<QuestItem>(_content.GetQuestItem(id)); }
        public QuestItem GetQuestItem(int id)
        {
            if (!_questItemMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetQuestItem(id);
                item = QuestItem.Create(serializable, this);
                _questItemMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Ammunition> GetAmmunitionId(int id) { return new ItemId<Ammunition>(_content.GetAmmunition(id)); }
        public Ammunition GetAmmunition(int id)
        {
            if (!_ammunitionMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetAmmunition(id);
                item = Ammunition.Create(serializable, this);
                _ammunitionMap.Add(id, item);
            }
            return item;
        }

		public ItemId<BulletPrefab> GetBulletPrefabId(int id) { return new ItemId<BulletPrefab>(_content.GetBulletPrefab(id)); }
        public BulletPrefab GetBulletPrefab(int id)
        {
            if (!_bulletPrefabMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetBulletPrefab(id);
                item = BulletPrefab.Create(serializable, this);
                _bulletPrefabMap.Add(id, item);
            }
            return item;
        }

		public ItemId<VisualEffect> GetVisualEffectId(int id) { return new ItemId<VisualEffect>(_content.GetVisualEffect(id)); }
        public VisualEffect GetVisualEffect(int id)
        {
            if (!_visualEffectMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetVisualEffect(id);
                item = VisualEffect.Create(serializable, this);
                _visualEffectMap.Add(id, item);
            }
            return item;
        }

		public ItemId<Weapon> GetWeaponId(int id) { return new ItemId<Weapon>(_content.GetWeapon(id)); }
        public Weapon GetWeapon(int id)
        {
            if (!_weaponMap.TryGetValue(id, out var item))
            {
                var serializable = _content.GetWeapon(id);
                item = Weapon.Create(serializable, this);
                _weaponMap.Add(id, item);
            }
            return item;
        }


        public IImageData GetImage(string name) { return _content.GetImage(name); }

        private void Clear()
        {
			_ammunitionObsoleteMap.Clear();
			_componentMap.Clear();
			_componentGroupTagMap.Clear();
			_componentModMap.Clear();
			_componentStatsMap.Clear();
			_componentStatUpgradeMap.Clear();
			_deviceMap.Clear();
			_droneBayMap.Clear();
			_factionMap.Clear();
			_gameObjectPrefabMap.Clear();
			_satelliteMap.Clear();
			_satelliteBuildMap.Clear();
			_shipMap.Clear();
			_shipBuildMap.Clear();
			_statUpgradeTemplateMap.Clear();
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

			_combatSettings = null;
			_databaseSettings = null;
			_debugSettings = null;
			_explorationSettings = null;
			_factionsSettings = null;
			_galaxySettings = null;
			_localizationSettings = null;
			_musicPlaylist = null;
			_shipModSettings = null;
			_shipSettings = null;
			_skillSettings = null;
			_specialEventSettings = null;
			_uiSettings = null;
			_weaponSlots = null;
        }

		private readonly Dictionary<int, AmmunitionObsolete> _ammunitionObsoleteMap = new Dictionary<int, AmmunitionObsolete>();
		private readonly Dictionary<int, Component> _componentMap = new Dictionary<int, Component>();
		private readonly Dictionary<int, ComponentGroupTag> _componentGroupTagMap = new Dictionary<int, ComponentGroupTag>();
		private readonly Dictionary<int, ComponentMod> _componentModMap = new Dictionary<int, ComponentMod>();
		private readonly Dictionary<int, ComponentStats> _componentStatsMap = new Dictionary<int, ComponentStats>();
		private readonly Dictionary<int, ComponentStatUpgrade> _componentStatUpgradeMap = new Dictionary<int, ComponentStatUpgrade>();
		private readonly Dictionary<int, Device> _deviceMap = new Dictionary<int, Device>();
		private readonly Dictionary<int, DroneBay> _droneBayMap = new Dictionary<int, DroneBay>();
		private readonly Dictionary<int, Faction> _factionMap = new Dictionary<int, Faction>();
		private readonly Dictionary<int, GameObjectPrefab> _gameObjectPrefabMap = new Dictionary<int, GameObjectPrefab>();
		private readonly Dictionary<int, Satellite> _satelliteMap = new Dictionary<int, Satellite>();
		private readonly Dictionary<int, SatelliteBuild> _satelliteBuildMap = new Dictionary<int, SatelliteBuild>();
		private readonly Dictionary<int, Ship> _shipMap = new Dictionary<int, Ship>();
		private readonly Dictionary<int, ShipBuild> _shipBuildMap = new Dictionary<int, ShipBuild>();
		private readonly Dictionary<int, StatUpgradeTemplate> _statUpgradeTemplateMap = new Dictionary<int, StatUpgradeTemplate>();
		private readonly Dictionary<int, Technology> _technologyMap = new Dictionary<int, Technology>();
		private readonly Dictionary<int, BehaviorTreeModel> _behaviorTreeMap = new Dictionary<int, BehaviorTreeModel>();
		private readonly Dictionary<int, Character> _characterMap = new Dictionary<int, Character>();
		private readonly Dictionary<int, CombatRules> _combatRulesMap = new Dictionary<int, CombatRules>();
		private readonly Dictionary<int, Fleet> _fleetMap = new Dictionary<int, Fleet>();
		private readonly Dictionary<int, LootModel> _lootMap = new Dictionary<int, LootModel>();
		private readonly Dictionary<int, QuestModel> _questMap = new Dictionary<int, QuestModel>();
		private readonly Dictionary<int, QuestItem> _questItemMap = new Dictionary<int, QuestItem>();
		private readonly Dictionary<int, Ammunition> _ammunitionMap = new Dictionary<int, Ammunition>();
		private readonly Dictionary<int, BulletPrefab> _bulletPrefabMap = new Dictionary<int, BulletPrefab>();
		private readonly Dictionary<int, VisualEffect> _visualEffectMap = new Dictionary<int, VisualEffect>();
		private readonly Dictionary<int, Weapon> _weaponMap = new Dictionary<int, Weapon>();

		private CombatSettings _combatSettings;
		private DatabaseSettings _databaseSettings;
		private DebugSettings _debugSettings;
		private ExplorationSettings _explorationSettings;
		private FactionsSettings _factionsSettings;
		private GalaxySettings _galaxySettings;
		private LocalizationSettings _localizationSettings;
		private MusicPlaylist _musicPlaylist;
		private ShipModSettings _shipModSettings;
		private ShipSettings _shipSettings;
		private SkillSettings _skillSettings;
		private SpecialEventSettings _specialEventSettings;
		private UiSettings _uiSettings;
		private WeaponSlots _weaponSlots;
	
        private readonly IJsonSerializer _serializer;
		private readonly DatabaseContent _content;
	}
}

