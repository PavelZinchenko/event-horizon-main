﻿using System;
using System.Collections.Generic;
using Zenject;
using Session.ContentObsolete;

namespace Session
{
    public class SessionDataObsolete : Services.Storage.ISerializableGameData, ISessionDataObsolete, ITickable
    {
        [Inject]
        public SessionDataObsolete(ContentFactoryObsolete contentFactory)
        {
            _contentFactory = contentFactory;
        }

        public long GameId { get; private set;  }
        public long TimePlayed { get { return (long) _timePlayed; } private set { _timePlayed = value; } }
        public string ModId { get; private set; }

        public void Tick()
        {
            if (_content != null && _content.Game.GameStartTime > 0)
                _timePlayed += UnityEngine.Time.deltaTime;
        }

        public long DataVersion
        {
            get
            {
                return _content.IsChanged ? _version + 1 : _version;
            }
            private set
            {
                _version = value;
            }
        }

        public IEnumerable<byte> Serialize()
        {
            if (_content.IsChanged)
                _version++;

            return _content.Serialize();
        }

        public bool TryDeserialize(long gameId, long timePlayed, long dataVersion, string modId, byte[] data, int startIndex)
        {
            try
            {
                UnityEngine.Debug.Log("SessionData.TryDeserialize");

                var content = DatabaseContentObsolete.TryDeserialize(data, startIndex, _contentFactory);
                if (content == null)
                    return false;

                UnityEngine.Debug.Log("SessionData.TryDeserialize - success " + content.Game.GameStartTime + "/" + dataVersion);

                GameId = gameId;
                TimePlayed = timePlayed;
                DataVersion = dataVersion;
                ModId = modId;
                _content = content;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
                return false;
            }

            try
            {
                //_dataLoadedTrigger.Fire();
                //_sessionCreatedTrigger.Fire();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            return true;
        }

        public void CreateNewGame(string modId, bool keepPurchases = true)
        {
            UnityEngine.Debug.Log("SessionData.CreateNewGame");

            if (_content != null && keepPurchases)
                _content = new DatabaseContentObsolete(_contentFactory, _content.Purchases, _content.Achievements);
            else
                _content = new DatabaseContentObsolete(_contentFactory);

            GameId = System.DateTime.UtcNow.Ticks;
            TimePlayed = 0;
            DataVersion = 0;
            ModId = modId;

            //_dataLoadedTrigger.Fire();
            //_sessionCreatedTrigger.Fire();
        }

        public GameData Game => _content?.Game;
        public StarMapData StarMap => _content?.Starmap;
        public InventoryData Inventory => _content?.Inventory;
        public FleetData Fleet => _content?.Fleet;
        public ShopData Shop => _content?.Shop;
        public EventData Events => _content?.Events;
        public BossData Bosses => _content?.Bosses;
        public RegionData Regions => _content?.Regions;
        public InAppPurchasesData Purchases => _content?.Purchases;
        public WormholeData Wormholes => _content?.Wormholes;
        public AchievementData Achievements => _content?.Achievements;
        public CommonObjectData CommonObjects => _content?.CommonObjects;
        public ResearchData Research => _content?.Research;
        public StatisticsData Statistics => _content?.Statistics;
        public ResourcesData Resources => _content?.Resources;
        public UpgradesData Upgrades => _content?.Upgrades;
        public PvpData Pvp => _content?.Pvp;
        public SocialData Social => _content?.Social;
        public QuestData Quests => _content?.Quests;

        private long _version;
        private double _timePlayed;

        private DatabaseContentObsolete _content;
        private readonly ContentFactoryObsolete _contentFactory;
    }

    //public class SessionDataStub : ISessionDataObsolete
    //{
    //    public long GameId { get; private set; }
    //    public long TimePlayed { get; private set; }
    //    public long DataVersion { get; private set; }
    //    public string ModId { get; private set; }

    //    public IEnumerable<byte> Serialize()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool TryDeserialize(long gameId, long timePlayed, long dataVersion, string modId, byte[] data, int startIndex)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void CreateNewGame(string modId, bool keepPurchases = true)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public bool IsGameStarted { get; private set; }
    //    public GameData Game { get; private set; }
    //    public StarMapData StarMap { get; private set; }
    //    public InventoryData Inventory { get; private set; }
    //    public FleetData Fleet { get; private set; }
    //    public ShopData Shop { get; private set; }
    //    public EventData Events { get; private set; }
    //    public BossData Bosses { get; private set; }
    //    public RegionData Regions { get; private set; }
    //    public InAppPurchasesData Purchases { get; private set; }
    //    public PvpData Pvp { get; private set; }
    //    public WormholeData Wormholes { get; private set; }
    //    public AchievementData Achievements { get; private set; }
    //    public CommonObjectData CommonObjects { get; private set; }
    //    public ResearchData Research { get; private set; }
    //    public StatisticsData Statistics { get; private set; }
    //    public ResourcesData Resources { get; private set; }
    //    public UpgradesData Upgrades { get; private set; }
    //    public SocialData Social { get; private set; }
    //    public QuestData Quests { get; private set; }
    //}
}
