#if UNITY_WEBGL

using Agava.WebUtility;
using UnityEngine;
using Zenject;

namespace Services.GameApplication
{
    public class WebGlApplication : MonoBehaviour, IApplication
    {
        [Inject] private readonly GamePausedSignal.Trigger _gamePausedTrigger;
        [Inject] private readonly AppActivatedSignal.Trigger _appActivateTrigger;

        private bool _applicationPaused;
        private bool _applicationInBackground;
        private bool _applicationFocused = true;
        private readonly GamePauseCounter _pauseCounter = new();

        public bool Paused => _pauseCounter.Paused || !IsActive;
        public bool IsActive => !_applicationPaused && !_applicationInBackground && _applicationFocused;

        private void OnEnable()
        {
            WebApplication.InBackgroundChangeEvent += OnApplicationInBackground;
        }

        private void OnDisable()
        {
            WebApplication.InBackgroundChangeEvent -= OnApplicationInBackground;
        }

        private void Start()
        {
#if !UNITY_EDITOR
            _applicationInBackground = WebApplication.InBackground;
#endif
            OnGamePaused();
        }

        private void OnApplicationInBackground(bool inBackground)
        {
            if (_applicationInBackground == inBackground)
                return;

            var wasActive = IsActive;
            _applicationInBackground = inBackground;

            if (wasActive != IsActive)
            {
                _appActivateTrigger.Fire(IsActive);
                OnGamePaused();
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (_applicationFocused == focused)
                return;

            var wasActive = IsActive;
            _applicationFocused = focused;

            if (wasActive != IsActive)
            {
                _appActivateTrigger.Fire(IsActive);
                OnGamePaused();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (_applicationPaused == paused)
                return;

            var wasActive = IsActive;
            _applicationPaused = paused;

            if (wasActive != IsActive)
            {
                _appActivateTrigger.Fire(IsActive);
                OnGamePaused();
            }
        }

        public void Pause(object sender)
        {
            if (_pauseCounter.TryPause(sender))
                OnGamePaused();
        }

        public void Resume(object sender)
        {
            if (_pauseCounter.TryResume(sender))
                OnGamePaused();
        }

        private void OnGamePaused()
        {
            Time.timeScale = Paused ? 0.0f : 1.0f;
            Timer.Paused = Paused;

            _gamePausedTrigger.Fire(Paused);
            Screen.sleepTimeout = Paused ? SleepTimeout.SystemSetting : SleepTimeout.NeverSleep;
        }
    }
}

#endif