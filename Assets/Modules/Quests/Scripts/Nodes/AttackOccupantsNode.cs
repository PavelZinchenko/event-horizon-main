using System.Collections.Generic;
using GameDatabase.Enums;
using Services.Localization;

namespace Domain.Quests
{
    public class AttackOccupantsNode : INode
    {
        public AttackOccupantsNode(int id, int starId)
        {
            _id = id;
            _starId = starId;
        }

        public int Id { get { return _id; } }
        public NodeType Type { get { return NodeType.AttackFleet; } }

        public string GetRequirementsText(ILocalization localization)
        {
#if UNITY_EDITOR
            return GetType().Name + " - " + _id;
#else
            return Type.ToString();
#endif
        }

        public bool TryGetBeacons(ICollection<int> beacons) { return false; }

        public void Initialize() { _started = false; }

        public INode VictoryNode { get; set; }
        public INode DefeatNode { get; set; }

        public bool TryProcessEvent(IQuestEventData eventData, out INode target)
        {
            target = this;
            if (eventData.Type != QuestEventType.CombatCompleted || !_started)
                return false;

            var data = (CombatEventData)eventData;

            target = data.IsVictory ? VictoryNode : DefeatNode;
            return true;
        }

        public bool TryProceed(out INode target)
        {
            target = this;
            return false;
        }

        public bool ActionRequired => !_started;
        public bool TryInvokeAction(IQuestActionProcessor processor)
        {
            _started = true;
            processor.AttackOccupants(_starId);
            return true;
        }

        private bool _started;
        private readonly int _id;
        private readonly int _starId;
    }
}
