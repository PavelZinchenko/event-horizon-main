using System.Collections.Generic;
using System.Linq;
using Combat.Component.Systems.Devices;
using Combat.Domain;
using Combat.Factory;
using CombatSimulation.Ai;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using Constructor;
using Constructor.Model;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;
using Services.Resources;

namespace Combat.Integration.Core
{
    // Maps a ship build to a core ship definition (phase 4, step 3a; rules in TODO/battle_simulator_phase4.md,
    // "Ship mapping (3a)"). The device index is the legacy system index (ShipFactory.CreateShip), so key bindings and
    // the AI read the same order. Parts the core cannot run become inert devices (D1).
    public sealed class CoreShipMapper
    {
        public CoreShipMapper(IDatabase database, IResourceLocator resourceLocator)
        {
            _database = database;
            _hullBuilder = new CoreHullBuilder(resourceLocator);
        }

        // Every ship and drone mapped so far; pilots and views look up the facts the core does not keep.
        public IReadOnlyDictionary<ShipDefinition, CoreShipMapping> Mappings => _mappings;

        // The view data of every mapped weapon, keyed by its BulletDefinition (cannon) or BeamDefinition (step 5).
        public IReadOnlyDictionary<object, CoreBulletSource> BulletSources => _bulletSources;

        // The body image of each mapped worm tail's segments (phase 5, step 7d).
        public IReadOnlyDictionary<WormTailDefinition, SpriteId> SegmentImages => _segmentImages;

        // Inert reasons the coverage report also gives (phase 5, step 1).
        public const string ObsoleteAmmunitionReason = "obsolete ammunition";
        public const string DroneReplicatorReason = "drone replicator";

        // Null for a device class the core runs, else its inert reason (D1).
        public static string DeviceInertReason(DeviceClass deviceClass)
        {
            switch (deviceClass)
            {
                case DeviceClass.PointDefense:
                case DeviceClass.TimeMachine:
                case DeviceClass.TeleporterV2:
                case DeviceClass.Detonator:
                case DeviceClass.ClonningCenter:
                case DeviceClass.WormTailV2:
                case DeviceClass.Jammer:
                case DeviceClass.DroneCamouflage:
                case DeviceClass.MissileCamouflage:
                    return null;
                default:
                    return $"device {deviceClass}";
            }
        }

        public CoreShipMapping Map(IShipInfo info) => Map(info.Specification, isDrone: false);

        public CoreShipMapping Map(IShipSpecification specification, bool isDrone) => Map(specification, isDrone, null, false);

