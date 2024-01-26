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
			var result = AimAndAttackHandler.State.Failed;

			for (int i = 0; i < context.SecondaryTargets.Count; ++i)
				result |= AimAndAttackHandler.AttackWithAllWeapons(context.Ship, context.SecondaryTargets[i],
					_directOnly, context.SelectedWeapons, context.Controls);

			if (HasFlag(result, AimAndAttackHandler.State.Attacking)) return NodeState.Success;
			if (HasFlag(result, AimAndAttackHandler.State.Aiming)) return NodeState.Running;
			return NodeState.Failure;
		}

		private static bool HasFlag(AimAndAttackHandler.State state, AimAndAttackHandler.State flag) => (state & flag) == flag;
	}
}
