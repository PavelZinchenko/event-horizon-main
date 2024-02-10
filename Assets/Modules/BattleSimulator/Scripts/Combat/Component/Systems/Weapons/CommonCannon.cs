using Combat.Component.Platform;
using Combat.Component.Triggers;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Systems.Weapons
{
    public class CommonCannon : WeaponBase
    {
        public CommonCannon(IWeaponPlatform platform, WeaponStats weaponStats, Factory.IBulletFactory bulletFactory, int keyBinding)
            : base(platform, weaponStats, bulletFactory, keyBinding)
        {
            MaxCooldown = weaponStats.FireRate > 0 ? 1f/weaponStats.FireRate : 0f;

            _energyConsumption = bulletFactory.Stats.EnergyCost;
			_bullets = BulletCompositeDisposable.Create(BulletFactory.Stats);
        }

        public override float ActivationCost { get { return _energyConsumption; } }
        public override bool CanBeActivated { get { return base.CanBeActivated && Platform.IsReady && Platform.EnergyPoints.Value >= _energyConsumption; } }
        public override float Cooldown { get { return Mathf.Max(Platform.Cooldown, base.Cooldown); } }

        protected override void OnUpdateView(float elapsedTime) {}

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (Active && CanBeActivated && Platform.EnergyPoints.TryGet(_energyConsumption))
            {
                Shot();
                TimeFromLastUse = 0;
                InvokeTriggers(ConditionType.OnActivate);
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

        private readonly float _energyConsumption;
		private readonly IBulletCompositeDisposable _bullets;
	}
}
