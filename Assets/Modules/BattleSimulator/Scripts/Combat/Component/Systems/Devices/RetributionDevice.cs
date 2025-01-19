using Combat.Component.Body;
using Combat.Component.Bullet.Action;
using Combat.Component.Platform;
using Combat.Component.Systems.Weapons;
using Combat.Component.Triggers;
using Combat.Component.Unit;
using Combat.Component.Unit.Classification;
using Combat.Component.View;
using Combat.Factory;
using Combat.Unit.HitPoints;
using GameDatabase.Model;
using UnityEngine;

namespace Combat.Component.Systems.Devices
{
    public class RetributionDevice : SystemBase, IDevice, IUnitAction, IWeaponPlatform
    {
        private readonly IUnit _unit;
        private readonly IBulletFactory _bulletFactory;

        public RetributionDevice(IUnit unit, IBulletFactory bulletFactory)
            : base(-1, SpriteId.Empty)
        {
            _unit = unit;
            _bulletFactory = bulletFactory;
            Body = new BodyWrapper(_unit.Body);
            EnergyPoints = new UnlimitedEnergy();
        }

        public GameDatabase.Enums.DeviceClass DeviceClass { get; }
		public override bool CanBeActivated { get { return false; } }
        public override IUnitAction UnitAction { get { return this; } }

        public void Deactivate() { }

        protected override void OnUpdatePhysics(float elapsedTime) { }
        protected override void OnUpdateView(float elapsedTime) { }
        protected override void OnDispose() { }

        #region IUnitAction implementation

        public Triggers.ConditionType TriggerCondition { get { return Triggers.ConditionType.OnDestroy; } }
        public bool TryUpdateAction(float elapsedTime) { return false; }
        public bool TryInvokeAction(Triggers.ConditionType condition)
        {
            _bulletFactory.Create(this, 0, 0, Vector2.zero);
            return false;
        }

        #endregion

        #region IWeaponPlatform implementation

        public UnitType Type => _unit.Type;
        public IBody Body { get; }
        public IUnit Owner => _unit;
        public IResourcePoints EnergyPoints { get; }
        public IBulletCompositeDisposable Bullets => null;
        public float MountAngle => 0;
        public bool IsReady => true;
        public float Cooldown => 0;
        public float AutoAimingAngle => 0;
        public Ship.IShip ActiveTarget { get => null; set { } }
        public void Aim(float bulletVelocity, float weaponRange, float relativeEffect) { }
        public void OnShot() { }
        public void SetView(IView view, Color color) { }

        #endregion
    }
}
