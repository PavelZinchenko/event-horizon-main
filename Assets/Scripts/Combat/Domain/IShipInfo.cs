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

        // The share of the ship's armor that is lost: 0 when never deployed, 1 when destroyed.
        float DamageFraction { get; }

        void Create(Factory.ShipFactory factory, Vector2 position, int aiLevel);
        void Destroy();

        // The combat core scene writes the slot state here after every step (migration phase 4, step 12).
        // From then on Status, Condition, ActivationTime and DamageFraction read these values, not ShipUnit.
        void ApplyCoreState(ShipStatus status, float damageFraction, long deploymentOrder);
    }
}
