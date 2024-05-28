using Combat.Collision;
using Combat.Component.Unit;
using Combat.Factory;

namespace Combat.Component.Bullet.Action
{
    public class CreateGravitationAction : IAction
    {
        public CreateGravitationAction(IUnit unit, SpaceObjectFactory factory, float radius, float power, float lifetime, bool frienlyFire, ConditionType condition = ConditionType.None)
        {
            _factory = factory;
            _unit = unit;
            _power = power;
            _radius = radius;
            _lifetime = lifetime;
            _frienlyFire = frienlyFire;
            Condition = condition;
        }

        public CreateGravitationAction(IUnit unit, SpaceObjectFactory factory, float radius, float power, bool friendlyFire, ConditionType condition = ConditionType.None)
            : this(unit, factory, radius, power, 0f, friendlyFire, condition)
        {
        }

        public ConditionType Condition { get; private set; }

        public void Dispose() { }

        public CollisionEffect Invoke()
        {
            _factory.CreateGravitation(_unit, _radius, _lifetime, _power/*, _frienlyFire*/);
            return CollisionEffect.None;
        }

        private readonly bool _frienlyFire;
        private readonly float _power;
        private readonly float _radius;
        private readonly float _lifetime;
        private readonly IUnit _unit;
        private readonly SpaceObjectFactory _factory;
    }
}
