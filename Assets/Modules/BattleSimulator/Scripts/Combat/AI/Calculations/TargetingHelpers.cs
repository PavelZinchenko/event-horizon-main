using Combat.Component.Body;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using UnityEngine;

namespace Combat.Ai.Calculations
{
    public static class TargetingHelpers
    {
        public static bool CantDetectTarget(IShip ship, IShip enemy)
        {
            if (enemy == null)
                return true;
            if (enemy.Features.TargetPriority != TargetPriority.None)
                return false;

            var distance = Helpers.Distance(ship, enemy);
            return distance > 5 + enemy.Body.Scale;
        }

        public static bool TryGetProjectileTarget(IWeapon weapon, IShip ship, IShip enemy, out Vector2 target)
        {
            if (CantDetectTarget(ship, enemy))
            {
                target = Vector2.zero;
                return false;
            }

            var position = weapon.Platform.Body.WorldPosition();
            var velocity = enemy.Body.Velocity - ship.Body.Velocity*weapon.Info.RelativeVelocityEffect;

            float timeInterval;
            if (!Geometry.GetTargetPosition(
                enemy.Body.Position,
                velocity,
                position,
                weapon.Info.BulletSpeed,
                enemy.Body.Scale * 0.5f,
                out target,
                out timeInterval))
            {
                return false;
            }

            return weapon.Info.Range >= timeInterval * weapon.Info.BulletSpeed;
        }

        public static bool TryGetDirectTarget(IWeapon weapon, IShip ship, IShip enemy, out Vector2 target)
        {
            if (CantDetectTarget(ship, enemy))
            {
                target = Vector2.zero;
                return false;
            }

            target = enemy.Body.Position;
            var distance = Vector2.Distance(weapon.Platform.Body.WorldPosition(), target) - enemy.Body.Scale * 0.4f;
            return weapon.Info.Range >= distance;
        }
    }
}
