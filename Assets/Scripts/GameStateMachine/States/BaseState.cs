using System;
using System.Collections.Generic;
using GameServices.SceneManager;
using CommonComponents.Signals;
using Zenject;

namespace GameStateMachine.States
{
    public enum GameStateCondition
    {
        NotLoaded,
        Active,
        Suspended,
    }

    public abstract class BaseState : IGameState
    {
        protected BaseState(IStateMachine stateMachine, GameStateFactory stateFactory)
        {
            _stateMachine = stateMachine;
            _stateFactory = stateFactory;
        }

        public virtual IEnumerable<GameScene> RequiredScenes { get { yield break; } }

        public abstract StateType Type { get; }

        public GameStateCondition Condition
        {
            get { return _condition; }
            set
            {
                if (_condition == value)
                    return;

				if (value == GameStateCondition.NotLoaded)
                {
                    _condition = value;
                    OnUnload();
                }
                else if (value == GameStateCondition.Suspended && _condition == GameStateCondition.Active)
                {
                    _condition = value;
                    OnSuspend();
                }
                else if (value == GameStateCondition.Active && _condition == GameStateCondition.NotLoaded)
                {
                    _condition = value;
                    OnLoad();
                }
                else if (value == GameStateCondition.Active && _condition == GameStateCondition.Suspended)
                {
                    _condition = value;
                    OnResume();
                }
                else
                {
                    throw new ArgumentException("Can't change state from " + _condition + " to " + value);
                }

				if (value == GameStateCondition.Active)
					OnActivate();
			}
		}

        public virtual void Update(float elapsedTime) {}
		public virtual void InstallBindings(DiContainer container) {}

		protected virtual void OnLoad() { }
        protected virtual void OnUnload() { }
        protected virtual void OnSuspend() { }
        protected virtual void OnResume() { }
		protected virtual void OnActivate() { }

		protected void Unload()
		{
			if (Condition != GameStateCondition.Active)
			{
				UnityEngine.Debug.Log($"{Type}.Unload(): State is not active");
				return;
			}

			_stateMachine.UnloadActiveState();
		}

		protected void Reload()
		{
			if (Condition != GameStateCondition.Active)
			{
				UnityEngine.Debug.Log($"{Type}.Reload(): State is not active");
				return;
			}

			_stateMachine.ReloadState();
		}

		protected void LoadState(IGameState state)
		{
			if (Condition != GameStateCondition.Active)
			{
				UnityEngine.Debug.Log($"{Type}.LoadState({state.Type}): State is not active");
				return;
			}

			_stateMachine.LoadState(state);
		}

		protected void LoadStateAdditive(IGameState state)
		{
			if (Condition != GameStateCondition.Active)
			{
				UnityEngine.Debug.Log($"{Type}.LoadStateAdditive({state.Type}): State is not active");
				return;
			}

			_stateMachine.LoadStateAdditive(state);
		}

        protected GameStateFactory StateFactory { get { return _stateFactory; } }

		private GameStateCondition _condition = GameStateCondition.NotLoaded;

        private readonly IStateMachine _stateMachine;
        private readonly GameStateFactory _stateFactory;
    }

    public class ExitSignal : SmartWeakSignal<ExitSignal> {}
}
