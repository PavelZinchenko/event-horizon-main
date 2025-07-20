using System;
using GameDatabase.DataModel;

namespace Domain.Quests
{
    public class QuestBuilder
    {
        public QuestBuilder(
            QuestModel model, 
            int starId, 
            int seed, 
            IQuestBuilderContext context)
        {
            _model = model;
            _starId = starId;
            _seed = seed;
            _context = context;
            _lootCache = new LootCache(context.LootItemFactory, context.Database);
            _requirementCache = new RequirementCache(_context, _lootCache);
            _lootCache.SetRequirementCache(_requirementCache);
            _enemyCache = new EnemyCache(context.Database);
            _parameters = new QuestInfo(_model, _context.StarMapDataProvider.GetStarData(_starId), seed);
        }

        public Quest Build(int activeNodeId = 0)
        {
            var nodeBuilder = new NodeBuilder(_model, _parameters, _enemyCache, _requirementCache, _lootCache, _context.StarMapDataProvider);
            var nodes = nodeBuilder.Build();

            if (nodes.Count == 0)
            {
                UnityEngine.Debug.LogException(new ArgumentException("QuestBuilder: quest has no nodes - " + _model.Id));
                return null;
            }

            INode activeNode;
            if (!nodes.TryGetValue(activeNodeId, out activeNode))
                activeNode = nodes.MinValue(item => (int)item.Key).Value;

            var quest = new Quest(_model, _starId, _seed);
            quest.Initialize(activeNode);
            return quest;
        }

        private readonly QuestModel _model;
        private readonly QuestInfo _parameters;
        private readonly int _starId;
        private readonly int _seed;
        private readonly IQuestBuilderContext _context;
        private readonly LootCache _lootCache;
        private readonly EnemyCache _enemyCache;
        private readonly RequirementCache _requirementCache;
    }
}
