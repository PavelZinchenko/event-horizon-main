using Combat.Collision.Manager;
using Combat.Component.Unit;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class DamageShieldAction : ICollisionAction
    {
        private readonly BulletImpactType _impactType;

        public DamageShieldAction(BulletImpactType impactType, float damage)
        {
            _impactType = impactType;
            _damage = damage;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (_impactType == BulletImpactType.DamageOverTime)
            {
                targetImpact.ShieldDamage += _damage * collisionData.TimeInterval;
            }
            else
            {
                if (!collisionData.IsNew || !_isAlive)
                    return;

                targetImpact.ShieldDamage += _damage;
                _isAlive = _impactType == BulletImpactType.HitAllTargets;
            }
        }

        public void Dispose() { }

        private bool _isAlive = true;
        private readonly float _damage;
    }
}
