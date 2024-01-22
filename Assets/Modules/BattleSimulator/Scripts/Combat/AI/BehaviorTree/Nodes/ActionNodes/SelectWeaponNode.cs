using System.Collections.Generic;
using Combat.Component.Ship;
using Combat.Component.Systems.Weapons;
using GameDatabase.Enums;
using Combat.Ai.BehaviorTree.Utils;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class SelectWeaponNode : INode
	{
		private readonly ShipWeaponList _weaponList;

		public static INode Create(IShip ship, AiWeaponCategory category)
		{
			if (category == AiWeaponCategory.All)
				return new SelectWeaponNode(null);

			var weapons = new List<int>();
			for (int i = 0; i < ship.Systems.All.Count; ++i)
			{
				var weapon = ship.Systems.All.Weapon(i);
				if (weapon == null) continue;

				if (IsBelongToCategory(weapon, category))
					weapons.Add(i);
			}

			if (weapons.Count == 0)
				return EmptyNode.Failure;

			return new SelectWeaponNode(new ShipWeaponList(ship, weapons));
		}

		public NodeState Evaluate(Context context)
		{
			context.SelectedWeapons = _weaponList;
			return context.SelectedWeapons.Count > 0 ? NodeState.Success : NodeState.Failure;
		}

		private SelectWeaponNode(ShipWeaponList weaponList)
		{
			_weaponList = weaponList;
		}

		private static bool IsBelongToCategory(IWeapon weapon, AiWeaponCategory category)
		{
			switch (category)
			{
				case AiWeaponCategory.Repair:
					return weapon.Info.Capability.HasCapability(WeaponCapability.RepairAlly);
				case AiWeaponCategory.Damage:
					return weapon.Info.Capability.HasCapability(WeaponCapability.DamageEnemy);
				case AiWeaponCategory.CaptureDrone:
					return weapon.Info.Capability.HasCapability(WeaponCapability.CaptureDrone);
				case AiWeaponCategory.All:
				default:
					return true;
			}
		}
	}
}
