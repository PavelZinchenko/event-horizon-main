using Combat.Component.Body;
using Combat.Component.Platform;
using Combat.Component.Satellite;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Scene;
using Combat.Unit;
using UnityEngine;

namespace Combat.Component.Controller
{
    class AutoAimingSatelliteController : IController, IAimingSystem
    {
        public AutoAimingSatelliteController(IShip ship, ISatellite satellite, Vector2 position, float rotation, float minAngle, float maxAngle, IScene scene) 
        {
            _minAngle = minAngle;
            _maxAngle = maxAngle;
            _defaultRotation = rotation;
            _position = position;
            _scene = scene;
            _ship = ship;
            _satellite = satellite;
        }

        public void Aim(float bulletVelocity, float weaponRange, float relativeEffect)
        {
            _bulletVelocity = bulletVelocity;
            _weaponRange = weaponRange;
            _relativeEffect = relativeEffect;
        }

        public void UpdatePhysics(float elapsedTime)
        {
            if (_ship.State == UnitState.Destroyed)
                _satellite.Destroy();
            else if (_ship.State == UnitState.Inactive)
                _satellite.Vanish();
            else
            {
                _timeFromTargetUpdate += elapsedTime;

                var requiredPosition = GetRequiredPosition();
                var requiredRotation = _ship.Body.WorldRotation() + GetTargetCourse();
                _satellite.MoveTowards(requiredPosition, requiredRotation, _ship.Body.WorldVelocity(), _velocityFactor, _angularVelocityFactor);
            }
        }

        public void Dispose() { }

        private Vector2 GetRequiredPosition()
        {
            return _ship.Body.WorldPosition() + RotationHelpers.Transform(_position, _ship.Body.WorldRotation());
        }


		public IUnit ActiveTarget
		{
			get => _target;
			set
			{
				_target = value;
				_timeFromTargetUpdate = 0;
			}
		}

		private float GetTargetCourse()
        {
            if (_timeFromTargetUpdate > _targetUpdateCooldown || (_timeFromTargetUpdate > _targetFindCooldown && !_target.IsActive()))
                ActiveTarget = _scene.Ships.GetEnemy(_ship, EnemyMatchingOptions.EnemyForTurret(), _defaultRotation + (_maxAngle - _minAngle) / 2, _maxAngle - _minAngle, _weaponRange);

            if (!_target.IsActive())
                return _defaultRotation;

            var targetPosition = _target.Body.WorldPosition();
            var platformPosition = GetRequiredPosition();
            float rotation;

            if (_bulletVelocity > 0)
            {
                var velocity = _target.Body.WorldVelocity() - _satellite.Body.WorldVelocity() * _relativeEffect;

                Vector2 target;
                float timeInterval;
                if (!Geometry.GetTargetPosition(
                    targetPosition,
                    velocity,
                    platformPosition,
                    _bulletVelocity,
                    out target,
                    out timeInterval))
                {
                    target = targetPosition;
                }

                rotation = RotationHelpers.Angle(platformPosition.Direction(target)) - _ship.Body.WorldRotation();;
            }
            else
            {
                rotation = RotationHelpers.Angle(platformPosition.Direction(targetPosition)) - _ship.Body.WorldRotation();
            }

            if (rotation < _defaultRotation + _minAngle)
                rotation = _defaultRotation;
            else if (rotation > _defaultRotation + _maxAngle)
                rotation = _defaultRotation;

            return rotation;
        }

        private float _bulletVelocity;
        private float _weaponRange;
        private float _relativeEffect = 1f;
        private float _timeFromTargetUpdate;

        private IUnit _target;
        private readonly float _minAngle;
        private readonly float _maxAngle;
        private readonly float _defaultRotation;
        private readonly Vector2 _position;
        private readonly IScene _scene;
        private readonly IUnit _ship;
        private readonly ISatellite _satellite;

        private const float _targetUpdateCooldown = 1.0f;
        private const float _targetFindCooldown = 0.1f;
        private const float _velocityFactor = 0.75f;
        private const float _angularVelocityFactor = 3.0f;
	}
}
