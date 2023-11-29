using System;
using GameDatabase;
using GameDatabase.DataModel;
using Zenject;

namespace Domain.Quests
{
    public class QuestFactory
    {
        [Inject] private readonly IDatabase _database;
        [Inject] private readonly IQuestBuilderContext _questBuilderContext;

        public Quest Create(QuestProgress progress)
        {
            var data = _database.GetQuest(progress.QuestId);
            if (data == null)
            {
                UnityEngine.Debug.LogException(new ArgumentException("QuestFactory: quest not found - " + progress.QuestId));
                return null;
            }

            var builder = new QuestBuilder(data, progress.StarId, progress.Seed, _questBuilderContext);
            return builder.Build(progress.ActiveNode);
        }

        public Quest Create(QuestModel data, int starId, int seed)
        {
            var builder = new QuestBuilder(data, starId, seed, _questBuilderContext);
            return builder.Build();
        }
    }
}
