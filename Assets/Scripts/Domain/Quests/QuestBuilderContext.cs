using GameDatabase;

namespace Domain.Quests
{
    public class QuestBuilderContext : IQuestBuilderContext
    {
        public QuestBuilderContext(
            IPlayerDataProvider playerDataProvider,
            IInventoryDataProvider inventoryDataProvider,
            IStarMapDataProvider starMapDataProvider,
            ICharacterDataProvider characterDataProvider,
            IQuestDataProvider questDataProvider,
            IGameDataProvider timeDataProvider,
            ILootItemFactory lootItemFactory,
            IDatabase database)
        {
            PlayerDataProvider = playerDataProvider;
            InventoryDataProvider = inventoryDataProvider;
            StarMapDataProvider = starMapDataProvider;
            CharacterDataProvider = characterDataProvider;
            QuestDataProvider = questDataProvider;
            GameDataProvider = timeDataProvider;
            LootItemFactory = lootItemFactory;
            Database = database;
        }

        public IPlayerDataProvider PlayerDataProvider { get; }
        public IInventoryDataProvider InventoryDataProvider { get; }
        public IStarMapDataProvider StarMapDataProvider { get; }
        public ICharacterDataProvider CharacterDataProvider { get; }
        public IQuestDataProvider QuestDataProvider { get; }
        public IGameDataProvider GameDataProvider { get; }
        public ILootItemFactory LootItemFactory { get; }
        public IDatabase Database { get; }
    }
}
