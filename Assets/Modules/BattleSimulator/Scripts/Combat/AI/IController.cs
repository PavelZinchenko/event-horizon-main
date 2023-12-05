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
	    void Update(float deltaTime);
        ControllerStatus Status { get; }
    }

    public interface IControllerFactory
    {
        IController Create(IShip ship);
    }
}
