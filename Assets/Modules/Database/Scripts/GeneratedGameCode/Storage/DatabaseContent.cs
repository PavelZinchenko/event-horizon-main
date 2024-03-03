//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.Storage
{
    public class DatabaseContent : IContentLoader
    {
        public DatabaseContent(IJsonSerializer jsonSerializer, IDataStorage storage)
        {
            _serializer = jsonSerializer;
            storage?.LoadContent(this);
            _allowDuplicates = true;
        }

        public void LoadParent(IDataStorage storage)
        {
            storage?.LoadContent(this);
        }

        public void LoadJson(string name, string content)
        {
            var item = _serializer.FromJson<SerializableItem>(content);
            var type = item.ItemType;

            if (type == ItemType.AmmunitionObsolete)
            {
			    if (!_ammunitionObsoleteMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<AmmunitionObsoleteSerializable>(content);
                    data.FileName = name;
                    _ammunitionObsoleteMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate AmmunitionObsolete ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Component)
            {
			    if (!_componentMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<ComponentSerializable>(content);
                    data.FileName = name;
                    _componentMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Component ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.ComponentMod)
            {
			    if (!_componentModMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<ComponentModSerializable>(content);
                    data.FileName = name;
                    _componentModMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate ComponentMod ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.ComponentStats)
            {
			    if (!_componentStatsMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<ComponentStatsSerializable>(content);
                    data.FileName = name;
                    _componentStatsMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate ComponentStats ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Device)
            {
			    if (!_deviceMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<DeviceSerializable>(content);
                    data.FileName = name;
                    _deviceMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Device ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.DroneBay)
            {
			    if (!_droneBayMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<DroneBaySerializable>(content);
                    data.FileName = name;
                    _droneBayMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate DroneBay ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Faction)
            {
			    if (!_factionMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<FactionSerializable>(content);
                    data.FileName = name;
                    _factionMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Faction ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.GameObjectPrefab)
            {
			    if (!_gameObjectPrefabMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<GameObjectPrefabSerializable>(content);
                    data.FileName = name;
                    _gameObjectPrefabMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate GameObjectPrefab ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Satellite)
            {
			    if (!_satelliteMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<SatelliteSerializable>(content);
                    data.FileName = name;
                    _satelliteMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Satellite ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.SatelliteBuild)
            {
			    if (!_satelliteBuildMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<SatelliteBuildSerializable>(content);
                    data.FileName = name;
                    _satelliteBuildMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate SatelliteBuild ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Ship)
            {
			    if (!_shipMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<ShipSerializable>(content);
                    data.FileName = name;
                    _shipMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Ship ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.ShipBuild)
            {
			    if (!_shipBuildMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<ShipBuildSerializable>(content);
                    data.FileName = name;
                    _shipBuildMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate ShipBuild ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Skill)
            {
			    if (!_skillMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<SkillSerializable>(content);
                    data.FileName = name;
                    _skillMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Skill ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Technology)
            {
			    if (!_technologyMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<TechnologySerializable>(content);
                    data.FileName = name;
                    _technologyMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Technology ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.BehaviorTree)
            {
			    if (!_behaviorTreeMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<BehaviorTreeSerializable>(content);
                    data.FileName = name;
                    _behaviorTreeMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate BehaviorTree ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Character)
            {
			    if (!_characterMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<CharacterSerializable>(content);
                    data.FileName = name;
                    _characterMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Character ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.CombatRules)
            {
			    if (!_combatRulesMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<CombatRulesSerializable>(content);
                    data.FileName = name;
                    _combatRulesMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate CombatRules ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Fleet)
            {
			    if (!_fleetMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<FleetSerializable>(content);
                    data.FileName = name;
                    _fleetMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Fleet ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Loot)
            {
			    if (!_lootMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<LootSerializable>(content);
                    data.FileName = name;
                    _lootMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Loot ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Quest)
            {
			    if (!_questMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<QuestSerializable>(content);
                    data.FileName = name;
                    _questMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Quest ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.QuestItem)
            {
			    if (!_questItemMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<QuestItemSerializable>(content);
                    data.FileName = name;
                    _questItemMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate QuestItem ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Ammunition)
            {
			    if (!_ammunitionMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<AmmunitionSerializable>(content);
                    data.FileName = name;
                    _ammunitionMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Ammunition ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.BulletPrefab)
            {
			    if (!_bulletPrefabMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<BulletPrefabSerializable>(content);
                    data.FileName = name;
                    _bulletPrefabMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate BulletPrefab ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.VisualEffect)
            {
			    if (!_visualEffectMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<VisualEffectSerializable>(content);
                    data.FileName = name;
                    _visualEffectMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate VisualEffect ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.Weapon)
            {
			    if (!_weaponMap.ContainsKey(item.Id))
                {
                    var data = _serializer.FromJson<WeaponSerializable>(content);
                    data.FileName = name;
                    _weaponMap.Add(data.Id, data);
                    return;
                }

                if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate Weapon ID - " + item.Id + " (" + name + ")");
            }
            else if (type == ItemType.CombatSettings)
            {
                if (CombatSettings == null)
                {
                    var data = _serializer.FromJson<CombatSettingsSerializable>(content);
                    data.FileName = name;
                    CombatSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate CombatSettings file found - " + name);
            }
            else if (type == ItemType.DatabaseSettings)
            {
                if (DatabaseSettings == null)
                {
                    var data = _serializer.FromJson<DatabaseSettingsSerializable>(content);
                    data.FileName = name;
                    DatabaseSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate DatabaseSettings file found - " + name);
            }
            else if (type == ItemType.DebugSettings)
            {
                if (DebugSettings == null)
                {
                    var data = _serializer.FromJson<DebugSettingsSerializable>(content);
                    data.FileName = name;
                    DebugSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate DebugSettings file found - " + name);
            }
            else if (type == ItemType.ExplorationSettings)
            {
                if (ExplorationSettings == null)
                {
                    var data = _serializer.FromJson<ExplorationSettingsSerializable>(content);
                    data.FileName = name;
                    ExplorationSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate ExplorationSettings file found - " + name);
            }
            else if (type == ItemType.FactionsSettings)
            {
                if (FactionsSettings == null)
                {
                    var data = _serializer.FromJson<FactionsSettingsSerializable>(content);
                    data.FileName = name;
                    FactionsSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate FactionsSettings file found - " + name);
            }
            else if (type == ItemType.FrontierSettings)
            {
                if (FrontierSettings == null)
                {
                    var data = _serializer.FromJson<FrontierSettingsSerializable>(content);
                    data.FileName = name;
                    FrontierSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate FrontierSettings file found - " + name);
            }
            else if (type == ItemType.GalaxySettings)
            {
                if (GalaxySettings == null)
                {
                    var data = _serializer.FromJson<GalaxySettingsSerializable>(content);
                    data.FileName = name;
                    GalaxySettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate GalaxySettings file found - " + name);
            }
            else if (type == ItemType.MusicPlaylist)
            {
                if (MusicPlaylist == null)
                {
                    var data = _serializer.FromJson<MusicPlaylistSerializable>(content);
                    data.FileName = name;
                    MusicPlaylist = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate MusicPlaylist file found - " + name);
            }
            else if (type == ItemType.ShipModSettings)
            {
                if (ShipModSettings == null)
                {
                    var data = _serializer.FromJson<ShipModSettingsSerializable>(content);
                    data.FileName = name;
                    ShipModSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate ShipModSettings file found - " + name);
            }
            else if (type == ItemType.ShipSettings)
            {
                if (ShipSettings == null)
                {
                    var data = _serializer.FromJson<ShipSettingsSerializable>(content);
                    data.FileName = name;
                    ShipSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate ShipSettings file found - " + name);
            }
            else if (type == ItemType.SkillSettings)
            {
                if (SkillSettings == null)
                {
                    var data = _serializer.FromJson<SkillSettingsSerializable>(content);
                    data.FileName = name;
                    SkillSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate SkillSettings file found - " + name);
            }
            else if (type == ItemType.SpecialEventSettings)
            {
                if (SpecialEventSettings == null)
                {
                    var data = _serializer.FromJson<SpecialEventSettingsSerializable>(content);
                    data.FileName = name;
                    SpecialEventSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate SpecialEventSettings file found - " + name);
            }
            else if (type == ItemType.UiSettings)
            {
                if (UiSettings == null)
                {
                    var data = _serializer.FromJson<UiSettingsSerializable>(content);
                    data.FileName = name;
                    UiSettings = data;
                    return;
                }

				if (!_allowDuplicates)
                    throw new DatabaseException("Duplicate UiSettings file found - " + name);
            }
            else
            {
                throw new DatabaseException("Unknown file type - " + type + "(" + name + ")");
            }
        }

		public void LoadLocalization(string name, string data)
        {
            _localizations.Add(name, data);
        }

        public void LoadImage(string name, IImageData image)
        {
            _images.Add(name, image);
        }

        public void LoadAudioClip(string name, IAudioClipData audioClip)
        {
            _audioClips.Add(name, audioClip);
        }

		public CombatSettingsSerializable CombatSettings { get; private set; }
		public DatabaseSettingsSerializable DatabaseSettings { get; private set; }
		public DebugSettingsSerializable DebugSettings { get; private set; }
		public ExplorationSettingsSerializable ExplorationSettings { get; private set; }
		public FactionsSettingsSerializable FactionsSettings { get; private set; }
		public FrontierSettingsSerializable FrontierSettings { get; private set; }
		public GalaxySettingsSerializable GalaxySettings { get; private set; }
		public MusicPlaylistSerializable MusicPlaylist { get; private set; }
		public ShipModSettingsSerializable ShipModSettings { get; private set; }
		public ShipSettingsSerializable ShipSettings { get; private set; }
		public SkillSettingsSerializable SkillSettings { get; private set; }
		public SpecialEventSettingsSerializable SpecialEventSettings { get; private set; }
		public UiSettingsSerializable UiSettings { get; private set; }

		public IEnumerable<AmmunitionObsoleteSerializable> AmmunitionObsoleteList => _ammunitionObsoleteMap.Values;
		public IEnumerable<ComponentSerializable> ComponentList => _componentMap.Values;
		public IEnumerable<ComponentModSerializable> ComponentModList => _componentModMap.Values;
		public IEnumerable<ComponentStatsSerializable> ComponentStatsList => _componentStatsMap.Values;
		public IEnumerable<DeviceSerializable> DeviceList => _deviceMap.Values;
		public IEnumerable<DroneBaySerializable> DroneBayList => _droneBayMap.Values;
		public IEnumerable<FactionSerializable> FactionList => _factionMap.Values;
		public IEnumerable<GameObjectPrefabSerializable> GameObjectPrefabList => _gameObjectPrefabMap.Values;
		public IEnumerable<SatelliteSerializable> SatelliteList => _satelliteMap.Values;
		public IEnumerable<SatelliteBuildSerializable> SatelliteBuildList => _satelliteBuildMap.Values;
		public IEnumerable<ShipSerializable> ShipList => _shipMap.Values;
		public IEnumerable<ShipBuildSerializable> ShipBuildList => _shipBuildMap.Values;
		public IEnumerable<SkillSerializable> SkillList => _skillMap.Values;
		public IEnumerable<TechnologySerializable> TechnologyList => _technologyMap.Values;
		public IEnumerable<BehaviorTreeSerializable> BehaviorTreeList => _behaviorTreeMap.Values;
		public IEnumerable<CharacterSerializable> CharacterList => _characterMap.Values;
		public IEnumerable<CombatRulesSerializable> CombatRulesList => _combatRulesMap.Values;
		public IEnumerable<FleetSerializable> FleetList => _fleetMap.Values;
		public IEnumerable<LootSerializable> LootList => _lootMap.Values;
		public IEnumerable<QuestSerializable> QuestList => _questMap.Values;
		public IEnumerable<QuestItemSerializable> QuestItemList => _questItemMap.Values;
		public IEnumerable<AmmunitionSerializable> AmmunitionList => _ammunitionMap.Values;
		public IEnumerable<BulletPrefabSerializable> BulletPrefabList => _bulletPrefabMap.Values;
		public IEnumerable<VisualEffectSerializable> VisualEffectList => _visualEffectMap.Values;
		public IEnumerable<WeaponSerializable> WeaponList => _weaponMap.Values;

		public AmmunitionObsoleteSerializable GetAmmunitionObsolete(int id) { return _ammunitionObsoleteMap.TryGetValue(id, out var item) ? item : null; }
		public ComponentSerializable GetComponent(int id) { return _componentMap.TryGetValue(id, out var item) ? item : null; }
		public ComponentModSerializable GetComponentMod(int id) { return _componentModMap.TryGetValue(id, out var item) ? item : null; }
		public ComponentStatsSerializable GetComponentStats(int id) { return _componentStatsMap.TryGetValue(id, out var item) ? item : null; }
		public DeviceSerializable GetDevice(int id) { return _deviceMap.TryGetValue(id, out var item) ? item : null; }
		public DroneBaySerializable GetDroneBay(int id) { return _droneBayMap.TryGetValue(id, out var item) ? item : null; }
		public FactionSerializable GetFaction(int id) { return _factionMap.TryGetValue(id, out var item) ? item : null; }
		public GameObjectPrefabSerializable GetGameObjectPrefab(int id) { return _gameObjectPrefabMap.TryGetValue(id, out var item) ? item : null; }
		public SatelliteSerializable GetSatellite(int id) { return _satelliteMap.TryGetValue(id, out var item) ? item : null; }
		public SatelliteBuildSerializable GetSatelliteBuild(int id) { return _satelliteBuildMap.TryGetValue(id, out var item) ? item : null; }
		public ShipSerializable GetShip(int id) { return _shipMap.TryGetValue(id, out var item) ? item : null; }
		public ShipBuildSerializable GetShipBuild(int id) { return _shipBuildMap.TryGetValue(id, out var item) ? item : null; }
		public SkillSerializable GetSkill(int id) { return _skillMap.TryGetValue(id, out var item) ? item : null; }
		public TechnologySerializable GetTechnology(int id) { return _technologyMap.TryGetValue(id, out var item) ? item : null; }
		public BehaviorTreeSerializable GetBehaviorTree(int id) { return _behaviorTreeMap.TryGetValue(id, out var item) ? item : null; }
		public CharacterSerializable GetCharacter(int id) { return _characterMap.TryGetValue(id, out var item) ? item : null; }
		public CombatRulesSerializable GetCombatRules(int id) { return _combatRulesMap.TryGetValue(id, out var item) ? item : null; }
		public FleetSerializable GetFleet(int id) { return _fleetMap.TryGetValue(id, out var item) ? item : null; }
		public LootSerializable GetLoot(int id) { return _lootMap.TryGetValue(id, out var item) ? item : null; }
		public QuestSerializable GetQuest(int id) { return _questMap.TryGetValue(id, out var item) ? item : null; }
		public QuestItemSerializable GetQuestItem(int id) { return _questItemMap.TryGetValue(id, out var item) ? item : null; }
		public AmmunitionSerializable GetAmmunition(int id) { return _ammunitionMap.TryGetValue(id, out var item) ? item : null; }
		public BulletPrefabSerializable GetBulletPrefab(int id) { return _bulletPrefabMap.TryGetValue(id, out var item) ? item : null; }
		public VisualEffectSerializable GetVisualEffect(int id) { return _visualEffectMap.TryGetValue(id, out var item) ? item : null; }
		public WeaponSerializable GetWeapon(int id) { return _weaponMap.TryGetValue(id, out var item) ? item : null; }

        public IEnumerable<KeyValuePair<string, IImageData>> Images => _images;
        public IEnumerable<KeyValuePair<string, IAudioClipData>> AudioClips => _audioClips;
        public IEnumerable<KeyValuePair<string, string>> Localizations => _localizations;

        private bool _allowDuplicates = false;
        private readonly IJsonSerializer _serializer;

		private readonly Dictionary<int, AmmunitionObsoleteSerializable> _ammunitionObsoleteMap = new();
		private readonly Dictionary<int, ComponentSerializable> _componentMap = new();
		private readonly Dictionary<int, ComponentModSerializable> _componentModMap = new();
		private readonly Dictionary<int, ComponentStatsSerializable> _componentStatsMap = new();
		private readonly Dictionary<int, DeviceSerializable> _deviceMap = new();
		private readonly Dictionary<int, DroneBaySerializable> _droneBayMap = new();
		private readonly Dictionary<int, FactionSerializable> _factionMap = new();
		private readonly Dictionary<int, GameObjectPrefabSerializable> _gameObjectPrefabMap = new();
		private readonly Dictionary<int, SatelliteSerializable> _satelliteMap = new();
		private readonly Dictionary<int, SatelliteBuildSerializable> _satelliteBuildMap = new();
		private readonly Dictionary<int, ShipSerializable> _shipMap = new();
		private readonly Dictionary<int, ShipBuildSerializable> _shipBuildMap = new();
		private readonly Dictionary<int, SkillSerializable> _skillMap = new();
		private readonly Dictionary<int, TechnologySerializable> _technologyMap = new();
		private readonly Dictionary<int, BehaviorTreeSerializable> _behaviorTreeMap = new();
		private readonly Dictionary<int, CharacterSerializable> _characterMap = new();
		private readonly Dictionary<int, CombatRulesSerializable> _combatRulesMap = new();
		private readonly Dictionary<int, FleetSerializable> _fleetMap = new();
		private readonly Dictionary<int, LootSerializable> _lootMap = new();
		private readonly Dictionary<int, QuestSerializable> _questMap = new();
		private readonly Dictionary<int, QuestItemSerializable> _questItemMap = new();
		private readonly Dictionary<int, AmmunitionSerializable> _ammunitionMap = new();
		private readonly Dictionary<int, BulletPrefabSerializable> _bulletPrefabMap = new();
		private readonly Dictionary<int, VisualEffectSerializable> _visualEffectMap = new();
		private readonly Dictionary<int, WeaponSerializable> _weaponMap = new();

        private readonly Dictionary<string, IImageData> _images = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IAudioClipData> _audioClips = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _localizations = new(StringComparer.OrdinalIgnoreCase);
	}
}

