using Combat.Collision.Manager;
using Combat.Component.Controller;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Unit;

namespace Combat.Collision.Behaviour.Action
{
    public sealed class BallLightningCollisionAction : ICollisionAction
    {
        public void Invoke(IUnit self, IUnit target, CollisionData collisionData,
            ref Impact selfImpact, ref Impact targetImpact)
        {
            if (!collisionData.IsNew || target == null || !target.IsActive())
                return;
            if (self is Combat.Component.Bullet.Bullet bullet &&
                bullet.Controller is BallLightningController controller &&
                target != bullet.Type.Owner)
                controller.Arm();
        }

        public void Dispose() { }
    }
}
