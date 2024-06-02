using System.Collections.Generic;
using System.ComponentModel;
using Constructor.Ships.Modification;
using GameDatabase;

public class ModificationFactory
{
    public ModificationFactory(IDatabase database)
    {
        _database = database;
    }

    public IShipModification Create(ModificationType type, int seed = -1)
    {
        if (seed < 0)
            seed = (_random ??= new()).Next();

        switch (type)
        {
            case ModificationType.Empty:
                return new EmptyModification();
            case ModificationType.AutoTargeting:
                return new AutoTargetingModification();
            case ModificationType.HeatDefense:
                return new HeatDefenseModification(seed, _database.ShipModSettings.HeatDefenseValue);
            case ModificationType.EnergyDefense:
                return new EnergyDefenseModification(seed, _database.ShipModSettings.EnergyDefenseValue);
            case ModificationType.KineticDefense:
                return new KineticDefenseModification(seed, _database.ShipModSettings.KineticDefenseValue);

            case ModificationType.ExtraBlueCells:
                return new EmptyModification();

            case ModificationType.Infected:
                return new InfectedModification(seed, _database);
            case ModificationType.LightWeight:
                return new LightWeightModification(seed, _database.ShipModSettings.WeightReduction);
            //case ModificationType.UnlimitedRespawn:
            //    return new UnlimitedRespawnModification();
            case ModificationType.WeaponClass:
                return new WeaponClassMod(_database.ShipModSettings.AttackReduction);
            case ModificationType.SatelliteSize:
                return new SatelliteSizeMod();
            case ModificationType.EnergyRechargeCooldown:
                return new EnergyRechargeCooldownMod(_database.ShipModSettings.EnergyReduction);
            case ModificationType.ShieldRechargeCooldown:
                return new ShieldRechargeCooldownMod(_database.ShipModSettings.ShieldReduction);
            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public IEnumerable<IShipModification> AvailableMods
    {
        get
        {
            yield return Create(ModificationType.Empty);
            yield return Create(ModificationType.AutoTargeting);
            yield return Create(ModificationType.HeatDefense);
            yield return Create(ModificationType.KineticDefense);
            yield return Create(ModificationType.EnergyDefense);
            yield return Create(ModificationType.LightWeight);
            yield return Create(ModificationType.Infected);
            //if (!_database.ShipModSettings.RemoveUnlimitedRespawnMod)
            //    yield return Create(ModificationType.UnlimitedRespawn);
            if (!_database.ShipModSettings.RemoveWeaponSlotMod)
                yield return Create(ModificationType.WeaponClass);

            if (!_database.ShipModSettings.RemoveBiggerSatellitesMod)
                yield return Create(ModificationType.SatelliteSize);
            if (!_database.ShipModSettings.RemoveEnergyRechargeCdMod)
                yield return Create(ModificationType.EnergyRechargeCooldown);
            if (!_database.ShipModSettings.RemoveShieldRechargeCdMod)
                yield return Create(ModificationType.ShieldRechargeCooldown);
        }
    }

    private System.Random _random;
    private readonly IDatabase _database;
}