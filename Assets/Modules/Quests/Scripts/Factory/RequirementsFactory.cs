using GameDatabase;
using GameDatabase.DataModel;

namespace Domain.Quests
{
    public class RequirementsFactory
    {
        public RequirementsFactory(IQuestBuilderContext context)
        {
            _context = context;
            _lootCache = new LootCache(context.LootItemFactory, new RequirementCache(this), context.Database);
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

        private readonly LootCache _lootCache;
        private readonly IQuestBuilderContext _context;

        private class LootCache : ILootCache
        {
            public LootCache(ILootItemFactory lootItemFactory, IRequirementCache requirementCache, IDatabase database)
            {
                _lootItemFactory = lootItemFactory;
                _database = database;
                _requirementCache = requirementCache;
            }

            public ILoot Get(LootModel model, QuestInfo context)
            {
                return new Loot(model, context, _lootItemFactory, _requirementCache, _database);
            }

            private readonly IDatabase _database;
            private readonly ILootItemFactory _lootItemFactory;
            private readonly IRequirementCache _requirementCache;
        }
        
        private class RequirementCache : IRequirementCache
        {
            public RequirementCache(RequirementsFactory requirementsFactory)
            {
                _requirementsFactory = requirementsFactory;
            }

            public INodeRequirements Get(Requirement requirement, QuestInfo context)
            {
                return _requirementsFactory.CreateForNode(requirement, context);
            }
            
            private readonly RequirementsFactory _requirementsFactory;
        }
    }
}
