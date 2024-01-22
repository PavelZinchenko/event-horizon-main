using GameDatabase.Enums;
using Combat.Ai.Calculations;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class AttackSecondaryTargetsNode : INode
	{
		private readonly bool _directOnly;

		public AttackSecondaryTargetsNode(AiDifficultyLevel aiLevel)
		{
			_directOnly = aiLevel < AiDifficultyLevel.Hard;
		}

		public NodeState Evaluate(Context context)
		{
			int activatedWeaponCount = 0;
			for (int i = 0; i < context.SecondaryTargets.Count; ++i)
				activatedWeaponCount += AimAndAttackHandler.AttackWithAllWeapons(context.Ship, context.SecondaryTargets[i],
					_directOnly, context.SelectedWeapons, context.Controls);

			return activatedWeaponCount > 0 ? NodeState.Running : NodeState.Failure;
		}
	}
}
