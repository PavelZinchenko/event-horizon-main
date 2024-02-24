using System;
using Combat.Component.Platform;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using Combat.Component.Triggers;
using Combat.Effects;
using Combat.Scene;
using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Audio;
using Services.ObjectPool;
using UnityEngine;
using Zenject;

namespace Combat.Factory
{
    public class WeaponFactory
    {
        [Inject] private readonly IScene _scene;
        [Inject] private readonly ISoundPlayer _soundPlayer;
        [Inject] private readonly IObjectPool _objectPool;
        [Inject] private readonly SpaceObjectFactory _spaceObjectFactory;
        [Inject] private readonly EffectFactory _effectFactory;
        [Inject] private readonly PrefabCache _prefabCache;

        public IWeapon Create(IWeaponData weaponData, IWeaponPlatform platform, float hitPointsMultiplier, IShip owner)
        {
            var bulletFactory = new BulletFactory(weaponData.Ammunition, weaponData.Stats, _scene, _soundPlayer, _objectPool, _prefabCache, _spaceObjectFactory, _effectFactory, owner);
            bulletFactory.Stats.HitPointsMultiplier = hitPointsMultiplier;
            var stats = weaponData.Weapon.Stats;
            stats.FireRate *= weaponData.Stats.FireRateMultiplier.Value;
            return Create(stats, weaponData.KeyBinding, bulletFactory, platform);
        }

        public IWeapon Create(IWeaponDataObsolete weaponData, IWeaponPlatform platform, float hitPointsMultiplier, IShip owner)
        {
            var bulletFactory = new BulletFactoryObsolete(weaponData.Ammunition, _scene, _soundPlayer, _objectPool, _prefabCache, _spaceObjectFactory, _effectFactory, owner);
            bulletFactory.Stats.HitPointsMultiplier = hitPointsMultiplier;
            return Create(weaponData.Weapon, weaponData.KeyBinding, bulletFactory, platform);
        }

        private IWeapon Create(WeaponStats weaponStats, int keyBinding, IBulletFactory bulletFactory, IWeaponPlatform platform)
        {
            switch (weaponStats.WeaponClass)
            {
                case WeaponClass.Manageable:
                    {
                        var weapon = new ManageableCannon(platform, weaponStats, bulletFactory, keyBinding);
                        if (weaponStats.ShotSound)
                            weapon.AddTrigger(new SoundEffect(_soundPlayer, weaponStats.ShotSound, ConditionType.OnActivate, ConditionType.OnDeactivate));

                        var effect = CreateEffect(weaponStats, bulletFactory);
                        if (effect != null)
                            weapon.AddTrigger(CreateFlashEffect(effect, bulletFactory, platform));

                        return weapon;
                    }
                case WeaponClass.Continuous:
                    {
                        var weapon = new ContinuousCannon(platform, weaponStats, bulletFactory, keyBinding);
                        if (weaponStats.ShotSound)
                            weapon.AddTrigger(new SoundEffect(_soundPlayer, weaponStats.ShotSound, ConditionType.OnActivate, ConditionType.OnDeactivate));

                        var effect = CreateEffect(weaponStats, bulletFactory);
                        if (effect != null)
                            weapon.AddTrigger(CreateFlashEffect(effect, bulletFactory, platform, ConditionType.OnActivate | ConditionType.OnRemainActive));

                        return weapon;
                    }
                case WeaponClass.MashineGun:
                    {
                        bulletFactory.Stats.RandomFactor = 0.2f;
                        var weapon = new MachineGun(platform, weaponStats, bulletFactory, keyBinding);
                        if (weaponStats.ShotSound)
                            weapon.AddTrigger(new SoundEffect(_soundPlayer, weaponStats.ShotSound, ConditionType.OnActivate));

                        var effect = CreateEffect(weaponStats, bulletFactory);
                        if (effect != null)
                            weapon.AddTrigger(CreateFlashEffect(effect, bulletFactory, platform));

                        return weapon;
                    }
                case WeaponClass.MultiShot:
                    {
                        bulletFactory.Stats.RandomFactor = 0.3f;
                        var weapon = new MultishotCannon(platform, weaponStats, bulletFactory, keyBinding);
                        if (weaponStats.ShotSound)
                            weapon.AddTrigger(new SoundEffect(_soundPlayer, weaponStats.ShotSound, ConditionType.OnActivate));

                        var effect = CreateEffect(weaponStats, bulletFactory);
                        if (effect != null)
                            weapon.AddTrigger(CreateFlashEffect(effect, bulletFactory, platform));

                        return weapon;
                    }
                case WeaponClass.RequiredCharging:
                    {
                        var weapon = new ChargeableCannon(platform, weaponStats, bulletFactory, keyBinding);

                        var effect = CreateEffect(weaponStats, bulletFactory);
                        if (effect != null)
                            weapon.AddTrigger(CreatePowerLevelEffect(effect, weapon, bulletFactory));

                        if (weaponStats.ShotSound)
                            weapon.AddTrigger(new SoundEffect(_soundPlayer, weaponStats.ShotSound, ConditionType.OnDischarge));
                        if (weaponStats.ChargeSound)
                            weapon.AddTrigger(new SoundEffect(_soundPlayer, weaponStats.ChargeSound, ConditionType.OnActivate));
                        return weapon;
                    }
                case WeaponClass.Common:
                    {
                        var weapon = new CommonCannon(platform, weaponStats, bulletFactory, keyBinding);
                        if (weaponStats.ShotSound)
                            weapon.AddTrigger(new SoundEffect(_soundPlayer, weaponStats.ShotSound, ConditionType.OnActivate));

                        var effect = CreateEffect(weaponStats, bulletFactory);
                        if (effect != null)
                            weapon.AddTrigger(CreateFlashEffect(effect, bulletFactory, platform));

                        return weapon;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEffect CreateEffect(WeaponStats weaponStats, IBulletFactory bulletFactory)
        {
            IEffect effect;
            if (weaponStats.VisualEffect != null)
                effect = CompositeEffect.Create(weaponStats.VisualEffect, _effectFactory, null);
            else if (weaponStats.ShotEffectPrefab)
                effect = _effectFactory.CreateEffect(weaponStats.ShotEffectPrefab);
            else
                return null;

            effect.Color = bulletFactory.Stats.FlashColor;
            effect.Size = bulletFactory.Stats.FlashSize;

            if (weaponStats.EffectSize > 0)
                effect.Size *= weaponStats.EffectSize;

            return effect;
        }

        private IUnitEffect CreateFlashEffect(IEffect effect, IBulletFactory bulletFactory, IWeaponPlatform platform, ConditionType condition = ConditionType.OnActivate)
        {
            return new FlashEffect(effect, platform.Body, bulletFactory.Stats.FlashTime, Vector2.zero, condition);
        }

        private IUnitEffect CreatePowerLevelEffect(IEffect effect, IWeapon weapon, IBulletFactory bulletFactory)
        {
            return new PowerLevelEffect(weapon, effect, Vector2.zero, bulletFactory.Stats.FlashTime, ConditionType.OnActivate);
        }
    }
}
