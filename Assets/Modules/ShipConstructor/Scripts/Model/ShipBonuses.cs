using GameDatabase.Model;

namespace Constructor.Model
{
    public struct ShipBonuses
    {
        public StatMultiplier ArmorPointsMultiplier;
        public StatMultiplier ShieldPointsMultiplier;
        public StatMultiplier EnergyMultiplier;
        public StatMultiplier DamageMultiplier;
        public StatMultiplier VelocityMultiplier;
        public StatMultiplier TurnRateMultiplier;
        public StatMultiplier RammingDamageMultiplier;
        public StatMultiplier ShieldRechargeMultiplier;

        public StatMultiplier ExtraHeatResistance;
        public StatMultiplier ExtraEnergyResistance;
        public StatMultiplier ExtraKineticResistance;
    }
}
