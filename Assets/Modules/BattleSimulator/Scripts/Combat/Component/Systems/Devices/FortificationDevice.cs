using Combat.Collision;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Stats;
using Combat.Component.Triggers;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Systems.Devices
{
    public class FortificationDevice : SystemBase, IDevice, IFeaturesModification, IStatsModification
    {
        public FortificationDevice(IShip ship, DeviceStats deviceSpec, int keyBinding)
            : base(keyBinding, deviceSpec.ControlButtonIcon)
        {
			DeviceClass = deviceSpec.DeviceClass;
            MaxCooldown = deviceSpec.Cooldown;

            _ship = ship;
            _activeColor = deviceSpec.Color;
            _energyCost = deviceSpec.EnergyConsumption;

            _power = deviceSpec.Power / 100;
            if (_power == 0)
            {
                _power = 0.5f;
            }
        }

		public GameDatabase.Enums.DeviceClass DeviceClass { get; }
		public override bool CanBeActivated { get { return base.CanBeActivated && (_isEnabled || _ship.Stats.Energy.Value >= _energyCost); } }

        public override IStatsModification StatsModification { get { return this; } }
        public bool TryApplyModification(ref Resistance data)
        {
            if (_isEnabled)
            {
                // We don't want to get into weird territory with the original
                // resistances, so in case if the fortification device has
                // power over 1, the original ship resistances are just
                // ignored, since the multiplier is clamped to 0
                var resMult = Mathf.Clamp01(1 - _power);
                
                data.Energy = _power + data.Energy * resMult;
                data.Heat = _power + data.Heat * resMult;
                data.Kinetic = _power + data.Kinetic * resMult;
            }

            return true;
        }

        public override IFeaturesModification FeaturesModification { get { return this; } }
        public bool TryApplyModification(ref FeaturesData data)
        {
            data.Color *= _color;
            return true;
        }

        public void Deactivate()
        {
            if (!_isEnabled)
                return;

            _isEnabled = false;
            TimeFromLastUse = 0;
            InvokeTriggers(ConditionType.OnDeactivate);            
        }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (Active && CanBeActivated && _ship.Stats.Energy.TryGet(_energyCost*elapsedTime))
            {
                if (!_isEnabled)
                {
                    InvokeTriggers(ConditionType.OnActivate);
                    _isEnabled = true;
                }
            }
            else if (_isEnabled)
            {
                Deactivate();
            }
        }

        protected override void OnUpdateView(float elapsedTime)
        {
            var color = _isEnabled ? _activeColor : Color.white;
            _color = Color.Lerp(_color, color, 5 * elapsedTime);
        }

        protected override void OnDispose() { }

        private bool _isEnabled;
        private Color _color = Color.white;
        private readonly Color _activeColor;
        private readonly float _energyCost;
        private readonly float _power;
        private readonly IShip _ship;
    }
}
