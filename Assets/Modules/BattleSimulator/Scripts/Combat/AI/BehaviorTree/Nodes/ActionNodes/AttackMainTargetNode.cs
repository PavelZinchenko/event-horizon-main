using GameDatabase.Enums;
using Combat.Ai.Calculations;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class AttackMainTargetNode : INode
	{
		private readonly bool _directOnly;

		public AttackMainTargetNode(AiDifficultyLevel aiLevel)
		{
			_directOnly = aiLevel < AiDifficultyLevel.Hard;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			UpdateTargetForTurrets(context);

			var result = AimAndAttackHandler.AttackWithAllWeapons(context.Ship, context.TargetShip,
				_directOnly, context.SelectedWeapons, context.Controls);

			if (HasFlag(result, AimAndAttackHandler.State.Attacking)) return NodeState.Success;
			if (HasFlag(result, AimAndAttackHandler.State.Aiming)) return NodeState.Running;
			return NodeState.Failure;
		}

		private void UpdateTargetForTurrets(Context context)
		{
			var weapons = context.SelectedWeapons;
			for (int i = 0; i < weapons.Count; ++i)
			{
				var weapon = weapons.GetWeaponByIndex(i);
				weapon.Platform.ActiveTarget = context.TargetShip;
			}
		}

		private static bool HasFlag(AimAndAttackHandler.State state, AimAndAttackHandler.State flag) => (state & flag) == flag;
	}
}
