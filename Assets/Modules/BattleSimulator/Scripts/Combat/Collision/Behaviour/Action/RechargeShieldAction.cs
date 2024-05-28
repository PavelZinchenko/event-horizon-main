using Combat.Collision.Manager;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class RechargeShieldAction : ICollisionAction
    {
        public RechargeShieldAction(float power, BulletImpactType impactType, float damageMultiplier, UnitSide side)
        {
            _power = power;
			_unitSide = side;
			_damageMultiplier = damageMultiplier > 0 ? damageMultiplier : 1;
            _impactType = impactType;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (_impactType == BulletImpactType.DamageOverTime)
            {
				if (_unitSide.IsAlly(target.Type.Side))
					targetImpact.ShieldDamage -= _power * collisionData.TimeInterval;
				else
					targetImpact.ShieldDamage += _power * collisionData.TimeInterval * _damageMultiplier;
			}
			else
            {
                if (!collisionData.IsNew || !_isAlive)
                    return;

				if (_unitSide.IsAlly(target.Type.Side))
					targetImpact.ShieldDamage -= _power;
				else
					targetImpact.ShieldDamage += _power * _damageMultiplier;
                
				_isAlive = _impactType == BulletImpactType.HitAllTargets;
            }
        }

        public void Dispose() { }

        private bool _isAlive = true;
		private readonly UnitSide _unitSide;
		private readonly float _damageMultiplier;
        private readonly float _power;
        private readonly BulletImpactType _impactType;
    }
}
