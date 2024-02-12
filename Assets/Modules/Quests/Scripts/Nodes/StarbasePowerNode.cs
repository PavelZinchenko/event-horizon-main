using GameDatabase.Enums;

namespace Domain.Quests
{
    public class StarbasePowerNode : ActionNode
    {
        public StarbasePowerNode(int id, int starId, int value, bool additive)
            : base(id, additive ? NodeType.ChangeFactionStarbasePower : NodeType.SetFactionStarbasePower)
        {
            _starId = starId;
            _value = value;
            _additive = additive;
        }

        protected override void InvokeAction(IQuestActionProcessor processor)
        {
            processor.SetFactionStarbasePower(_starId, _value, _additive);
        }

        private readonly int _starId;
        private readonly int _value;
        private readonly bool _additive;
    }
}
