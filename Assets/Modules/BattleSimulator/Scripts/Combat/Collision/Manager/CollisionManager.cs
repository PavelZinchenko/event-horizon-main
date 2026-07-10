using Combat.Component.Unit;
using Combat.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.Ship;
using Combat.Unit.Object;

namespace Combat.Collision.Manager
{
    public class CollisionManager : ICollisionManager
    {
        public void OnCollision(IUnit first, IUnit second, CollisionData collision)
        {
            ProcessCollision(first, second, collision);
        }

        //public void OnCollisionExit(IUnit first, IUnit second, CollisionData collision)
        //{
        //}

        //public void OnCollisionStay(IUnit first, IUnit second, CollisionData collision)
        //{
        //    ProcessCollision(first, second, collision);
        //}

        private void ProcessCollision(IUnit first, IUnit second, CollisionData collisionData)
        {
            var behaviour = first.CollisionBehaviour;
            if (behaviour == null)
                return;

            if (!first.IsActive() || !second.IsActive())
                return;
            if (CombatRelations.AreAllies(first.Type, second.Type) &&
                !first.Type.CanHitAllies && !second.Type.CanHitAllies)
                return;

            // Waterdrop interactions have to be resolved before the incoming
            // laser bullet processes its ordinary hit/destroy behaviour.
            if (first is Ship firstShip && firstShip.TryHandleWaterdropCollision(second, collisionData))
                return;
            if (second is Ship secondShip && secondShip.TryHandleWaterdropCollision(first, collisionData))
                return;

            var selfImpact = new Impact();
            var targetImpact = new Impact();

            behaviour.Process(first, second, collisionData, ref selfImpact, ref targetImpact);

            first.OnCollision(selfImpact, second, collisionData);
            second.OnCollision(targetImpact, first, collisionData);
        }
    }
}
