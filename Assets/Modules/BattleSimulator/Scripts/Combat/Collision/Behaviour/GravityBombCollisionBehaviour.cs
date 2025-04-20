using Combat.Collision.Manager;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using UnityEngine;

namespace Combat.Collision.Behaviour
{
    public class GravityBombCollisionBehaviour : ICollisionBehaviour
    {
        public GravityBombCollisionBehaviour(float power)
        {
            _power = power;
        }

        public void Process(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (!collisionData.IsNew) return;

            switch (target.Type.Class)
            {
                case UnitClass.Missile:
                case UnitClass.EnergyBolt:
                case UnitClass.AreaOfEffect:
                    targetImpact.Effects |= CollisionEffect.Destroy;
                    break;
            }

            Push(self, target, ref targetImpact, _power * Mathf.Sqrt(target.Body.Weight));
        }

        private void Push(IUnit self, IUnit target, ref Impact targetImpact, float power)
        {
            var center = self.Body.WorldPosition();
            var position = target.Body.WorldPosition();
            var offset = Random.insideUnitCircle * target.Body.Scale * _randomShift;
            var impulse = (position - center).normalized * power;
            targetImpact.AddImpulse(position + offset, impulse);
        }

        public void Dispose()
        {
        }

        private readonly float _power;
        private const float _randomShift = 0.25f;
    }
}
