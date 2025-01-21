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
        private IUnit _target;
        private Vector2 _position;
        private float _rotation;
        private float _lifetime;

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
                _position = RotationHelpers.Transform(_bullet.Body.Position - _target.Body.Position, -target.Body.Rotation)*_offsetMultiplier;
                _bullet.Lifetime.Reset(_lifetime);
            }

            if (_bullet.Lifetime.Left <= elapsedTime)
            {
                _bullet.Detonate();
                return;
            }

            if (!_target.IsActive())
            {
                _bullet.Vanish();
                return;
            }

            _bullet.Body.Turn(_target.Body.Rotation + _rotation);
            _bullet.Body.Move(_target.Body.Position + RotationHelpers.Transform(_position, _target.Body.Rotation));
            _bullet.Body.ApplyAcceleration(_target.Body.Velocity - _bullet.Body.Velocity);
            _bullet.Body.ApplyAngularAcceleration(_target.Body.AngularVelocity - _bullet.Body.AngularVelocity);
        }

        private bool IsTargetValid(IUnit target)
        {
            if (target == null) return false;
            if (target.State != UnitState.Active) return false;
            if (!target.Collider.Enabled) return false;
            return true;
        }
    }
}
