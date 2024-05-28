﻿using Combat.Component.Bullet;
using Combat.Component.Platform;
using GameDatabase.DataModel;
using UnityEngine;

namespace Combat.Component.Systems.Weapons
{
    public abstract class WeaponBase : SystemBase, IWeapon
    {
        private readonly WeaponStats _weaponStats;

        public WeaponBase(IWeaponPlatform platform, WeaponStats weaponStats, Factory.IBulletFactory bulletFactory, int keyBinding)
            : base(keyBinding, weaponStats.ControlButtonIcon)
        {
            _weaponStats = weaponStats;
            ActivationCost = bulletFactory.Stats.ActivationCost;
            Platform = platform;
            BulletFactory = bulletFactory;
            Info = new WeaponInfo(GetWeaponType(weaponStats), weaponStats.Spread, weaponStats.FireRate, bulletFactory, platform);
        }

        public WeaponInfo Info { get; }
        public IWeaponPlatform Platform { get; }

        public virtual float PowerLevel => 1.0f;
        public virtual IBullet ActiveBullet => null;
        public override float ActivationCost { get; }

        public void Aim() => Platform.Aim(Info.BulletSpeed, Info.Range, Info.RelativeVelocityEffect);

        protected Factory.IBulletFactory BulletFactory { get; }
        protected IBullet CreateBullet() => BulletFactory.Create(Platform, _weaponStats.Spread, 0, Vector2.zero);

        protected bool TryConsumeEnergy(float value)
        {
            if (value <= 0) return true;
            return Platform.EnergyPoints.TryGet(value);
        }

        private WeaponType GetWeaponType(WeaponStats weaponStats)
        {
            switch (weaponStats.WeaponClass)
            {
                case GameDatabase.Enums.WeaponClass.Manageable:
                    return WeaponType.Manageable;
                case GameDatabase.Enums.WeaponClass.Continuous:
                    return WeaponType.Continuous;
                case GameDatabase.Enums.WeaponClass.RequiredCharging:
                    return WeaponType.RequiredCharging;
                case GameDatabase.Enums.WeaponClass.Common:
                case GameDatabase.Enums.WeaponClass.MashineGun:
                case GameDatabase.Enums.WeaponClass.MultiShot:
                default:
                    return WeaponType.Common;
            }
        }
    }
}
