using Combat.Component.Platform;
using Combat.Component.Triggers;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Systems.Weapons
{
    public class MachineGun : WeaponBase
    {
        public MachineGun(IWeaponPlatform platform, WeaponStats weaponStats, Factory.IBulletFactory bulletFactory, int keyBinding)
            : base(platform, weaponStats, bulletFactory, keyBinding)
        {
            MaxCooldown = weaponStats.FireRate > 0 ? 1f / weaponStats.FireRate : 0f;

            _energyConsumption = bulletFactory.Stats.EnergyCost;
            _magazine = weaponStats.Magazine;
			_bullets = BulletCompositeDisposable.Create(BulletFactory.Stats);
        }

        public override bool CanBeActivated { get { return base.CanBeActivated && Platform.EnergyPoints.Value > _energyConsumption; } }
        public override float Cooldown { get { return Mathf.Max(base.Cooldown, Platform.Cooldown); } }

        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (Active && _shots < _magazine && CanBeActivated)
            {
                if (Platform.IsReady && Platform.EnergyPoints.TryGet(_energyConsumption))
                {
                    Shot();
                    _shots++;
                    InvokeTriggers(ConditionType.OnActivate);
                }
            }
            else if (_shots >= _magazine)
            {
                _shots = 0;
                TimeFromLastUse = 0;
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
            _bullets.Add(CreateBullet());
        }

        private int _shots;
        private readonly int _magazine;
        private readonly float _energyConsumption;
		private readonly IBulletCompositeDisposable _bullets;
	}
}
