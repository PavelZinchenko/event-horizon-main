using System.Collections.Generic;
using GameDatabase.Enums;
using Services.Localization;

namespace Domain.Quests
{
    public class BattleNode : INode
    {
        public BattleNode(int id, QuestEnemyData enemyData, ILoot specialLoot)
        {
            _id = id;
            _specialLoot = specialLoot;
            _enemyData = enemyData;
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
            processor.StartCombat(_enemyData, _specialLoot);
            return true;
        }

        private bool _started;
        private readonly int _id;
        private readonly QuestEnemyData _enemyData;
        private readonly ILoot _specialLoot;
    }
}
