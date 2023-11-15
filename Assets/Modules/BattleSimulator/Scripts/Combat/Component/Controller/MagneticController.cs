using Combat.Component.Unit;
using Combat.Scene;
using Combat.Unit;

namespace Combat.Component.Controller
{
    public class MagneticController : IController
    {
        public MagneticController(IUnit unit, float maxVelocity, float acceleration, float maxRange, bool lookForward, IScene scene)
        {
            _unit = unit;
            _scene = scene;
            _maxVelocity = maxVelocity;
            _acceleration = acceleration;
            _maxRange = maxRange;
            _lookForward = lookForward;
        }

        public void Dispose() { }

        public void UpdatePhysics(float elapsedTime)
        {
            if (_unit.Body.Parent != null)
                return;

            _timeFromLastUpdate += elapsedTime;

            if (_timeFromLastUpdate > _targetUpdateCooldown)
            {
                _target = _scene.Ships.GetEnemy(_unit, 0f, _maxRange * 1.3f, _lookForward ? 30f : 360f, false, false);
                _timeFromLastUpdate = 0;
            }

            UpdateVelocity(elapsedTime);
        }

        private void UpdateVelocity(float deltaTime)
        {
            if (!_target.IsActive()) return;

            var velocity = _unit.Body.Velocity;
            var position = _unit.Body.Position;
            var targetVelocity = _target.Body.Velocity;
            var targetPosition = _target.Body.Position;

            var timeToTarget = (targetPosition - position).magnitude / _maxVelocity;
            targetPosition += targetVelocity * timeToTarget;

            var requiredVelocity = (targetPosition - position).normalized * _maxVelocity;
            _unit.Body.ApplyAcceleration((requiredVelocity - velocity).normalized * _acceleration * deltaTime);

            _rotation = RotationHelpers.Angle(velocity);

            if (_lookForward)
                _unit.Body.Turn(_rotation);
        }

        private float _rotation;
        private float _timeFromLastUpdate = _targetUpdateCooldown;
        private IUnit _target;
        private readonly bool _lookForward;
        private readonly IUnit _unit;
        private readonly IScene _scene;
        private readonly float _maxVelocity;
        private readonly float _acceleration;
        private readonly float _maxRange;
        private const float _targetUpdateCooldown = 2.0f;
    }
}
