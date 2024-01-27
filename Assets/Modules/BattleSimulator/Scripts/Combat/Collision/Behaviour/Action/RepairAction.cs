using Combat.Collision.Manager;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class RepairAction : ICollisionAction
    {
        public RepairAction(float power, BulletImpactType impactType, DamageType damageType, float damageMultiplier)
        {
            _power = power;
			_damageMultiplier = damageMultiplier > 0 ? damageMultiplier : 1;
			_damageType = damageType;
            _impactType = impactType;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            if (_impactType == BulletImpactType.DamageOverTime)
            {
				if (self.Type.Side.IsAlly(target.Type.Side))
	                targetImpact.Repair += _power * collisionData.TimeInterval;
				else
					targetImpact.AddDamage(_damageType, _power * collisionData.TimeInterval * _damageMultiplier);
			}
			else
            {
                if (!collisionData.IsNew || !_isAlive)
                    return;

				if (self.Type.Side.IsAlly(target.Type.Side))
					targetImpact.Repair += _power;
				else
					targetImpact.AddDamage(_damageType, _power * _damageMultiplier);
                
				_isAlive = _impactType == BulletImpactType.HitAllTargets;
            }
        }

        public void Dispose() { }

        private bool _isAlive = true;
		private readonly float _damageMultiplier;
        private readonly float _power;
		private readonly DamageType _damageType;
        private readonly BulletImpactType _impactType;
    }
}
