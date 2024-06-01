using System.Collections.Generic;
using System.Linq;
using Combat.Component.Systems;
using Combat.Component.Systems.Devices;
using Combat.Component.Systems.DroneBays;
using Combat.Component.Systems.Weapons;
using GameDatabase.Enums;

namespace Combat.Component.Ship
{
    public static class ShipSystemsExtensions
    {
        public static List<List<int>> GetKeyBindings(this IReadOnlyList<ISystem> shipSystems)
        {
            var keys = new Dictionary<int, List<int>>();
            for (var i = 0; i < shipSystems.Count; ++i)
            {
                var system = shipSystems[i];
                if (system.KeyBinding < 0)
                    continue;

                List<int> list;
                if (!keys.TryGetValue(system.KeyBinding, out list))
                {
                    list = new List<int>();
                    keys.Add(system.KeyBinding, list);
                }
                list.Add(i);
            }

            return keys.OrderBy(item => item.Key).Select(item => item.Value).ToList();
        }

        public static bool ShouReleaseButtonImmediately(this IEnumerable<ISystem> systems)
        {
            foreach (var system in systems)
            {
                if (system is IDevice device)
                {
                    switch (device.DeviceClass)
                    {
                        case DeviceClass.TeleporterV2:
                        case DeviceClass.Teleporter:
                        case DeviceClass.Detonator:
                        case DeviceClass.PointDefense:
                        case DeviceClass.PartialShield:
                        case DeviceClass.ClonningCenter:
                        case DeviceClass.TimeMachine:
                            break;
                        default:
                            return false;
                    }
                }
                if (system is IWeapon weapon)
                {
                    switch (weapon.Info.WeaponType)
                    {
                        case WeaponType.Continuous:
                            if (weapon.Info.Firerate > 0 && weapon.Info.Firerate < 1.0) return false;
                            break;
                    }
                }
            }

            return true;
        }

        public static IEnumerable<int> GetDroneBayIndices(this IReadOnlyList<ISystem> shipSystems)
        {
            for (var i = 0; i < shipSystems.Count; ++i)
            {
                var system = shipSystems.DroneBay(i);
                if (system == null || system.KeyBinding < 0)
                    continue;
                yield return i;
            }

            yield break;
        }

        public static IWeapon Weapon(this IReadOnlyList<ISystem> shipSystems, int id)
        {
            return shipSystems[id] as IWeapon;
        }

        public static IDroneBay DroneBay(this IReadOnlyList<ISystem> shipSystems, int id)
        {
            return shipSystems[id] as IDroneBay;
        }

        public static IDevice Device(this IReadOnlyList<ISystem> shipSystems, int id)
        {
            return shipSystems[id] as IDevice;
        }

		public static int FindFirstDevice(this IReadOnlyList<ISystem> shipSystems, GameDatabase.Enums.DeviceClass deviceClass)
		{
			for (int i = 0; i < shipSystems.Count; ++i)
				if (shipSystems[i] is IDevice device && device.DeviceClass == deviceClass)
					return i;

			return -1;
		}

		public static int FindFirst<T>(this IReadOnlyList<ISystem> shipSystems) where T : ISystem
		{
			for (int i = 0; i < shipSystems.Count; ++i)
				if (shipSystems[i] is T)
					return i;

			return -1;
		}

		public static int FindFirst<T>(this IReadOnlyList<ISystem> shipSystems, out T system) where T : class, ISystem
		{
			for (int i = 0; i < shipSystems.Count; ++i)
			{
				if (shipSystems[i] is not T item) continue;
				system = item;
				return i;
			}

			system = null;
			return -1;
		}
	}
}
