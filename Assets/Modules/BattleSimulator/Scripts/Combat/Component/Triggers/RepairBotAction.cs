using Combat.Component.Ship;
using Combat.Component.Systems;
using Combat.Factory;
using Combat.Unit;
using Combat.Unit.Auxiliary;
using GameDatabase.Model;
using UnityEngine;

namespace Combat.Component.Triggers
{
    public class RepairBotAction : IUnitAction
    {
        public RepairBotAction(
            IShip ship,
            ISystem device,
            SatelliteFactory factory,
            float repairRate, 
            float deviceSize, 
            float flightRadius,
            float hitPoints,
            float cooldown,
            Color color, 
            AudioClipId activationSound)
        {
            _device = device;
            _factory = factory;
            _ship = ship;
            _deviceSize = deviceSize;
            _flightRadius = flightRadius;
            _color = color;
            _hitPoints = hitPoints;
            _repairRate = repairRate;
            _activationSound = activationSound;
            _cooldown = cooldown;
            _timeLeft = cooldown;
        }

        public ConditionType TriggerCondition { get { return ConditionType.OnActivate | ConditionType.OnDeactivate; } }

        public bool TryUpdateAction(float elapsedTime)
        {
            if (!_isActive && Bot.State == UnitState.Inactive)
            {
                return false;
            }

            if (Bot.State == UnitState.Inactive)
            {
                CreateBot();
            }
            else if (Bot.State == UnitState.Destroyed)
            {
                _device.Enabled = false;
                _timeLeft -= elapsedTime;
                if (_timeLeft > 0)
                    return true;

                CreateBot();
                _timeLeft = _cooldown;
            }

            return true;
        }

        private void CreateBot()
        {
            _repairBot = _factory.CreateRepairBot(_ship, _repairRate, _deviceSize,
                _flightRadius, _deviceSize, _hitPoints, _color, _activationSound);
            
            _repairBot.Enabled = _isActive;
            _device.Enabled = true;
        }

        public bool TryInvokeAction(ConditionType condition)
        {
            if (condition.Contains(ConditionType.OnDeactivate))
            {
                _isActive = false;
                Bot.Enabled = false;
            }
            else if (condition.Contains(ConditionType.OnActivate))
            {
                _isActive = true;
                Bot.Enabled = true;
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (_repairBot.IsActive())
                _repairBot.Vanish();
        }

        private IAuxiliaryUnit Bot
        {
            get
            {
                if (_repairBot == null) CreateBot();
                return _repairBot;
            }
        }

        private bool _isActive;
        private float _timeLeft;
        private readonly float _cooldown;
        private IAuxiliaryUnit _repairBot;
        private readonly ISystem _device;
        private readonly float _repairRate;
        private readonly Color _color;
        private readonly AudioClipId _activationSound;
        private readonly float _hitPoints;
        private readonly float _deviceSize;
        private readonly float _flightRadius;
        private readonly IShip _ship;
        private readonly SatelliteFactory _factory;
    }
}