        // bay is the drone bay of a drone, else null; it selects the drone's tree and range (step 7f). A clone (phase 5,
        // step 7c) is a drone without a bay that runs the clone tree.
        private CoreShipMapping Map(IShipSpecification specification, bool isDrone, IDroneBayData bay, bool isClone)
        {
            var stats = specification.Stats;
            var modelScale = stats.ShipModel.ModelScale;
            var barrels = new List<BarrelDefinition>();
            var satellites = new List<SatelliteDefinition>();
            var satelliteSources = new List<CoreSatelliteSource>();
            var turrets = new List<CoreTurretSource>();
            var devices = new List<DeviceDefinition>();
            var infos = new List<CoreDeviceInfo>();
            var aiWeapons = new List<AiWeapon>();
            var aiDevices = new List<AiDevice>();
            var aiDroneBays = new List<AiDroneBay>();
            var tint = default(ColorData?);

            // Legacy JammerDevice and CamouflageDevice change the ship's features (phase 5, step 9a): the jammer sets High
            // priority, each camouflage raises its chance to the device Power, clamped to [0, 1].
            var targetPriority = isDrone ? TargetPriority.Low : TargetPriority.Normal;
            var chanceToAvoidDrone = 0f;
            var chanceToAvoidMissile = 0f;

            // Legacy ShipSpecDecorator scales the clone's weapon and ramming damage, but combat never reads the scaled
            // stats; the core applies them (owner, 2026-11-02: a legacy bug).
            var attack = isClone ? CloneMultiplier : 1f;

            foreach (var platform in specification.Platforms)
            {
                // Legacy ShipFactory.CreatePlatform: a hull barrel, or a satellite with at most one barrel in its own frame
                // (phase 5, step 12c). A satellite without a barrel has no weapons.
                var barrelIndex = -1;
                var companion = platform.Companion;
                if (companion == null)
                {
                    barrelIndex = barrels.Count;
                    barrels.Add(ToBarrel(platform, modelScale));
                    if (barrels[barrelIndex].IsTurret)
                        turrets.Add(new CoreTurretSource(barrelIndex, platform.Image, platform.Size * modelScale, stats.TurretColor.Color));
                }
                else
                {
                    var satellite = satellites.Count;
                    var satelliteScale = companion.Satellite.ModelScale;
                    var aiming = IsAimingSatellite(platform);
                    satellites.Add(ToSatellite(companion, platform, modelScale, aiming));
                    satelliteSources.Add(new CoreSatelliteSource(companion.Satellite.ModelImage, satelliteScale));
                    if (companion.Satellite.Barrels.Count > 0)
                    {
                        barrelIndex = barrels.Count;
                        barrels.Add(ToBarrel(platform, satelliteScale, satellite, aiming));
                        if (barrels[barrelIndex].IsTurret)
                            turrets.Add(new CoreTurretSource(barrelIndex, platform.Image, platform.Size * satelliteScale, stats.TurretColor.Color));
                    }
                }

                foreach (var data in platform.Weapons)
                {
                    var weapon = attack == 1f ? data : new ScaledWeapon(data, attack);
                    var name = $"weapon {weapon.Weapon.Id.Value}, ammunition {weapon.Ammunition.Id.Value}";
                    var device = CoreWeaponMapper.Map(weapon, barrelIndex, stats.ArmorMultiplier.Value, AddChildSource, out var reason);
                    AddBulletSource(device, weapon);
                    if (device != null)
                        aiWeapons.Add(ToAiWeapon(weapon, devices.Count, Positive(stats.Weight)));
                    var cost = device is BeamDefinition beam ? beam.ActivationCost : (device as CannonDefinition)?.EnergyCost ?? 0f;
                    Add(devices, infos, device, new CoreDeviceInfo(name, weapon.KeyBinding, weapon.Weapon.Stats.ControlButtonIcon, cost, reason));
                }

                // Until step 4 (D2) the core has no obsolete ammunition.
                foreach (var weapon in platform.WeaponsObsolete)
                    Add(devices, infos, null, new CoreDeviceInfo("weapon with obsolete ammunition", weapon.KeyBinding,
                        weapon.Weapon.ControlButtonIcon, 0f, ObsoleteAmmunitionReason));
            }

            foreach (var item in specification.Devices)
            {
                var device = item.Device;
                if (!IsLegacyDevice(device.DeviceClass))
                    continue;

                var key = IsAlwaysAutomatic(device.DeviceClass) ? -1 : item.KeyBinding;
                if (IsPassive(device.DeviceClass))
                {
                    if (device.DeviceClass == DeviceClass.Jammer)
                        targetPriority = TargetPriority.High;
                    else if (device.DeviceClass == DeviceClass.DroneCamouflage)
                        chanceToAvoidDrone = System.Math.Max(chanceToAvoidDrone, Fraction(device.Power));
                    else
                        chanceToAvoidMissile = System.Math.Max(chanceToAvoidMissile, Fraction(device.Power));

                    // The slot keeps the legacy system index; the device itself does nothing (CanBeActivated false).
                    aiDevices.Add(new AiDevice(devices.Count, device.DeviceClass, true));
                    Add(devices, infos, InertDefinition, new CoreDeviceInfo(DeviceName(device), key, device.ControlButtonIcon, 0f, null));
                    continue;
                }

                var mapped = MapDevice(device, stats, out var activationCost);
                if (mapped != null)
                {
                    // Legacy WarpDrive.MaxRange for the Chase node and DetonatorDevice.ActivationTime for DetonateShip;
                    // other classes ignore them.
                    var range = device.DeviceClass == DeviceClass.TeleporterV2 ? NonNegative(device.Range) : 0f;
                    var detonator = mapped as DetonatorDefinition;
                    aiDevices.Add(new AiDevice(devices.Count, device.DeviceClass, key < 0, range,
                        detonator?.Activation.Duration ?? 0f));

                    // Legacy CreateStrongExplosion draws the area in the device colour; the first detonator tints the
                    // ship during its lock.
                    if (detonator != null)
                    {
                        _bulletSources[detonator.Explosion] = new CoreBulletSource(null, ExplosionFlashSize,
                            device.Color, null);
                        tint = tint ?? device.Color;
                    }
                    if (mapped is WormTailDefinition tail)
                        _segmentImages[tail] = SegmentImage(device);
                    Add(devices, infos, mapped,
                        new CoreDeviceInfo(DeviceName(device), key, device.ControlButtonIcon, activationCost, null));
                }
                else
                {
                    Add(devices, infos, null, new CoreDeviceInfo(DeviceName(device), key, device.ControlButtonIcon, 0f,
                        DeviceInertReason(device.DeviceClass)));
                }
            }

            if (!isDrone)
                AddDroneBays(specification, devices, infos, aiDroneBays);

            var engine = new EngineStats(stats.EnginePower, stats.TurnRate, stats.Weight, stats.Layout.CellCount, _database.ShipSettings);
            var definition = new ShipDefinition(
                _hullBuilder.Build(stats.ShipModel),
                Positive(stats.Weight),
                ToEngine(engine),
                Positive(stats.ArmorPoints),
                new ResourceDefinition(NonNegative(stats.ShieldPoints), stats.ShieldRechargeRate, NonNegative(stats.ShieldRechargeCooldown)),
                new ResourceDefinition(NonNegative(stats.EnergyPoints), stats.EnergyRechargeRate, NonNegative(stats.EnergyRechargeCooldown)),
                ToEngine(new EngineStats(stats.EnginePowerWihoutEnergy, stats.TurnRateWihoutEnergy, stats.Weight, stats.Layout.CellCount, _database.ShipSettings)),
                rammingDamageMultiplier: NonNegative(stats.RammingDamageMultiplier) * attack,
                barrels: barrels,
                satellites: satellites,
                devices: devices,
                resistances: LegacyDamageModel.Resistances(
                    Fraction(stats.KineticResistancePercentage),
                    Fraction(stats.EnergyResistancePercentage),
                    Fraction(stats.ThermalResistancePercentage),
                    Fraction(stats.ShieldCorrosiveResistancePercentage),
                    NonNegative(stats.EnergyAbsorptionPercentage),
                    Positive(stats.ArmorMultiplier.Value)),
                // Legacy Body.WorldScale: the target size in the area falloff (phase 5, step 3).
                scale: NonNegative(modelScale),
                teleportImmune: stats.ShipModel.ShipType == ShipType.Starbase || stats.ShipModel.ShipType == ShipType.Special,
                targetPriority: targetPriority,
                chanceToAvoidMissile: chanceToAvoidMissile);

            // AI side table (phase 4, steps 6 and 7b): the legacy RequirementChecker facts and the target facts, without
            // inert devices (D6). Legacy Ship gives a drone Low priority; the jammer sets High (step 9a).
            var ai = new AiShipProfile(aiWeapons, aiDevices, aiDroneBays,
                isDrone: isDrone,
                isStarbase: stats.ShipModel.ShipType == ShipType.Starbase,
                sizeClass: stats.ShipModel.SizeClass,
                enginePower: stats.EnginePower,
                turnRate: stats.TurnRate,
                propulsion: engine.Propulsion,
                kineticResistance: stats.KineticResistancePercentage,
                rammingDamageMultiplier: stats.RammingDamageMultiplier * attack,
                modelScale: modelScale,
                chanceToAvoidDrone: chanceToAvoidDrone);

            // Legacy DroneBay and ControllerFactory.CreateDroneController and CreateCloneController (T6).
            var aiTree = isClone
                ? BehaviorTreeSelection.ForClone(_database.CombatSettings, specification.CustomAi)
                : bay == null
                    ? specification.CustomAi
                    : BehaviorTreeSelection.ForDrone(_database.CombatSettings, bay.DroneBay, bay.Drone.CustomAI, bay.Behaviour == DroneBehaviour.Defensive);
            var kind = isClone ? "clone" : isDrone ? "drone" : "ship";
            var mapping = new CoreShipMapping($"{kind} {specification.Info.Id.Value}", definition, infos,
                stats.ShipModel.ModelImage, modelScale, ai, aiTree, bay?.DroneBay.Range ?? 0f, stats.Autopilot, tint, isClone, turrets,
                satelliteSources);
            _mappings[definition] = mapping;
            return mapping;
        }

