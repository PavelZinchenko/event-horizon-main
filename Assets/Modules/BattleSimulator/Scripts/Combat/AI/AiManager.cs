using System;
using System.Collections.Generic;
using CommonComponents.Signals;
using Zenject;

namespace Combat.Ai
{
	public sealed class AiManager : BackgroundTask, IAiManager, IInitializable, IDisposable, IFixedTickable
	{
	    public AiManager(CeasefireSignal ceasefireSignal, PlayerInputSignal playerInputSignal)
	    {
	        _ceasefireSignal = ceasefireSignal;
            _playerInputSignal = playerInputSignal;
	    }

		public void Add(IController item)
		{
			lock (_lockObject) 
			{
				_recentlyAddedControllers.Add(item);
			}
        }

        public void Initialize()
		{
            _ceasefireSignal.Event += OnCeasefire;
            _playerInputSignal.Event += OnPlayerInput;
            
            _currentFrame = 0;
			_lastFrame = -1;
			_fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;

			StartTask();
		}

		public void Dispose()
		{
            _ceasefireSignal.Event -= OnCeasefire;
            _playerInputSignal.Event -= OnPlayerInput;
            
            UnityEngine.Debug.Log("AiManager.Dispose");
			StopTask();
		}

		public void FixedTick()
		{
#if UNITY_WEBGL
			_currentFrame++;
			DoWork();
#else
			System.Threading.Interlocked.Increment(ref _currentFrame);
#endif
		}

		protected override bool DoWork()
		{
			if (_currentFrame == _lastFrame)
				return false;

            lock (_lockObject) 
			{
				if (_recentlyAddedControllers.Count > 0) 
				{
					_controllers.AddRange (_recentlyAddedControllers);
					_recentlyAddedControllers.Clear();
				}
			}

			var needCleanup = false;
			var count = _controllers.Count;
			var deltaTime = _fixedDeltaTime * (_currentFrame - _lastFrame);            
            _options.DisableThreats = count > _maxControllersBeforeOptimization;
            _options.TimeSinceLastPlayerInput = _timeSinceLastPlayerInput;
            _timeSinceLastPlayerInput += deltaTime;

			for (var i = 0; i < count; ++i)
			{
				var controller = _controllers[i];
				if (!IsDead(controller))
					controller.Update(deltaTime, _options);
				else
					needCleanup = true;
			}

			if (needCleanup)
				_controllers.RemoveAll(IsDead);

			_lastFrame = _currentFrame;
			return true;
		}

		private static bool IsDead(IController controller) => controller.Status == ControllerStatus.Dead;

		protected override void OnIdle ()
		{
			System.Threading.Thread.Sleep((int)(_fixedDeltaTime*1000));
		}

		private void OnCeasefire(bool value)
		{
			_options.CeaseFire = value;
		}

        private void OnPlayerInput()
        {
            _timeSinceLastPlayerInput = 0;
        }

        private float _timeSinceLastPlayerInput;
		private int _currentFrame;
		private int _lastFrame;
		private float _fixedDeltaTime;
        private Options _options;

        private readonly object _lockObject = new object();
	    private readonly CeasefireSignal _ceasefireSignal;
        private readonly PlayerInputSignal _playerInputSignal;
        private readonly List<IController> _recentlyAddedControllers = new List<IController>();
        private readonly List<IController> _controllers = new List<IController>();

        private const int _maxControllersBeforeOptimization = 30;

        public struct Options
        {
            public bool CeaseFire;
            public bool DisableThreats;
            public float TimeSinceLastPlayerInput;
        }
    }

    public class CeasefireSignal : Signal<CeasefireSignal, bool> { }
    public class PlayerInputSignal : Signal<PlayerInputSignal> { }
}
