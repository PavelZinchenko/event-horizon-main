using Combat.Ai.BehaviorTree.Utils;
using Combat.Component.Systems.Weapons;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ChargeWeaponsNode : INode
	{
		private readonly ShipSystemList<IWeapon> _weapons;

		public static INode Create(IShip ship)
		{
			var weapons = ship.Systems.All.FindWeaponsByType(WeaponType.RequiredCharging);
			if (weapons.Count == 0) return EmptyNode.Failure;
			return new ChargeWeaponsNode(weapons);
		}

		public NodeState Evaluate(Context context)
		{
			int charging = 0;
			int charged = 0;

			for (int i = 0; i < _weapons.Count; ++i)
			{
				var weapon = _weapons[i];
				var energyCost = weapon.Value.Info.EnergyCost;

				if (context.EnergyLevel < energyCost && context.EnergyLevelPercentage < 0.99f)
					continue;

				context.Controls.ActivateSystem(context.SelectedWeapons.Ids[i]);
				var powerlevel = weapon.Value.PowerLevel;

				if (powerlevel < 1)
				{
					context.LockedEnergy += energyCost * (1f - powerlevel);
					charging++;
				}
				else 
					charged++;
			}

			if (charging > 0) return NodeState.Running;
			return charged > 0 ? NodeState.Success : NodeState.Failure;
		}

		private ChargeWeaponsNode(ShipSystemList<IWeapon> weapons)
		{
			_weapons = weapons;
		}
	}
}
