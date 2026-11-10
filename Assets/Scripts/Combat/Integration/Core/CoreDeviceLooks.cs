using CombatSimulation.Core;
using CombatView;
using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;
using GameDatabase.Model;
using UnityEngine;

namespace Combat.Integration.Core
{
    // The sounds and effects of a ship's weapons, devices and drone bays (migration phase 5b, step 7a), by the rules of
    // legacy WeaponFactory, DeviceFactory and DroneBayFactory. Built once per ship definition with its look.
    public sealed class CoreDeviceLooks
    {
        public CoreDeviceLooks(CoreVisualEffects effects, CoreSoundOutput sounds)
        {
            _effects = effects;
            _sounds = sounds;
        }

        public DeviceLook[] Create(CoreShipMapping mapping)
        {
            var sources = mapping.DeviceViews;
            var result = new DeviceLook[sources.Count];
            for (var i = 0; i < result.Length; ++i)
            {
                var source = sources[i];
                if (source == null)
                    continue;
                if (source.Weapon != null)
                    result[i] = Weapon(source.Weapon, source.Barrel, mapping.Definition);
                else if (source.Device.HasValue)
                    result[i] = Device(source.Device.Value, mapping.ModelScale);
                else if (source.DroneBay.HasValue)
                    result[i] = DroneBay(source.DroneBay.Value);
            }

            return result;
        }

        // Legacy WeaponFactory.CreateEffect: the weapon's record, else its prefab, in the bullet colour (the weapon's,
        // else the body's), Body.Size x EffectSize wide. The flash fades over BulletStats.FlashTime; the charge effect
        // over at least 0.5 s. Both at power level 1.
        private DeviceLook Weapon(IWeaponData weapon, int barrel, ShipDefinition ship)
        {
            var stats = weapon.Weapon.Stats;
            var body = weapon.Ammunition.Body;
            var flashTime = weapon.Ammunition.Controller.Continuous ? Mathf.Max(MinFlashTime, body.Lifetime) : MinFlashTime;
            var mode = Mode(stats.WeaponClass);
            var look = new DeviceLook(DeviceViewKind.Weapon)
            {
                Sound = _sounds.Resolve(stats.ShotSound),
                ChargeSound = _sounds.Resolve(stats.ChargeSound),
                Record = _effects.Get(stats.VisualEffect),
                Prefab = stats.VisualEffect == null ? EffectName(stats.ShotEffectPrefab) : null,
                Color = (weapon.Stats.Color ?? body.Color).ToUnity(),
                Size = body.Size * (stats.EffectSize > 0f ? stats.EffectSize : 1f),
                Lifetime = mode == WeaponViewMode.Charging ? Mathf.Max(flashTime, MinChargeTime) : flashTime,
                Mode = mode,
                Barrel = barrel,
                BarrelDefinition = barrel >= 0 && barrel < ship.Barrels.Count ? ship.Barrels[barrel] : null,
            };
            return look;
        }

        private static WeaponViewMode Mode(WeaponClass weaponClass)
        {
            switch (weaponClass)
            {
                case WeaponClass.Manageable: return WeaponViewMode.Held;
                case WeaponClass.Continuous: return WeaponViewMode.Continuous;
                case WeaponClass.RequiredCharging: return WeaponViewMode.Charging;
                default: return WeaponViewMode.Single;
            }
        }

