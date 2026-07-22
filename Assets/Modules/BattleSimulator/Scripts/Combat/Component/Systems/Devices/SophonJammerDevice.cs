using Combat.Component.Body;
using Combat.Component.Platform;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using Combat.Component.Triggers;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Factory;
using Combat.Scene;
using Combat.Unit.HitPoints;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using UnityEngine;
using TriggerConditionType = Combat.Component.Triggers.ConditionType;
using BodyWrapper = Combat.Component.Bullet.Action.BodyWrapper;

namespace Combat.Component.Systems.Devices
{
    public sealed class SophonJammerDevice : SystemBase, IDevice, IWeaponPlatform
    {
        public SophonJammerDevice(IShip ship, DeviceStats deviceSpec, int keyBinding, IScene scene)
            : this(ship, deviceSpec, keyBinding, scene, null)
        {
        }

        public SophonJammerDevice(IShip ship, DeviceStats deviceSpec, int keyBinding, IScene scene,
            IBulletFactory bulletFactory)
            : base(keyBinding, deviceSpec.ControlButtonIcon)
        {
            _ship = ship;
            _bulletFactory = bulletFactory;
            DeviceClass = deviceSpec.DeviceClass;
            MaxCooldown = deviceSpec.Cooldown;
            _energyCost = deviceSpec.EnergyConsumption;
            Body = ship != null ? new BodyWrapper(ship.Body) : null;
            EnergyPoints = new UnlimitedEnergy();
        }

        public DeviceClass DeviceClass { get; }
        public override float ActivationCost => _energyCost;
        public override bool CanBeActivated => base.CanBeActivated && _ship != null && _bulletFactory != null &&
                                               _ship.Stats.Energy.Value >= _energyCost;

        public void RequestActivation()
        {
            TryFireEmpProjectile();
        }

        public void Deactivate() { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            // Touch input fires immediately through RequestActivation(). AI and
            // keyboard-held controls still use this edge/cooldown path.
            bool activationEdge = _ship != null && (_ship.Type.Side == UnitSide.Player
                ? Active && !_wasPressed
                : Active);
            if (activationEdge)
                TryFireEmpProjectile();

            _wasPressed = Active;
        }

        private bool TryFireEmpProjectile()
        {
            if (!CanBeActivated || !_ship.Stats.Energy.TryGet(_energyCost))
                return false;

            // The carrier uses Combat/Bullets/Empty, travels only a fraction of a
            // unit and expires almost immediately. BulletFactoryObsolete attaches
            // CreateEnemyFleetEmpAction to OnExpire/OnDetonate before scene entry.
            _bulletFactory.Create(this, 0f, 0f, Vector2.zero);
            TimeFromLastUse = 0f;
            InvokeTriggers(TriggerConditionType.OnActivate);
            return true;
        }

        protected override void OnUpdateView(float elapsedTime) { }
        protected override void OnDispose() { }

        #region IWeaponPlatform

        public UnitType Type => _ship.Type;
        public IBody Body { get; }
        public IUnit Owner => _ship;
        public IResourcePoints EnergyPoints { get; }
        public IBulletCompositeDisposable Bullets => null;
        public float MountAngle => 0f;
        public bool IsReady => true;
        public float Cooldown => 0f;
        public float AutoAimingAngle => 0f;
        public IShip ActiveTarget { get => null; set { } }
        public void Aim(float bulletVelocity, float weaponRange, float relativeEffect) { }
        public void OnShot() { }
        public void SetView(IView view, Color color) { }

        #endregion

        private readonly IShip _ship;
        private readonly IBulletFactory _bulletFactory;
        private readonly float _energyCost;
        private bool _wasPressed;
    }
}
