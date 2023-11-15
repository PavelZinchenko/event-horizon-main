using Constructor.Model;
using GameDatabase.Model;
using Services.Localization;

namespace Constructor.Ships.Modification
{
    public class LightWeightModification : IShipModification
    {
        public LightWeightModification(int seed, float value)
        {
            Seed = seed;
            _value = StatMultiplier.FromValue(value);
        }

        public ModificationType Type => ModificationType.LightWeight;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Ship_Lightweight", UnityEngine.Mathf.RoundToInt(-_value.Bonus * 100).ToString());
        }

        public void Apply(ref ShipBaseStats ship)
        {
            ship.ShipWeightMultiplier *= _value;
        }

        public int Seed { get; }

        private readonly StatMultiplier _value;
    }
}
