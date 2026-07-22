using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Unit;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class PushAction : ICollisionAction
    {
        public PushAction(float impulse, BulletImpactType impactType)
        {
            _impactType = impactType;
            _impulse = impulse;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (_impactType == BulletImpactType.DamageOverTime)
            {
                var impulse = self.Body.Velocity * _impulse * collisionData.TimeInterval;
                targetImpact.AddImpulse(target.Body.WorldPosition(), impulse);
            }
            else
            {
                if (!collisionData.IsNew || !_isAlive)
                    return;

                var impulse = self.Body.Velocity * _impulse;
                targetImpact.AddImpulse(target.Body.WorldPosition(), impulse);

                _isAlive = _impactType == BulletImpactType.HitAllTargets;
            }
        }

        public void Dispose() { }

        private bool _isAlive = true;
        private readonly float _impulse;
        private readonly BulletImpactType _impactType;
    }
}
