using Constructor.Model;
using Services.Localization;

namespace Constructor.Ships.Modification
{
    public class HeatDefenseModification : IShipModification
    {
        public HeatDefenseModification(int seed, float value)
        {
            Seed = seed;
            _value = value;
        }

        public ModificationType Type => ModificationType.HeatDefense;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Ship_HeatDefense");
        }

        public void Apply(ref ShipBaseStats stats)
        {
            stats.HeatResistanceMultiplier += _value;
        }

        public int Seed { get; }
		public bool ChangesBarrels => false;

		private readonly float _value;
    }
}
