using Combat.Collision.Manager;
using Combat.Component.Unit;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public interface IPowerLevelIndicator
    {
        float PowerLevel { get; }
    }

    public class ProgressiveDamageAction : ICollisionAction, IPowerLevelIndicator
    {
        public ProgressiveDamageAction(DamageType damageType, float damage, float growTime, BulletImpactType impactType)
        {
            _impactType = impactType;
            _damageType = damageType;
            _damage = damage;
            _growTime = growTime;
        }

        public float PowerLevel => _elapsedTime < _growTime ? _elapsedTime / _growTime : 1.0f;

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (_impactType != BulletImpactType.DamageOverTime) return; // this effect can only be used with DOTs for now

            if (collisionData.IsNew)
            {
                _elapsedTime = 0;
                return;
            }

            _elapsedTime += collisionData.TimeInterval;
            var damage = _damage * collisionData.TimeInterval;
            if (_elapsedTime < _growTime)
                damage = damage * _elapsedTime / _growTime;

            targetImpact.AddDamage(_damageType, damage);
        }

        public void Dispose() {}

        private float _elapsedTime;
        private readonly float _damage;
        private readonly float _growTime;
        private readonly BulletImpactType _impactType;
        private readonly DamageType _damageType;
    }
}
