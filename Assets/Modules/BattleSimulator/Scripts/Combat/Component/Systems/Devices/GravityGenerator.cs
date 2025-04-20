using Combat.Component.Ship;
using Combat.Component.Triggers;
using Combat.Factory;
using GameDatabase.DataModel;

namespace Combat.Component.Systems.Devices
{
    public class GravityGenerator : SystemBase, IDevice
    {
        public GravityGenerator(IShip ship, SpaceObjectFactory factory, DeviceStats deviceSpec, int keyBinding)
            : base(keyBinding, deviceSpec.ControlButtonIcon)
        {
			DeviceClass = deviceSpec.DeviceClass;
            MaxCooldown = deviceSpec.Cooldown;

            _ship = ship;
            _factory = factory;
            _radius = deviceSpec.Range;
            _power = deviceSpec.Power;
            _energyCost = deviceSpec.EnergyConsumption;
        }

        public GameDatabase.Enums.DeviceClass DeviceClass { get; }
		public override float ActivationCost { get { return _energyCost; } }
        public override bool CanBeActivated { get { return base.CanBeActivated && _ship.Stats.Energy.Value >= _energyCost; } }

        public void Deactivate() { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (Active && CanBeActivated && _ship.Stats.Energy.TryGet(_energyCost))
            {
                InvokeTriggers(ConditionType.OnActivate);
                TimeFromLastUse = 0;
                _factory.CreateGravityBomb(_ship, _radius, 0.5f, _power);
                InvokeTriggers(ConditionType.OnDeactivate);
            }
        }

        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnDispose() { }

        private bool _isEnabled;
        private readonly float _energyCost;
        private readonly float _radius;
        private readonly float _power;
        private readonly IShip _ship;
        private readonly SpaceObjectFactory _factory;
    }
}