        // The device classes the core runs (DeviceInertReason null); null for the others. The activation cost is legacy
        // ISystem.ActivationCost, for the automatic use rule.
        private static DeviceDefinition MapDevice(DeviceStats device, IShipStats stats, out float activationCost)
        {
            activationCost = 0f;
            var modelScale = stats.ShipModel.ModelScale;
            switch (device.DeviceClass)
            {
                case DeviceClass.PointDefense:
                    activationCost = device.EnergyConsumption;
                    return new PointDefenseDefinition(
                        Positive(device.Size + modelScale / 2f),
                        NonNegative(device.Cooldown),
                        NonNegative(device.EnergyConsumption),
                        NonNegative(device.Power * stats.DamageMultiplier.Value));
                // Legacy InfinityStone: Cooldown, and Lifetime for the time without contacts (phase 5, step 7a).
                case DeviceClass.TimeMachine:
                    return new InfinityStoneDefinition(NonNegative(device.Cooldown), NonNegative(device.Lifetime));
                // Legacy WarpDrive: EnergyConsumption once per use, Power as the speed, Range (phase 5, step 7a).
                case DeviceClass.TeleporterV2:
                    activationCost = NonNegative(device.EnergyConsumption);
                    return new WarpDriveDefinition(activationCost, NonNegative(device.Cooldown), Positive(device.Power),
                        NonNegative(device.Range));
                // Legacy DetonatorDevice: EnergyConsumption once per use, Lifetime as the lock, and a heat explosion of
                // Power x cells x damage multiplier within Range x sqrt(model scale) (phase 5, step 7b).
                case DeviceClass.Detonator:
                    activationCost = NonNegative(device.EnergyConsumption);
                    return new DetonatorDefinition(activationCost, NonNegative(device.Cooldown), NonNegative(device.Lifetime),
                        DetonatorDefinition.CreateExplosion(
                            NonNegative(device.Power * stats.Layout.CellCount * stats.DamageMultiplier.Value),
                            (int)DamageType.Heat, Positive(device.Range), NonNegative(modelScale), Positive(stats.Weight)));
                // Legacy WormTailDeviceV2: Power x the ship's armor per segment, RoundToInt(Size) segments, Cooldown to
                // regrow, Offset for the hinges; the circle of the segment prefab (phase 5, step 7d).
                case DeviceClass.WormTailV2:
                    return new WormTailDefinition(NonNegative(device.Power), System.Math.Max(1, (int)System.Math.Round(device.Size)),
                        NonNegative(device.Cooldown), device.Offset.x, device.Offset.y,
                        IsSegment2(device) ? Segment2Radius : SegmentRadius);
                default:
                    return null;
            }
        }

