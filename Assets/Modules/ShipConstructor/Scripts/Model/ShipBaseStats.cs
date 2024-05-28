using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace Constructor.Model
{
    public struct ShipBaseStats
    {
        public StatMultiplier DamageMultiplier;
        public StatMultiplier ArmorMultiplier;
        public StatMultiplier ShieldMultiplier;
        public StatMultiplier EnergyMultiplier;
        public StatMultiplier ShipWeightMultiplier;
        public StatMultiplier EquipmentWeightMultiplier;
        public StatMultiplier VelocityMultiplier;
        public StatMultiplier TurnRateMultiplier;
        public StatMultiplier EnergyResistanceMultiplier;
        public StatMultiplier HeatResistanceMultiplier;
        public StatMultiplier KineticResistanceMultiplier;
        public StatMultiplier ShieldRechargeCooldownMultiplier;
        public StatMultiplier EnergyRechargeCooldownMultiplier;
        public StatMultiplier DroneBuildSpeedMultiplier;
        public StatMultiplier DroneAttackMultiplier;
        public StatMultiplier DroneDefenseMultiplier;
        public float RegenerationRate;
        public bool UnlimitedRespawn;
        public IShipLayout Layout;

        public ImmutableCollection<Device> BuiltinDevices;
		public ImmutableCollection<Barrel> Barrels;
        public SizeClass MaxSatelliteSize;
    }
}
