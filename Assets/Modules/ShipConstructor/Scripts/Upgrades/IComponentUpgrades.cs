using Constructor.Model;
using GameDatabase.DataModel;

namespace Constructor
{
    public interface IComponentUpgrades
    {
        void Apply(ref ShipEquipmentStats stats);
        void Apply(ref WeaponStats weapon, ref AmmunitionObsoleteStats ammunition);
        void Apply(ref WeaponStatModifier statModifier);
        void Apply(ref DeviceStats device);
        void Apply(ref DroneBayStats droneBay);
    }
}