        // Legacy DeviceFactory: the sound on OnActivate (not for the point defense and the repair bot, which play it
        // themselves); CreateEffect takes the record, else the prefab, in the device colour, Size x a scale. The decoy
        // and the repair bot play their sounds through their own views.
        private DeviceLook Device(DeviceStats device, float modelScale)
        {
            DeviceLook look;
            switch (device.DeviceClass)
            {
                case DeviceClass.Detonator:
                case DeviceClass.Fortification:
                case DeviceClass.Stealth:
                case DeviceClass.SuperStealth:
                case DeviceClass.EnergyShield:
                case DeviceClass.PartialShield:
                    look = new DeviceLook(DeviceViewKind.Toggle);
                    break;
                // Legacy adds the engine flashes only for an EffectPrefab, as FlashEffect lasting 0.5 s.
                case DeviceClass.Accelerator:
                    look = new DeviceLook(DeviceViewKind.Accelerator);
                    if (device.EffectPrefab)
                        SetEffect(look, device, device.Size * modelScale, 0f);
                    break;
                case DeviceClass.Ghost:
                    look = new DeviceLook(DeviceViewKind.Ghost);
                    SetEffect(look, device, device.Size * modelScale, ShortFlashTime);
                    break;
                case DeviceClass.TimeMachine:
                    look = new DeviceLook(DeviceViewKind.InfinityStone);
                    SetEffect(look, device, device.Size * modelScale, ShortFlashTime);
                    break;
                case DeviceClass.GravityGenerator:
                    look = new DeviceLook(DeviceViewKind.GravityGenerator);
                    SetEffect(look, device, device.Size * 2f * device.Range, device.Lifetime);
                    break;
                // Legacy StaticEffect for 0.5 s at Size x the ship's scale.
                case DeviceClass.Teleporter:
                    look = new DeviceLook(DeviceViewKind.Teleporter);
                    SetEffect(look, device, device.Size * modelScale, StaticEffectTime);
                    break;
                case DeviceClass.TeleporterV2:
                    look = new DeviceLook(DeviceViewKind.WarpDrive);
                    SetEffect(look, device, device.Size * modelScale, StaticEffectTime);
                    break;
                // Legacy PointDefenseCollisionAction: the hard-coded Lightning prefab for 0.1 s in the device colour.
                case DeviceClass.PointDefense:
                    look = new DeviceLook(DeviceViewKind.PointDefense)
                        { Prefab = PointDefensePrefab, Color = device.Color.ToUnity(), Lifetime = ZapTime };
                    break;
                // Legacy ClonningDevice: the EffectPrefab on the clone for 0.5 s, at the prefab's own size and colour.
                case DeviceClass.ClonningCenter:
                    look = new DeviceLook(DeviceViewKind.ClonningCenter)
                        { Prefab = EffectName(device.EffectPrefab), Lifetime = StaticEffectTime };
                    break;
                default:
                    return null;
            }

            look.Sound = _sounds.Resolve(device.Sound);
            return look;
        }

        private void SetEffect(DeviceLook look, DeviceStats device, float size, float lifetime)
        {
            look.Record = _effects.Get(device.VisualEffect);
            look.Prefab = device.VisualEffect == null ? EffectName(device.EffectPrefab) : null;
            look.Color = device.Color.ToUnity();
            look.Size = size;
            look.Lifetime = lifetime;
        }

        // Legacy DroneBayFactory: the launch effect in white at size 1 for 0.2 s, and the launch sound.
        private DeviceLook DroneBay(DroneBayStats bay) => new DeviceLook(DeviceViewKind.DroneBay)
        {
            Sound = _sounds.Resolve(bay.LaunchSound),
            Prefab = EffectName(bay.LaunchEffectPrefab),
            Lifetime = MinFlashTime,
        };

        // Legacy EffectFactory loads Combat/Effects/<name>; the new prefabs keep the names (owner question 1 (a)).
        private static string EffectName(PrefabId id)
        {
            if (!id)
                return null;

            var path = id.ToString();
            var slash = path.LastIndexOf('/');
            return slash >= 0 ? path.Substring(slash + 1) : path;
        }

        // Legacy BulletStats.FlashTime, WeaponFactory.CreatePowerLevelEffect and the device flash and static effect times.
        private const float MinFlashTime = 0.2f;
        private const float MinChargeTime = 0.5f;
        private const float ShortFlashTime = 0.2f;
        private const float StaticEffectTime = 0.5f;
        private const float ZapTime = 0.1f;
        private const string PointDefensePrefab = "Lightning";

        private readonly CoreVisualEffects _effects;
        private readonly CoreSoundOutput _sounds;
    }
}
