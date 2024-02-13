using System;
using GameDatabase;
using GameServices;
using Domain.Quests;
using Services.InternetTime;
using Session;
using CommonComponents;

namespace Game
{
    public class HolidayManager : GameServiceBase
    {
        public HolidayManager(
            SessionDataLoadedSignal dataLoadedSignal, 
            SessionCreatedSignal sessionCreatedSignal,
            InternetTimeService timeService, 
            IQuestManager questManager, 
            IDatabase database) 
            : base(dataLoadedSignal, sessionCreatedSignal)
        {
            _timeService = timeService;
            _questManager = questManager;
            _database = database;
        }

        public bool IsChristmas
        {
            get
            {
                var settings = _database.SpecialEventSettings;
                if (!settings.EnableXmasEvent) return false;

                var date = _timeService.DateTime;
                var xmasDate = new DateTime(date.Month < 6 ? date.Year - 1 : date.Year, 12, 25);

                return IsValidDate(xmasDate, date, settings.XmasDaysBefore, settings.XmasDaysAfter);
            }
        }

        public bool IsHalloween
        {
            get
            {
                var settings = _database.SpecialEventSettings;
                if (!settings.EnableHalloweenEvent) return false;

                var date = _timeService.DateTime;
                var halloweenDate = new DateTime(date.Year, 10, 31);

                return IsValidDate(halloweenDate, date, settings.HalloweenDaysBefore, settings.HalloweenDaysAfter);
            }
        }

        public bool IsEaster
        {
            get
            {
                var settings = _database.SpecialEventSettings;
                if (!settings.EnableEasterEvent) return false;
                var date = _timeService.DateTime;

                var y = date.Year;
                var d = 225 - 11 * (y % 19);
                while (d > 50) d -= 30;
                if (d > 48) d--;
                var e = (y + (y / 4) + d + 1) % 7;
                var q = d + 7 - e;

                var easterDate = q < 32 ? new System.DateTime(y, 3, q) : new System.DateTime(y, 4, q - 31);

                return IsValidDate(easterDate, date, settings.EasterDaysBefore, settings.EasterDaysAfter);
            }
        }

        protected override void OnSessionDataLoaded()
        {
        }

        protected override void OnSessionCreated()
        {
            var random = RandomState.FromTickCount();
            if (IsEaster) _questManager.StartQuest(_database.SpecialEventSettings.EasterQuest, random.Next());
            if (IsChristmas) _questManager.StartQuest(_database.SpecialEventSettings.XmasQuest, random.Next());
            if (IsHalloween) _questManager.StartQuest(_database.SpecialEventSettings.HalloweenQuest, random.Next());
        }

        private static bool IsValidDate(DateTime eventDate, DateTime now, int daysBefore, int daysAfter)
        {
            if (eventDate > now)
            {
                //UnityEngine.Debug.LogWarning("Days to event: " + (eventDate - now).Days);
                return (eventDate - now).Days <= daysBefore;
            }
            else
            {
                //UnityEngine.Debug.LogWarning("Days since event: " + (now - eventDate).Days);
                return (now - eventDate).Days <= daysAfter;
            }
        }

        private readonly InternetTimeService _timeService;
        private readonly IQuestManager _questManager;
        private readonly IDatabase _database;
    }
}
