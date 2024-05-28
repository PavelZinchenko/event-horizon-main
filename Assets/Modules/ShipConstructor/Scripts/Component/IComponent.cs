using System.Collections.Generic;
using Constructor.Model;
using Constructor.Modification;
using Constructor.Ships;
using GameDatabase.DataModel;
using GameDatabase.Enums;

namespace Constructor.Component
{
	public interface IWeaponPlatformStats
	{
        void ChangeAutoAimingArc(float angle);
        void ChangeTurnRate(float deltaAngle);
    }

    public interface IComponent
	{
        ShipEquipmentStats GetStats();
		void UpdateWeaponPlatform(IWeaponPlatformStats stats);
	    IEnumerable<WeaponData> Weapons { get; }
	    IEnumerable<KeyValuePair<WeaponStats, AmmunitionObsoleteStats>> WeaponsObsolete { get; }
		IEnumerable<KeyValuePair<DroneBayStats, ShipBuild>> DroneBays { get; }
        IEnumerable<DeviceStats> Devices { get; }
        ActivationType ActivationType { get; }
		bool IsSuitable(IShipModel ship);
        float PassiveEnergyConsumption { get; }
        IComponentUpgrades Upgrades { get; set; }
        IModification Modification { get; set; }
	}

    public struct WeaponData
    {
        public Weapon Weapon;
        public Ammunition Ammunition;
        public WeaponStatModifier StatModifier;
    }

    public static class ComponentExtensions
    {
        public static bool RequiresEnergyToInstall(this IComponent component) => component.PassiveEnergyConsumption > 0;
    }
}
