using Combat.Collision.Manager;
using Combat.Component.Bullet.Lifetime;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class RestoreLifeAction : ICollisionAction
    {
        public RestoreLifeAction(ILifetime bulletLifetime, float power, BulletImpactType impactType)
        {
            _bulletLifetime = bulletLifetime;
            _impactType = impactType;
            _power = power;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact,
            ref Impact targetImpact)
        {
            float value;
            if (_impactType == BulletImpactType.DamageOverTime)
            {
                value = _power * collisionData.TimeInterval;
            }
            else
            {
                if (!collisionData.IsNew || !_isAlive)
                    return;

                value = _power;
                _isAlive = _impactType == BulletImpactType.HitAllTargets;
            }

            if (target.Type.Class == UnitClass.Ship || target.Type.Class == UnitClass.Drone)
            {
                _bulletLifetime.Restore(value);
            }
        }

        public void Dispose() { }

        private ILifetime _bulletLifetime;
        private bool _isAlive = true;
        private readonly float _power;
        private readonly BulletImpactType _impactType;
    }
}
