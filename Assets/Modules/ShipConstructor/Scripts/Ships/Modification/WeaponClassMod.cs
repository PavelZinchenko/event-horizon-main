using System.Linq;
using Constructor.Model;
using GameDatabase.DataModel;
using Services.Localization;

namespace Constructor.Ships.Modification
{
    public class WeaponClassMod : IShipModification
    {
        public WeaponClassMod(float value)
        {
            _attackReduction = value;
        }

        public static bool IsSuitable(Ship ship)
        {
            return ship.Barrels.Any(item => !string.IsNullOrEmpty(item.WeaponClass));
        }

        public ModificationType Type => ModificationType.WeaponClass;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Ship_WeaponClassMod", UnityEngine.Mathf.RoundToInt(_attackReduction*100));
        }

        public void Apply(ref ShipBaseStats stats)
        {
            stats.IgnoreWeaponClass = true;
            stats.DamageMultiplier *= 1f - _attackReduction;
        }

        public int Seed => 0;
        private float _attackReduction;
    }
}
