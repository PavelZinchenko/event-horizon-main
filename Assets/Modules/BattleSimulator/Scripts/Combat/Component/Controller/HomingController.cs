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

            if (/*!_target.IsActive() || */_timeFromLastUpdate > _targetUpdateCooldown)
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
            var requiredAngularVelocity = 0f;
            
            Vector2 targetPosition;
            bool canCatchUp;
            if (_smartAim)
            {
                canCatchUp = Geometry.GetTargetPosition(_target.Body.WorldPosition(), _target.Body.Velocity,
                    _unit.Body.WorldPosition(),
                    _maxVelocity, out targetPosition, out _);
            }
            else if(_target != null)
            {
                targetPosition = _target.Body.WorldPosition();
                canCatchUp = true;
            }
            else
            {
                return;
            }
            
            if (_target != null && _target.IsActive() && canCatchUp)
            {
                var direction = _unit.Body.WorldPosition().Direction(targetPosition);
                var target = RotationHelpers.Angle(direction);
                var rotation = _unit.Body.WorldRotation();
                var delta = Mathf.DeltaAngle(rotation, target);
                requiredAngularVelocity = delta > 5 ? _maxAngularVelocity : delta < -5 ? -_maxAngularVelocity : 0f;
            }

            if (_unit.Body.Parent == null)
            {
                _unit.Body.ApplyAngularAcceleration(requiredAngularVelocity - _unit.Body.AngularVelocity);
            }
            else
            {
                _computedVelocity += (requiredAngularVelocity - _computedVelocity) * Mathf.Deg2Rad / (0.2f + _unit.Body.Weight * 2f);
                var delta = _unit.Body.Rotation + _computedVelocity * elapsedTime;
                if(!Mathf.Approximately(delta, 0))
                    _unit.Body.Turn(delta);
            }
        }

        private float _timeFromLastUpdate = _targetUpdateCooldown;
        private float _computedVelocity = 0;
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