        // Legacy DeviceFactory: the database prefab record (built on the WormSegment prefab), else ObjectPrefab.
        private static bool IsSegment2(DeviceStats device) =>
            device.Prefab == null && device.ObjectPrefab.ToString() == new PrefabId(Segment2Prefab, PrefabId.Type.Object).ToString();

        private static SpriteId SegmentImage(DeviceStats device) =>
            device.Prefab is GameObjectPrefab_WormTailSegment segment ? segment.BodyImage :
            new SpriteId(IsSegment2(device) ? Segment2Image : SegmentImageName, SpriteId.Type.Satellite);

        // Legacy CreateDroneBays: clonning centers, then the replicator if the ship builds drones, then the bays.
        private void AddDroneBays(IShipSpecification specification, List<DeviceDefinition> devices, List<CoreDeviceInfo> infos,
            List<AiDroneBay> aiDroneBays)
        {
            // Legacy ClonningDevice: EnergyConsumption once per clone and Cooldown (phase 5, step 7c). The clone is the
            // ship through ShipSpecDecorator; every center of the ship launches the same clone.
            CoreShipMapping clone = null;
            foreach (var item in specification.ClonningCenters)
            {
                clone = clone ?? Map(new ClonningDevice.ShipSpecDecorator(specification, CloneMultiplier, CloneMultiplier,
                    CloneMultiplier), isDrone: true, bay: null, isClone: true);
                aiDroneBays.Add(new AiDroneBay(devices.Count, item.KeyBinding < 0));
                var device = item.Device;
                Add(devices, infos, new ClonningCenterDefinition(NonNegative(device.EnergyConsumption), NonNegative(device.Cooldown),
                    clone.Definition), new CoreDeviceInfo(DeviceName(device), item.KeyBinding, device.ControlButtonIcon, 0f, null));
            }

            if (!specification.DroneBays.Any())
                return;

            if (specification.Stats.DroneBuildSpeed > 0)
                Add(devices, infos, null, new CoreDeviceInfo("drone replicator", -1, SpriteId.Empty, 0f, DroneReplicatorReason));

            foreach (var item in specification.DroneBays)
            {
                var bay = item.DroneBay;
                var drone = Map(BuildDrone(item), isDrone: true, item, isClone: false);
                aiDroneBays.Add(new AiDroneBay(devices.Count, item.KeyBinding < 0));
                Add(devices, infos, new DroneBayDefinition(System.Math.Max(1, bay.Capacity), NonNegative(bay.EnergyConsumption), drone.Definition),
                    new CoreDeviceInfo($"drone bay, drone {item.Drone.Id.Value}", item.KeyBinding, bay.ControlButtonIcon, 0f, null));
            }
        }

