using Constructor;
using GameServices.Player;

namespace Combat.Domain
{
    public static class ShipBuilderExtensions
    {
        public static ShipBuilder ApplyPlayerSkills(this ShipBuilder builder, PlayerSkills skills)
        {
            builder.Bonuses.DamageMultiplier *= skills.AttackMultiplier;
            builder.Bonuses.ArmorPointsMultiplier *= skills.DefenseMultiplier;
            builder.Bonuses.ShieldPointsMultiplier *= skills.DefenseMultiplier + skills.ShieldStrengthBonus;
            builder.Bonuses.ShieldRechargeMultiplier *= skills.ShieldRechargeMultiplier;

            builder.Bonuses.ExtraHeatResistance *= 1.0f + skills.HeatResistance;
            builder.Bonuses.ExtraEnergyResistance *= 1.0f + skills.EnergyResistance;
            builder.Bonuses.ExtraKineticResistance *= 1.0f + skills.KineticResistance;

            return builder;
        }
    }
}
