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
        IGameDataProvider GameDataProvider { get; }
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
        int StarbasePower { get; }
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

    public interface IGameDataProvider
    {
        bool IsGameStarted { get; }
        long TotalPlayTime { get; }
        int GameSeed { get; }
        int Counter { get; }
    }
}