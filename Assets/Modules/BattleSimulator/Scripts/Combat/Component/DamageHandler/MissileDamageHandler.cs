using Combat.Collision;
using Combat.Component.Unit;

namespace Combat.Component.DamageHandler
{
    public class MissileDamageHandler : IDamageHandler
    {
        public MissileDamageHandler(IUnit unit, float hitPoints)
        {
            _unit = unit;
            _hitPoints = hitPoints;
        }

        public CollisionEffect ApplyDamage(Impact impact, IUnit source)
        {
            var damage = impact.GetTotalDamage(Resistance.Empty);

            switch (source.Type.Class) // fix: prevent harpoon from falling apart after collision with high-level ship
            {
                case Unit.Classification.UnitClass.Missile:
                case Unit.Classification.UnitClass.EnergyBolt:
                case Unit.Classification.UnitClass.AreaOfEffect:
                    _hitPoints -= damage;
                    break;
            }
            
            return _hitPoints > 0 ? CollisionEffect.None : CollisionEffect.Destroy;
        }

        public void Dispose() { }

        private float _hitPoints;
        private readonly IUnit _unit;
    }
}
