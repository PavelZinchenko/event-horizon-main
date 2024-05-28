using Combat.Component.Bullet;
using Combat.Component.Platform;
using Combat.Component.Triggers;
using Combat.Unit;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Systems.Weapons
{
    public class ContinuousCannon : WeaponBase
    {
        public ContinuousCannon(IWeaponPlatform platform, WeaponStats weaponStats, Factory.IBulletFactory bulletFactory, int keyBinding)
            : base(platform, weaponStats, bulletFactory, keyBinding)
        {
            MaxCooldown = weaponStats.FireRate > 0 ? 1f / weaponStats.FireRate : 0f;

            _energyConsumption = bulletFactory.Stats.EnergyCost;
        }

		public override bool CanBeActivated => base.CanBeActivated && (HasActiveBullet || Platform.IsReady);
		public override float Cooldown => Mathf.Max(base.Cooldown, Platform.Cooldown);

		public override IBullet ActiveBullet => HasActiveBullet ? _activeBullet : null;

		protected override void OnUpdateView(float elapsedTime) {}

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (HasActiveBullet)
            {
                Aim();

                if (Active && TryConsumeEnergy(_energyConsumption * elapsedTime))
                {
                    _activeBullet.Lifetime.Restore();
                    InvokeTriggers(ConditionType.OnRemainActive);
                }
                else
                {
                    TimeFromLastUse = 0;
                    InvokeTriggers(ConditionType.OnDeactivate);
                }
            }
            else if (Active && CanBeActivated && TryConsumeEnergy(ActivationCost))
            {
                Shot();
                InvokeTriggers(ConditionType.OnActivate);
            }
        }

        protected override void OnDispose() 
		{
			if (BulletFactory.Stats.IsBoundToCannon)
				_activeBullet?.Vanish();
		}

        private void Shot()
        {
            Aim();
            _activeBullet = CreateBullet();
            _activeBullet.Lifetime.Restore();
        }

		private bool HasActiveBullet => _activeBullet.IsActive();

		private IBullet _activeBullet;
        private readonly float _energyConsumption;
    }
}
