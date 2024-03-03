using Combat.Component.Body;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using UnityEngine;

namespace Combat.Component.Platform
{
    public class WeaponPlatformBody : MonoBehaviour, IWeaponPlatformBody
    {
        public static WeaponPlatformBody Create(IScene scene, IUnit parent, Vector2 position, float rotation, float offset, float maxAngle, float rotationSpeed, bool hasTurret)
        {
            var gameobject = new GameObject("Body");
            parent.Body.AddChild(gameobject.transform);
            var body = gameobject.AddComponent<WeaponPlatformBody>();
            body.Initialize(scene, parent, position, rotation, offset, maxAngle, rotationSpeed, hasTurret);
            return body;
        }

        public IBody Parent => _parent.Body;
        public Vector2 Position => _position;
        public float Rotation => _rotation;
        public float Offset => _offset;
        public Vector2 Velocity => Vector2.zero;
        public float AngularVelocity => _angularVelocity;
        public float Weight => 0.0f;
        public float Scale => _scale;
        public Vector2 VisualPosition => _position;

        public float VisualRotation 
        {
            get 
            {
                if (Mathf.Approximately(_angularVelocity, 0)) return _rotation;
                var deltaTime = (float)(Time.timeAsDouble - Time.fixedTimeAsDouble);
                return _rotation + _angularVelocity*deltaTime;
            }
        }

        public void Move(Vector2 position) {}
        public void Turn(float rotation) {}
        public void ApplyAcceleration(Vector2 acceleration) {}
        public void ApplyAngularAcceleration(float acceleration) {}
        public void SetSize(float size) {}

        public void ApplyForce(Vector2 position, Vector2 force)
        {
            Parent.ApplyForce(position, force);
        }

        public void SetVelocityLimit(float value) {}

        public void Dispose()
        {
            Destroy(gameObject);
        }

        public float FixedRotation => _initialRotation;
        public float AutoAimingAngle => _maxAngle;

        public void UpdatePhysics(float elapsedTime)
        {
            _timeFromTargetUpdate += elapsedTime;
            _idleTime += elapsedTime;
            UpdateRotation(elapsedTime);

            if (_hasTurret && _weaponRange > 0)
                Aim(_bulletVelocity, _weaponRange, _relativeEffect);
        }

        public void UpdateView(float elapsedTime)
        {
            transform.localEulerAngles = new Vector3(0, 0, VisualRotation);
        }

        public void AddChild(Transform child)
        {
            child.parent = transform;
        }

		public IShip ActiveTarget 
		{
			get => _target; 
			set
			{
                _timeFromTargetUpdate = 0f;
                if (_target == value) return;
                _target = value;
                UpdateRotataionOnTargetChange();
			} 
		}

        public void Aim(float bulletVelocity, float weaponRange, float relativeEffect)
        {
            _bulletVelocity = bulletVelocity;
            _weaponRange = weaponRange;
            _relativeEffect = relativeEffect;

            if (!IsValidTarget(_target))
                _target = null;

            if (_timeFromTargetUpdate < _targetFindCooldown) return;
            if (_timeFromTargetUpdate < _targetUpdateCooldown && _target.IsActive()) return;

			ActiveTarget = _scene.Ships.GetEnemyForTurret(_parent, ((IBody)this).WorldPosition(),
                _parent.Body.WorldRotation() + _initialRotation, _maxAngle, _weaponRange);
        }

		private bool IsValidTarget(IUnit target)
		{
			if (target == null) return false;
			if (target.Type.Side.IsAlly(_parent.Type.Side)) return true;
			if (target is not IShip ship) return true;
			if (ship.Features.TargetPriority == TargetPriority.None) return false;
			return true;
		}

        private void Initialize(IScene scene, IUnit parent, Vector2 position, float rotation, float offset, float maxAngle, float rotationSpeed, bool hasTurret)
        {
            _scene = scene;
            _parent = parent;
            _position = parent.Body.ChildPosition(position);
            _initialRotation = rotation;
            _offset = offset;
            _maxAngle = maxAngle;
            _scale = 1f / parent.Body.Scale;
            _rotationSpeed = rotationSpeed > 0 ? rotationSpeed : 360;
            _hasTurret = hasTurret;

            transform.localPosition = _position;
            transform.localScale = Vector3.one*_scale;
        }

        private void UpdateRotataionOnTargetChange()
        {
            if (_hasTurret) return;
            if (_idleTime <= 0) return;
            if (!_target.IsActive()) return;
            UpdateRotation(_idleTime);
            _idleTime = 0;
        }

        private void UpdateRotation(float elapsedTime)
        {
            var targetRotation = _initialRotation;
            float rotation;

            if (!_target.IsActive())
            {
                rotation = Mathf.MoveTowardsAngle(_rotation, targetRotation, _rotationSpeed * elapsedTime);
                _angularVelocity = Mathf.DeltaAngle(_rotation, rotation) / elapsedTime;
                _rotation = rotation;
                return;
            }

            var targetPosition = _target.Body.WorldPosition();
            var platformPosition = ((IBody)this).WorldPosition();

            if (_bulletVelocity > 0)
            {
                var velocity = _target.Body.WorldVelocity() - Parent.WorldVelocity() * _relativeEffect;

                if (!Geometry.GetTargetPosition(
                    targetPosition,
                    velocity,
                    platformPosition,
                    _bulletVelocity,
                    out Vector2 target,
                    out float timeInterval))
                {
                    target = targetPosition;
                }

                rotation = RotationHelpers.Angle(platformPosition.Direction(target)) - Parent.WorldRotation();
            }
            else
            {
                rotation = RotationHelpers.Angle(platformPosition.Direction(targetPosition)) - Parent.WorldRotation();
            }

            var delta = Mathf.DeltaAngle(targetRotation, rotation);
            if (delta > _maxAngle)
                targetRotation += _maxAngle;
            else if (delta < -_maxAngle)
                targetRotation -= _maxAngle;
            else
                targetRotation = rotation;

            rotation = Mathf.MoveTowardsAngle(_rotation, targetRotation, _rotationSpeed * elapsedTime);
            _angularVelocity = Mathf.DeltaAngle(_rotation, rotation) / elapsedTime;
            _rotation = rotation;
            _idleTime = 0;

        }

        private float _bulletVelocity;
        private float _weaponRange;
        private float _relativeEffect = 1;

        private float _timeFromTargetUpdate = _targetUpdateCooldown;
        private IShip _target;

        private float _rotation;
        private float _angularVelocity;

        private bool _hasTurret;
        private Vector2 _position;
        private float _initialRotation;
        private float _maxAngle;
        private float _offset;
        private float _scale;
        private IUnit _parent;
        private IScene _scene;
        private float _idleTime;
        private float _rotationSpeed;

        private const float _targetUpdateCooldown = 1.0f;
        private const float _targetFindCooldown = 0.1f;
    }
}
