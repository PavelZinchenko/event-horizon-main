using Combat.Collision.Manager;
using Combat.Component.Ship;
using Combat.Component.Unit;
using GameDatabase.Enums;

namespace Combat.Collision.Behaviour.Action
{
    public class DealDamageAction : ICollisionAction
    {
        public DealDamageAction(DamageType damageType, float damage, BulletImpactType impactType,
            bool ignoreDefenseBonus = false, bool selfSharpening = false, bool piercingBeam = false)
        {
            _ignoreDefenseBonus = ignoreDefenseBonus;
            _impactType = impactType;
            _damageType = damageType;
            _damage = damage;
            _selfSharpening = selfSharpening;
            _piercingBeam = piercingBeam;
        }

        public void Invoke(IUnit self, IUnit target, CollisionData collisionData, ref Impact selfImpact, ref Impact targetImpact)
        {
            // Lingering area hazards affect ships, not weapon projectiles.
            // Otherwise a missile's persistent cloud can destroy unrelated
            // electromagnetic-cannon shots while they are in flight.
            if (_impactType == BulletImpactType.DamageOverTime &&
                self.Type.Class == Combat.Component.Unit.Classification.UnitClass.AreaOfEffect &&
                target.Type.Class != Combat.Component.Unit.Classification.UnitClass.Ship &&
                target.Type.Class != Combat.Component.Unit.Classification.UnitClass.Drone)
                return;

            // Dimensional damage is absolute: it ignores defense multipliers
            // in addition to shield and resistance handling in ShipStats.
            var damage = _damageType == DamageType.True
                ? _damage
                : (_ignoreDefenseBonus ? _damage * target.DefenseMultiplier : _damage);

            if (_selfSharpening && target is IShip ship)
            {
                var resistance = GetResistance(ship, _damageType);
                damage *= 1f - UnityEngine.Mathf.Max(0f, UnityEngine.Mathf.Min(1f, resistance) - 0.7f);
                targetImpact.SetResistanceCap(_damageType, 0.3f);
            }

            if (_impactType == BulletImpactType.DamageOverTime)
            {
                targetImpact.AddDamage(_damageType, damage * collisionData.TimeInterval);
            }
            else
            {
                if (!collisionData.IsNew || !_isAlive)
                    return;

                if (_piercingBeam)
                {
                    // Four targets maximum: 80%, 60%, 40%, then 20%.
                    damage *= UnityEngine.Mathf.Max(0.2f, 0.8f - 0.2f * _piercingHitCount);
                    ++_piercingHitCount;
                }

                targetImpact.AddDamage(_damageType, damage);
                // An ordinary beam normally completes after its first hit.
                // The refitted piercing beam deliberately stays alive long
                // enough to damage four different targets instead.
                _isAlive = _piercingBeam
                    ? _piercingHitCount < 4
                    : _impactType == BulletImpactType.HitAllTargets;
            }
        }

        public void Dispose() {}

        private bool _isAlive = true;
        private readonly float _damage;
        private readonly BulletImpactType _impactType;
        private readonly DamageType _damageType;
        private readonly bool _ignoreDefenseBonus;
        private readonly bool _selfSharpening;
        private readonly bool _piercingBeam;
        private int _piercingHitCount;

        private static float GetResistance(IShip ship, DamageType type)
        {
            var resistance = ship.Stats.Resistance;
            switch (type)
            {
                case DamageType.Impact: return resistance.Kinetic;
                case DamageType.Energy: return resistance.Energy;
                case DamageType.Heat: return resistance.Heat;
                case DamageType.Corrosive: return resistance.Corrosive;
                default: return 0f;
            }
        }
    }
}
