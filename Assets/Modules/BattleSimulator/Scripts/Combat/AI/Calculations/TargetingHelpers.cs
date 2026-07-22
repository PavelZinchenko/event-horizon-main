using Combat.Component.Body;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Ship.Effects;
using Combat.Component.Systems.Weapons;
using Combat.Unit;
using UnityEngine;

namespace Combat.Ai.Calculations
{
    public static class TargetingHelpers
    {
        public static bool CantDetectTarget(IShip ship, IShip enemy)
        {
            if (ship == null || enemy == null || !ship.IsActive() || !enemy.IsActive())
                return true;
            if (!RadarStatus.CanDetect(ship, enemy))
                return true;
            if (enemy.Features.TargetPriority != TargetPriority.None)
                return false;

            // Preview 4: the old fixed five-unit limit was smaller than most
            // useful weapon ranges and made otherwise visible targets impossible
            // to lock. Combat radar now covers the complete battle area.
            return false;
        }

        public static bool TryGetProjectileTarget(IWeapon weapon, IShip ship, IShip enemy, out Vector2 target, out float distance)
        {
            if (CantDetectTarget(ship, enemy))
            {
                target = Vector2.zero;
                distance = 0;
                return false;
            }

            var position = weapon.Platform.Body.WorldPosition();
            var velocity = enemy.Body.Velocity - ship.Body.Velocity * weapon.Info.RelativeVelocityEffect;
            var bulletSpeed = weapon.Info.BulletSpeed;

            if (!Geometry.GetTargetPosition(
                enemy.Body.Position,
                velocity,
                position,
                bulletSpeed,
                enemy.Body.Scale * 0.4f,
                out target,
                out var timeInterval))
            {
                distance = 0;
                return false;
            }

            distance = bulletSpeed * timeInterval;
            return weapon.Info.Range >= distance;
        }

        public static bool TryGetDirectTarget(IWeapon weapon, IShip ship, IShip enemy, out Vector2 target, out float distance)
        {
            if (CantDetectTarget(ship, enemy))
            {
                target = Vector2.zero;
                distance = 0;
                return false;
            }

            target = enemy.Body.Position;
            distance = Vector2.Distance(weapon.Platform.Body.WorldPosition(), target) - enemy.Body.Scale * 0.4f;
            return weapon.Info.Range >= distance;
        }
    }
}
