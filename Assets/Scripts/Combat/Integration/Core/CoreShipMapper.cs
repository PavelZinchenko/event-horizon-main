using System.Collections.Generic;
using System.Linq;
using Combat.Domain;
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

        public CoreShipMapping Map(IShipInfo info) => Map(info.Specification, isDrone: false);

        public CoreShipMapping Map(IShipSpecification specification, bool isDrone)
        {
            var stats = specification.Stats;
            var modelScale = stats.ShipModel.ModelScale;
            var barrels = new List<BarrelDefinition>();
            var devices = new List<DeviceDefinition>();
            var infos = new List<CoreDeviceInfo>();

            foreach (var platform in specification.Platforms)
            {
                // D9: satellites are not attached in phase 4, so their weapons have no barrel.
                var barrelIndex = -1;
                if (platform.Companion == null)
                {
                    barrelIndex = barrels.Count;
                    barrels.Add(ToBarrel(platform, modelScale));
                }

                foreach (var weapon in platform.Weapons)
                {
                    var name = $"weapon {weapon.Weapon.Id.Value}, ammunition {weapon.Ammunition.Id.Value}";
                    string reason;
                    var device = barrelIndex < 0
                        ? Inert("satellite weapon", out reason)
                        : CoreWeaponMapper.Map(weapon, barrelIndex, stats.ArmorMultiplier.Value, out reason);
                    var cost = device is BeamDefinition beam ? beam.ActivationCost : (device as CannonDefinition)?.EnergyCost ?? 0f;
                    Add(devices, infos, device, new CoreDeviceInfo(name, weapon.KeyBinding, weapon.Weapon.Stats.ControlButtonIcon, cost, reason));
                }

                // Until step 4 (D2) the core has no obsolete ammunition.
                foreach (var weapon in platform.WeaponsObsolete)
                    Add(devices, infos, null, new CoreDeviceInfo("weapon with obsolete ammunition", weapon.KeyBinding,
                        weapon.Weapon.ControlButtonIcon, 0f, "obsolete ammunition"));
            }

            foreach (var item in specification.Devices)
            {
                var device = item.Device;
                if (!IsLegacyDevice(device.DeviceClass))
                    continue;

                var key = IsAlwaysAutomatic(device.DeviceClass) ? -1 : item.KeyBinding;
                if (device.DeviceClass == DeviceClass.PointDefense)
                {
                    var pointDefense = new PointDefenseDefinition(
                        Positive(device.Size + modelScale / 2f),
                        NonNegative(device.Cooldown),
                        NonNegative(device.EnergyConsumption),
                        NonNegative(device.Power * stats.DamageMultiplier.Value));
                    Add(devices, infos, pointDefense,
                        new CoreDeviceInfo(DeviceName(device), key, device.ControlButtonIcon, device.EnergyConsumption, null));
                }
                else
                {
                    Add(devices, infos, null, new CoreDeviceInfo(DeviceName(device), key, device.ControlButtonIcon, 0f,
                        $"device {device.DeviceClass}"));
                }
            }

            if (!isDrone)
                AddDroneBays(specification, devices, infos);

            var definition = new ShipDefinition(
                _hullBuilder.Build(stats.ShipModel),
                Positive(stats.Weight),
                ToEngine(new EngineStats(stats.EnginePower, stats.TurnRate, stats.Weight, stats.Layout.CellCount, _database.ShipSettings)),
                Positive(stats.ArmorPoints),
                new ResourceDefinition(NonNegative(stats.ShieldPoints), stats.ShieldRechargeRate, NonNegative(stats.ShieldRechargeCooldown)),
                new ResourceDefinition(NonNegative(stats.EnergyPoints), stats.EnergyRechargeRate, NonNegative(stats.EnergyRechargeCooldown)),
                ToEngine(new EngineStats(stats.EnginePowerWihoutEnergy, stats.TurnRateWihoutEnergy, stats.Weight, stats.Layout.CellCount, _database.ShipSettings)),
                rammingDamageMultiplier: NonNegative(stats.RammingDamageMultiplier),
                barrels: barrels,
                devices: devices,
                resistances: LegacyDamageModel.Resistances(
                    Fraction(stats.KineticResistancePercentage),
                    Fraction(stats.EnergyResistancePercentage),
                    Fraction(stats.ThermalResistancePercentage),
                    Fraction(stats.ShieldCorrosiveResistancePercentage),
                    NonNegative(stats.EnergyAbsorptionPercentage),
                    Positive(stats.ArmorMultiplier.Value)));

            var mapping = new CoreShipMapping($"{(isDrone ? "drone" : "ship")} {specification.Info.Id.Value}", definition, infos);
            _mappings[definition] = mapping;
            return mapping;
        }

        // Legacy CreateDroneBays: clonning centers, then the replicator if the ship builds drones, then the bays.
        private void AddDroneBays(IShipSpecification specification, List<DeviceDefinition> devices, List<CoreDeviceInfo> infos)
        {
            foreach (var item in specification.ClonningCenters)
                Add(devices, infos, null, new CoreDeviceInfo(DeviceName(item.Device), item.KeyBinding, item.Device.ControlButtonIcon, 0f,
                    "clonning center"));

            if (!specification.DroneBays.Any())
                return;

            if (specification.Stats.DroneBuildSpeed > 0)
                Add(devices, infos, null, new CoreDeviceInfo("drone replicator", -1, SpriteId.Empty, 0f, "drone replicator"));

            foreach (var item in specification.DroneBays)
            {
                var bay = item.DroneBay;
                var drone = Map(BuildDrone(item), isDrone: true);
                Add(devices, infos, new DroneBayDefinition(System.Math.Max(1, bay.Capacity), NonNegative(bay.EnergyConsumption), drone.Definition),
                    new CoreDeviceInfo($"drone bay, drone {item.Drone.Id.Value}", item.KeyBinding, bay.ControlButtonIcon, 0f, null));
            }
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

        // Legacy FixedPlatform pose: (Position × 0.5 + dir(Rotation) × (Offset + Size) × 0.5) × ModelScale.
        private static BarrelDefinition ToBarrel(IWeaponPlatformData platform, float modelScale)
        {
            var angle = platform.Rotation * DegreesToRadians;
            var offset = (platform.Offset + platform.Size) * 0.5f;
            var x = (platform.Position.x * 0.5f + (float)System.Math.Cos(angle) * offset) * modelScale;
            var y = (platform.Position.y * 0.5f + (float)System.Math.Sin(angle) * offset) * modelScale;
            return new BarrelDefinition(new Vec2(x, y), angle);
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
        private const float DegreesToRadians = (float)(System.Math.PI / 180.0);

        private static readonly DeviceDefinition InertDefinition = new InertDeviceDefinition();

        private readonly IDatabase _database;
        private readonly CoreHullBuilder _hullBuilder;
        private readonly Dictionary<ShipDefinition, CoreShipMapping> _mappings = new Dictionary<ShipDefinition, CoreShipMapping>();
    }
}
