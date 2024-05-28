using Combat.Collision;
using Combat.Component.Unit;
using Combat.Unit;

namespace Combat.Component.DamageHandler
{
    public class BulletDamageHandler : IDamageHandler
    {
        public BulletDamageHandler(IUnit unit, bool canRepair)
        {
            _unit = unit;
            _canRepair = canRepair;
        }

        public CollisionEffect ApplyDamage(Impact impact, IUnit source)
        {
            if (_canRepair && impact.Repair > 0)
                if (_unit.Type.Owner.IsActive())
                    _unit.Type.Owner.Affect(new Impact { Repair = impact.Repair }, null);

            return CollisionEffect.None;
        }

        public void Dispose() {}

        private readonly bool _canRepair;
        private readonly IUnit _unit;
    }
}
