using Combat.Collision.Manager;
using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class AffectDroneAction : ICollisionAction
    {
        public enum EffectType 
        {
            DriveCrazy,
            Capture,
        }

        public AffectDroneAction(BulletImpactType impactType, EffectType effectType)
        {
            _effectType = effectType;
            _impactType = impactType;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (!_isAlive || !collisionData.IsNew)
                return;

            switch (target.Type.Class)
            {
                case UnitClass.Drone:
                    if (target is IShip ship && ship.Features.ImmuneToEffects) return;
                    break;
                case UnitClass.Missile:
                    break;
                default:
                    return;
            }

            switch (_effectType)
            {
                case EffectType.Capture:
                    target.Type.Owner = self.Type.Owner;
                    break;
                case EffectType.DriveCrazy:
                    target.Type.Owner = null;
                    break;
            }


            _isAlive = _impactType == BulletImpactType.DamageOverTime || _impactType == BulletImpactType.HitAllTargets;
        }

        public void Dispose() {}

        private bool _isAlive = true;
        private readonly EffectType _effectType;
        private readonly BulletImpactType _impactType;
    }
}
