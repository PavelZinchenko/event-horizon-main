using GameServices.LevelManager;
using UnityEngine;
using Zenject;

namespace Services.GameApplication
{
    public class MobileApplication : MonoBehaviour, IApplication
    {
        [Inject] private readonly GamePausedSignal.Trigger _gamePausedTrigger;
        [Inject] private readonly AppActivatedSignal.Trigger _appActivateTrigger;

        private bool _applicationPaused;
        private bool _applicationFocused = true;
        private SceneLoadedSignal _sceneLoadedSignal;
        private readonly GamePauseCounter _pauseCounter = new();

        public bool Paused => _pauseCounter.Paused || !IsActive;
        public bool IsActive => !_applicationPaused && _applicationFocused;

        [Inject]
        private void Initialize(SceneLoadedSignal sceneLoadedSignal)
        {
            _sceneLoadedSignal = sceneLoadedSignal;
            _sceneLoadedSignal.Event += OnSceneLoaded;
        }

        private void Start()
        {
            OnGamePaused();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (_applicationFocused == focused)
                return;

            _applicationFocused = focused;
            _appActivateTrigger.Fire(IsActive);

            OnGamePaused();
        }

        private void OnApplicationPause(bool paused)
        {
            if (_applicationPaused == paused)
                return;

            _applicationPaused = paused;
            _appActivateTrigger.Fire(IsActive);

            OnGamePaused();
        }

        private void OnSceneLoaded()
        {
            if (!_applicationFocused) // unity bug
            {
                _applicationFocused = true;
                _appActivateTrigger.Fire(IsActive);
            }

            if (_pauseCounter.Paused)
                _pauseCounter.Reset();

            OnGamePaused();
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
