using System.Linq;
using System.Collections.Generic;
using Constructor.Model;
using Constructor.Modification;
using Constructor.Ships;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using GameDatabase.Model;

namespace Constructor.Component
{
    public class Empty : IComponent
    {
		public ShipEquipmentStats GetStats() => new ShipEquipmentStats();
		public void UpdateWeaponPlatform(IWeaponPlatformStats stats) {}
	    public bool IsSuitable(IShipModel ship) { return false; }
	    public IEnumerable<WeaponData> Weapons { get { return Enumerable.Empty<WeaponData>(); } }
        public IEnumerable<KeyValuePair<WeaponStats, AmmunitionObsoleteStats>> WeaponsObsolete { get { return Enumerable.Empty<KeyValuePair<WeaponStats, AmmunitionObsoleteStats>>(); } }
        public IEnumerable<DeviceStats> Devices { get { return Enumerable.Empty<DeviceStats>(); } }
        public IEnumerable<KeyValuePair<DroneBayStats, ShipBuild>> DroneBays { get { return Enumerable.Empty<KeyValuePair<DroneBayStats, ShipBuild>>(); } }
        public ActivationType ActivationType { get { return ActivationType.None; } }
        public IModification Modification { get; set; }
		public ImmutableCollection<ComponentMod> SuitableModifications => ImmutableCollection<ComponentMod>.Empty;
        public float PassiveEnergyConsumption => 0f;
        public IComponentUpgrades Upgrades { get => null; set {} }
    }
}
