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
			var result = AimAndAttackHandler.State.Failed;

			if (context.TargetShip != null)
				result |= AimAndAttackHandler.AttackWithAllWeapons(context.Ship, context.TargetShip,
					_directOnly, context.SelectedWeapons, context.Controls);

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
