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
		Reloading,
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

				var oldValue = _condition;
				_condition = value;

				if (oldValue == GameStateCondition.Active)
					OnDeactivate();

				if (_reloadWhenPossible && value == GameStateCondition.Active)
				{
					_reloadWhenPossible = false;
					_stateMachine.ReloadState();
				}
				else if (value == GameStateCondition.NotLoaded)
				{
					OnUnload();
				}
				else if (value == GameStateCondition.Suspended && oldValue == GameStateCondition.Active)
				{
					OnSuspend();
				}
				else if (value == GameStateCondition.Active && oldValue == GameStateCondition.NotLoaded)
				{
					OnLoad();
				}
				else if (value == GameStateCondition.Active && oldValue == GameStateCondition.Suspended)
				{
					OnResume();
				}
				else if (value == GameStateCondition.Active && oldValue == GameStateCondition.Reloading)
				{
					OnReloaded();
				}
				else if (value == GameStateCondition.Reloading && oldValue == GameStateCondition.Active)
				{
					OnBeginReload();
				}
				else
				{
					_condition = oldValue;
					throw new ArgumentException("Can't change state from " + oldValue + " to " + value);
				}

				if (value == GameStateCondition.Active)
					OnActivate();
			}
		}

		public virtual void Update(float elapsedTime) { }
		public virtual void InstallBindings(DiContainer container) { }

		protected virtual void OnLoad() { }
		protected virtual void OnUnload() { }
		protected virtual void OnSuspend() { }
		protected virtual void OnResume() { }
		protected virtual void OnActivate() { }
		protected virtual void OnDeactivate() { }
		protected virtual void OnBeginReload() { }
		protected virtual void OnReloaded() { }

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
				_reloadWhenPossible = true;
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

		private bool _reloadWhenPossible;
        private readonly IStateMachine _stateMachine;
        private readonly GameStateFactory _stateFactory;
    }

    public class ExitSignal : SmartWeakSignal<ExitSignal> {}
}
