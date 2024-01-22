using System.Collections.Generic;
using Combat.Component.Systems.Weapons;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree.Utils
{
	public class ShipWeaponList
	{
		private readonly IShip _ship;
		private readonly IReadOnlyList<int> _ids;

		public float RangeMax { get; }
		public float RangeMin { get; }

		public int Count => _ids.Count;
		public IReadOnlyList<int> Ids => _ids;
		public IWeapon GetWeaponById(int id) => _ship.Systems.All.Weapon(id);
		public IWeapon GetWeaponByIndex(int index) => _ship.Systems.All.Weapon(_ids[index]);

		public ShipWeaponList(IShip ship)
		{
			_ship = ship;
			RangeMin = RangeMax = -1;
			var list = new List<int>();
			var systems = ship.Systems.All;
			for (int i = 0; i < systems.Count; ++i)
			{
				var weapon = systems.Weapon(i);
				if (weapon == null) continue;
				list.Add(i);
				if (weapon.Info.Capability == WeaponCapability.None) continue;
				var range = weapon.Info.Range;
				if (range > RangeMax) RangeMax = range;
				if (range < RangeMin || RangeMin < 0) RangeMin = range;
			}

			_ids = list.AsReadOnly();
		}

		public ShipWeaponList(IShip ship, IReadOnlyList<int> weapons)
		{
			_ship = ship;
			_ids = weapons ?? EmptyList<int>.Instance;

			RangeMin = RangeMax = -1;
			for (int i = 0; i < weapons.Count; ++i)
			{
				var weapon = ship.Systems.All.Weapon(weapons[i]);
				if (weapon.Info.Capability == WeaponCapability.None) continue;
				var range = weapon.Info.Range;
				if (range > RangeMax) RangeMax = range;
				if (range < RangeMin || RangeMin < 0) RangeMin = range;
			}
		}
	}
}
