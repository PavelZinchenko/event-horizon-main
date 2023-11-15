using Constructor.Model;
using GameDatabase;
using GameDatabase.DataModel;
using GameDatabase.Model;
using Services.Localization;

namespace Constructor.Ships.Modification
{
    public class InfectedModification : IShipModification
    {
        public InfectedModification(int seed, IDatabase database)
        {
            Seed = seed;
            _armorMultiplier = database.ShipModSettings.RegenerationArmor;
            _repairRate = database.ShipModSettings.RegenerationValue;
            _device = database.GetDevice(new ItemId<Device>(18)); // Toxic waste
        }

        public ModificationType Type => ModificationType.Infected;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Ship_Infected",
                UnityEngine.Mathf.RoundToInt((1f - _armorMultiplier)*100).ToString(), (_repairRate*100).ToString());
        }

        public void Apply(ref ShipBaseStats stats)
        {
            stats.RegenerationRate += _repairRate;
            stats.ArmorMultiplier *= _armorMultiplier;
            stats.BuiltinDevices += _device;
        }

        public int Seed { get; }

        private readonly float _armorMultiplier;
        private readonly float _repairRate;
        private readonly Device _device;
    }
}
