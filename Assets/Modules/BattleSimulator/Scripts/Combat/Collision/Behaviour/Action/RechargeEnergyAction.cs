using Combat.Collision.Manager;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class RechargeEnergyAction : ICollisionAction
    {
        public RechargeEnergyAction(float power, BulletImpactType impactType, float drainMultiplier, UnitSide side)
        {
            _power = power;
            _unitSide = side;
            _drainMultiplier = drainMultiplier > 0 ? drainMultiplier : 1;
            _impactType = impactType;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (_impactType == BulletImpactType.DamageOverTime)
            {
                if (_unitSide.IsAlly(target.Type.Side))
                    targetImpact.EnergyDrain -= _power * collisionData.TimeInterval;
                else
                    targetImpact.EnergyDrain += _power * collisionData.TimeInterval * _drainMultiplier;
            }
            else
            {
                if (!collisionData.IsNew || !_isAlive)
                    return;

                if (_unitSide.IsAlly(target.Type.Side))
                    targetImpact.EnergyDrain -= _power;
                else
                    targetImpact.EnergyDrain += _power * _drainMultiplier;

                _isAlive = _impactType == BulletImpactType.HitAllTargets;
            }
        }

        public void Dispose() { }

        private bool _isAlive = true;
        private readonly UnitSide _unitSide;
        private readonly float _drainMultiplier;
        private readonly float _power;
        private readonly BulletImpactType _impactType;
    }
}
