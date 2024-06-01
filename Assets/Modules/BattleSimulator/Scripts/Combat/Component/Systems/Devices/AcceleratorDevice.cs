﻿using Combat.Component.Engine;
using Combat.Component.Ship;
using Combat.Component.Triggers;
using GameDatabase.DataModel;

namespace Combat.Component.Systems.Devices
{
    public class AcceleratorDevice : SystemBase, IDevice, IEngineModification
    {
        public AcceleratorDevice(IShip ship, DeviceStats deviceSpec, int keyBinding)
            : base(keyBinding, deviceSpec.ControlButtonIcon)
        {
            MaxCooldown = deviceSpec.Cooldown;
			DeviceClass = deviceSpec.DeviceClass;

            _ship = ship;
            _power = deviceSpec.Power;

            _energyCost = UnityEngine.Mathf.Max(1f, deviceSpec.EnergyConsumption * _ship.Specification.Stats.EngineEnergyConsumption);
        }

        public override IEngineModification EngineModification { get { return this; } }
		public GameDatabase.Enums.DeviceClass DeviceClass { get; }

        public bool TryApplyModification(ref EngineData data)
        {
            if (_isEnabled)
            {
                data.Throttle = 1.0f;
                data.Propulsion *= _power;
                data.Velocity *= _power;
            }

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
            if (Active && CanBeActivated && _ship.Stats.Energy.TryGet(_energyCost * elapsedTime))
            {
                if (!_isEnabled)
                {
                    InvokeTriggers(ConditionType.OnActivate);
                    _isEnabled = true;
                }
                else
                {
                    InvokeTriggers(ConditionType.OnRemainActive);
                }
            }
            else if (_isEnabled)
            {
                Deactivate();
            }
        }

        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnDispose() { }

        private bool _isEnabled;
        private readonly float _energyCost;
        private readonly float _power;
        private readonly IShip _ship;
    }
}