using UnityEngine;
using Zenject;

namespace Services.GameApplication
{
    public class StandaloneApplication : MonoBehaviour, IApplication
    {
        [Inject] private readonly GamePausedSignal.Trigger _gamePausedTrigger;
        [Inject] private readonly AppActivatedSignal.Trigger _appActivateTrigger;
        [Inject] private readonly Settings.IGameSettings _settings;

        private bool _applicationPaused;
        private bool _applicationFocused = true;
        private readonly GamePauseCounter _pauseCounter = new();

        public bool Paused => _pauseCounter.Paused || !IsActive;
        public bool IsActive => !_applicationPaused && (_applicationFocused || _settings.RunInBackground);

        private void Start()
        {
            UpdateStatus();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (_applicationFocused == focused)
                return;

            _applicationFocused = focused;
            _appActivateTrigger.Fire(IsActive);

            UpdateStatus();
        }

        private void OnApplicationPause(bool paused)
        {
            if (_applicationPaused == paused)
                return;

            _applicationPaused = paused;
            _appActivateTrigger.Fire(IsActive);

            UpdateStatus();
        }

        public void Pause(object sender)
        {
            if (_pauseCounter.TryPause(sender))
                UpdateStatus();
        }

        public void Resume(object sender)
        {
            if (_pauseCounter.TryResume(sender))
                UpdateStatus();
        }

        private void UpdateStatus()
        {
            Time.timeScale = Paused ? 0.0f : 1.0f;
            Timer.Paused = Paused;

            _gamePausedTrigger.Fire(Paused);
        }
    }
}
