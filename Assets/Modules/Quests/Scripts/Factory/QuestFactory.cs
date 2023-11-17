using System;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;
using Zenject;

namespace Domain.Quests
{
    public class QuestFactory
    {
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly IQuestBuilderContext _questBuilderContext;

        public Quest Create(int questId, int starId, int activeNode, int seed)
        {
            var data = _database.GetQuest(ItemId<QuestModel>.Create(questId));
            if (data == null)
            {
                UnityEngine.Debug.LogException(new ArgumentException("QuestFactory: quest not found - " + questId));
                return null;
            }

            var builder = new QuestBuilder(data, starId, seed, _questBuilderContext);
            return builder.Build(activeNode);
        }

        public Quest Create(QuestModel data, int starId, int seed)
        {
            var builder = new QuestBuilder(data, starId, seed, _questBuilderContext);
            return builder.Build();
        }
    }
}
