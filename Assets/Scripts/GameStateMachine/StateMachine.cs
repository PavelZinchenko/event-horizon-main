using System;
using System.Collections.Generic;
using System.Linq;
using GameServices.SceneManager;
using GameStateMachine.States;
using Scripts.GameStateMachine;
using UniRx;
using CommonComponents.Signals;
using Zenject;

namespace GameStateMachine
{
    public class StateMachine : IStateMachine, IInitializable, IDisposable, ITickable
    {
        [Inject] private readonly InitializationState.Factory _initializationStateFactory;
        [Inject] private readonly IGameSceneManager _gameSceneManager;
        [Inject] private readonly GameStateChangedSignal.Trigger _gameStateChangedTrigger;

        public StateMachine(SceneManagerStateChangedSignal sceneManagerStateChangedSignal)
        {
            _sceneManagerStateChangedSignal = sceneManagerStateChangedSignal;
            _sceneManagerStateChangedSignal.Event += OnSceneManagerStateChanged;
        }

        public void Initialize()
        {
            UnityEngine.Debug.Log("StateMachine.Initialize");
            LoadState(_initializationStateFactory.Create());
        }

        public void Dispose()
        {
            while (_states.Any())
                _states.Pop().Condition = GameStateCondition.NotLoaded;
        }

        public void Tick()
        {
            if (_states.Any())
                _states.Peek().Update(UnityEngine.Time.deltaTime);
        }

        public StateType ActiveState { get { return _states.Any() ? _states.Peek().Type : StateType.Undefined; } }

        public void UnloadActiveState()
        {
            UnloadState();

            if (ActiveState == StateType.Undefined)
                UnityEngine.Application.Quit();
        }

        public void LoadState(IGameState state)
        {
            if (_isLevelLoading)
            {
                throw new BadGameStateException(state.Type + ": state already loading");
            }

            if (_states.Any())
                _states.Pop().Condition = GameStateCondition.NotLoaded;

            _states.Push(state);
            LoadRequiredScenes();
        }

        public void LoadStateAdditive(IGameState state)
        {
            if (_isLevelLoading)
            {
                throw new BadGameStateException(state.Type + ": state already loading");
            }

            if (_states.Any())
                _states.Peek().Condition = GameStateCondition.Suspended;

            _states.Push(state);
            LoadRequiredScenes();
        }

		public void ReloadState()
		{
			if (!_states.TryPeek(out var state))
				return;

			var lastState = _states.Pop();
			lastState.Condition = GameStateCondition.Reloading;
			LoadRequiredScenes();
			_states.Push(lastState);
			LoadRequiredScenes();
		}

		private void UnloadState()
        {
            if (_isLevelLoading)
            {
                var state = _states.Peek();
                throw new BadGameStateException(state?.Type + ": state already loading");
            }

            var lastState = _states.Pop();
            lastState.Condition = GameStateCondition.NotLoaded;

            LoadRequiredScenes();
        }

        private void LoadRequiredScenes()
        {
            _isLevelLoading = true;

			//Observable.Timer(TimeSpan.FromSeconds(5), Scheduler.MainThreadIgnoreTimeScale).Subscribe(_ => _gameSceneManager.Load(RequiredScenes));

			if (_states.TryPeek(out var state))
				SceneContext.ExtraBindingsInstallMethod = state.InstallBindings;

			_gameSceneManager.Load(RequiredScenes);
        }

        private IEnumerable<GameScene> RequiredScenes
        {
            get { return _states.SelectMany(state => state.RequiredScenes); }
        }

        private void OnSceneManagerStateChanged(State state)
        {
            if (state == State.Ready && _isLevelLoading)
            {
                _isLevelLoading = false;
				SceneContext.ExtraBindingsInstallMethod = null;

                if (_states.Any())
                {
                    var currentState = _states.Peek();
                    currentState.Condition = GameStateCondition.Active;
                    _gameStateChangedTrigger.Fire(currentState.Type);
                }
            }
        }

        private bool _isLevelLoading;
        private readonly Stack<IGameState> _states = new Stack<IGameState>();
        private readonly SceneManagerStateChangedSignal _sceneManagerStateChangedSignal;
    }

    public class GameStateChangedSignal : SmartWeakSignal<GameStateChangedSignal, StateType> {}
}
