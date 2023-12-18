using System.Collections.Generic;
using GameDatabase.Enums;
using Services.Localization;

namespace Domain.Quests
{
    public class AttackEnemyNode : INode
    {
		public enum EnemyType
		{
			Occupants,
			StarbaseDefenders,
		}

        public AttackEnemyNode(int id, int starId, EnemyType enemyType)
        {
            _id = id;
            _starId = starId;
			_enemyType = enemyType;
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

			switch (_enemyType)
			{
				case EnemyType.Occupants: processor.AttackOccupants(_starId); break;
				case EnemyType.StarbaseDefenders: processor.AttackStarbase(_starId); break;
				default: throw new System.InvalidOperationException();
			}


			return true;
        }

        private bool _started;
		private readonly EnemyType _enemyType;
		private readonly int _id;
        private readonly int _starId;
    }
}
