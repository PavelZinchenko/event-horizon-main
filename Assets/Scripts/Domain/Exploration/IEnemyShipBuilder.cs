using Combat.Factory;
using UnityEngine;

namespace Game.Exploration
{
    public interface IEnemyShipBuilder
    {
        Combat.Component.Ship.Ship Build(ShipFactory shipFactory, SpaceObjectFactory objectFactory, Vector2 position, float rotation);

        // The ship that Build creates, for the combat core (exploration plan, E6). The same seed gives the same ship.
        Constructor.IShipSpecification CreateSpecification();
    }
}
