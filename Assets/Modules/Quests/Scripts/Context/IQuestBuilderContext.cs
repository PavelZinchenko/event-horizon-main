using System.Collections.Generic;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;

namespace Domain.Quests
{
    public interface IQuestBuilderContext
    {
        IPlayerDataProvider PlayerDataProvider { get; }
        IInventoryDataProvider InventoryDataProvider { get; }
        IStarMapDataProvider StarMapDataProvider { get; }
        ICharacterDataProvider CharacterDataProvider { get; }
        IQuestDataProvider QuestDataProvider { get; }
        ITimeDataProvider TimeDataProvider { get; }
        ILootItemFactory LootItemFactory { get; }
        IDatabase Database { get; }
    }

    public interface IPlayerDataProvider
    {
        IStarDataProvider CurrentStar { get; }
    }

    public interface IInventoryDataProvider
    {
        int GetQuantity(ItemId<QuestItem> id);
    }

    public interface IStarMapDataProvider
    {
        IStarDataProvider GetStarData(int id);
        IStarDataProvider CurrentStar { get; }
        int RandomStarAtDistance(int centerStarId, int distance, System.Random random);
        IEnumerable<IRegionDataProvider> GetRegionsNearby(int centerStarId, int minDistance, int maxDistance);
    }

    public interface ICharacterDataProvider
    {
        int GetCharacterAttitude(ItemId<Character> id);
    }

    public interface IRegionDataProvider
    {
        int HomeStarId { get; }
        Faction Faction { get; }
        int Relations { get; }
        bool IsCaptured { get; }
        bool IsVisited { get; }
        bool IsHome { get; }
    }

    public interface IStarDataProvider
    {
        int Id { get; }
        int Level { get; }
        bool IsSecured { get; }
        IRegionDataProvider Region { get; }
    }

    public interface ITimeDataProvider
    {
        long TotalPlayTime { get; }
    }

    public interface IQuestDataProvider
    {
        bool IsActive(int id);
        bool IsActiveOrCompleted(int id);
        bool IsActive(int id, int starId);
        bool HasBeenCompleted(int id);
        long LastStartTime(int id);
        long LastCompletionTime(int id);
        long QuestStartTime(int id, int starId);
        int GameSeed { get; }
    }
}