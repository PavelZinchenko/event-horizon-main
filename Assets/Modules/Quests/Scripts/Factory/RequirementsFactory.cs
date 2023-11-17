using GameDatabase;
using GameDatabase.DataModel;

namespace Domain.Quests
{
    public class RequirementsFactory
    {
        public RequirementsFactory(IQuestBuilderContext context)
        {
            _context = context;
            _lootCache = new LootCache(context.LootItemFactory, context.Database);
        }

        // TODO: move to another place
        public QuestGiver CreateQuestGiver(QuestOrigin data)
        {
            return new QuestGiver(data, _context.StarMapDataProvider);
        }

        public IQuestRequirements CreateForQuest(Requirement requirement, int seed)
        {
            if (requirement == null)
                return EmptyRequirements.Instance;

            var builder = new RequirementsBuilder(new QuestInfo(seed), _lootCache, _context);
            return requirement.Create(builder);
        }

        public INodeRequirements CreateForNode(Requirement requirement, QuestInfo context)
        {
            if (requirement == null)
                return EmptyRequirements.Instance;

            var builder = new RequirementsBuilder(context, _lootCache, _context);
            return requirement.Create(builder);
        }

        private readonly ILootCache _lootCache;
        private readonly IQuestBuilderContext _context;

        private class LootCache : ILootCache
        {
            public LootCache(ILootItemFactory lootItemFactory, IDatabase database)
            {
                _lootItemFactory = lootItemFactory;
                _database = database;
            }

            public ILoot Get(LootModel model, QuestInfo context)
            {
                return new Loot(model, context, _lootItemFactory, _database);
            }

            private readonly IDatabase _database;
            private readonly ILootItemFactory _lootItemFactory;
        }
    }
}
