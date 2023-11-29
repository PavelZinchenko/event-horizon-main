using Combat.Collision;
using Combat.Component.Unit;
using Combat.Unit;

namespace Combat.Component.DamageHandler
{
    public class DefaultDamageHandler : IDamageHandler
    {
        public DefaultDamageHandler(IUnit unit, bool canRepair)
        {
            _unit = unit;
            _canRepair = canRepair;
        }

        public CollisionEffect ApplyDamage(Impact impact)
        {
            impact.ApplyImpulse(_unit.Body);

            if (_canRepair && impact.Repair > 0)
                if (_unit.Type.Owner.IsActive())
                    _unit.Type.Owner.Affect(new Impact { Repair = impact.Repair });

            return CollisionEffect.None;
        }

        public void Dispose() {}

        private readonly bool _canRepair;
        private readonly IUnit _unit;
    }
}
