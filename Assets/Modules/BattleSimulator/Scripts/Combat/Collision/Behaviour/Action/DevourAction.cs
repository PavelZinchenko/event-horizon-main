using Combat.Collision.Manager;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Unit.Object;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Collision.Behaviour.Action
{
    public class DevourAction : ICollisionAction
    {
        private readonly float _damage;
        private readonly BulletImpactType _impactType;
        private readonly DamageType _damageType;
        private bool _isAlive = true;

        public DevourAction(DamageType damageType, float damage, BulletImpactType impactType)
        {
            _impactType = impactType;
            _damageType = damageType;
            _damage = damage;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (target is Asteroid)
            {
                targetImpact.Effects |= CollisionEffect.Destroy;
            }
            else if (target.Type.Class == UnitClass.Ship)
            {
                var ship = target as IShip;
                if (ship.Specification.Stats.ShipModel.SizeClass == GameDatabase.Enums.SizeClass.Starbase)
                {
                    selfImpact.Effects |= CollisionEffect.Destroy;
                    return;
                }

                AddDamage(ref targetImpact, collisionData);
            }
            else if (target.Type.Class == UnitClass.SpaceObject || target.Type.Class == UnitClass.Shield || target.Type.Class == UnitClass.Limb)
            {
                AddDamage(ref targetImpact, collisionData);
            }
            else
            {
                targetImpact.Effects |= CollisionEffect.Destroy;
            }
        }

        private void AddDamage(ref Impact targetImpact, CollisionData collisionData)
        {
            if (_impactType == BulletImpactType.DamageOverTime)
            {
                targetImpact.AddDamage(_damageType, _damage * collisionData.TimeInterval);
            }
            else
            {
                if (!collisionData.IsNew || !_isAlive)
                    return;

                targetImpact.AddDamage(_damageType, _damage);
                _isAlive = _impactType == BulletImpactType.HitAllTargets;
            }
        }

        public void Dispose() { }
    }
}
