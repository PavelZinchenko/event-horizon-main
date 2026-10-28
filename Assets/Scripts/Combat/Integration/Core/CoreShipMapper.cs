using Combat.Domain;
using CombatSimulation.Core;
using CombatSimulation.Physics;
using Constructor.Model;
using GameDatabase;
using Services.Resources;

namespace Combat.Integration.Core
{
    // Maps a quick-combat ship to a core ship definition (migration phase 3c, owner decision S1).
    // Stats come from the ship and the hull from Ship.Collider (phase 4, step 2); every ship gets the same fixture
    // loadout: one cannon on device 0.
    // Unvalidated: phase 4 replaces it with the tested stat mapping and the real devices.
    public sealed class CoreShipMapper
    {
        public CoreShipMapper(IDatabase database, IResourceLocator resourceLocator)
        {
            _database = database;
            _hullBuilder = new CoreHullBuilder(resourceLocator);
        }

        public ShipDefinition Map(IShipInfo info)
        {
            var stats = info.Specification.Stats;
            var cellCount = stats.Layout.CellCount;

            var hull = _hullBuilder.Build(stats.ShipModel);

            // The fixture muzzle sits on the hull edge ahead of the ship; step 3 maps the real barrels.
            var reach = 0f;
            foreach (var shape in hull)
            foreach (var part in shape.Parts)
                reach = System.Math.Max(reach, part.Reach);
            var muzzle = new BarrelDefinition(new Vec2(reach, 0f));
            var cannon = new CannonDefinition(0, CannonCooldown, CannonEnergyCost, 0f, Bullet);

            return new ShipDefinition(
                hull,
                Positive(stats.Weight),
                ToEngine(new EngineStats(stats.EnginePower, stats.TurnRate, stats.Weight, cellCount, _database.ShipSettings)),
                Positive(stats.ArmorPoints),
                new ResourceDefinition(NonNegative(stats.ShieldPoints), stats.ShieldRechargeRate, NonNegative(stats.ShieldRechargeCooldown)),
                new ResourceDefinition(NonNegative(stats.EnergyPoints), stats.EnergyRechargeRate, NonNegative(stats.EnergyRechargeCooldown)),
                ToEngine(new EngineStats(stats.EnginePowerWihoutEnergy, stats.TurnRateWihoutEnergy, stats.Weight, cellCount, _database.ShipSettings)),
                rammingDamageMultiplier: NonNegative(stats.RammingDamageMultiplier),
                barrels: new[] { muzzle },
                devices: new DeviceDefinition[] { cannon });
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

        // Fixture loadout. Untuned proposal values; change them here.
        private const float CannonCooldown = 0.2f;
        private const float CannonEnergyCost = 2f;
        private static readonly BulletDefinition Bullet = new BulletDefinition(radius: 0.15f, speed: 30f, range: 15f, damage: 20f,
            damageType: LegacyDamageModel.Impact);

        private const float MinValue = 0.01f;
        private const float DegreesToRadians = (float)(System.Math.PI / 180.0);

        private readonly IDatabase _database;
        private readonly CoreHullBuilder _hullBuilder;
    }
}