        // Legacy BulletStats: BodySize is Body.Size at power level 1; the colour is the weapon's, else the body's.
        private void AddBulletSource(DeviceDefinition device, IWeaponData weapon)
        {
            object key = device is CannonDefinition cannon ? cannon.Bullet : device as BeamDefinition;
            if (key == null)
                return;

            var body = weapon.Ammunition.Body;
            _bulletSources[key] = new CoreBulletSource(body.BulletPrefab, body.Size, weapon.Stats.Color ?? body.Color,
                HitEffect(weapon.Ammunition));
        }

        // Phase 5, step 2: a child bullet looks like its own ammunition, in the colour of its trigger (legacy CreateFactory).
        private void AddChildSource(BulletDefinition bullet, Ammunition ammunition, ColorData color) =>
            _bulletSources[bullet] = new CoreBulletSource(ammunition.Body.BulletPrefab, ammunition.Body.Size, color,
                HitEffect(ammunition));

        // Legacy WeaponInfo from BulletStats at power level 1. Recoil counts for common weapons only, per unit of ship weight.
        // The spread is the weapon's, in radians.
        private static AiWeapon ToAiWeapon(IWeaponData weapon, int deviceIndex, float shipWeight)
        {
            var bullet = new BulletStats(weapon.Ammunition, weapon.Stats, false);
            var weaponClass = ToAiWeaponClass(weapon.Weapon.Stats.WeaponClass);
            var recoil = weaponClass == AiWeaponClass.Common ? bullet.Recoil / shipWeight : 0f;
            return new AiWeapon(deviceIndex, weaponClass, bullet.BehaviorType, (AiWeaponCapability)bullet.Capability,
                bullet.BulletHitRange, recoil, weapon.Weapon.Stats.Spread * DegreesToRadians);
        }

        // Legacy WeaponBase.GetWeaponType.
        private static AiWeaponClass ToAiWeaponClass(WeaponClass weaponClass)
        {
            switch (weaponClass)
            {
                case WeaponClass.Manageable:
                    return AiWeaponClass.Manageable;
                case WeaponClass.Continuous:
                    return AiWeaponClass.Continuous;
                case WeaponClass.RequiredCharging:
                    return AiWeaponClass.RequiredCharging;
                default:
                    return AiWeaponClass.Common;
            }
        }

        private static BulletTrigger_PlaySfx HitEffect(Ammunition ammunition)
        {
            foreach (var trigger in ammunition.Triggers)
                if (trigger.Condition == BulletTriggerCondition.Hit && trigger is BulletTrigger_PlaySfx sfx && sfx.VisualEffect != null)
                    return sfx;

            return null;
        }

