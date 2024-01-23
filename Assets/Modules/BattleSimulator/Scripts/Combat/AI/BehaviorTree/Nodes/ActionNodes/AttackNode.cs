using GameDatabase.Enums;
using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class AttackNode : INode
	{
		private readonly bool _directOnly;

		public AttackNode(AiDifficultyLevel aiLevel)
		{
			_directOnly = aiLevel < AiDifficultyLevel.Hard;
		}

		public NodeState Evaluate(Context context)
		{
			int activatedWeaponCount = 0;

			if (context.TargetShip != null)
				activatedWeaponCount += AimAndAttackHandler.AttackWithAllWeapons(context.Ship, context.TargetShip,
					_directOnly, context.SelectedWeapons, context.Controls);

			for (int i = 0; i < context.SecondaryTargets.Count; ++i)
				activatedWeaponCount += AimAndAttackHandler.AttackWithAllWeapons(context.Ship, context.SecondaryTargets[i],
					_directOnly, context.SelectedWeapons, context.Controls);

			return activatedWeaponCount > 0 ? NodeState.Running : NodeState.Failure;
		}
	}
}
