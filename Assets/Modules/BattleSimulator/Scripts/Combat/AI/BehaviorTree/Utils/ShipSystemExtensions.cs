using System.Collections.Generic;
using Combat.Component.Systems;
using Combat.Component.Systems.Weapons;
using Combat.Component.Systems.Devices;

namespace Combat.Ai.BehaviorTree.Utils
{
	public static class ShipSystemsExtensions
	{
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
	}
}
