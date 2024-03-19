using System;
using Combat.Component.Bullet;
using UnityEngine;

namespace Combat.Component.Controller
{
    public class MovingBeamController : IController
    {
        public MovingBeamController(IBullet unit, float maxLength, float bulletSpeed, Vector2 parentVelicity, IController parentController)
        {
            _unit = unit;
			_bulletSpeed = bulletSpeed;
			_parentVelocity = parentVelicity;
			
			_collider = _unit.Collider as Collider.RayCastCollider ??
			            throw new ArgumentException($"MovingBeamController only accepts units RayCastCollider, but got unit with {_unit.Collider.GetType()} collider instead", nameof(unit));
			
			_maxLength = maxLength;
			_collider.MaxRange = 0;
			_parentController = parentController;
			
			// If bullet has zero speed it won't grow, so just set length directly and mark as completed
			if (bulletSpeed <= 0)
			{
				_collider.MaxRange = maxLength;
				_completed = true;
			} 
        }

        public void Dispose() { }

        public void UpdatePhysics(float elapsedTime)
        {
			if (_completed)
			{
				_parentController?.UpdatePhysics(elapsedTime);
				return;
			}

			if (_elapsedTime == 0)
				_unit.Body.ApplyAcceleration(_parentVelocity - _unit.Body.Velocity);

			_elapsedTime += elapsedTime;

			var length = _bulletSpeed * _elapsedTime;
			if (length >= _maxLength)
			{
				_completed = true;
				_unit.Body.ApplyAcceleration(RotationHelpers.Direction(_unit.Body.Rotation) * _bulletSpeed);
				length = _maxLength;
			}

			_collider.MaxRange = length;
			_unit.Lifetime.Restore();
		}

		private bool _completed;
		private float _maxLength;
		private float _elapsedTime;
		private readonly Vector2 _parentVelocity;
		private readonly float _bulletSpeed;
		private readonly Collider.RayCastCollider _collider;
        private readonly IBullet _unit;
		private readonly IController _parentController;
    }
}
