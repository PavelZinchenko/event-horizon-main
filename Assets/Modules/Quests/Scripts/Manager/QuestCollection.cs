using System.Collections.Generic;
using System.Linq;
using GameDatabase.DataModel;
using GameModel;

namespace Domain.Quests
{
    internal class QuestCollection
    {
        public void Assign(IEnumerable<QuestModel> quests, RequirementsFactory factory, int seed)
        {
            _quests.Clear();
            _quests.AddRange(quests.Select(item => new QuestInfo
            {
				QuestGiver = factory.CreateQuestGiver(item.Origin),
                Requirements = factory.CreateForQuest(item.Requirement, seed + item.Id.Value), 
                Quest = item
            }));
        }

        public void Clear()
        {
            _quests.Clear();
        }

        public Quest CreateFirstAvailable(QuestFactory questFactory)
        {
            if (_allowedQuests.Count == 0) return null;
            var item = _allowedQuests[0];

            return questFactory.Create(item.Quest, item.StarId, item.Seed);
        }

        public Quest CreateRandomWeighted(QuestFactory questFactory, int seed)
        {
			var random = new System.Random(seed);
            var value = random.NextFloat() * (_totalWeight < 1f ? 1f : _totalWeight);

            foreach (var item in _allowedQuests)
            {
                value -= item.Quest.Weight;
                if (value > 0.0001f) continue;
                return questFactory.Create(item.Quest, item.StarId, item.Seed);
            }

            return null;
        }

        public void UpdateQuests(int currentStarId, int seed, long currentTime, IQuestManagerContext context)
        {
            _allowedQuests.Clear();
            _totalWeight = 0f;
            var maxLevel = StarLayout.GetStarLevel(currentStarId, context.GameDataProvider.GameSeed);
            var random = new System.Random(seed);

            foreach (var item in _quests)
            {
                var quest = item.Quest;
                //if (quest.Weight <= 0) continue;
                if (quest.Level > maxLevel) continue;
                if (quest.IsOnCooldown(context.QuestDataStorage, currentTime, random)) continue;

                var starId = item.QuestGiver.GetStartSystem(currentStarId, seed);
                if (starId < 0) continue;

                if (!quest.CanBeStarted(context.QuestDataStorage, starId)) continue;
                if (!item.Requirements.CanStart(starId, seed)) continue;

                _allowedQuests.Add(new AllowedQuest { Quest = quest, StarId = starId, Seed = seed });
                _totalWeight += quest.Weight;
            }
        }

        private float _totalWeight;
        private readonly List<QuestInfo> _quests = new();
        private readonly List<AllowedQuest> _allowedQuests = new();

		private struct AllowedQuest
        {
            public QuestModel Quest;
            public int StarId;
            public int Seed;
        }

		private struct QuestInfo
        {
            public QuestGiver QuestGiver;
            public IQuestRequirements Requirements;
            public QuestModel Quest;
        }
    }
}
