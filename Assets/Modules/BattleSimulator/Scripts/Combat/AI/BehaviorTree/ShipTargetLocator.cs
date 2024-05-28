using Combat.Component.Ship;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;

namespace Combat.Ai.BehaviorTree
{
    public class ShipTargetLocator
    {
        private readonly IScene _scene;

        public ShipTargetLocator(IScene scene)
        {
            _scene = scene;
        }

        public IShip FindEnemyForShip(IShip ship, bool ignoreDrones, float maxRange = 0f)
        {
            var options = EnemyMatchingOptions.EnemyForShip(maxRange);
            options.IgnoreDrones = ignoreDrones;
            return _scene.Ships.GetEnemy(ship, options);
        }

        public IShip FindEnemyForHomelessDrone(IShip ship, bool ignoreDrones, float maxRange = 0f)
        {
            var options = EnemyMatchingOptions.EnemyForDrone(maxRange);
            options.IgnoreDrones = ignoreDrones;
            return _scene.Ships.GetEnemy(ship, options);
        }

        public IShip FindEnemyForDrone(IShip ship, IShip mothership, bool ignoreDrones, float maxRange)
        {
            var options = EnemyMatchingOptions.EnemyForDrone(maxRange);
            options.IgnoreDrones = ignoreDrones;
            return _scene.Ships.GetEnemy(ship, mothership, options);
        }

        public IShip FindEnemyForDefenderShip(IShip ship, IShip mothership, bool ignoreDrones, float maxRange)
        {
            var options = EnemyMatchingOptions.EnemyForShip(maxRange);
            options.IgnoreDrones = ignoreDrones;
            return _scene.Ships.GetEnemy(ship, mothership, options);
        }

        public IShip FindTargetForRepairDrone(IShip ship, IShip mothership, float maxRange)
        {
            return GetTargetForRepairDrone(_scene.Ships, ship, mothership, maxRange);
        }

        private static IShip GetTargetForRepairDrone(IUnitList<IShip> shipList, IShip drone, IShip mothership, float maxDistance, float maxHp = 0.75f)
        {
            IShip target = null;
            var targetDistance = float.MaxValue;

            var shipPosition = drone.Body.Position;
            var mothershipPosition = mothership != null ? mothership.Body.Position : shipPosition;
            var mothershipSize = mothership != null ? mothership.Body.Scale : drone.Body.Scale;

            lock (shipList.LockObject)
            {
                foreach (var ship in shipList.Items)
                {
                    if (ship == mothership) continue;
                    if (!ship.IsActive() || ship.Type.Side.IsEnemy(drone.Type.Side)) continue;
                    if (ship.Type.Class != UnitClass.Ship) continue;
                    if (ship.Stats.Armor.Percentage > maxHp) continue;

                    var targetPosition = ship.Body.Position;
                    if (maxDistance > 0)
                    {
                        var mothershipDistance = mothershipPosition.Direction(targetPosition).magnitude;
                        var summarySize = 0.5f * (ship.Body.Scale + mothershipSize);
                        if (mothershipDistance - summarySize > maxDistance) continue;
                    }

                    var distance = shipPosition.Direction(targetPosition).magnitude;
                    if (distance > targetDistance) continue;

                    target = ship;
                    targetDistance = distance;
                }
            }

            return target;
        }
    }
}
