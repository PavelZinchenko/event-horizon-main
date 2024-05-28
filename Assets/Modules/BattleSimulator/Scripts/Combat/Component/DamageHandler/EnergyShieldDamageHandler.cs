using Combat.Collision;
using Combat.Component.Unit;
using Combat.Unit;
using Combat.Unit.Auxiliary;

namespace Combat.Component.DamageHandler
{
    public class EnergyShieldDamageHandler : IDamageHandler
    {
        public EnergyShieldDamageHandler(IAuxiliaryUnit shield, float energyConsumption)
        {
            _shield = shield;
            _energyConsumption = energyConsumption;
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

            var damage = impact.GetTotalDamage(Resistance.Empty);
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

        public void RemoveDamage(ref Impact impact, float totalAbsorbed)
        {
            if (impact.ShieldDamage >= totalAbsorbed)
            {
                impact.ShieldDamage -= totalAbsorbed;
                return;
            }

            totalAbsorbed -= impact.ShieldDamage;
            impact.ShieldDamage = 0;

            var total = impact.KineticDamage + impact.EnergyDamage + impact.HeatDamage + impact.DirectDamage;
            if (total <= totalAbsorbed || total <= 0.000001f)
            {
                impact.RemoveDamage();
                return;
            }

            impact.KineticDamage -= totalAbsorbed * impact.KineticDamage / total;
            impact.EnergyDamage -= totalAbsorbed * impact.EnergyDamage / total;
            impact.HeatDamage -= totalAbsorbed * impact.HeatDamage / total;
            impact.DirectDamage -= totalAbsorbed * impact.DirectDamage / total;
        }

        private readonly IAuxiliaryUnit _shield;
        private readonly float _energyConsumption;
    }
}
