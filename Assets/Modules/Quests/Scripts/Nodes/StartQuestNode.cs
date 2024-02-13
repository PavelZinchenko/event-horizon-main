using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Domain.Quests
{
    public class StartQuestNode : ActionNode
    {
        public StartQuestNode(int id, QuestModel quest, int seed)
            : base(id, NodeType.StartQuest)
        {
            _quest = quest;
            _seed = seed;
        }

        protected override void InvokeAction(IQuestActionProcessor processor) { processor.StartQuest(_quest, _seed); }

        private readonly int _seed;
        private readonly QuestModel _quest;
    }
}