        // Legacy DroneBay constructor without the random speed spread (owner, 2026-10-29).
        private IShipSpecification BuildDrone(IDroneBayData data)
        {
            var stats = data.DroneBay;
            var builder = new ShipBuilder(data.Drone);
            builder.Bonuses.ArmorPointsMultiplier = StatMultiplier.FromValue(stats.DefenseMultiplier);
            builder.Bonuses.ShieldPointsMultiplier = StatMultiplier.FromValue(stats.DefenseMultiplier);
            builder.Bonuses.DamageMultiplier = StatMultiplier.FromValue(stats.DamageMultiplier);
            builder.Bonuses.VelocityMultiplier = StatMultiplier.FromValue(stats.SpeedMultiplier);
            builder.Bonuses.TurnRateMultiplier = StatMultiplier.FromValue(stats.SpeedMultiplier);
            return builder.Build(_database.ShipSettings);
        }

        // Legacy FixedPlatform pose: (Position × 0.5 + dir(Rotation) × (Offset + Size) × 0.5) × the carrier's model scale.
        // The barrel of an aiming satellite is a fixed platform: the satellite turns instead.
        private static BarrelDefinition ToBarrel(IWeaponPlatformData platform, float modelScale,
            int satellite = BarrelDefinition.Hull, bool fixedMount = false)
        {
            var angle = platform.Rotation * DegreesToRadians;
            var offset = (platform.Offset + platform.Size) * 0.5f;
            var x = (platform.Position.x * 0.5f + (float)System.Math.Cos(angle) * offset) * modelScale;
            var y = (platform.Position.y * 0.5f + (float)System.Math.Sin(angle) * offset) * modelScale;
            return new BarrelDefinition(new Vec2(x, y), angle, fixedMount ? 0f : NonNegative(platform.AutoAimingArc) * DegreesToRadians,
                NonNegative(platform.RotationSpeed) * DegreesToRadians,
                (bool)platform.Image && (platform.Weapons.Any() || platform.WeaponsObsolete.Any()), offset * modelScale, satellite);
        }

        // Legacy SatelliteFactory.CreateSatellite: an AutoAimingSatelliteController for an arc of at least 5° without an
        // image and below 360°; else a SatelliteController that keeps the platform rotation.
        private static bool IsAimingSatellite(IWeaponPlatformData platform) =>
            platform.Companion.Satellite.Barrels.Count > 0 && !(bool)platform.Image &&
            platform.AutoAimingArc >= MinAimingSatelliteArc && platform.AutoAimingArc < 360f;

        // Legacy SatelliteFactory: the left slot (-0.5, 0.5) × ship scale + (0, 1.5 × satellite scale) with the arc
        // -10°..190°, the right slot mirrored. The weight is cells × DefaultWeightPerCell / 1000 (CompanionSpec).
        private static SatelliteDefinition ToSatellite(ICompanionData companion, IWeaponPlatformData platform, float shipScale,
            bool aiming)
        {
            var side = companion.Location == CompanionLocation.Left ? 1f : -1f;
            var satelliteScale = companion.Satellite.ModelScale;
            var slot = new Vec2(-0.5f * shipScale, side * (0.5f * shipScale + 1.5f * satelliteScale));
            var min = aiming ? (side > 0f ? -10f : -190f) * DegreesToRadians : 0f;
            var max = aiming ? (side > 0f ? 190f : 10f) * DegreesToRadians : 0f;
            return new SatelliteDefinition(slot, platform.Rotation * DegreesToRadians, Positive(companion.Weight), min, max);
        }

        private static void Add(List<DeviceDefinition> devices, List<CoreDeviceInfo> infos, DeviceDefinition device, CoreDeviceInfo info)
        {
            devices.Add(device ?? InertDefinition);
            infos.Add(info);
        }

        private static DeviceDefinition Inert(string reason, out string inertReason)
        {
            inertReason = reason;
            return null;
        }

        // Built-in devices have no name; the class names them in logs.
        private static string DeviceName(DeviceStats device) =>
            string.IsNullOrEmpty(device.Name) ? device.DeviceClass.ToString() : device.Name;

