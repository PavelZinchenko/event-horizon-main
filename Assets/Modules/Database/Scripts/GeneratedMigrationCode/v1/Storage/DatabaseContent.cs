//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using GameDatabase.Storage;
using GameDatabase.Model;
using DatabaseMigration.v1.Enums;
using DatabaseMigration.v1.Serializable;

namespace DatabaseMigration.v1.Storage
{
    public class DatabaseContent : IContentLoader
    {
        public DatabaseContent(IJsonSerializer jsonSerializer, IDataStorage storage)
        {
            _serializer = jsonSerializer;
            storage?.LoadContent(this);
        }

        public int VersionMajor 
        {
            get => DatabaseSettings != null ? DatabaseSettings.DatabaseVersion : 1;
            set => (DatabaseSettings ?? (DatabaseSettings = new DatabaseSettingsSerializable())).DatabaseVersion = value;
        }

        public int VersionMinor
        {
            get => DatabaseSettings != null ? DatabaseSettings.DatabaseVersionMinor : 0;
            set => (DatabaseSettings ?? (DatabaseSettings = new DatabaseSettingsSerializable())).DatabaseVersionMinor = value;
        }

        public void LoadJson(string name, string content)
        {
            var item = _serializer.FromJson<SerializableItem>(content);
            var type = item.ItemType;

            if (type == ItemType.AmmunitionObsolete)
            {
                var data = _serializer.FromJson<AmmunitionObsoleteSerializable>(content);
                data.FileName = name;
                AmmunitionObsoleteList.Add(data);
            }
            else if (type == ItemType.Component)
            {
                var data = _serializer.FromJson<ComponentSerializable>(content);
                data.FileName = name;
                ComponentList.Add(data);
            }
            else if (type == ItemType.ComponentMod)
            {
                var data = _serializer.FromJson<ComponentModSerializable>(content);
                data.FileName = name;
                ComponentModList.Add(data);
            }
            else if (type == ItemType.ComponentStats)
            {
                var data = _serializer.FromJson<ComponentStatsSerializable>(content);
                data.FileName = name;
                ComponentStatsList.Add(data);
            }
            else if (type == ItemType.Device)
            {
                var data = _serializer.FromJson<DeviceSerializable>(content);
                data.FileName = name;
                DeviceList.Add(data);
            }
            else if (type == ItemType.DroneBay)
            {
                var data = _serializer.FromJson<DroneBaySerializable>(content);
                data.FileName = name;
                DroneBayList.Add(data);
            }
            else if (type == ItemType.Faction)
            {
                var data = _serializer.FromJson<FactionSerializable>(content);
                data.FileName = name;
                FactionList.Add(data);
            }
            else if (type == ItemType.Satellite)
            {
                var data = _serializer.FromJson<SatelliteSerializable>(content);
                data.FileName = name;
                SatelliteList.Add(data);
            }
            else if (type == ItemType.SatelliteBuild)
            {
                var data = _serializer.FromJson<SatelliteBuildSerializable>(content);
                data.FileName = name;
                SatelliteBuildList.Add(data);
            }
            else if (type == ItemType.Ship)
            {
                var data = _serializer.FromJson<ShipSerializable>(content);
                data.FileName = name;
                ShipList.Add(data);
            }
            else if (type == ItemType.ShipBuild)
            {
                var data = _serializer.FromJson<ShipBuildSerializable>(content);
                data.FileName = name;
                ShipBuildList.Add(data);
            }
            else if (type == ItemType.Skill)
            {
                var data = _serializer.FromJson<SkillSerializable>(content);
                data.FileName = name;
                SkillList.Add(data);
            }
            else if (type == ItemType.Technology)
            {
                var data = _serializer.FromJson<TechnologySerializable>(content);
                data.FileName = name;
                TechnologyList.Add(data);
            }
            else if (type == ItemType.Character)
            {
                var data = _serializer.FromJson<CharacterSerializable>(content);
                data.FileName = name;
                CharacterList.Add(data);
            }
            else if (type == ItemType.Fleet)
            {
                var data = _serializer.FromJson<FleetSerializable>(content);
                data.FileName = name;
                FleetList.Add(data);
            }
            else if (type == ItemType.Loot)
            {
                var data = _serializer.FromJson<LootSerializable>(content);
                data.FileName = name;
                LootList.Add(data);
            }
            else if (type == ItemType.Quest)
            {
                var data = _serializer.FromJson<QuestSerializable>(content);
                data.FileName = name;
                QuestList.Add(data);
            }
            else if (type == ItemType.QuestItem)
            {
                var data = _serializer.FromJson<QuestItemSerializable>(content);
                data.FileName = name;
                QuestItemList.Add(data);
            }
            else if (type == ItemType.Ammunition)
            {
                var data = _serializer.FromJson<AmmunitionSerializable>(content);
                data.FileName = name;
                AmmunitionList.Add(data);
            }
            else if (type == ItemType.BulletPrefab)
            {
                var data = _serializer.FromJson<BulletPrefabSerializable>(content);
                data.FileName = name;
                BulletPrefabList.Add(data);
            }
            else if (type == ItemType.VisualEffect)
            {
                var data = _serializer.FromJson<VisualEffectSerializable>(content);
                data.FileName = name;
                VisualEffectList.Add(data);
            }
            else if (type == ItemType.Weapon)
            {
                var data = _serializer.FromJson<WeaponSerializable>(content);
                data.FileName = name;
                WeaponList.Add(data);
            }
            else if (type == ItemType.DatabaseSettings)
            {
                var data = _serializer.FromJson<DatabaseSettingsSerializable>(content);
                data.FileName = name;

				if (DatabaseSettings != null)
                    throw new DatabaseException("Duplicate DatabaseSettings file found - " + name);
                DatabaseSettings = data;
            }
            else if (type == ItemType.DebugSettings)
            {
                var data = _serializer.FromJson<DebugSettingsSerializable>(content);
                data.FileName = name;

				if (DebugSettings != null)
                    throw new DatabaseException("Duplicate DebugSettings file found - " + name);
                DebugSettings = data;
            }
            else if (type == ItemType.ExplorationSettings)
            {
                var data = _serializer.FromJson<ExplorationSettingsSerializable>(content);
                data.FileName = name;

				if (ExplorationSettings != null)
                    throw new DatabaseException("Duplicate ExplorationSettings file found - " + name);
                ExplorationSettings = data;
            }
            else if (type == ItemType.FrontierSettings)
            {
                var data = _serializer.FromJson<FrontierSettingsSerializable>(content);
                data.FileName = name;

				if (FrontierSettings != null)
                    throw new DatabaseException("Duplicate FrontierSettings file found - " + name);
                FrontierSettings = data;
            }
            else if (type == ItemType.GalaxySettings)
            {
                var data = _serializer.FromJson<GalaxySettingsSerializable>(content);
                data.FileName = name;

				if (GalaxySettings != null)
                    throw new DatabaseException("Duplicate GalaxySettings file found - " + name);
                GalaxySettings = data;
            }
            else if (type == ItemType.ShipModSettings)
            {
                var data = _serializer.FromJson<ShipModSettingsSerializable>(content);
                data.FileName = name;

				if (ShipModSettings != null)
                    throw new DatabaseException("Duplicate ShipModSettings file found - " + name);
                ShipModSettings = data;
            }
            else if (type == ItemType.ShipSettings)
            {
                var data = _serializer.FromJson<ShipSettingsSerializable>(content);
                data.FileName = name;

				if (ShipSettings != null)
                    throw new DatabaseException("Duplicate ShipSettings file found - " + name);
                ShipSettings = data;
            }
            else if (type == ItemType.SkillSettings)
            {
                var data = _serializer.FromJson<SkillSettingsSerializable>(content);
                data.FileName = name;

				if (SkillSettings != null)
                    throw new DatabaseException("Duplicate SkillSettings file found - " + name);
                SkillSettings = data;
            }
            else if (type == ItemType.SpecialEventSettings)
            {
                var data = _serializer.FromJson<SpecialEventSettingsSerializable>(content);
                data.FileName = name;

				if (SpecialEventSettings != null)
                    throw new DatabaseException("Duplicate SpecialEventSettings file found - " + name);
                SpecialEventSettings = data;
            }
            else
            {
                throw new DatabaseException("Unknown file type - " + type + "(" + name + ")");
            }
        }

