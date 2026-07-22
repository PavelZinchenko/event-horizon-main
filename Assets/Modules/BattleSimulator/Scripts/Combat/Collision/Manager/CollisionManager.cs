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
            // Unity can report the same projectile contact from either
            // collider first.  When the macro-electron is reported first,
            // process the incoming projectile as the attacker so its actual
            // weapon damage reaches BallLightningDamageHandler.
            if (IsBallLightning(first) && IsProjectile(second) && !IsBallLightning(second))
            {
                ProcessProjectileHit(second, first, collisionData);
                return;
            }
            if (IsBallLightning(second) && IsProjectile(first) && !IsBallLightning(first))
            {
                // Ray-cast laser bullets are normally reported as the first
                // collider. Route them through the same explicit projectile
                // path as physical rounds so allied laser damage can charge a
                // macro-electron instead of being discarded as friendly fire.
                ProcessProjectileHit(first, second, collisionData);
                return;
            }

            var behaviour = first.CollisionBehaviour;
            if (behaviour == null)
                return;

            if (!first.IsActive() || !second.IsActive())
                return;
            if (CombatRelations.AreAllies(first.Type, second.Type) &&
                !first.Type.CanHitAllies && !second.Type.CanHitAllies &&
                !IsBallLightningInteraction(first, second))
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

        private static bool IsBallLightningInteraction(IUnit first, IUnit second)
        {
            return IsBallLightning(first) && IsProjectile(second) ||
                   IsBallLightning(second) && IsProjectile(first);
        }

        private static bool IsBallLightning(IUnit unit)
        {
            return unit is Combat.Component.Bullet.Bullet bullet &&
                   bullet.Controller is Combat.Component.Controller.BallLightningController;
        }

        private static bool IsProjectile(IUnit unit)
        {
            return unit is Combat.Component.Bullet.Bullet;
        }

        private static void ProcessProjectileHit(IUnit projectile, IUnit target, CollisionData collisionData)
        {
            if (!projectile.IsActive() || !target.IsActive())
                return;

            var behaviour = projectile.CollisionBehaviour;
            if (behaviour == null)
                return;

            var projectileImpact = new Impact();
            var targetImpact = new Impact();
            behaviour.Process(projectile, target, collisionData, ref projectileImpact, ref targetImpact);
            projectile.OnCollision(projectileImpact, target, collisionData);
            target.OnCollision(targetImpact, projectile, collisionData);
        }
    }
}
