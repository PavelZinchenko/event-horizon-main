using Combat.Component.Bullet;
using Combat.Component.Platform;
using Combat.Component.Triggers;
using Combat.Unit;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Systems.Weapons
{
    public class ManageableCannon : WeaponBase
    {
        public ManageableCannon(IWeaponPlatform platform, WeaponStats weaponStats, Factory.IBulletFactory bulletFactory, int keyBinding)
            : base(platform, weaponStats, bulletFactory, keyBinding)
        {
            MaxCooldown = weaponStats.FireRate > 0 ? 1f / weaponStats.FireRate : 0f;
            _energyConsumption = bulletFactory.Stats.EnergyCost;
        }

        public override float ActivationCost { get { return _energyConsumption; } }
        public override bool CanBeActivated { get { return base.CanBeActivated && Platform.IsReady && (HasActiveBullet || Platform.EnergyPoints.Value >= _energyConsumption); } }
        public override float Cooldown { get { return Mathf.Max(base.Cooldown, Platform.Cooldown); } }
        public override IBullet ActiveBullet => HasActiveBullet ? _activeBullet : null;

        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            if (_activeBullet != null)
            {
                if (_activeBullet.State != UnitState.Active)
                {
                    Platform.OnShot();
                    TimeFromLastUse = 0;
                    _activeBullet = null;
                    InvokeTriggers(ConditionType.OnDeactivate);
                }
                else if (!Active)
                {
                    _activeBullet.Detonate();
                }
            }
            else if (Active && CanBeActivated && Platform.EnergyPoints.TryGet(_energyConsumption))
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
            if (HasActiveBullet)
                _activeBullet.Detonate();

            Aim();
            _activeBullet = CreateBullet();
        }

        private bool HasActiveBullet { get { return _activeBullet.IsActive(); } }

        private IBullet _activeBullet;
        private readonly float _energyConsumption;
    }
}