        public void Export(IContentLoader contentLoader)
        {
            foreach (var item in AmmunitionObsoleteList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in ComponentList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in ComponentModList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in ComponentStatsList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in DeviceList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in DroneBayList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in FactionList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in SatelliteList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in SatelliteBuildList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in ShipList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in ShipBuildList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in SkillList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in TechnologyList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in CharacterList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in FleetList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in LootList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in QuestList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in QuestItemList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in AmmunitionList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in BulletPrefabList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in VisualEffectList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            foreach (var item in WeaponList)
                contentLoader.LoadJson(item.FileName, _serializer.ToJson(item));
            if (DatabaseSettings != null)
                contentLoader.LoadJson(DatabaseSettings.FileName, _serializer.ToJson(DatabaseSettings));
            if (DebugSettings != null)
                contentLoader.LoadJson(DebugSettings.FileName, _serializer.ToJson(DebugSettings));
            if (ExplorationSettings != null)
                contentLoader.LoadJson(ExplorationSettings.FileName, _serializer.ToJson(ExplorationSettings));
            if (FrontierSettings != null)
                contentLoader.LoadJson(FrontierSettings.FileName, _serializer.ToJson(FrontierSettings));
            if (GalaxySettings != null)
                contentLoader.LoadJson(GalaxySettings.FileName, _serializer.ToJson(GalaxySettings));
            if (ShipModSettings != null)
                contentLoader.LoadJson(ShipModSettings.FileName, _serializer.ToJson(ShipModSettings));
            if (ShipSettings != null)
                contentLoader.LoadJson(ShipSettings.FileName, _serializer.ToJson(ShipSettings));
            if (SkillSettings != null)
                contentLoader.LoadJson(SkillSettings.FileName, _serializer.ToJson(SkillSettings));
            if (SpecialEventSettings != null)
                contentLoader.LoadJson(SpecialEventSettings.FileName, _serializer.ToJson(SpecialEventSettings));
            foreach (var item in _images)
                contentLoader.LoadImage(item.Key, item.Value);
            foreach (var item in _audioClips)
                contentLoader.LoadAudioClip(item.Key, item.Value);
            foreach (var item in _localizations)
                contentLoader.LoadLocalization(item.Key, item.Value);
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

		public DatabaseSettingsSerializable DatabaseSettings { get; private set; }
		public DebugSettingsSerializable DebugSettings { get; private set; }
		public ExplorationSettingsSerializable ExplorationSettings { get; private set; }
		public FrontierSettingsSerializable FrontierSettings { get; private set; }
		public GalaxySettingsSerializable GalaxySettings { get; private set; }
		public ShipModSettingsSerializable ShipModSettings { get; private set; }
		public ShipSettingsSerializable ShipSettings { get; private set; }
		public SkillSettingsSerializable SkillSettings { get; private set; }
		public SpecialEventSettingsSerializable SpecialEventSettings { get; private set; }

		public List<AmmunitionObsoleteSerializable> AmmunitionObsoleteList { get; } = new List<AmmunitionObsoleteSerializable>();
		public List<ComponentSerializable> ComponentList { get; } = new List<ComponentSerializable>();
		public List<ComponentModSerializable> ComponentModList { get; } = new List<ComponentModSerializable>();
		public List<ComponentStatsSerializable> ComponentStatsList { get; } = new List<ComponentStatsSerializable>();
		public List<DeviceSerializable> DeviceList { get; } = new List<DeviceSerializable>();
		public List<DroneBaySerializable> DroneBayList { get; } = new List<DroneBaySerializable>();
		public List<FactionSerializable> FactionList { get; } = new List<FactionSerializable>();
		public List<SatelliteSerializable> SatelliteList { get; } = new List<SatelliteSerializable>();
		public List<SatelliteBuildSerializable> SatelliteBuildList { get; } = new List<SatelliteBuildSerializable>();
		public List<ShipSerializable> ShipList { get; } = new List<ShipSerializable>();
		public List<ShipBuildSerializable> ShipBuildList { get; } = new List<ShipBuildSerializable>();
		public List<SkillSerializable> SkillList { get; } = new List<SkillSerializable>();
		public List<TechnologySerializable> TechnologyList { get; } = new List<TechnologySerializable>();
		public List<CharacterSerializable> CharacterList { get; } = new List<CharacterSerializable>();
		public List<FleetSerializable> FleetList { get; } = new List<FleetSerializable>();
		public List<LootSerializable> LootList { get; } = new List<LootSerializable>();
		public List<QuestSerializable> QuestList { get; } = new List<QuestSerializable>();
		public List<QuestItemSerializable> QuestItemList { get; } = new List<QuestItemSerializable>();
		public List<AmmunitionSerializable> AmmunitionList { get; } = new List<AmmunitionSerializable>();
		public List<BulletPrefabSerializable> BulletPrefabList { get; } = new List<BulletPrefabSerializable>();
		public List<VisualEffectSerializable> VisualEffectList { get; } = new List<VisualEffectSerializable>();
		public List<WeaponSerializable> WeaponList { get; } = new List<WeaponSerializable>();

        public IEnumerable<KeyValuePair<string, IImageData>> Images => _images;
        public IEnumerable<KeyValuePair<string, IAudioClipData>> AudioClips => _audioClips;
        public IEnumerable<KeyValuePair<string, string>> Localizations => _localizations;

        private readonly IJsonSerializer _serializer;

        private readonly Dictionary<string, IImageData> _images = new Dictionary<string, IImageData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IAudioClipData> _audioClips = new Dictionary<string, IAudioClipData>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _localizations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	}
}
