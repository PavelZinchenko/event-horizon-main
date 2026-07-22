using Combat.Component.Platform;
using Combat.Component.Ship;
using Combat.Component.Systems.Devices;
using Combat.Component.Triggers;
using Combat.Component.Unit;
using Combat.Scene;
using Combat.Unit;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Systems.Weapons
{
    /// <summary>
    /// Deploys a strategic field directly on the currently locked ship.  This
    /// intentionally has no projectile: an invalid or out-of-range target does
    /// not consume energy and does not start either cooldown.
    /// </summary>
    public sealed class StrategicFieldWeapon : WeaponBase
    {
        public StrategicFieldWeapon(IWeaponPlatform platform, WeaponStats weaponStats,
            Factory.IBulletFactory bulletFactory, int keyBinding, IScene scene, IShip owner,
            StrategicFieldEffect.FieldKind fieldKind)
            : base(platform, weaponStats, bulletFactory, keyBinding)
        {
            _scene = scene;
            _owner = owner;
            _fieldKind = fieldKind;
            _range = bulletFactory.Stats.BulletHitRange;
            _energyConsumption = bulletFactory.Stats.EnergyCost;
            MaxCooldown = weaponStats.FireRate > 0f ? 1f / weaponStats.FireRate : 0f;
        }

        public override bool CanBeActivated => base.CanBeActivated && Platform.IsReady && HasValidTarget();
        public override float Cooldown => Mathf.Max(Platform.Cooldown, base.Cooldown);

        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (!Active || !CanBeActivated) return;

            // Re-read after validation so a destroyed/changed lock cannot create
            // a field or consume resources in the same physics tick.
            var target = CurrentLockedTarget();
            if (!IsInRange(target) || !TryConsumeEnergy(_energyConsumption)) return;

            if (Platform is IUnitTargetingPlatform unitTargetingPlatform)
                unitTargetingPlatform.ActiveUnitTarget = target;
            else
                Platform.ActiveTarget = target as IShip;
            StrategicFieldEffect.Create(_scene, _owner, target.Body.WorldPosition(), _fieldKind);
            Platform.OnShot();
            TimeFromLastUse = 0f;
            InvokeTriggers(ConditionType.OnActivate);
        }

        protected override void OnDispose() { }

        private bool HasValidTarget() => IsInRange(CurrentLockedTarget()) &&
                                         Platform.EnergyPoints.Value >= _energyConsumption;

        private IUnit CurrentLockedTarget()
        {
            var target = _scene.LockedTarget;
            return target is IShip || IsDualVectorFoil(target) ? target : null;
        }

        private bool IsInRange(IUnit target)
        {
            if (target == null || !target.IsActive()) return false;
            return Vector2.Distance(_owner.Body.WorldPosition(), target.Body.WorldPosition()) <= _range;
        }

        private static bool IsDualVectorFoil(IUnit target)
        {
            return target is Combat.Component.Bullet.Bullet bullet &&
                   bullet.Controller is Combat.Component.Controller.StrategicWeaponController controller &&
                   controller.Kind == Combat.Component.Controller.StrategicWeaponController.WeaponKind.DualVectorFoil;
        }

        private readonly IScene _scene;
        private readonly IShip _owner;
        private readonly StrategicFieldEffect.FieldKind _fieldKind;
        private readonly float _range;
        private readonly float _energyConsumption;
    }
}
