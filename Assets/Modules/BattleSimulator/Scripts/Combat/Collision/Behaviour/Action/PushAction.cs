using Combat.Collision.Manager;
using Combat.Component.Body;
using Combat.Component.Unit;
using GameDatabase.Enums;
using UnityEngine;

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
                var impulse = ClampImpulse(self.Body.Velocity * _impulse * collisionData.TimeInterval, target.Body.Weight);
                targetImpact.AddCappedImpulse(target.Body.WorldPosition(), impulse, MaxImpulse(target.Body.Weight));
            }
            else
            {
                if (!collisionData.IsNew || !_isAlive)
                    return;

                var impulse = ClampImpulse(self.Body.Velocity * _impulse, target.Body.Weight);
                targetImpact.AddCappedImpulse(target.Body.WorldPosition(), impulse, MaxImpulse(target.Body.Weight));

                _isAlive = _impactType == BulletImpactType.HitAllTargets;
            }
        }

        private static Vector2 ClampImpulse(Vector2 impulse, float targetWeight)
        {
            var maxDeltaV = 8f;
            var limit = maxDeltaV * Mathf.Max(targetWeight, 0.001f);
            if (impulse.sqrMagnitude > limit * limit)
                return impulse.normalized * limit;
            return impulse;
        }

        private static float MaxImpulse(float targetWeight)
        {
            return 24f * Mathf.Max(targetWeight, 0.001f);
        }

        public void Dispose() { }

        private bool _isAlive = true;
        private readonly float _impulse;
        private readonly BulletImpactType _impactType;
    }
}
