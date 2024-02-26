using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using UnityEngine;

namespace Combat.Ai.BehaviorTree.Utils
{
    public static class ThreatAnalyzer
    {
        public static bool IsThreat(IShip ship, IUnit unit)
        {
            switch (unit.Type.Class)
            {
                case UnitClass.Ship:
                case UnitClass.Drone:
                case UnitClass.Decoy:
                case UnitClass.Loot:
                case UnitClass.Camera:
                    return false;
            }

            return ship.Type.Side.IsEnemy(unit.Type.Side);
        }

        public static bool IsObstacle(IShip ship, IUnit unit)
        {
            if (!IsSuitableUnit(ship, unit)) return false;

            const float maxAngle = 180f; // TODO: move to settings
            const float maxDistance = 1.0f;
            var direction = unit.Body.Position - ship.Body.Position;
            var distance = direction.magnitude - 0.5f * (ship.Body.Scale + unit.Body.Scale);
            var angle = RotationHelpers.Angle(direction);
            var delta = Mathf.Abs(Mathf.DeltaAngle(ship.Body.Rotation, angle));
            return distance < maxDistance && delta < maxAngle;
        }

        private static bool IsSuitableUnit(IUnit ship, IUnit unit)
        {
            switch (unit.Type.Class)
            {
                case UnitClass.SpaceObject:
                    return ship.Body.Weight <= unit.Body.Weight;
                case UnitClass.Platform:
                    return true;
                case UnitClass.Shield:
                case UnitClass.Ship:
                case UnitClass.Limb:
                    if (unit.Type.Side.IsEnemy(ship.Type.Side)) return false;
                    return ship.Body.Weight <= unit.Body.Weight;
                default:
                    return false;
            }
        }
    }
}
