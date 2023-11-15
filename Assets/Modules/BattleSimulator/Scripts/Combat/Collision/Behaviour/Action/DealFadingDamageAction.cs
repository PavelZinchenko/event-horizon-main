using Combat.Collision.Manager;
using Combat.Component.Bullet.Lifetime;
using Combat.Component.Unit;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class DealFadingDamageAction : ICollisionAction
    {
        public DealFadingDamageAction(ILifetime lifetime, DamageType damageType, float damage)
        {
            _lifetime = lifetime;
            _damageType = damageType;
            _damage = damage;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            var lifetime = _lifetime.Value + collisionData.TimeInterval * 0.5f;
            var damage = lifetime >= 1f ? _damage : _damage * lifetime;
            targetImpact.AddDamage(_damageType, damage * collisionData.TimeInterval);
        }

        public void Dispose() { }

        private readonly float _damage;
        private readonly DamageType _damageType;
        private readonly ILifetime _lifetime;
    }
}
