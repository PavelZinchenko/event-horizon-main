using Combat.Component.Ship;

namespace Combat.Ai
{
    public enum ControllerStatus
    {
        Active,
        Idle,
        Dead,
    }

    public interface IController
    {
        void Update(float deltaTime, in AiManager.Options options);
        ControllerStatus Status { get; }
    }

    public interface IControllerFactory
    {
        IController Create(IShip ship);
    }
}
