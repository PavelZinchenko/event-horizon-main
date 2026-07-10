using Combat.Collision.Manager;
using Combat.Component.Unit;

namespace Combat.Collision.Behaviour.Action
{
    public sealed class KineticPenetrationAction : ICollisionAction
    {
        public KineticPenetrationAction(float penetration) => _penetration = penetration;

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData,
            ref Impact selfImpact, ref Impact targetImpact)
        {
            if (collisionData.IsNew)
                targetImpact.KineticResistancePenetration =
                    UnityEngine.Mathf.Max(targetImpact.KineticResistancePenetration, _penetration);
        }

        public void Dispose() { }
        private readonly float _penetration;
    }
}
