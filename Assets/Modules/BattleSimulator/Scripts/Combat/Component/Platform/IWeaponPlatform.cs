using System;
using UnityEngine;
using Combat.Component.Body;
using Combat.Component.Systems.Weapons;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Unit.HitPoints;
using Combat.Component.Ship;

namespace Combat.Component.Platform
{
    public interface IWeaponPlatform : IDisposable
    {
        [Obsolete] UnitType Type { get; }
        IBody Body { get; }
        IUnit Owner { get; }
        IResourcePoints EnergyPoints { get; }

        IBulletCompositeDisposable Bullets => null;
        float MountAngle { get; }

        bool IsReady { get; }
        float Cooldown { get; }
        float AutoAimingAngle { get; }
		IShip ActiveTarget { get; set; }

		void Aim(float bulletVelocity, float weaponRange, float relativeEffect);
        void OnShot();
        void SetView(IView view, Color color);

        void UpdatePhysics(float elapsedTime);
        void UpdateView(float elapsedTime);
    }

    public static class WeaponPlatformExtensions
    {
        public static float WorldMountAngle(this IWeaponPlatform platform)
        {
            return platform.Body.Parent.WorldRotation() + platform.MountAngle;
        }

        public static float AimingDeltaAngle(this IWeaponPlatform platform, Vector2 target)
        {
            var position = platform.Body.WorldPosition();
            var rotation = platform.Body.WorldRotation();
            var targetAngle = RotationHelpers.Angle(position.Direction(target));
            return Mathf.DeltaAngle(rotation, targetAngle);
        }

        public static float RequiredShipCourseCorrection(this IWeaponPlatform platform, Vector2 target)
        {
            var position = platform.Body.WorldPosition();
            var targetAngle = RotationHelpers.Angle(position.Direction(target));
            var mountAngle = platform.WorldMountAngle();
            var delta = Mathf.DeltaAngle(mountAngle, targetAngle);

            var turningAngle = platform.AutoAimingAngle;
            if (delta > 0)
                return delta > turningAngle ? delta - turningAngle : 0;
            else
                return delta < -turningAngle ? delta + turningAngle : 0;
        }

        public static float OptimalShipCourse(this IWeaponPlatform platform, Vector2 target)
        {
            var position = platform.Body.WorldPosition();
            return RotationHelpers.Angle(position.Direction(target)) - platform.MountAngle;
        }
    }
}
