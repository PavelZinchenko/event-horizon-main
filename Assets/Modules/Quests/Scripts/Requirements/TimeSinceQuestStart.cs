using System;
using Services.Localization;

namespace Domain.Quests
{
    public class TimeSinceQuestStart : IRequirements
    {
        public TimeSinceQuestStart(int questId, int starId, IQuestDataProvider questData, IGameDataProvider timeData, long totalTicks)
        {
            _questId = questId;
            _starId = starId;
            _totalTicks = totalTicks;
            _questData = questData;
            _timeData = timeData;
        }

        public bool IsMet => ElapsedTime >= _totalTicks;

        public bool CanStart(int starId, int seed) { return IsMet; }

        public string GetDescription(ILocalization localization)
        {
            var seconds = (_totalTicks - ElapsedTime) / TimeSpan.TicksPerSecond;
            return TimeSinceLastCompletion.TimeToString(localization, seconds);
        }

        public int BeaconPosition => -1;

        private long ElapsedTime => _timeData.TotalPlayTime - _questData.QuestStartTime(_questId, _starId);

        private readonly long _totalTicks;
        private readonly int _starId;
        private readonly int _questId;
        private readonly IQuestDataProvider _questData;
        private readonly IGameDataProvider _timeData;
    }
}