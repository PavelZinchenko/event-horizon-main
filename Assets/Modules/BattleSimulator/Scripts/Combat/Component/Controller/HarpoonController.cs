using Combat.Component.Bullet;
using Combat.Component.Physics.Joint;
using Combat.Component.Platform;
using Combat.Component.Unit;
using Combat.Unit;

namespace Combat.Component.Controller
{
    public class HarpoonController : IController
    {
        public HarpoonController(IBullet bullet, IWeaponPlatform parent, float maxDistance)
        {
            _bullet = bullet;
            _parent = parent;
            _maxDistance = maxDistance;
            _parentJoint = _bullet.Physics.CreateDistanceJoint(_parent.Owner.Physics, _maxDistance);
        }

        public void Dispose()
        {
            if (_parentJoint != null) _parentJoint.Dispose();
            if (_targetJoint != null) _targetJoint.Dispose();

            _parentJoint = null;
            _targetJoint = null;
        }

        public void UpdatePhysics(float elapsedTime)
        {
            var hasTarget = IsTargetValid();
            if (!hasTarget)
            {
                //if (_target != null)
                //    UnityEngine.Debug.LogError($"Target lost - {_target.Type.Class}");
                _target = _bullet.Collider.ActiveTrigger;
                hasTarget = TryCreateTargetJoint();
            }

            if (!hasTarget) return;

            var sqrDistance = (_bullet.Body.Position - _parent.Owner.Body.Position).sqrMagnitude;
            if (sqrDistance > _maxDistance * _maxDistance * _tolerance)
            {
                _targetJoint?.Dispose();
                _parentJoint?.Dispose();
                _bullet.Detonate();
            }
        }

        private bool IsTargetValid()
        {
            if (_target == null) return false;
            if (_target.State != UnitState.Active) return false;
            if (!_target.Collider.Enabled) return false;
            return true;
        }

        private bool TryCreateTargetJoint()
        {
            if (_targetJoint != null)
            {
                _targetJoint.Dispose();
                _targetJoint = null;
                return false;
            }

            if (!IsTargetValid()) return false;
            _targetJoint = _bullet.Physics.CreateFixedJoint(_target.Physics, true);
            return true;
        }

        private IUnit _target;
        private IJoint _parentJoint;
        private IJoint _targetJoint;
        private readonly IBullet _bullet;
        private readonly IWeaponPlatform _parent;
        private readonly float _maxDistance;
        private const float _tolerance = 1.5f;
    }
}
