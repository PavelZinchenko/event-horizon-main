namespace Combat.Ai.BehaviorTree.Nodes
{
	public class ChargeWeaponsNode : INode
	{
		public NodeState Evaluate(Context context)
		{
			int charging = 0;
			int charged = 0;

			for (int i = 0; i < context.SelectedWeapons.Count; ++i)
			{
				var weapon = context.SelectedWeapons.GetWeaponByIndex(i);
				if (weapon.Info.WeaponType != Component.Systems.Weapons.WeaponType.RequiredCharging) continue;
				context.Controls.ActivateSystem(context.SelectedWeapons.Ids[i]);

				if (weapon.PowerLevel >= 1)
					charged++;
				else
					charging++;
			}

			if (charging > 0) return NodeState.Running;
			return charged > 0 ? NodeState.Success : NodeState.Failure;
		}
	}
}
