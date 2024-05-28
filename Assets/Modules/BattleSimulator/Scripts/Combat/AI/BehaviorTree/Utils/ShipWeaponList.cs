using System.Collections.Generic;

namespace Combat.Ai.BehaviorTree.Utils
{
	public class ShipWeaponList
	{
		private readonly IReadOnlyList<WeaponWrapper> _weapons;
		private readonly float _rangeMin;
		private readonly float _rangeMax;

		public float RangeMax => _rangeMax;
		public float RangeMin => _rangeMin;

		public IReadOnlyList<WeaponWrapper> List => _weapons;

		public ShipWeaponList(ShipCapabilities shipCapabilities)
		{
            _weapons = shipCapabilities.Weapons;
            UpdateAttackRange(ref _rangeMin, ref _rangeMax);
		}

		public ShipWeaponList(IReadOnlyList<WeaponWrapper> weapons)
		{
			_weapons = weapons ?? System.Array.Empty<WeaponWrapper>();
            UpdateAttackRange(ref _rangeMin, ref _rangeMax);
        }

        private void UpdateAttackRange(ref float rangeMin, ref float rangeMax)
        {
            for (int i = 0; i < _weapons.Count; ++i)
            {
                var data = _weapons[i];
                var range = data.Weapon.Info.Range;
                if (range > rangeMax) rangeMax = range;
                if (range < rangeMin || rangeMin <= 0) rangeMin = range;
            }
        }
    }
}
