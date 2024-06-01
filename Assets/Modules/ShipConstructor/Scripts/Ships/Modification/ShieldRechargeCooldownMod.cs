using System.Linq;
using Constructor.Model;
using GameDatabase.DataModel;
using Services.Localization;

namespace Constructor.Ships.Modification
{
    public class ShieldRechargeCooldownMod : IShipModification
    {
        public ShieldRechargeCooldownMod(float value)
        {
            _shieldReduction = value;
        }

        public static bool IsSuitable(Ship ship)
        {
            return ship.Barrels.Any(item => !string.IsNullOrEmpty(item.WeaponClass));
        }

        public ModificationType Type => ModificationType.ShieldRechargeCooldown;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Ship_ShieldCooldownMod", UnityEngine.Mathf.RoundToInt(_shieldReduction*100));
        }

        public void Apply(ref ShipBaseStats stats)
        {
            stats.ShieldMultiplier *= 1f - _shieldReduction;
            stats.ShieldRechargeCooldownMultiplier *= 0;
        }

        public bool ChangesBarrels => false;
        public int Seed => 0;
        private float _shieldReduction;
    }
}
