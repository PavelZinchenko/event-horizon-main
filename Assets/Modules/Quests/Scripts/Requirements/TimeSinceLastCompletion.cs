using System;
using GameDatabase.Model;
using GameDatabase.DataModel;
using Services.Localization;

namespace Domain.Quests
{
    public class TimeSinceLastCompletion : IRequirements
    {
        public TimeSinceLastCompletion(int questId, IQuestDataProvider questData, ITimeDataProvider timeData, long totalTicks)
        {
            _questId = questId;
            _totalTicks = totalTicks;
            _questData = questData;
            _timeData = timeData;
        }

        public bool IsMet => ElapsedTime >= _totalTicks;

        public bool CanStart(int starId, int seed) { return IsMet; }

        public string GetDescription(ILocalization localization)
        {
            var seconds = (_totalTicks - ElapsedTime) / TimeSpan.TicksPerSecond;
            return TimeToString(localization, seconds);
        }

        public int BeaconPosition => -1;

        public static string TimeToString(ILocalization localization, long seconds)
        {
            if (seconds <= 0) return string.Empty;

            var text = localization.GetString("$TimeLeft");
            if (seconds > 24 * 60 * 60)
                text += localization.GetString("$TimeDays", seconds / 86400);
            else if (seconds > 60 * 60)
                text += localization.GetString("$TimeHours", seconds / 3600);
            else if (seconds > 60)
                text += localization.GetString("$TimeMinutes", seconds / 60);
            else
                text += localization.GetString("$TimeSeconds", seconds);

            return text;
        }

        private long ElapsedTime => _timeData.TotalPlayTime - _questData.LastCompletionTime(_questId);

        private readonly long _totalTicks;
        private readonly int _questId;
        private readonly IQuestDataProvider _questData;
        private readonly ITimeDataProvider _timeData;
    }
}