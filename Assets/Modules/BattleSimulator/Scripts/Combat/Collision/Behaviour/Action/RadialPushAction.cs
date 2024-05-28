using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Unit;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class RadialPushAction : ICollisionAction
    {
        public RadialPushAction(float impulse, BulletImpactType impactType)
        {
            _impactType = impactType;
            _impulse = impulse;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (_impactType == BulletImpactType.DamageOverTime)
            {
                Push(self, target, ref targetImpact, _impulse * collisionData.TimeInterval);
            }
            else
            {
                if (!collisionData.IsNew || !_isAlive) return;
                Push(self, target, ref targetImpact, _impulse);
                _isAlive = _impactType == BulletImpactType.HitAllTargets;
            }
        }

        private void Push(IUnit self, IUnit target, ref Impact targetImpact, float power)
        {
            var center = self.Body.WorldPosition();
            var position = target.Body.WorldPosition();
            var impulse = (position - center).normalized * power;
            targetImpact.AddImpulse(center, impulse);
        }

        public void Dispose() { }

        private bool _isAlive = true;
        private readonly float _impulse;
        private readonly BulletImpactType _impactType;
    }
}
