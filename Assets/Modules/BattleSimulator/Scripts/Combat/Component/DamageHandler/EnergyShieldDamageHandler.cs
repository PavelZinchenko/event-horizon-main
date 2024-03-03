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
            impact.ApplyImpulse(_shield.Body);
            impact.RemoveImpulse();

            var parent = _shield.Type.Owner;
            if (parent == null)
                return CollisionEffect.None;

            var damage = impact.GetTotalDamage(Resistance.Empty);
            var owner = source.GetOwnerShip();

            if (parent.Stats.Energy.TryGet(damage*_energyConsumption))
            {
                impact.RemoveDamage();
                owner?.Stats.Performance.OnDamageShield(damage);
            }
            else
            {
                var energy = parent.Stats.Energy.Value;
                parent.Stats.Energy.Get(energy);
                var absorbed = energy/_energyConsumption;
                owner?.Stats.Performance.OnDamageShield(absorbed);
                impact.RemoveDamage(absorbed, Resistance.Empty);
                _shield.Enabled = false;
            }

            parent.Affect(impact, source);

            return CollisionEffect.None;
        }

        public void Dispose()
        {
        }

        private readonly IAuxiliaryUnit _shield;
        private readonly float _energyConsumption;
    }
}
