using GameDatabase.Enums;
using Combat.Ai.Calculations;
using Combat.Component.Ship;

namespace Combat.Ai.BehaviorTree.Nodes
{
	public class AttackMainTargetNode : INode
	{
		private readonly bool _directOnly;
        private readonly bool _allowRotation;

        public AttackMainTargetNode(AiDifficultyLevel aiLevel, bool allowRotation)
		{
			_directOnly = aiLevel < AiDifficultyLevel.Hard;
            _allowRotation = allowRotation;
		}

		public NodeState Evaluate(Context context)
		{
			if (context.TargetShip == null)
				return NodeState.Failure;

			UpdateTargetForTurrets(context);

            AimAndAttackHandler.State result;
            if (_allowRotation)
            {
                result = AimAndAttackHandler.AttackWithAllWeapons(context.Ship, context.TargetShip,
                    _directOnly, context.SelectedWeapons, context.Controls);
            }
            else
            {
                result = AimAndAttackHandler.AttackWhileStandingStill(context.Ship,
                    context.TargetShip, _directOnly, context.SelectedWeapons, context.Controls);
            }

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
