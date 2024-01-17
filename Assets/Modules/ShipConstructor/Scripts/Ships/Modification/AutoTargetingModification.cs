using System.Linq;
using Constructor.Model;
using GameDatabase.DataModel;
using Services.Localization;

namespace Constructor.Ships.Modification
{
    public class AutoTargetingModification : IShipModification
    {
        public static bool IsSuitable(Ship ship)
        {
            return /*ship.Info.SizeClass <= SizeClass.Destroyer &&*/ ship.Barrels.Any(item => item.AutoAimingArc == 0f);
        }

        public ModificationType Type => ModificationType.AutoTargeting;

        public string GetDescription(ILocalization localization)
        {
            return localization.GetString("$Ship_AutoTargeting");
        }

        public void Apply(ref ShipBaseStats stats)
        {
			for (int i = 0; i < stats.Barrels.Count; ++i)
			{
				var barrel = stats.Barrels[i];
				if (barrel.AutoAimingArc < _autoAiming)
					barrel.SetAutoAimingArc(_autoAiming);
			}
		}

		public bool ChangesBarrels => true;
		public int Seed => 0;
		private const float _autoAiming = 20f;
    }
}
