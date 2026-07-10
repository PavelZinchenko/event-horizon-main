using Combat.Component.Engine;
using Combat.Component.Ship;
using Combat.Component.Triggers;
using Combat.Component.Unit.Classification;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Component.Systems.Devices
{
    // The droplet's pilot system is intentionally a sustained toggle.  It
    // keeps full thrust through turns and steers toward the player's radar lock.
    public sealed class SophonGuidanceDevice : SystemBase, IDevice, IEngineModification
    {
        public SophonGuidanceDevice(IShip ship, DeviceStats stats, int keyBinding, IScene scene)
            : base(keyBinding, stats.ControlButtonIcon)
        {
            _ship = ship;
            _scene = scene;
            DeviceClass = stats.DeviceClass;
            MaxCooldown = stats.Cooldown;
            _energyCost = Mathf.Max(0.1f, stats.EnergyConsumption);
        }

        public DeviceClass DeviceClass { get; }
        public override IEngineModification EngineModification => this;
        public override bool CanBeActivated => base.CanBeActivated && (_enabled || _ship.Stats.Energy.Value > 0.01f);

        public bool TryApplyModification(ref EngineData data)
        {
            if (!_enabled)
                return true;

            data.Throttle = 1f;
            data.Deceleration = 0f;
            data.TurnRate *= 12f;
            data.AngularVelocity *= 4f;
            return true;
        }

        public void Deactivate()
        {
            if (!_enabled)
                return;
            _enabled = false;
            TimeFromLastUse = 0f;
            InvokeTriggers(ConditionType.OnDeactivate);
        }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (!Active || !CanBeActivated || !_ship.Stats.Energy.TryGet(_energyCost * elapsedTime))
            {
                Deactivate();
                return;
            }

            if (!_enabled)
            {
                _enabled = true;
                InvokeTriggers(ConditionType.OnActivate);
            }
            else
            {
                InvokeTriggers(ConditionType.OnRemainActive);
            }

            var target = _scene.LockedEnemyShip;
            if (!target.IsActive() || !CombatRelations.AreEnemies(_ship.Type, target.Type))
                return;

            var delta = target.Body.WorldPosition() - _ship.Body.WorldPosition();
            var distance = delta.magnitude;
            var leadTime = Mathf.Clamp(distance / 80f, 0f, 2f);
            var predicted = target.Body.WorldPosition() + target.Body.WorldVelocity() * leadTime;
            _ship.Controls.Course = RotationHelpers.Angle(predicted - _ship.Body.WorldPosition());
            _ship.Controls.Throttle = 1f;
        }

        protected override void OnUpdateView(float elapsedTime) { }
        protected override void OnDispose() { }

        private readonly IShip _ship;
        private readonly IScene _scene;
        private readonly float _energyCost;
        private bool _enabled;
    }
}
