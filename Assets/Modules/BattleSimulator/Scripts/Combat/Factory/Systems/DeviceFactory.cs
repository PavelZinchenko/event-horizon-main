using Combat.Component.Features;
using Combat.Component.Ship;
using Combat.Component.Systems;
using Combat.Component.Systems.Devices;
using Combat.Component.Triggers;
using Combat.Effects;
using Combat.Scene;
using Combat.Services;
using Constructor;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using UnityEngine;
using Zenject;

namespace Combat.Factory
{
    public class DeviceFactory
    {
        [Inject] private readonly IScene _scene;
        [Inject] private readonly SpaceObjectFactory _spaceObjectFactory;
        [Inject] private readonly SatelliteFactory _satelliteFactory;
        [Inject] private readonly EffectFactory _effectFactory;
        [Inject] private readonly ShipFactory _shipFactory;
        [Inject] private readonly IGameServicesProvider _services;

        public ISystem Create(IDeviceData deviceData, IShip ship, IShipSpecification shipSpec)
        {
            var stats = deviceData.Device;

            SystemBase device;
            ConditionType soundEffectCondition = ConditionType.OnActivate;

            switch (stats.DeviceClass)
            {
                case DeviceClass.ClonningCenter:
                    device = new ClonningDevice(ship, stats, _shipFactory, shipSpec, _effectFactory, deviceData.KeyBinding);
                    break;
                case DeviceClass.TimeMachine:
                    {
                        device = new InfinityStone(ship, stats, deviceData.KeyBinding);

                        var effect = CreateEffect(stats, ship);
                        if (effect != null)
                            device.AddTrigger(new FlashEffect(effect, ship.Body, 0.2f, Vector2.zero, ConditionType.OnActivate));
                    }
                    break;
                case DeviceClass.Accelerator:
                    {
                        device = new AcceleratorDevice(ship, stats, deviceData.KeyBinding);
                        if (stats.EffectPrefab)
                        {
                            foreach (var engine in shipSpec.Stats.ShipModel.Engines)
                                device.AddTrigger(new FlashEffect(CreateEffect(stats, ship), ship.Body, 0.5f, engine.Position * 0.5f,
                                    ConditionType.OnRemainActive | ConditionType.OnActivate));
                        }
                    }
                    break;
                case DeviceClass.Decoy:
                    device = new DecoyDevice(ship, stats, shipSpec.Stats.ArmorMultiplier.Value * 10, deviceData.KeyBinding, _spaceObjectFactory);
                    break;
                case DeviceClass.Ghost:
                    {
                        device = new GhostDevice(ship, stats, deviceData.KeyBinding);
                        soundEffectCondition = ConditionType.OnActivate | ConditionType.OnDeactivate;
                        var effect = CreateEffect(stats, ship);
                        if (effect != null)
                            device.AddTrigger(new FlashEffect(effect, ship.Body, 0.2f, Vector2.zero, ConditionType.OnDeactivate | ConditionType.OnActivate));
                    }
                    break;
                case DeviceClass.PointDefense:
                    {
                        var pointDefense = new PointDefenseSystem(ship, stats, deviceData.KeyBinding);
                        device = pointDefense;
                        device.AddTrigger(new PointDefenseAction(ship, pointDefense, stats.Size + ship.Body.Scale/2f, 
                            stats.Power*shipSpec.Stats.DamageMultiplier.Value, stats.EnergyConsumption, stats.Cooldown, stats.Color, _satelliteFactory, stats.Sound));
                        soundEffectCondition = ConditionType.None;
                    }
                    break;
                case DeviceClass.GravityGenerator:
                    device = new GravityGenerator(ship, stats, deviceData.KeyBinding);
                    break;
                case DeviceClass.EnergyShield:
                    {
                        var prefab = stats.Prefab != null ? _services.PrefabCache.LoadPrefab(stats.Prefab) : _services.PrefabCache.LoadPrefab(stats.EffectPrefab);
                        var energyShield = _satelliteFactory.CreateEnergyShield(ship, prefab, 1f / (stats.Power * shipSpec.Stats.ShieldMultiplier.Value), stats.Size, stats.Color);
                        var energyShieldDevice = new EnergyShieldDevice(ship, stats, deviceData.KeyBinding);
                        device = energyShieldDevice;
                        device.AddTrigger(new AuxiliaryUnitAction(energyShieldDevice, energyShield));
                    }
                    break;
                case DeviceClass.PartialShield:
                    {
                        var energyShield = _satelliteFactory.CreateFrontalShield(ship, 1f / (stats.Power * shipSpec.Stats.ShieldMultiplier.Value), stats.Offset, stats.Size, stats.Color);
                        var energyShieldDevice = new FrontalShieldDevice(ship, stats, deviceData.KeyBinding);
                        device = energyShieldDevice;
                        device.AddTrigger(new AuxiliaryUnitAction(energyShieldDevice, energyShield));
                    }
                    break;
                case DeviceClass.RepairBot:
                    device = new RepairSystem(ship, stats, deviceData.KeyBinding);

                    var repairRate = stats.Power * ship.Stats.Armor.MaxValue / 100;
                    var hitPoints = ship.Stats.HitPointsMultiplier * stats.Size;
                    var trigger = new RepairBotAction(ship, device, _satelliteFactory, repairRate, stats.Size, stats.Range, hitPoints, stats.Lifetime, 
                        stats.Color, stats.Sound);

                    device.AddTrigger(trigger);
                    soundEffectCondition = ConditionType.None;
                    break;
                case DeviceClass.Detonator:
                    device = new DetonatorDevice(ship, stats, deviceData.KeyBinding, _spaceObjectFactory, shipSpec.Stats.Layout.CellCount*shipSpec.Stats.DamageMultiplier.Value);
                    break;
                case DeviceClass.Stealth:
                    device = new StealthDevice(ship, stats, deviceData.KeyBinding, false);
                    break;
                case DeviceClass.Teleporter:
                    device = new TeleporterDevice(ship, stats, deviceData.KeyBinding);
                    if (stats.VisualEffect != null)
                        device.AddTrigger(new StaticEffect(stats.VisualEffect, _effectFactory, ship.Body, 0.5f, stats.Size * ship.Body.Scale, stats.Color, ConditionType.OnActivate | ConditionType.OnDeactivate));
                    else if (stats.EffectPrefab)
                        device.AddTrigger(new StaticEffect(stats.EffectPrefab, _effectFactory, ship.Body, 0.5f, stats.Size * ship.Body.Scale, stats.Color, ConditionType.OnActivate | ConditionType.OnDeactivate));
                    break;
                case DeviceClass.TeleporterV2:
                    device = new WarpDrive(ship, stats, deviceData.KeyBinding);
                    if (stats.VisualEffect != null)
                        device.AddTrigger(new StaticEffect(stats.VisualEffect, _effectFactory, ship.Body, 0.5f, stats.Size * ship.Body.Scale, stats.Color, ConditionType.OnActivate | ConditionType.OnDeactivate));
                    else if (stats.EffectPrefab)
                        device.AddTrigger(new StaticEffect(stats.EffectPrefab, _effectFactory, ship.Body, 0.5f, stats.Size * ship.Body.Scale, stats.Color, ConditionType.OnActivate | ConditionType.OnDeactivate));
                    break;
                case DeviceClass.Brake:
                    device = new BrakeDevice(ship, stats, ship.Body.Weight);
                    break;
                case DeviceClass.SuperStealth:
                    device = new StealthDevice(ship, stats, deviceData.KeyBinding, true);
                    break;
                case DeviceClass.Fortification:
                    device = new FortificationDevice(ship, stats, deviceData.KeyBinding);
                    break;
                case DeviceClass.ToxicWaste:
                    device = new ToxicWaste(ship, stats, _spaceObjectFactory, shipSpec.Stats.DamageMultiplier.Value);
                    break;
                case DeviceClass.WormTail:
                    var tailSegment = stats.Prefab != null ? _services.PrefabCache.LoadPrefab(stats.Prefab) : _services.PrefabCache.LoadPrefab(stats.ObjectPrefab);
                    device = new WormTailDevice(stats, _spaceObjectFactory.CreateWormTail(ship, Mathf.FloorToInt(stats.Size), 0.1f,
                        ship.Stats.Armor.MaxValue * stats.Power, tailSegment, shipSpec.Stats.ShipColor));
                    break;
                case DeviceClass.WormTailV2:
                    var tailSegmentV2 = stats.Prefab != null ? _services.PrefabCache.LoadPrefab(stats.Prefab) : _services.PrefabCache.LoadPrefab(stats.ObjectPrefab);
                    device = new WormTailDeviceV2(stats, tailSegmentV2, ship, _spaceObjectFactory);
                    break;
                case DeviceClass.Jammer:
                    device = new JammerDevice(TargetPriority.High);
                    break;
                case DeviceClass.DroneCamouflage:
                    device = new CamouflageDevice(stats.Power, 0f, stats.DeviceClass);
                    break;
                case DeviceClass.MissileCamouflage:
                    device = new CamouflageDevice(0f, stats.Power, stats.DeviceClass);
                    break;
                case DeviceClass.Retribution:
                    {
                        var statModifiers = new WeaponStatModifier { DamageMultiplier = StatMultiplier.FromValue(stats.Power * shipSpec.Stats.DamageMultiplier.Value) };
                        var bulletFactory = new BulletFactory(stats.AmmunitionId, statModifiers, _scene, _services, _spaceObjectFactory, _effectFactory, ship);
                        device = new RetributionDevice(ship, bulletFactory);
                    }
                    break;
                default:
                    return null;
            }

            if (stats.Sound && soundEffectCondition != ConditionType.None)
                device.AddTrigger(CreateSoundEffect(stats, soundEffectCondition));

            return device;
        }

        private IEffect CreateEffect(DeviceStats stats, IShip ship)
        {
            IEffect effect;
            if (stats.VisualEffect != null)
                effect = CompositeEffect.Create(stats.VisualEffect, _effectFactory, null);
            else if (stats.EffectPrefab)
                effect = _effectFactory.CreateEffect(stats.EffectPrefab);
            else
                return null;

            effect.Color = stats.Color;
            effect.Size = stats.Size * ship.Body.Scale;

            return effect;
        }

        private SoundEffect CreateSoundEffect(DeviceStats stats, ConditionType condition)
        {
            return stats.Sound ? new SoundEffect(_services.SoundPlayer, stats.Sound, condition) : null;
        }
    }
}
