using Combat.Component.Ship;
using Combat.Component.Unit;
using Combat.Factory;

namespace Combat.Component.Triggers
{
    public class GravityFieldAction : IUnitAction
    {
        public GravityFieldAction(IShip ship, SpaceObjectFactory factory, float radius, float power)
        {
            _factory = factory;
            _ship = ship;
            _radius = radius;
            _power = power;
        }

        public ConditionType TriggerCondition { get { return ConditionType.OnActivate | ConditionType.OnDeactivate; } }

        public bool TryUpdateAction(float elapsedTime)
        {
            return true;
        }

        public bool TryInvokeAction(ConditionType condition)
        {
            if (condition.Contains(ConditionType.OnDeactivate))
            {
                _unit.Vanish();
                _unit = null;
                return false;
            }

            if (condition.Contains(ConditionType.OnActivate))
            {
                _unit ??= _factory.CreateGravitation(_ship, _radius, 0, _power);
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            _unit?.Vanish();
            _unit = null;
        }

        private IUnit _unit;
        private readonly float _radius;
        private readonly float _power;
        private readonly IShip _ship;
        private readonly SpaceObjectFactory _factory;
    }
}
