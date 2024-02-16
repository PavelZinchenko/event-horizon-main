using System.Collections.Generic;
using Combat.Component.Systems;
using Combat.Component.Systems.Weapons;
using Combat.Component.Systems.Devices;

namespace Combat.Ai.BehaviorTree.Utils
{
	public static class ShipSystemsExtensions
	{
        public static void CalculateAttackRange(this IReadOnlyList<ISystem> systems, out float rangeMin, out float rangeMax)
        {
            rangeMin = 0;
            rangeMax = 0;
            for (int i = 0; i < systems.Count; ++i)
                systems.UpdateAttackRangeIfWeapon(i, ref rangeMin, ref rangeMax);
        }

        public static bool UpdateAttackRangeIfWeapon(this IReadOnlyList<ISystem> systems, int id, ref float rangeMin, ref float rangeMax)
		{
			var weapon = systems[id] as IWeapon;
			if (weapon == null) return false;

			if (weapon.Info.Capability != WeaponCapability.None)
			{
				var range = weapon.Info.Range;
				if (range > rangeMax) rangeMax = range;
				if (range < rangeMin || rangeMin <= 0) rangeMin = range;
			}

			return true;
		}

		public static ShipSystemList<IDevice> FindDevices(this IReadOnlyList<ISystem> shipSystems, GameDatabase.Enums.DeviceClass deviceClass)
		{
			var systems = new ShipSystemList<IDevice>();
			for (int i = 0; i < shipSystems.Count; ++i)
				if (shipSystems[i] is IDevice device && device.DeviceClass == deviceClass)
					systems.Add(device, i);

			return systems;
		}

		public static ShipSystemList<IWeapon> FindWeaponsByType(this IReadOnlyList<ISystem> shipSystems, WeaponType weaponType)
		{
			var systems = new ShipSystemList<IWeapon>();
			for (int i = 0; i < shipSystems.Count; ++i)
				if (shipSystems[i] is IWeapon weapon && weapon.Info.WeaponType == weaponType)
					systems.Add(weapon, i);

			return systems;
		}

		public static bool HasWeapon(this IReadOnlyList<ISystem> systems, WeaponCapability capability = WeaponCapability.None)
		{
			foreach (var item in systems)
				if (item is IWeapon weapon)
					if (weapon.Info.Capability.HasCapability(capability))
						return true;

			return false;
		}

		public static bool HasWeapon(this IReadOnlyList<ISystem> systems, WeaponType type)
		{
			foreach (var item in systems)
				if (item is IWeapon weapon)
					if (weapon.Info.WeaponType == type)
						return true;

			return false;
		}

		public static bool HasDevice(this IReadOnlyList<ISystem> shipSystems, GameDatabase.Enums.DeviceClass deviceClass)
		{
			var systems = new ShipSystemList<IDevice>();
			for (int i = 0; i < shipSystems.Count; ++i)
				if (shipSystems[i] is IDevice device && device.DeviceClass == deviceClass)
					return true;

			return false;
		}
	}
}
