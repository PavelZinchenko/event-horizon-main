using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Unit;
using GameDatabase.Enums;
using UnityEngine;

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
            var direction = position - center;
            var distance = direction.magnitude;
            if (distance < 0.01f)
                return;
            var weight = Mathf.Max(target.Body.Weight, 0.001f);
            var impulse = ClampImpulse(direction / distance * power, weight);
            targetImpact.AddCappedImpulse(center, impulse, 24f * weight);
        }

        private static Vector2 ClampImpulse(Vector2 impulse, float targetWeight)
        {
            var limit = 8f * targetWeight;
            if (impulse.sqrMagnitude > limit * limit)
                return impulse.normalized * limit;
            return impulse;
        }

        public void Dispose() { }

        private bool _isAlive = true;
        private readonly float _impulse;
        private readonly BulletImpactType _impactType;
    }
}
