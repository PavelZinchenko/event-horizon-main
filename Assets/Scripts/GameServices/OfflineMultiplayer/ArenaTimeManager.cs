using System;
using Economy;
using Session;

namespace GameServices.Multiplayer
{
    public class ArenaTimeManager
    {
        public ArenaTimeManager(ISessionData session)
        {
            _session = session;
        }

        public long CurrentTime
        {
            get
            {
                var now = System.DateTime.UtcNow.Ticks;
                var lastFightTime = _session.Pvp.LastFightTime;

                if (lastFightTime == 0) return now;
                if (_lastFightDataReceivedTime == 0) return now;

                var deltaTime = System.Math.Max(0, now - _lastFightDataReceivedTime);

                return lastFightTime + _timeFromLastFight + deltaTime;
            }
        }

        public void OnLastFightTimeReceived(long timeFromLastFight)
        {
            var now = System.DateTime.UtcNow.Ticks;
            _timeFromLastFight = timeFromLastFight;
            _lastFightDataReceivedTime = now;

            var time = CurrentTime;
            _session.Pvp.LastFightTime = now - _timeFromLastFight;
            OnServerTimeReceived(time);
        }

        public void OnLastUpdateTimeReceived(long timeFromLastUpdate)
        {
            _timeFromLastUpdate = timeFromLastUpdate;
        }

        public bool ShouldUpdatePlayerData
        {
            get { return _timeFromLastUpdate > TimeSpan.TicksPerHour; }
        }

        public int AvailableBattles
        {
            get
            {
#if IAP_DISABLED
                return ArenaMaxBattles;
#endif

                var timerStartTime = _session.Pvp.TimerStartTime;
                if (timerStartTime == 0) return ArenaMaxBattles;

                var recovered = (int)((CurrentTime - timerStartTime) / ArenaRecoveryTime);
                return ArenaMaxBattles - System.Math.Max(0, _session.Pvp.FightsFromTimerStart - recovered);
            }
        }

        private void OnServerTimeReceived(long time)
        {
            var timerStartTime = _session.Pvp.TimerStartTime;
            if (timerStartTime == 0) return;

            var recovered = (int)((time - timerStartTime) / ArenaRecoveryTime);
            if (recovered >= _session.Pvp.FightsFromTimerStart)
            {
                _session.Pvp.FightsFromTimerStart = 0;
                _session.Pvp.TimerStartTime = 0;
            }
        }

        public long TimeToNextBattle
        {
            get
            {
                var timerStartTime = _session.Pvp.TimerStartTime;
                if (timerStartTime == 0) return 0;

                var elapsedTime = CurrentTime - timerStartTime;
                if (elapsedTime < 0) return ArenaRecoveryTime;

                var recovered = (int)(elapsedTime / ArenaRecoveryTime);
                if (recovered >= _session.Pvp.FightsFromTimerStart) return 0;

                return ArenaRecoveryTime - elapsedTime % ArenaRecoveryTime;
            }
        }

        public void DecreaseAvailableBattles()
        {
            if (_session.Pvp.TimerStartTime == 0)
                _session.Pvp.TimerStartTime = CurrentTime;

            _session.Pvp.FightsFromTimerStart++;
        }

        public void IncreaseAvailableBattles(int count)
        {
            var fightsFromTimerStart = _session.Pvp.FightsFromTimerStart;
            if (count <= 0 || count >= fightsFromTimerStart)
            {
                _session.Pvp.FightsFromTimerStart = 0;
                _session.Pvp.TimerStartTime = 0;
            }
            else
            {
                _session.Pvp.FightsFromTimerStart -= count;
            }
        }

        public void Reset()
        {
            _timeFromLastFight = TimeSpan.TicksPerDay;
            _timeFromLastUpdate = TimeSpan.TicksPerDay;
            _lastFightDataReceivedTime = 0;
        }

        private long _timeFromLastFight;
        private long _timeFromLastUpdate;
        private long _lastFightDataReceivedTime;
        private readonly ISessionData _session;
        
        public const long ArenaRecoveryTime = 10*System.TimeSpan.TicksPerMinute;
        public const int ArenaMaxBattles = 10;
    }
}
