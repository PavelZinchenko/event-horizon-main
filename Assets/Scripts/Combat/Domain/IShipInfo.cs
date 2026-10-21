using Combat.Ai;
using Combat.Component.Unit.Classification;
using UnityEngine;

namespace Combat.Domain
{
    public enum ShipStatus
    {
        Ready,
        Active,
        Destroyed,
    }

    public interface IShipInfo
    {
        ShipStatus Status { get; }

        Component.Ship.IShip ShipUnit { get; }
        Constructor.Ships.IShip ShipData { get; }

        // Built once by the fleet model, with player skills where the rules allow them.
        Constructor.IShipSpecification Specification { get; }

        float Condition { get; }
        float ActivationTime { get; }
        UnitSide Side { get; }

        void Create(Factory.ShipFactory factory, Vector2 position, int aiLevel);
        void Destroy();
    }
}
