using Combat.Component.Body;
using Combat.Component.Unit;
using Combat.Scene;
using Combat.Unit;
using UnityEngine;

namespace Combat.Component.Controller
{
    public class HomingController : IController
    {
        public HomingController(IUnit unit, float maxVelocity, float maxAngularVelocity, float acceleration, float maxRange, bool smartAim, IScene scene)
        {
            _unit = unit;
            _scene = scene;
            _maxVelocity = maxVelocity;
            _maxAngularVelocity = maxAngularVelocity;
            _acceleration = acceleration;
            _maxRange = maxRange;
            _smartAim = smartAim;
        }

        public void Dispose() {}

        public void UpdatePhysics(float elapsedTime)
        {
            _timeFromLastUpdate += elapsedTime;

            if (_timeFromLastUpdate > _targetUpdateCooldown)
            {
                _target = _scene.Ships.GetEnemy(_unit, 0f, _maxRange*1.3f, 15f, false, false);
                _timeFromLastUpdate = 0;
            }
            
            
            UpdateVelocity(elapsedTime);
            UpdateRotation(elapsedTime);
        }

        private void UpdateVelocity(float deltaTime)
        {
            if (_unit.Body.Parent != null) return;

            var forward = RotationHelpers.Direction(_unit.Body.Rotation);
            var velocity = _unit.Body.Velocity;
            var forwardVelocity = Vector2.Dot(velocity, forward);
            if (forwardVelocity >= _maxVelocity)
                return;

            var requiredVelocity = Mathf.Max(forwardVelocity, _maxVelocity) * forward;
            var dir = (requiredVelocity - velocity).normalized;

            _unit.Body.ApplyAcceleration(dir * _acceleration * deltaTime);
        }

        private void UpdateRotation(float elapsedTime)
        {
            if(_target == null || !_target.IsActive()) return;

            if (!_smartAim || !Geometry.GetTargetPosition(_target.Body.WorldPosition(), _target.Body.Velocity,
                    _unit.Body.WorldPosition(),
                    _maxVelocity, out var targetPosition, out _))
            {
                targetPosition = _target.Body.WorldPosition();
            }
            
            
            var direction = _unit.Body.WorldPosition().Direction(targetPosition);
            var target = RotationHelpers.Angle(direction);
            var rotation = _unit.Body.WorldRotation();
            var delta = Mathf.DeltaAngle(rotation, target);
            var requiredAngularVelocity = delta > 5 ? _maxAngularVelocity : delta < -5 ? -_maxAngularVelocity : 0f;
            

            if (_unit.Body.Parent == null)
            {
                _unit.Body.ApplyAngularAcceleration(requiredAngularVelocity - _unit.Body.AngularVelocity);
            }
            else
            {
                // Simulate ApplyAngularAcceleration behavior
                _computedVelocity += (requiredAngularVelocity - _computedVelocity) * Mathf.Deg2Rad / (0.2f + _unit.Body.Weight * 2f);
                var turn = _unit.Body.Rotation + _computedVelocity * elapsedTime;
                if(!Mathf.Approximately(turn, 0))
                    _unit.Body.Turn(turn);
            }
        }

        private float _timeFromLastUpdate = _targetUpdateCooldown;
        private float _computedVelocity;
        private readonly bool _smartAim;
        private IUnit _target;
        private readonly IUnit _unit;
        private readonly IScene _scene;
        private readonly float _maxVelocity;
        private readonly float _maxAngularVelocity;
        private readonly float _acceleration;
        private readonly float _maxRange;
        private const float _targetUpdateCooldown = 2.0f;
    }
}
