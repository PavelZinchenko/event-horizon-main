using Combat.Collision.Manager;
using Combat.Component.Unit;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class DealFadingDamageAction : ICollisionAction
    {
        public DealFadingDamageAction(DamageType damageType, float damage)
        {
            _damageType = damageType;
            _damage = damage;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            targetImpact.AddDamage(_damageType, _damage * collisionData.TimeInterval);
        }

        public void Dispose() { }

        private readonly float _damage;
        private readonly DamageType _damageType;
    }
}
