using System.Collections.Generic;
using Combat.Component.Systems.Weapons;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree.Utils
{
	public class ShipWeaponList
	{
		private readonly IReadOnlyList<int> _ids;
		private readonly IShip _ship;
		private readonly float _rangeMin;
		private readonly float _rangeMax;

		public float RangeMax => _rangeMax;
		public float RangeMin => _rangeMin;

		public int Count => _ids.Count;
		public IReadOnlyList<int> Ids => _ids;
		public IWeapon GetWeaponById(int id) => _ship.Systems.All.Weapon(id);
		public IWeapon GetWeaponByIndex(int index) => _ship.Systems.All.Weapon(_ids[index]);

		public ShipWeaponList(IShip ship)
		{
			_ship = ship;
			var list = new List<int>();
			var systems = ship.Systems.All;
			for (int i = 0; i < systems.Count; ++i)
				if (systems.UpdateAttackRangeIfWeapon(i, ref _rangeMin, ref _rangeMax))
					list.Add(i);

			_ids = list.AsReadOnly();
		}

		public ShipWeaponList(IShip ship, IReadOnlyList<int> weapons)
		{
			_ship = ship;
			_ids = weapons ?? EmptyList<int>.Instance;

			var systems = ship.Systems.All;
			for (int i = 0; i < weapons.Count; ++i)
				systems.UpdateAttackRangeIfWeapon(weapons[i], ref _rangeMin, ref _rangeMax);
		}
	}
}
