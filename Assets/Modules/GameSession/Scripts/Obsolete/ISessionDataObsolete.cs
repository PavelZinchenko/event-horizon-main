using Services.Storage;
using Session.ContentObsolete;

namespace Session
{
    public interface ISessionContentObsolete
    {
        GameData Game { get; }
        StarMapData StarMap { get; }
        InventoryData Inventory { get; }
        FleetData Fleet { get; }
        ShopData Shop { get; }
        EventData Events { get; }
        BossData Bosses { get; }
        RegionData Regions { get; }
        InAppPurchasesData Purchases { get; }
        PvpData Pvp { get; }
        WormholeData Wormholes { get; }
        AchievementData Achievements { get; }
        CommonObjectData CommonObjects { get; }
        ResearchData Research { get; }
        StatisticsData Statistics { get; }
        ResourcesData Resources { get; }
        UpgradesData Upgrades { get; }
        SocialData Social { get; }
        QuestData Quests { get; }
    }

    public interface ISessionDataObsolete : ISessionContentObsolete, IGameData {}
}
