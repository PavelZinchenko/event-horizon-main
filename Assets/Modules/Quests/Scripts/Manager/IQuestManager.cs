using System.Collections.Generic;
using Domain.Quests;
using GameDatabase.DataModel;

namespace GameServices.Quests
{
    public interface IQuestManager
    {
        bool ActionRequired { get; }
        void InvokeAction(IQuestActionProcessor processor);
        IEnumerable<IQuest> Quests { get; }
        bool IsQuestObjective(int starId);
        void AbandonQuest(IQuest quest);
        void StartQuest(QuestModel questModel);
    }
}
