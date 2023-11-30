using Combat.Collision.Manager;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;

namespace Combat.Collision.Behaviour.Action
{
    public class TeleportAction : ICollisionAction
    {
        private readonly float _power;

        public TeleportAction(float power)
        {
            _power = power;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            var range = _power;// / Mathf.Sqrt(Mathf.Sqrt(target.Body.Weight));

            if (!collisionData.IsNew) return;

            if (target.Type.Class == UnitClass.Ship)
            {
                var ship = target as IShip;
                if (ship.Specification.Stats.ShipModel.SizeClass == GameDatabase.Enums.SizeClass.Starbase) return;
            }

            if (target.Type.Class == UnitClass.Ship /*|| target.Type.Class == UnitClass.SpaceObject*/ ||
                target.Type.Class == UnitClass.Shield || target.Type.Class == UnitClass.Limb)
            {
                target.Body.Move(target.Body.Position + RotationHelpers.Direction(target.Body.Rotation) * range);
                target.Body.ApplyAcceleration(-target.Body.Velocity);
            }
        }

        public void Dispose() { }
    }
}
