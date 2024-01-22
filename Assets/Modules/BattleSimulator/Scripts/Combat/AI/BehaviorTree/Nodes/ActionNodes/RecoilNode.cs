using Combat.Component.Ship;
using Combat.Ai.BehaviorTree.Utils;
using Combat.Component.Systems.Weapons;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class RecoilNode : INode
	{
		private ShipSystemList<IWeapon> _weapons;

		public static INode Create(IShip ship)
		{
			ShipSystemList<IWeapon> weapons = new();
			var systems = ship.Systems.All;
			for (int i = 0; i < systems.Count; ++i)
			{
				var weapon = systems.Weapon(i);
				if (weapon != null && weapon.Info.Recoil > 0.1f) // TODO: move to database
					weapons.Add(weapon, i);
			}

			return weapons.Count > 0 ? new RecoilNode(weapons) : EmptyNode.Failure;
		}

		public NodeState Evaluate(Context context)
		{
			for (int i = 0; i < _weapons.Count; ++i)
				context.Controls.ActivateSystem(_weapons[i].Id);

			return NodeState.Running;
		}

		private RecoilNode(ShipSystemList<IWeapon> weapons)
		{
			_weapons = weapons;
		}
	}
}
