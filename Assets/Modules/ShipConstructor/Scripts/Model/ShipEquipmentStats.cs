using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace Constructor.Model
{
    public struct ShipEquipmentStats
    {
        public float ArmorPoints;
        public float ArmorRepairRate;
        public StatMultiplier ArmorRepairCooldownMultiplier;

        public float EnergyPoints;
        public float EnergyRecharge;
        public float EnergyConsumption;
        public StatMultiplier EnergyRechargeCooldownMultiplier;

        public float ShieldPoints;
        public float ShieldRechargeRate;
        public StatMultiplier ShieldRechargeCooldownMultiplier;

        public float ArmorRepairBaseCooldown;
        public float HullRepairBaseCooldown;
        public float EnergyRechargeBaseCooldown;
        public float ShieldRechargeBaseCooldown;

        public float Weight;
        public float WeightReduction;

        public float EnergyAbsorption;
        public float RammingDamage;
        public StatMultiplier RammingDamageMultiplier;

        public float KineticResistance;
        public float EnergyResistance;
        public float ThermalResistance;

        public float EnginePower;
        public float TurnRate;
        public float EnginePowerWithoutEnergy;
        public float TurnRateWithoutEnergy;
        public float EngineEnergyConsumption;

        public bool Autopilot;

        public StatMultiplier DroneRangeMultiplier;
        public StatMultiplier DroneDamageMultiplier;
        public StatMultiplier DroneDefenseMultiplier;
        public StatMultiplier DroneSpeedMultiplier;
        public float DroneReconstructionSpeed;
        public StatMultiplier DroneReconstructionTimeMultiplier;

        public StatMultiplier WeaponFireRateMultiplier;
        public StatMultiplier WeaponDamageMultiplier;
        public StatMultiplier WeaponRangeMultiplier;
        public StatMultiplier WeaponEnergyCostMultiplier;

        public static ShipEquipmentStats FromComponent(ComponentStats component, int cellCount)
        {
            var stats = new ShipEquipmentStats();

            var multiplier = component.Type == ComponentStatsType.PerOneCell ? cellCount : 1.0f;

            stats.ArmorPoints = component.ArmorPoints * multiplier;
            stats.ArmorRepairRate = component.ArmorRepairRate * multiplier;
            stats.ArmorRepairCooldownMultiplier = new StatMultiplier(component.ArmorRepairCooldownModifier * multiplier);

            stats.EnergyPoints = component.EnergyPoints * multiplier;
            stats.EnergyRechargeCooldownMultiplier = new StatMultiplier(component.EnergyRechargeCooldownModifier * multiplier);

            if (component.EnergyRechargeRate > 0)
                stats.EnergyRecharge = component.EnergyRechargeRate * multiplier;
            else
                stats.EnergyConsumption = -component.EnergyRechargeRate * multiplier;

            stats.ShieldPoints = component.ShieldPoints * multiplier;
            stats.ShieldRechargeRate = component.ShieldRechargeRate * multiplier;
            stats.ShieldRechargeCooldownMultiplier = new StatMultiplier(component.ShieldRechargeCooldownModifier * multiplier);

            if (component.Weight > 0)
                stats.Weight = multiplier * component.Weight;
            else if (component.Weight < 0)
                stats.WeightReduction = multiplier * component.Weight;

            stats.RammingDamage = component.RammingDamage * multiplier;
            stats.EnergyAbsorption = component.EnergyAbsorption * multiplier;

            stats.KineticResistance = component.KineticResistance * multiplier;
            stats.EnergyResistance = component.EnergyResistance * multiplier;
            stats.ThermalResistance = component.ThermalResistance * multiplier;

            stats.EnginePower = component.EnginePower * multiplier;
            stats.TurnRate = component.TurnRate * multiplier;

            if (component.EnergyRechargeRate >= 0 && component.EnginePower > 0)
                stats.EnginePowerWithoutEnergy += component.EnginePower * multiplier;
            if (component.EnergyRechargeRate >= 0 && component.TurnRate > 0)
                stats.TurnRateWithoutEnergy += component.TurnRate * multiplier;
            if (component.EnergyRechargeRate < 0 && component.EnginePower > 0)
                stats.EngineEnergyConsumption -= component.EnergyRechargeRate;

            stats.Autopilot = component.Autopilot;

            stats.DroneRangeMultiplier = new StatMultiplier(component.DroneRangeModifier * multiplier);
            stats.DroneDamageMultiplier = new StatMultiplier(component.DroneDamageModifier * multiplier);
            stats.DroneDefenseMultiplier = new StatMultiplier(component.DroneDefenseModifier * multiplier);
            stats.DroneSpeedMultiplier = new StatMultiplier(component.DroneSpeedModifier * multiplier);
            stats.DroneReconstructionTimeMultiplier = new StatMultiplier(component.DroneBuildTimeModifier * multiplier);
            stats.DroneReconstructionSpeed = component.DronesBuiltPerSecond * multiplier;

            stats.WeaponFireRateMultiplier = new StatMultiplier(component.WeaponFireRateModifier * multiplier);
            stats.WeaponDamageMultiplier = new StatMultiplier(component.WeaponDamageModifier * multiplier);
            stats.WeaponRangeMultiplier = new StatMultiplier(component.WeaponRangeModifier * multiplier);
            stats.WeaponEnergyCostMultiplier = new StatMultiplier(component.WeaponEnergyCostModifier * multiplier);

            return stats;
        }

        public void AddNegativeStatsOnly(in ShipEquipmentStats other)
        {
            EnergyConsumption += other.EnergyConsumption;
            Weight += other.Weight;
        }

        public void AddStats(in ShipEquipmentStats other)
        {
            ArmorPoints += other.ArmorPoints;
            ArmorRepairRate += other.ArmorRepairRate;
            ArmorRepairCooldownMultiplier += other.ArmorRepairCooldownMultiplier;

            EnergyPoints += other.EnergyPoints;
            EnergyRecharge += other.EnergyRecharge;
            EnergyConsumption += other.EnergyConsumption;
            EnergyRechargeCooldownMultiplier += other.EnergyRechargeCooldownMultiplier;

            ShieldPoints += other.ShieldPoints;
            ShieldRechargeRate += other.ShieldRechargeRate;
            ShieldRechargeCooldownMultiplier += other.ShieldRechargeCooldownMultiplier;

            Weight += other.Weight;
            WeightReduction += other.WeightReduction;

            EnergyAbsorption += other.EnergyAbsorption;
            RammingDamage += other.RammingDamage;
            RammingDamageMultiplier += other.RammingDamageMultiplier;

            KineticResistance += other.KineticResistance;
            EnergyResistance += other.EnergyResistance;
            ThermalResistance += other.ThermalResistance;

            EnginePower += other.EnginePower;
            TurnRate += other.TurnRate;
            EnginePowerWithoutEnergy += other.EnginePowerWithoutEnergy;
            TurnRateWithoutEnergy += other.TurnRateWithoutEnergy;
            EngineEnergyConsumption += other.EngineEnergyConsumption;

            Autopilot |= other.Autopilot;

            DroneRangeMultiplier += other.DroneRangeMultiplier;
            DroneDamageMultiplier += other.DroneDamageMultiplier;
            DroneDefenseMultiplier += other.DroneDefenseMultiplier;
            DroneSpeedMultiplier += other.DroneSpeedMultiplier;
            DroneReconstructionSpeed += other.DroneReconstructionSpeed;
            DroneReconstructionTimeMultiplier += other.DroneReconstructionTimeMultiplier;

            WeaponFireRateMultiplier += other.WeaponFireRateMultiplier;
            WeaponDamageMultiplier += other.WeaponDamageMultiplier;
            WeaponRangeMultiplier += other.WeaponRangeMultiplier;
            WeaponEnergyCostMultiplier += other.WeaponEnergyCostMultiplier;
        }
    }
}
