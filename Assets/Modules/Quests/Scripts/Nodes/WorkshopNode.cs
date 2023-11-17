using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Domain.Quests
{
    public class WorkshopNode : ActionNode
    {
        public WorkshopNode(int id, Faction faction, int level)
            : base(id, NodeType.OpenWorkshop)
        {
            _level = level;
            _faction = faction;
        }

        protected override void InvokeAction(IQuestActionProcessor processor)
        {
            processor.OpenWorkshop(_faction, _level);
        }

        private readonly int _level;
        private readonly Faction _faction;
    }
}
