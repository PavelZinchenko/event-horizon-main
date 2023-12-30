using System.Collections.Generic;
using GameServices.SceneManager;
using GameStateMachine.States;
using Zenject;

namespace GameStateMachine
{
    public interface IGameState
    {
        GameStateCondition Condition { get; set; }

        StateType Type { get; }
        IEnumerable<GameScene> RequiredScenes { get; }
		void InstallBindings(DiContainer container);
        void Update(float elapsedTime);
    }
}
