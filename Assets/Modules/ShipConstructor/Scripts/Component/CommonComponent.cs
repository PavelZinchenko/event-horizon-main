using System.Collections.Generic;
using Constructor.Model;
using Constructor.Modification;
using Constructor.Ships;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Extensions;

namespace Constructor.Component
{
    public class CommonComponent : IComponent
    {
        private const float _shipSizeEnergyCostFactor = 0.05f;

        public CommonComponent(GameDatabase.DataModel.Component data, int shipSize)
        {
            _shipSize = shipSize;
            _component = data;
        }

        public ShipEquipmentStats GetStats()
        {
            var stats = ShipEquipmentStats.FromComponent(_component.Stats, _component.Layout.CellCount);

            if (_component.Device != null)
                stats.EnergyConsumption += DevicePassiveEnergyConsumption(_component.Device.Stats, _shipSize);

            if (_component.DroneBay != null)
                stats.EnergyConsumption += _component.DroneBay.Stats.PassiveEnergyConsumption * (0.5f + _shipSize * 0.005f);

            if (Modification != null)
                Modification.Apply(ref stats);

            return stats;
        }

        public float PassiveEnergyConsumption
        {
            get
            {
                var energyConsumption = -_component.Stats.EnergyRechargeRate;
                if (_component.Device != null)
                    energyConsumption += DevicePassiveEnergyConsumption(_component.Device.Stats, _shipSize);
                if (_component.DroneBay != null)
                    energyConsumption += _component.DroneBay.Stats.PassiveEnergyConsumption;

                return energyConsumption > 0 ? energyConsumption : 0f;
            }
        }

        public bool IsSuitable(IShipModel ship)
        {
            return CompatibilityChecker.IsCompatibleComponent(_component, ship);
        }

        public void UpdateWeaponPlatform(IWeaponPlatformStats stats)
        {
            if (_component.Stats.AutoAimingArc > 0)
                stats.ChangeAutoAimingArc(_component.Stats.AutoAimingArc);
            if (_component.Stats.TurretTurnSpeed != 0f)
                stats.ChangeTurnRate(_component.Stats.TurretTurnSpeed);
        }

        public IEnumerable<WeaponData> Weapons
        {
            get
            {
                if (_component.Weapon != null && _component.Ammunition != null)
                {
                    var statModifiers = new WeaponStatModifier();

                    if (Modification != null)
                        Modification.Apply(ref statModifiers);

                    yield return new WeaponData { Weapon = _component.Weapon, Ammunition = _component.Ammunition, StatModifier = statModifiers };
                }
            }
        }

        public IEnumerable<KeyValuePair<WeaponStats, AmmunitionObsoleteStats>> WeaponsObsolete
        {
            get
            {
                if (_component.Weapon != null && _component.AmmunitionObsolete != null)
                {
                    var weaponStats = _component.Weapon.Stats;
                    var ammoStats = _component.AmmunitionObsolete.Stats;

                    if (Modification != null)
                        Modification.Apply(ref weaponStats, ref ammoStats);

                    yield return new KeyValuePair<WeaponStats, AmmunitionObsoleteStats>(weaponStats, ammoStats);
                }
            }
        }

        public IEnumerable<DeviceStats> Devices
        {
            get
            {
                if (_component.Device != null)
                {
                    var stats = _component.Device.Stats;
                    stats.EnergyConsumption = DeviceEnergyConsumption(stats, _shipSize);

                    if (Modification != null)
                        Modification.Apply(ref stats);

                    yield return stats;
                }
            }
        }

        public IEnumerable<KeyValuePair<DroneBayStats, ShipBuild>> DroneBays
        {
            get
            {
                if (_component.DroneBay != null && _component.Drone != null)
                {
                    var stats = _component.DroneBay.Stats;

                    if (Modification != null)
                        Modification.Apply(ref stats);

                    yield return new KeyValuePair<DroneBayStats, ShipBuild>(stats, _component.Drone);
                }
            }
        }

        private float DevicePassiveEnergyConsumption(in DeviceStats stats, int shipSize)
        {
            var energy = stats.PassiveEnergyConsumption;
            if (stats.ScaleEnergyWithShipSize)
                energy *= _shipSizeEnergyCostFactor * shipSize;

            return energy;
        }

        private float DeviceEnergyConsumption(in DeviceStats stats, int shipSize)
        {
            var energy = stats.EnergyConsumption;
            if (stats.ScaleEnergyWithShipSize)
                energy *= _shipSizeEnergyCostFactor * shipSize;

            return energy;
        }

        public ActivationType ActivationType => _component.GetActivationType();
        public IComponentUpgrades Upgrades { get; set; }
        public IModification Modification { get; set; }

        private readonly int _shipSize;
        private readonly GameDatabase.DataModel.Component _component;
    }
}
