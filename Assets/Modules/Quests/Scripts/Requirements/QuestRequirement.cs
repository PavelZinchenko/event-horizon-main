using System;
using GameDatabase.DataModel;
using Services.Localization;

namespace Domain.Quests
{
    public class QuestRequirement : IRequirements
    {
        public enum RequiredStatus
        {
            Active,
            Completed,
        }

        public QuestRequirement(QuestModel quest, RequiredStatus status, IQuestDataProvider questData)
        {
            _quest = quest;
            _status = status;
            _questData = questData;
        }

        public bool IsMet
        {
            get
            {
                switch (_status)
                {
                    case RequiredStatus.Active:
                        return _questData.IsActive(_quest.Id.Value);
                    case RequiredStatus.Completed:
                        return _questData.HasBeenCompleted(_quest.Id.Value);
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public bool CanStart(int starId, int seed) { return IsMet; }

        public string GetDescription(ILocalization localization)
        {
#if UNITY_EDITOR
            return "COMPLETE QUEST " + _quest.Id;
#else
            return string.Empty;
#endif
        }

        public int BeaconPosition { get { return -1; } }

        private readonly QuestModel _quest;
        private readonly RequiredStatus _status;
        private readonly IQuestDataProvider _questData;
    }
}
