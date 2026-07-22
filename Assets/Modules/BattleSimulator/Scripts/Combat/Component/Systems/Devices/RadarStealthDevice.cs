using Combat.Component.Ship;
using Combat.Component.Ship.Effects;
using Combat.Component.Triggers;
using GameDatabase.DataModel;

namespace Combat.Component.Systems.Devices
{
    public sealed class RadarStealthDevice : SystemBase, IDevice
    {
        public RadarStealthDevice(IShip ship, DeviceStats deviceSpec, int keyBinding)
            : base(keyBinding, deviceSpec.ControlButtonIcon)
        {
            DeviceClass = deviceSpec.DeviceClass;
            MaxCooldown = deviceSpec.Cooldown;
            _ship = ship;
            _energyCost = deviceSpec.EnergyConsumption;
            _lifetime = deviceSpec.Lifetime;
        }

        public GameDatabase.Enums.DeviceClass DeviceClass { get; }
        public override float ActivationCost => _energyCost;
        public override bool CanBeActivated => base.CanBeActivated && _ship.Stats.Energy.Value >= _energyCost;

        public void Deactivate() { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (Active && !_wasPressed && CanBeActivated && _ship.Stats.Energy.TryGet(_energyCost))
            {
                RadarStatus.ApplyStealth(_ship, _lifetime);
                TimeFromLastUse = 0f;
                InvokeTriggers(ConditionType.OnActivate);
            }

            _wasPressed = Active;
        }

        protected override void OnUpdateView(float elapsedTime) { }
        protected override void OnDispose() { }

        private bool _wasPressed;
        private readonly IShip _ship;
        private readonly float _energyCost;
        private readonly float _lifetime;
    }
}
