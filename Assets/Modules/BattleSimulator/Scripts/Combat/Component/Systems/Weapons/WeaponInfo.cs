﻿using Combat.Component.Body;
using Combat.Component.Platform;
using Combat.Factory;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Component.Systems.Weapons
{
    public enum WeaponType
    {
        Common,
        Manageable,
        Continuous,
        RequiredCharging,
    }

    [System.Flags]
	public enum WeaponCapability
	{
		None = 0,
		DamageEnemy = 1,
		RepairAlly = 2,
		CaptureDrone = 4,
        RechargeAlly = 8,
    }

    [System.Obsolete]
    public enum BulletEffectType
    {
        Common,
        DamageOverTime,
        Repair,
        Special,
        ForDronesOnly,
    }

    public class WeaponInfo
    {
        public WeaponInfo(WeaponType weaponType, float spread, float firerate, IBulletFactory bulletFactory, IWeaponPlatform platform)
        {
            _bulletFactory = bulletFactory;
            WeaponType = weaponType;
            Firerate = firerate;
            Spread = spread;

            if (platform.Body.Parent != null && weaponType == WeaponType.Common)
                Recoil = _bulletFactory.Stats.Recoil / platform.Body.TotalWeight();
        }

        public WeaponType WeaponType { get; }
        public AiBulletBehavior BulletType => _bulletFactory.Stats.BehaviorType;
        public bool RequiresAiming => BulletType != AiBulletBehavior.AreaOfEffect;
        public WeaponCapability Capability => _bulletFactory.Stats.Capability;
		[System.Obsolete] public BulletEffectType BulletEffectType => _bulletFactory.Stats.EffectType;
        public float Range => _bulletFactory.Stats.BulletHitRange;
        public float RelativeVelocityEffect => _bulletFactory.Stats.RelativeVelocityEffect;
        public float BulletSpeed => _bulletFactory.Stats.BulletSpeed;
        public float EnergyCost => _bulletFactory.Stats.EnergyCost;
        public float Recoil { get; private set; }
        public float Spread { get; }
        public float Firerate { get; }

        private readonly IBulletFactory _bulletFactory;
    }

	public static class WeaponCapabilityExtensions
	{
		public static bool HasCapability(this WeaponCapability caps, WeaponCapability flag) => (caps & flag) == flag;
	}
}
