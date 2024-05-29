using Combat.Component.Bullet;
using UnityEngine;

namespace Combat.Component.Controller
{
    public class MovingBeamController : IController
    {
        private enum State
        {
            Initial,
            Starting,
            Moving,
        }

        private State _state = State.Initial;
        private float _maxLength;
        private float _elapsedTime;
        private float _maxRange;
        private readonly Vector2 _parentVelocity;
        private readonly float _bulletSpeed;
        private readonly Collider.RayCastCollider _collider;
        private readonly IBullet _unit;
        private readonly IController _parentController;

        public MovingBeamController(IBullet unit, float maxLength, float bulletSpeed, float maxRange, Vector2 parentVelicity, IController parentController)
        {
            _unit = unit;
			_bulletSpeed = bulletSpeed;
			_parentVelocity = parentVelicity;
			_collider = _unit.Collider as Collider.RayCastCollider;
			_maxLength = maxLength;
			_collider.MaxRange = 0;
			_parentController = parentController;
            _maxRange = maxRange;
        }

        public void Dispose() { }

        public void UpdatePhysics(float elapsedTime)
        {
            _elapsedTime += elapsedTime;

            if (_state == State.Initial)
            {
                _unit.Body.ApplyAcceleration(_parentVelocity - _unit.Body.Velocity);
                _state = State.Starting;
            }
            else if (_state == State.Starting)
            {
                var length = _bulletSpeed * _elapsedTime;
                if (length >= _maxLength)
                {
                    _state = State.Moving;
                    _unit.Body.ApplyAcceleration(RotationHelpers.Direction(_unit.Body.Rotation) * _bulletSpeed);
                    length = _maxLength;
                }
                
                if (length >= _maxRange)
                    length = _maxRange;

                _collider.MaxRange = length;
                _unit.Lifetime.Restore();
            }
            else if (_state == State.Moving)
            {
                _parentController?.UpdatePhysics(elapsedTime);
                var length = _unit.Lifetime.Left * _bulletSpeed;
                if (length < _maxLength)
                    _collider.MaxRange = length;
            }
        }
    }
}
