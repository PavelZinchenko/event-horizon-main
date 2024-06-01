using System.Linq;
using Constructor.Model;
using GameDatabase.DataModel;
using Services.Localization;

namespace Constructor.Ships.Modification
{
    public class EnergyRechargeCooldownMod : IShipModification
    {
        public EnergyRechargeCooldownMod(float value)
        {
            _energyReduction = value;
        }

        public static bool IsSuitable(Ship ship)
        {
            return ship.Barrels.Any(item => !string.IsNullOrEmpty(item.WeaponClass));
        }

        public ModificationType Type => ModificationType.EnergyRechargeCooldown;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Ship_EnergyCooldownMod", UnityEngine.Mathf.RoundToInt(_energyReduction*100));
        }

        public void Apply(ref ShipBaseStats stats)
        {
            stats.EnergyMultiplier *= 1f - _energyReduction;
            stats.EnergyRechargeCooldownMultiplier *= 0.01f;
        }

        public bool ChangesBarrels => false;
        public int Seed => 0;
        private float _energyReduction;
    }
}
