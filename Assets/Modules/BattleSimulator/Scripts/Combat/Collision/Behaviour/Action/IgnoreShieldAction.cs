using Combat.Collision.Manager;
using Combat.Component.Unit;

namespace Combat.Collision.Behaviour.Action
{
    public class IgnoreShieldAction : ICollisionAction
    {
        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            targetImpact.IgnoresShield = true;
        }

        public void Dispose() { }
    }
}