        // Legacy DeviceFactory.Create returns null for any other class, and the device gets no system index.
        private static bool IsLegacyDevice(DeviceClass deviceClass) =>
            deviceClass >= DeviceClass.Accelerator && deviceClass <= DeviceClass.Retribution;

        // Legacy devices that only change the ship's features and never activate (phase 5, step 9a).
        private static bool IsPassive(DeviceClass deviceClass) =>
            deviceClass == DeviceClass.Jammer || deviceClass == DeviceClass.DroneCamouflage ||
            deviceClass == DeviceClass.MissileCamouflage;

        // These legacy devices ignore the key binding: SystemBase(-1).
        private static bool IsAlwaysAutomatic(DeviceClass deviceClass)
        {
            switch (deviceClass)
            {
                case DeviceClass.Brake:
                case DeviceClass.ToxicWaste:
                case DeviceClass.WormTail:
                case DeviceClass.WormTailV2:
                case DeviceClass.Jammer:
                case DeviceClass.DroneCamouflage:
                case DeviceClass.MissileCamouflage:
                case DeviceClass.Retribution:
                    return true;
                default:
                    return false;
            }
        }

        // Legacy engine values are in degrees; the core uses radians.
        private static EngineDefinition ToEngine(EngineStats engine) =>
            new EngineDefinition(
                NonNegative(engine.Propulsion),
                NonNegative(engine.Velocity),
                NonNegative(engine.TurnRate * DegreesToRadians),
                NonNegative(engine.AngularVelocity * DegreesToRadians));

        private static float Positive(float value) => value > MinValue ? value : MinValue;
        private static float NonNegative(float value) => value > 0f ? value : 0f;
        private static float Fraction(float value) => value > 0f ? value < 1f ? value : 1f : 0f;

        private const float MinValue = 0.01f;
        private const float MinAimingSatelliteArc = 5f;

        // Legacy ClonningDevice: size, attack and defence factors of its ShipSpecDecorator (phase 5, step 7c). Defence
        // scales armor, shield and their rates; attack scales weapon damage and the ramming damage multiplier.
        private const float CloneMultiplier = 0.9f;

        // The circle colliders of the legacy segment prefabs at body scale 1 (WormSegment, WormSegment2) and their body
        // sprites. A mod prefab other than WormSegment2 gets the WormSegment circle.
        private const float SegmentRadius = 0.4419455f;
        private const float Segment2Radius = 0.35618508f;
        private const string Segment2Prefab = "WormSegment2";
        private const string SegmentImageName = "worm_segment";
        private const string Segment2Image = "worm_segment2";

        // The hit flash of a detonator explosion: the generic size 1 (D11), not the 20-unit area.
        private const float ExplosionFlashSize = 1f;
        private const float DegreesToRadians = (float)(System.Math.PI / 180.0);

        private static readonly DeviceDefinition InertDefinition = new InertDeviceDefinition();

        // A weapon with its damage multiplier scaled: legacy ShipBuilder's platform damage multiplier for a clone.
        private sealed class ScaledWeapon : IWeaponData
        {
            public ScaledWeapon(IWeaponData weapon, float damage)
            {
                _weapon = weapon;
                _damage = damage;
            }

            public Weapon Weapon => _weapon.Weapon;
            public Ammunition Ammunition => _weapon.Ammunition;
            public int KeyBinding => _weapon.KeyBinding;

            public WeaponStatModifier Stats
            {
                get
                {
                    var stats = _weapon.Stats;
                    stats.DamageMultiplier *= _damage;
                    return stats;
                }
            }

            private readonly IWeaponData _weapon;
            private readonly float _damage;
        }

        private readonly IDatabase _database;
        private readonly CoreHullBuilder _hullBuilder;
        private readonly Dictionary<ShipDefinition, CoreShipMapping> _mappings = new Dictionary<ShipDefinition, CoreShipMapping>();
        private readonly Dictionary<object, CoreBulletSource> _bulletSources = new Dictionary<object, CoreBulletSource>();
        private readonly Dictionary<WormTailDefinition, SpriteId> _segmentImages = new Dictionary<WormTailDefinition, SpriteId>();
    }
}
