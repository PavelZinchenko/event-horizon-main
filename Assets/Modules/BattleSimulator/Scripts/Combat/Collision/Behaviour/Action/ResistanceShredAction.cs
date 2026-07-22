using Combat.Collision.Manager;
using Combat.Component.Ship;
using Combat.Component.Ship.Effects;
using Combat.Component.Unit;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public sealed class ResistanceShredAction : ICollisionAction
    {
        public ResistanceShredAction(DamageType type, float duration, BulletImpactType impactType)
        {
            _type = type;
            _duration = duration;
            _impactType = impactType;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (!collisionData.IsNew || !_alive || target is not IShip ship)
                return;

            ResistanceShred.Apply(ship, _type, _duration);
            _alive = _impactType == BulletImpactType.HitAllTargets;
        }

        public void Dispose() { }

        private bool _alive = true;
        private readonly DamageType _type;
        private readonly float _duration;
        private readonly BulletImpactType _impactType;
    }
}
