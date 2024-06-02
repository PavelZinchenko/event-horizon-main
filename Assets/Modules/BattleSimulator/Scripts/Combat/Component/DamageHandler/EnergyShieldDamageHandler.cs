using Combat.Collision;
using Combat.Component.Unit;
using Combat.Unit;
using Combat.Unit.Auxiliary;

namespace Combat.Component.DamageHandler
{
    public class EnergyShieldDamageHandler : IDamageHandler
    {
        public EnergyShieldDamageHandler(IAuxiliaryUnit shield, float energyConsumption, float corrosiveResistance)
        {
            _shield = shield;
            _energyConsumption = energyConsumption;
            _corrosiveResistance = corrosiveResistance;
        }

        public CollisionEffect ApplyDamage(Impact impact, IUnit source)
        {
            const float energyDrainResist = 0.75f;

            if (impact.IgnoresShield)
            {
                _shield.Enabled = false;
                return CollisionEffect.None;
            }

            impact.ApplyImpulse(_shield.Body);
            impact.RemoveImpulse();

            var parent = _shield.Type.Owner;
            if (parent == null)
                return CollisionEffect.None;

            var damage = impact.GetTotalDamageToShield(_corrosiveResistance);
            impact.EnergyDrain = Resistance.ModifyDamage(impact.EnergyDrain, energyDrainResist);

            var owner = source.GetOwnerShip();
            if (parent.Stats.Energy.TryGet(damage*_energyConsumption))
            {
                impact.RemoveDamage();
                if (damage > 0) owner?.Stats.Performance?.OnDamageShield(damage);
            }
            else
            {
                var energy = parent.Stats.Energy.Value;
                parent.Stats.Energy.Get(energy);
                var absorbed = energy/_energyConsumption;
                owner?.Stats.Performance?.OnDamageShield(absorbed);
                RemoveDamage(ref impact, absorbed);
                _shield.Enabled = false;
            }

            parent.Affect(impact, source);

            return CollisionEffect.None;
        }

        public void Dispose()
        {
        }

        private void RemoveDamage(ref Impact impact, float totalAbsorbed)
        {
            if (impact.ShieldDamage >= totalAbsorbed)
            {
                impact.ShieldDamage -= totalAbsorbed;
                return;
            }

            totalAbsorbed -= impact.ShieldDamage;
            impact.ShieldDamage = 0;

            var corrosiveDamage = Resistance.ModifyDamage(impact.CorrosiveDamage, _corrosiveResistance);
            if (corrosiveDamage >= totalAbsorbed)
            {
                impact.CorrosiveDamage *= (corrosiveDamage - totalAbsorbed)/corrosiveDamage;
                return;
            }

            totalAbsorbed -= corrosiveDamage;
            impact.CorrosiveDamage = 0;

            var normalDamage = impact.KineticDamage + impact.EnergyDamage + impact.HeatDamage;
            if (normalDamage <= 0) return;

            impact.KineticDamage -= totalAbsorbed * impact.KineticDamage / normalDamage;
            impact.EnergyDamage -= totalAbsorbed * impact.EnergyDamage / normalDamage;
            impact.HeatDamage -= totalAbsorbed * impact.HeatDamage / normalDamage;
            impact.CorrosiveDamage -= totalAbsorbed * impact.CorrosiveDamage / normalDamage;
        }

        private readonly IAuxiliaryUnit _shield;
        private readonly float _corrosiveResistance;
        private readonly float _energyConsumption;
    }
}
