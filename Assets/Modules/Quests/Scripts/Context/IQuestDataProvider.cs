using System.Collections.Generic;
using GameDatabase.DataModel;
using GameDatabase.Model;

namespace Domain.Quests
{
    public interface IQuestDataProvider
    {
        bool IsActive(int id);
        bool IsActiveOrCompleted(int id);
        bool IsActive(int id, int starId);
        bool HasBeenCompleted(int id);
        long LastStartTime(int id);
        long LastCompletionTime(int id);
        long QuestStartTime(int id, int starId);
        int TotalQuestCount();
        IEnumerable<QuestProgress> GetActiveQuests();
    }

    public interface IQuestDataStorage : IQuestDataProvider
    {
        void SetQuestProgress(QuestProgress data);
        void SetQuestCompleted(int questId, int starId);
        void SetQuestFailed(int questId, int starId);
        void SetQuestCancelled(int questId, int starId);
    }

    public readonly struct QuestProgress
    {
        public QuestProgress(int questId, int starId, int node, int seed)
        {
            QuestId = ItemId<QuestModel>.Create(questId);
            StarId = starId;
            ActiveNode = node;
            Seed = seed;
        }

        public readonly ItemId<QuestModel> QuestId;
        public readonly int StarId;
        public readonly int ActiveNode;
        public readonly int Seed;
    }
}
