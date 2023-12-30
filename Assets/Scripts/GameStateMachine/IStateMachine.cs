namespace GameStateMachine
{
    public interface IStateMachine
    {
        StateType ActiveState { get; }

		void ReloadState();
        void LoadState(IGameState state);
        void LoadStateAdditive(IGameState state);
        void UnloadActiveState();
    }
}
