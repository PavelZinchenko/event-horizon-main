using Combat.Component.Bullet;
using Combat.Component.Unit;
using Combat.Unit;
using UnityEngine;

namespace Combat.Component.Controller
{
    public class StickyController : IController
    {
        private const float _offsetMultiplier = 0.9f;

        private readonly IBullet _bullet;
        private readonly float _lifetime;
        private IUnit _target;
        private Vector2 _position;
        private float _rotation;
        private float _cooldown;

        public StickyController(IBullet bullet, float lifetime)
        {
            _bullet = bullet;
            _lifetime = lifetime;
        }

        public void Dispose() { }

        public void UpdatePhysics(float elapsedTime)
        {
            if (_target == null)
            {
                var target = _bullet.Collider.ActiveTrigger;
                if (!IsTargetValid(target)) return;
                _target = target;
                _rotation = _bullet.Body.Rotation - target.Body.Rotation;
                _position = RotationHelpers.Transform(_bullet.Body.Position - _target.Body.WorldPosition(), -target.Body.WorldRotation())*_offsetMultiplier;
                _bullet.Lifetime.Take(-_lifetime);
                _cooldown = _lifetime;
            }

            _cooldown -= elapsedTime;
            if (_cooldown <= 0)
            {
                _bullet.Detonate();
                return;
            }

            if (!_target.IsActive())
            {
                _bullet.Vanish();
                return;
            }

            var targetRotation = _target.Body.WorldRotation();
            var targetPosition = _target.Body.WorldPosition();

            _bullet.Body.Turn(targetRotation + _rotation);
            _bullet.Body.Move(targetPosition + RotationHelpers.Transform(_position, targetRotation));
            _bullet.Body.ApplyAcceleration(_target.Body.WorldVelocity() - _bullet.Body.Velocity);
            _bullet.Body.ApplyAngularAcceleration(_target.Body.WorldAngularVelocity() - _bullet.Body.AngularVelocity);
        }

        private bool IsTargetValid(IUnit target)
        {
            if (target == null) return false;
            if (target.State != UnitState.Active) return false;
            if (!target.Collider.Enabled) return false;

            switch (target.Type.Class)
            {
                case Unit.Classification.UnitClass.SpaceObject:
                case Unit.Classification.UnitClass.Ship:
                case Unit.Classification.UnitClass.Drone:
                case Unit.Classification.UnitClass.Decoy:
                case Unit.Classification.UnitClass.Platform:
                case Unit.Classification.UnitClass.Limb:
                case Unit.Classification.UnitClass.Shield:
                    break;
                default: 
                    return false;
            }

            return true;
        }
    }
}
