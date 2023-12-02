using CommonComponents.Utils;

namespace Services.GameApplication
{
    public interface IApplication
    {
        bool IsActive { get; }
        bool Paused { get; }

        void Pause();
        void Resume();
    }

    public class GamePausedSignal : SmartWeakSignal<bool>
    {
        public class Trigger : TriggerBase { }
    }

    public class AppActivatedSignal : SmartWeakSignal<bool>
    {
        public class Trigger : TriggerBase { }
    }

    public class GamePauseCounter
    {
        public bool Paused => _gamePauseCounter > 0;

        public bool TryPause()
        {
            _gamePauseCounter++;
            return _gamePauseCounter == 1;
        }

        public bool TryResume()
        {
            if (_gamePauseCounter == 0) return false;
            return --_gamePauseCounter == 0;
        }

        public void Reset()
        {
            _gamePauseCounter = 0;
        }

        private int _gamePauseCounter = 0;
    }
}
