using Combat.Component.Bullet;
using Combat.Component.Platform;
using Combat.Component.Triggers;
using Combat.Unit;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Component.Systems.Weapons
{
    public class ChargeableCannon : WeaponBase
    {
        public ChargeableCannon(IWeaponPlatform platform, WeaponStats weaponStats, Factory.IBulletFactory bulletFactory, int keyBinding)
            : base(platform, weaponStats, bulletFactory, keyBinding)
        {
            _energyConsumption = bulletFactory.Stats.EnergyCost;
            _chargeTotalTime = 1.0f / weaponStats.FireRate;
			_bullets = BulletCompositeDisposable.Create(BulletFactory.Stats);
        }

        public override bool CanBeActivated { get { return _chargeTime > 0 || (Platform.IsReady && Platform.EnergyPoints.Value >= _energyConsumption*0.5f); } }
        public override float Cooldown => Platform.Cooldown;

        public override float PowerLevel => Mathf.Clamp01(_chargeTime / _chargeTotalTime);
        public override IBullet ActiveBullet => HasActiveBullet ? _activeBullet : null;

        protected override void OnUpdateView(float elapsedTime) {}

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (Active && CanBeActivated && _chargeTime > 0 && (_chargeTime > _chargeTotalTime || TryConsumeEnergy(_energyConsumption*elapsedTime / _chargeTotalTime)))
            {
                Aim();
                _chargeTime += elapsedTime;
                UpdatePower();
            }
            else if (_chargeTime > 0)
            {
                if (_chargeTime > 0.1f) Shot();
                _chargeTime = 0;
                InvokeTriggers(ConditionType.OnDeactivate);
            }
            else if (Active && CanBeActivated)
            {
                InvokeTriggers(ConditionType.OnActivate);
                _chargeTime += elapsedTime;
                UpdatePower();
            }
            else if (HasActiveBullet && Info.BulletType == AiBulletBehavior.Beam)
            {
                Aim();
            }
        }

        protected override void OnDispose() 
		{
			_bullets.Dispose();
		}

		private void Shot()
        {
            Aim();
            Platform.OnShot();
            _activeBullet = CreateBullet();
			_bullets.Add(_activeBullet);

            InvokeTriggers(ConditionType.OnDischarge);
        }

        private void UpdatePower()
        {
            BulletFactory.Stats.PowerLevel = PowerLevel;
        }

        private bool HasActiveBullet { get { return _activeBullet.IsActive(); } }

        private float _chargeTime;

        private IBullet _activeBullet;
        private readonly float _chargeTotalTime;
        private readonly float _energyConsumption;
		private readonly IBulletCompositeDisposable _bullets;
    }
}
