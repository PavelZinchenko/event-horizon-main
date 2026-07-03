using Combat.Component.Engine;
using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using Combat.Component.Triggers;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using UnityEngine;
using Combat.Scene;

namespace Combat.Component.Systems.Devices
{
    public class WarpDrive : SystemBase, IDevice, IFeaturesModification, IEngineModification, ISystemsModification
    {
        private const float _minTime = 0.1f;

        private readonly float _energyCost;
        private readonly float _range;
        private readonly IShip _ship;
        private readonly float _speed;
        private readonly bool _controllable;
        private readonly float _totalTime;
        private readonly bool _isStarshipEarthWarpDrive;
        private bool _isEnabled;
        private float _timeLeft;
        private float _activationElapsed;

        public WarpDrive(IShip ship, DeviceStats deviceSpec, int keyBinding, IScene scene, bool isStarshipEarthWarpDrive)
            : base(keyBinding, deviceSpec.ControlButtonIcon)
        {
			DeviceClass = deviceSpec.DeviceClass;
            MaxCooldown = deviceSpec.Cooldown;

            _ship = ship;
            _isStarshipEarthWarpDrive = isStarshipEarthWarpDrive;
            _range = isStarshipEarthWarpDrive ? Mathf.Max(deviceSpec.Range, 100000f) : deviceSpec.Range;
            _speed = isStarshipEarthWarpDrive ? deviceSpec.Power * 10f : deviceSpec.Power;
            _totalTime = _range / _speed;
            _energyCost = deviceSpec.EnergyConsumption;
            _controllable = _ship.Systems.All.FindFirstDevice(DeviceClass.WormTail) < 0;
            _scene = scene;
        }

        public float MaxRange => _range;
        public bool IsWarping => _isEnabled;

        public override IFeaturesModification FeaturesModification => this;
        public bool TryApplyModification(ref FeaturesData data)
        {
            if (_isEnabled)
            {
                data.Opacity = 0.1f;
                data.Invulnerable = true;
            }

            return true;
        }

        public override IEngineModification EngineModification => this;
        public bool TryApplyModification(ref EngineData data)
        {
            if (_isEnabled)
            {
                data.Throttle = 0.0f;
                data.HasCourse = false;
                data.Deceleration = 0f;
            }

            return true;
        }

        public override ISystemsModification SystemsModification => this;
        public bool IsAlive => true;
        public bool CanActivateSystem(ISystem system)
        {
            if (!_isEnabled || system == this) return true;

            if (system is IWeapon weapon)
            {
                switch (weapon.Info.WeaponType)
                {
                    case WeaponType.Continuous:
                    case WeaponType.RequiredCharging:
                        return true;
                    default:
                        return false;
                }
            }
            else if (system is IDevice device)
            {
                switch (device.DeviceClass)
                {
                    case DeviceClass.EnergyShield:
                        return false;
                    default:
                        return true;
                }
            }

            return true;
        }

        public void OnSystemActivated(ISystem system) { }

        public DeviceClass DeviceClass { get; }
		public override float ActivationCost => _energyCost;
        public override bool CanBeActivated => base.CanBeActivated && (_isEnabled || _ship.Stats.Energy.Value >= _energyCost);

        public void Deactivate() { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (_isEnabled)
            {
                _activationElapsed += elapsedTime;
                if (_isStarshipEarthWarpDrive && !_ship.Stats.Energy.TryGet(_energyCost * elapsedTime))
                    _timeLeft = 0;
                _timeLeft -= elapsedTime;

                if (_isStarshipEarthWarpDrive && Active && _activationElapsed >= BlackDomainHoldTime)
                {
                    if (_trail == null)
                        _trail = WarpTrailEffect.Create(_scene, _ship);
                    _trail.Record(_ship.Body.Position);
                }

                if (!Active && _totalTime - _timeLeft > _minTime || _timeLeft <= 0)
                {
                    InvokeTriggers(ConditionType.OnDeactivate);
                    TimeFromLastUse = 0;
                    _isEnabled = false;
                    _trail = null;
                    _ship.Collider.Enabled = true;
                    _ship.Body.ApplyAcceleration(-_ship.Body.Velocity);
                }
            }
            else if (Active && CanBeActivated)
            {
                if (_controllable)
                {
                    _isEnabled = true;
                    _activationElapsed = 0f;
                    _timeLeft = _totalTime;
                    InvokeTriggers(ConditionType.OnActivate);
                    _ship.Collider.Enabled = false;
                    _ship.Body.ApplyAcceleration(-_ship.Body.Velocity + RotationHelpers.Direction(_ship.Body.Rotation) * _speed);
                }
                else
                {
                    InvokeTriggers(ConditionType.OnActivate);
                    _ship.Body.Move(_ship.Body.Position + RotationHelpers.Direction(_ship.Body.Rotation) * _range);
                    _ship.Body.ApplyAcceleration(-_ship.Body.Velocity);
                    InvokeTriggers(ConditionType.OnDeactivate);
                    TimeFromLastUse = 0;
                }

                _ship.Body.ApplyAngularAcceleration(-_ship.Body.AngularVelocity);
            }

        }

        protected override void OnUpdateView(float elapsedTime) {}

        protected override void OnDispose() {}

        private readonly IScene _scene;
        private WarpTrailEffect _trail;
        private const float BlackDomainHoldTime = 0.45f;
    }
}
